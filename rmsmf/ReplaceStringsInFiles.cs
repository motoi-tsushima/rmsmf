using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace rmsmf
{
    public class ReplaceStringsInFiles
    {
        /// <summary>
        /// 置換単語テーブル
        /// </summary>
        private string[,] _replaceWords;

        /// <summary>
        /// 置換対象ファイル名一覧
        /// </summary>
        private string[] _files = null;

        /// <summary>
        /// 書き込みBOM指定が有効です(NULLの場合はBOM指定なし)
        /// </summary>
        private bool? _enableBOM = false;

        /// <summary>
        /// 文字エンコーディングの自動判定モード
        /// </summary>
        private CommandOptions.EncodingDetectionType _encodingDetectionMode = CommandOptions.EncodingDetectionType.Normal;
        /// <summary>
        /// 文字エンコーディングの自動判定モード
        /// </summary>
        public CommandOptions.EncodingDetectionType EncodingDetectionMode
        {
            get { return _encodingDetectionMode; }
            set { this._encodingDetectionMode = value; }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="replaceWords">置換単語テーブル</param>
        /// <param name="files">置換対象ファイル名一覧</param>
        public ReplaceStringsInFiles(string[,] replaceWords, string[] files, bool? enableBOM)
        {
            this._replaceWords = replaceWords;
            this._files = files;
            this._enableBOM = enableBOM;
        }

        /// <summary>
        /// 置換実行
        /// </summary>
        /// <param name="encoding">読み取りファイルの文字エンコーディング</param>
        /// <param name="writeEncoding">書き込みファイルの文字エンコーディング</param>
        /// <param name="writeNewLine">書き込みファイルの改行コード</param>
        /// <returns>成功した場合はtrue</returns>
        public bool Replace(Encoding encoding, Encoding writeEncoding, string writeNewLine)
        {
            try
            {
                // ファイル単位でマルチスレッド処理を実行
                Parallel.ForEach(this._files, fileName =>
                    ProcessSingleFile(fileName, encoding, writeEncoding, writeNewLine));
                
                return true;
            }
            catch (UnauthorizedAccessException uae)
            {
                throw new RmsmfException(uae.Message, uae);
            }
            catch (AggregateException ae)
            {
                HandleAggregateException(ae);
                return false;
            }
        }

        /// <summary>
        /// 単一ファイルの置換処理を実行
        /// </summary>
        /// <param name="fileName">処理対象ファイル名</param>
        /// <param name="encoding">読み取りエンコーディング</param>
        /// <param name="writeEncoding">書き込みエンコーディング</param>
        /// <param name="writeNewLine">書き込み改行コード</param>
        private void ProcessSingleFile(string fileName, Encoding encoding, Encoding writeEncoding, string writeNewLine)
        {
            if (!File.Exists(fileName))
            {
                return;
            }

            string tempFileName = CreateTempFileWithReplacements(fileName, encoding, writeEncoding, writeNewLine);
            
            if (tempFileName != null)
            {
                ReplaceOriginalFileAtomically(fileName, tempFileName);
            }
        }

        /// <summary>
        /// 置換処理を行い一時ファイルを作成
        /// </summary>
        /// <param name="fileName">元ファイル名</param>
        /// <param name="encoding">読み取りエンコーディング</param>
        /// <param name="writeEncoding">書き込みエンコーディング</param>
        /// <param name="writeNewLine">書き込み改行コード</param>
        /// <returns>作成した一時ファイル名（失敗時はnull）</returns>
        private string CreateTempFileWithReplacements(
            string fileName, 
            Encoding encoding, 
            Encoding writeEncoding, 
            string writeNewLine)
        {
            string tempFileName = fileName + FileConstants.TempFileExtension;

            // 一時ファイルがすでに存在する場合は削除
            if (File.Exists(tempFileName))
            {
                File.Delete(tempFileName);
            }

            // ファイルを開いてエンコーディングを判定
            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                var encodingResult = EncodingHelper.DetectOrUseSpecifiedEncoding(
                    fs, fileName, encoding, this._encodingDetectionMode);

                if (encodingResult.Encoding == null)
                {
                    // エンコーディング不明の場合は処理をスキップ
                    HandleUnknownEncoding(fileName, encodingResult);
                    return null;
                }

                // 書き込みエンコーディングの決定
                int writeCodePage = DetermineWriteCodePage(writeEncoding, encodingResult.CodePage);
                Encoding finalWriteEncoding = GetWriteEncoding(
                    writeCodePage, encodingResult.BomExists, this._enableBOM);

                // 置換処理を実行
                using (var reader = new StreamReader(fs, encodingResult.Encoding, true))
                {
                    ReadWriteForReplace(reader, tempFileName, encodingResult.Encoding, finalWriteEncoding, writeNewLine);
                }
            }

            return tempFileName;
        }

        /// <summary>
        /// エンコーディング不明時の処理
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <param name="encodingResult">エンコーディング判定結果</param>
        private void HandleUnknownEncoding(string fileName, EncodingDetectionResult encodingResult)
        {
            string dispLine = EncodingHelper.CreateUnknownEncodingDisplayLine(
                fileName, encodingResult.BomExists, encodingResult.CodePage);
            Console.WriteLine("{0}", dispLine);
        }

        /// <summary>
        /// 書き込みコードページを決定
        /// </summary>
        /// <param name="writeEncoding">指定された書き込みエンコーディング</param>
        /// <param name="sourceCodePage">元ファイルのコードページ</param>
        /// <returns>使用する書き込みコードページ</returns>
        private int DetermineWriteCodePage(Encoding writeEncoding, int sourceCodePage)
        {
            return writeEncoding == null ? sourceCodePage : writeEncoding.CodePage;
        }

        /// <summary>
        /// 一時ファイルで元ファイルを原子的に置換
        /// </summary>
        /// <param name="originalFileName">元ファイル名</param>
        /// <param name="tempFileName">一時ファイル名</param>
        private void ReplaceOriginalFileAtomically(string originalFileName, string tempFileName)
        {
            string backupFileName = originalFileName + FileConstants.BackupFileExtension;
            bool backupCreated = false;

            try
            {
                // バックアップを作成
                File.Copy(originalFileName, backupFileName, true);
                backupCreated = true;

                // 元ファイルを削除
                File.Delete(originalFileName);

                // 一時ファイルを元ファイル名に変更
                File.Move(tempFileName, originalFileName);

                // 成功したらバックアップを削除
                if (File.Exists(backupFileName))
                {
                    File.Delete(backupFileName);
                }
            }
            catch (Exception ex)
            {
                // エラー時はバックアップから復元を試みる
                RestoreFromBackup(originalFileName, tempFileName, backupFileName, backupCreated);
                throw new RmsmfException($"ファイル操作に失敗しました: {originalFileName}", ex);
            }
        }

        /// <summary>
        /// バックアップからファイルを復元
        /// </summary>
        /// <param name="originalFileName">元ファイル名</param>
        /// <param name="tempFileName">一時ファイル名</param>
        /// <param name="backupFileName">バックアップファイル名</param>
        /// <param name="backupCreated">バックアップが作成されたかどうか</param>
        private void RestoreFromBackup(
            string originalFileName, 
            string tempFileName, 
            string backupFileName, 
            bool backupCreated)
        {
            if (!backupCreated || !File.Exists(backupFileName))
            {
                return;
            }

            try
            {
                // 元ファイルが削除されていたら復元
                if (!File.Exists(originalFileName))
                {
                    File.Move(backupFileName, originalFileName);
                }

                // 一時ファイルがあれば削除
                if (File.Exists(tempFileName))
                {
                    File.Delete(tempFileName);
                }
            }
            catch (Exception restoreEx)
            {
                // 復元に失敗した場合も警告を表示
                Console.WriteLine($"Warning: Failed to restore backup: {restoreEx.Message}");
            }
        }

        /// <summary>
        /// 集約例外を処理
        /// </summary>
        /// <param name="ae">集約例外</param>
        private void HandleAggregateException(AggregateException ae)
        {
            // エラー詳細情報を収集
            var errorDetails = ae.InnerExceptions
                .Select((ex, index) => new
                {
                    Index = index + 1,
                    FileName = ExtractFileNameFromException(ex),
                    ErrorType = ex.GetType().Name,
                    Message = ex.Message
                })
                .ToList();

            // 各エラーの詳細を表示
            Console.WriteLine();
            Console.WriteLine("=== エラー詳細 ===");
            foreach (var error in errorDetails)
            {
                Console.WriteLine($"エラー {error.Index}:");
                
                if (!string.IsNullOrEmpty(error.FileName))
                {
                    Console.WriteLine($"  ファイル: {error.FileName}");
                }
                
                Console.WriteLine($"  種類: {error.ErrorType}");
                Console.WriteLine($"  メッセージ: {error.Message}");
                Console.WriteLine();
            }

            // サマリー情報を表示
            string errorMessage = string.Format(ValidationMessages.ErrorsOccurred, errorDetails.Count) +
                                  " " + ValidationMessages.OtherFilesProcessedSuccessfully;
            throw new RmsmfException(errorMessage, ae);
        }

        /// <summary>
        /// 例外からファイル名を抽出
        /// </summary>
        /// <param name="ex">例外</param>
        /// <returns>ファイル名（抽出できない場合はnull）</returns>
        private string ExtractFileNameFromException(Exception ex)
        {
            // RmsmfException からファイル名を抽出
            if (ex is RmsmfException rmsmfEx)
            {
                // "ファイル操作に失敗しました: ファイル名" のパターン
                var match = System.Text.RegularExpressions.Regex.Match(
                    rmsmfEx.Message, 
                    @"(?:ファイル操作に失敗しました|File operation failed):\s*(.+)$");
                
                if (match.Success && match.Groups.Count > 1)
                {
                    return match.Groups[1].Value.Trim();
                }

                // "ファイル ファイル名 が" のパターン
                match = System.Text.RegularExpressions.Regex.Match(
                    rmsmfEx.Message,
                    @"(?:ファイル|File)\s+(.+?)\s+(?:が|is)");

                if (match.Success && match.Groups.Count > 1)
                {
                    return match.Groups[1].Value.Trim();
                }
            }

            // IOException や UnauthorizedAccessException からファイル名を抽出
            if (ex is IOException || ex is UnauthorizedAccessException)
            {
                // メッセージから一般的なパターンでファイルパスを抽出
                var match = System.Text.RegularExpressions.Regex.Match(
                    ex.Message,
                    @"'([^']+)'|""([^""]+)""|:\s*([^\s,]+)");

                if (match.Success)
                {
                    for (int i = 1; i < match.Groups.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(match.Groups[i].Value))
                        {
                            return match.Groups[i].Value;
                        }
                    }
                }
            }

            // スタックトレースからファイル情報を取得（最終手段）
            if (!string.IsNullOrEmpty(ex.StackTrace))
            {
                var match = System.Text.RegularExpressions.Regex.Match(
                    ex.StackTrace,
                    @"fileName[^:]*:\s*([^\r\n]+)");

                if (match.Success && match.Groups.Count > 1)
                {
                    return match.Groups[1].Value.Trim();
                }
            }

            return null;
        }

        /// <summary>
        /// 置換メイン処理
        /// </summary>
        /// <param name="reader">読み取りファイルストリーム</param>
        /// <param name="writeFileName">書き込みファイル名</param>
        /// <param name="encoding">読み取りファイルの文字エンコーディング</param>
        /// <param name="writeEncoding">書き込みファイルの文字エンコーディング</param>
        /// <param name="writeNewline">書き込みファイルの改行コード</param>
        /// <returns>正常終了=true</returns>
        public bool ReadWriteForReplace(StreamReader reader, string writeFileName, Encoding encoding, Encoding writeEncoding, string writeNewline)
        {
            bool rc = true;

            // 書き込みファイルを開く
            using (var writer = new StreamWriter(writeFileName, true, writeEncoding))
            {
                // 読み取りファイルを全て読み込む
                string content = reader.ReadToEnd();
                
                // StringBuilderを使用して効率的に置換処理を行う
                StringBuilder sb = new StringBuilder(content);

                if(this._replaceWords != null)
                {
                    // 置換単語リストの行単位に置換を実施する
                    int replaceWordsCount = this._replaceWords.GetLength(1);

                    for (int i = 0; i < replaceWordsCount; i++)
                    {
                        // StringBuilderのReplaceメソッドを使用（効率的）
                        sb.Replace(this._replaceWords[0, i], this._replaceWords[1, i]);
                    }
                }

                if(writeNewline != null)
                {
                    // まず全ての改行コードをLFに正規化
                    sb.Replace("\r\n", "\n");
                    sb.Replace("\r", "\n");

                    // 目的の改行コードに変換
                    if(writeNewline == CommandOptions.NewLineCRLF)
                    {
                        sb.Replace("\n", "\r\n");
                    }
                    else if(writeNewline == CommandOptions.NewLineCR)
                    {
                        sb.Replace("\n", "\r");
                    }
                    // NewLineLFの場合は既にLFなので何もしない
                }

                // 書き込みファイルへ上書きする
                writer.Write(sb.ToString());
            }

            return rc;
        }

        /// <summary>
        /// 書き込みエンコーディング取得
        /// </summary>
        /// <param name="codePage">書き込むコードページ</param>
        /// <param name="bomExist">読み込みファイルのBOMの有無</param>
        /// <param name="enableBOM">オプションのBOM指定の有無</param>
        /// <returns>書き込みエンコーディング</returns>
        private Encoding GetWriteEncoding(int codePage, bool bomExist, bool? enableBOM)
        {
            Encoding writeEncoding = null;

            // BOMなし
            if (enableBOM == null)
            {
                // BOMは存在するか
                if (bomExist)
                {
                    // BOMは存在する
                    // BOM と同じエンコーディングで書き込む
                    writeEncoding = Encoding.GetEncoding(codePage);
                }
                else
                {
                    // BOMは存在しない
                    // 書き込みエンコーディングをリセット
                    // utf-8
                    if (codePage == 65001)
                        writeEncoding = new UTF8Encoding(false);
                    // utf-16 Little Endian
                    else if (codePage == 1200)
                        writeEncoding = new UnicodeEncoding(false, false);
                    // utf-16 Big Endian
                    else if (codePage == 1201)
                        writeEncoding = new UnicodeEncoding(true, false);
                    // utf-32 Little Endian
                    else if (codePage == 12000)
                        writeEncoding = new UTF32Encoding(false, false);
                    // utf-32 Big Endian
                    else if (codePage == 12001)
                        writeEncoding = new UTF32Encoding(true, false);
                    else
                        writeEncoding = Encoding.GetEncoding(codePage);
                }
            }
            // BOM有り
            else
            {
                // 書き込み文字エンコーディングとBOMを再設定する

                bool existByteOrderMark = enableBOM == true ? true : false;

                // 書き込みエンコーディングの指定が無い場合、BOM指定のみ再設定する

                // utf-8
                if (codePage == 65001)
                    writeEncoding = new UTF8Encoding(existByteOrderMark);
                // utf-16 Little Endian
                else if (codePage == 1200)
                    writeEncoding = new UnicodeEncoding(false, existByteOrderMark);
                // utf-16 Big Endian
                else if (codePage == 1201)
                    writeEncoding = new UnicodeEncoding(true, existByteOrderMark);
                // utf-32 Little Endian
                else if (codePage == 12000)
                    writeEncoding = new UTF32Encoding(false, existByteOrderMark);
                // utf-32 Big Endian
                else if (codePage == 12001)
                    writeEncoding = new UTF32Encoding(true, existByteOrderMark);
                else
                    writeEncoding = Encoding.GetEncoding(codePage);
            }

            return writeEncoding;
        }

    }
}
