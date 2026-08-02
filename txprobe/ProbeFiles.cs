using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rmsmf;

namespace txprobe
{
    public class ProbeFiles
    {
        /// <summary>
        /// 検索単語テーブル
        /// </summary>
        private string[] _searchWords;

        /// <summary>
        /// 検索対象ファイル名一覧
        /// </summary>
        private string[] _files = null;

        /// <summary>
        /// プローブ機能有効フラグ
        /// </summary>
        private bool _enableProbe = false;

        /// <summary>
        /// 出力ファイルリストのファイル名
        /// </summary>
        private string _output_filelist_filename = null;

        /// <summary>
        /// 出力ファイルリストの文字エンコーディング
        /// </summary>
        private Encoding _filesEncoding = null;

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
        /// 出力データコレクション（マルチスレッド対応）
        /// </summary>
        private ConcurrentBag<string> _outputCollection = null;

        public ProbeFiles(string[] searchWords, string[] files, bool enableProbe, string outputFileListName, Encoding filesEncoding) 
        { 
            _searchWords = searchWords;
            _files = files;
            _enableProbe = enableProbe;
            _output_filelist_filename = outputFileListName;
            _filesEncoding = filesEncoding ?? Encoding.UTF8;
        }

        /// <summary>
        /// _files に記載されたファイルの内容を探索する
        /// </summary>
        /// <param name="encoding">読み取りファイルの文字エンコーディング</param>
        /// <returns>true = 探索成功, false = 異常終了</returns>
        public bool Probe(Encoding encoding)
        {
            try
            {
                InitializeOutputCollection();

                // ファイル単位でマルチスレッド処理を実行
                Parallel.ForEach(this._files, fileName =>
                    ProcessSingleFile(fileName, encoding));

                // すべてのデータ収集が完了したら、ソートして書き込み
                FinalizeOutputCollection();

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
        /// 出力コレクションの初期化
        /// </summary>
        private void InitializeOutputCollection()
        {
            if (this._output_filelist_filename == null)
            {
                return;
            }

            _outputCollection = new ConcurrentBag<string>();

            // 出力ファイルのフルパスを取得
            string outputFullPath = Path.GetFullPath(this._output_filelist_filename);

            // 入力ファイルリストから出力ファイルを除外
            this._files = this._files.Where(f =>
                !string.Equals(Path.GetFullPath(f), outputFullPath, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }

        /// <summary>
        /// 出力コレクションのファイナライズ（ソートして出力）
        /// </summary>
        private void FinalizeOutputCollection()
        {
            if (this._output_filelist_filename != null && _outputCollection != null)
            {
                WriteSortedOutput();
            }
        }

        /// <summary>
        /// 単一ファイルの検索処理を実行
        /// </summary>
        /// <param name="fileName">処理対象ファイル名</param>
        /// <param name="encoding">読み取りエンコーディング</param>
        private void ProcessSingleFile(string fileName, Encoding encoding)
        {
            if (!File.Exists(fileName))
            {
                return;
            }

            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                // エンコーディングを判定または取得
                var encodingResult = rmsmf.EncodingHelper.DetectOrUseSpecifiedEncoding(
                    fs, fileName, encoding, 
                    (rmsmf.CommandOptions.EncodingDetectionType)this._encodingDetectionMode);

                if (encodingResult.Encoding == null)
                {
                    // エンコーディング不明の場合
                    HandleUnknownEncoding(fileName, encodingResult);
                    return;
                }

                // 検索処理を実行
                using (var reader = new StreamReader(fs, encodingResult.Encoding, true))
                {
                    ReadForSearch(fileName, reader, encodingResult);
                }
            }
        }

        /// <summary>
        /// エンコーディング不明時の処理
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <param name="encodingResult">エンコーディング判定結果</param>
        private void HandleUnknownEncoding(string fileName, rmsmf.EncodingDetectionResult encodingResult)
        {
            string dispLine = rmsmf.EncodingHelper.CreateUnknownEncodingDisplayLine(fileName, encodingResult);
            Console.WriteLine("{0}", dispLine);
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
            string errorMessage = string.Format(rmsmf.ValidationMessages.ErrorsOccurred, errorDetails.Count) +
                                  " " + rmsmf.ValidationMessages.OtherFilesProcessedSuccessfully;
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
                // "ファイル ファイル名 が大きすぎます" のパターン
                var match = System.Text.RegularExpressions.Regex.Match(
                    rmsmfEx.Message,
                    @"(?:ファイル|File)\s+(.+?)\s+(?:が|is)");

                if (match.Success && match.Groups.Count > 1)
                {
                    return match.Groups[1].Value.Trim();
                }

                // 一般的なパターン: "メッセージ: ファイル名"
                match = System.Text.RegularExpressions.Regex.Match(
                    rmsmfEx.Message,
                    @":\s*(.+)$");

                if (match.Success && match.Groups.Count > 1)
                {
                    var possibleFileName = match.Groups[1].Value.Trim();
                    // ファイルパスっぽいかチェック（拡張子があるか、パス区切り文字があるか）
                    if (possibleFileName.Contains("\\") || possibleFileName.Contains("/") ||
                        possibleFileName.Contains("."))
                    {
                        return possibleFileName;
                    }
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
        /// 収集したデータをソートしてファイルに書き込む
        /// </summary>
        private void WriteSortedOutput()
        {
            try
            {
                // ConcurrentBagからリストに変換してソート
                // StringComparer.OrdinalIgnoreCaseを使用してファイルパスをソート
                // （Windowsファイルシステムと同じ動作、カルチャー非依存）
                var sortedLines = _outputCollection.OrderBy(line => line, StringComparer.OrdinalIgnoreCase).ToList();

                // ソート済みデータをファイルに書き込み
                using (var ofs = new StreamWriter(this._output_filelist_filename, false, this._filesEncoding))
                {
                    foreach (var line in sortedLines)
                    {
                        ofs.WriteLine(line);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format(rmsmf.ValidationMessages.FileWriteError, ex.Message));
                throw;
            }
        }

        /// <summary>
        /// コレクションにデータを追加（スレッドセーフ）
        /// </summary>
        /// <param name="line">追加する行</param>
        private void AddToOutputCollection(string line)
        {
            if (_outputCollection != null)
            {
                _outputCollection.Add(line);
            }
        }

        /// <summary>
        /// 検索メイン処理
        /// </summary>
        /// <param name="fileName">読み取りファイル名</param>
        /// <param name="reader">読み取りファイルストリーム</param>
        /// <param name="encodingResult">エンコーディング判定結果（BOM有無・改行コードの種類を含む）</param>
        /// <returns>正常終了=true</returns>
        public bool ReadForSearch(string fileName, StreamReader reader, rmsmf.EncodingDetectionResult encodingResult)
        {
            bool rc = true;

            string dispBOM = rmsmf.EncodingHelper.GetBomDisplayString(encodingResult.BomExists);
            string encodingName = rmsmf.EncodingHelper.GetEncodingName(encodingResult.Encoding, encodingResult.EncodingInfo);
            string lineBreakType = rmsmf.EncodingHelper.GetLineBreakDisplayString(encodingResult.LineBreak);

            // 読み取りファイルを全て読み込む
            string readLine = reader.ReadToEnd();

            if (this._searchWords != null)
            {
                // 検索を実施する
                int searchWordsCount = this._searchWords.GetLength(0);
                bool wordFound = false;

                for (int i = 0; i < searchWordsCount; i++)
                {
                    if (readLine.IndexOf(this._searchWords[i]) >= 0)
                    {
                        // 検索単語の探索
                        if (this._enableProbe == true)
                        {
                            // プローブ機能が有効の場合、検索単語を探索する
                            // 検索単語が見つかった場合、結果を表示する
                            string dispLine = fileName + "," + this._searchWords[i];
                            Console.WriteLine("{0}", dispLine);
                            wordFound = true;
                        }
                        else
                        {
                            // プローブ機能が無効の場合、検索結果のみ表示する
                            wordFound = true;
                        }
                    }
                }

                if (this._enableProbe == false && wordFound == true)
                {
                    string dispLine = fileName + "," + encodingName + "," + lineBreakType + "," + dispBOM;
                    Console.WriteLine("{0}", dispLine);

                    // スレッドセーフなコレクションへの追加
                    AddToOutputCollection(dispLine);
                }
            }
            else
            {
                // 検索単語が指定されていない場合、ファイル探索結果のみ表示する
                string dispLine = fileName + "\t," + encodingName + "\t," + lineBreakType + "\t," + dispBOM;
                Console.WriteLine("{0}", dispLine);

                // スレッドセーフなコレクションへの追加
                AddToOutputCollection(dispLine);
            }

            return rc;
        }

    }
}
