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
        private CommandOptions.EncodingJudgmentType _encodingJudgmentMode = CommandOptions.EncodingJudgmentType.Normal;
        /// <summary>
        /// 文字エンコーディングの自動判定モード
        /// </summary>
        public CommandOptions.EncodingJudgmentType EncodingJudgmentMode
        {
            get { return _encodingJudgmentMode; }
            set { this._encodingJudgmentMode = value; }
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
        /// <returns></returns>
        public bool Replace(Encoding encoding, Encoding writeEncoding, string writeNewLine)
        {
            bool success = false;
            Encoding inEncoding;
            Encoding inWriteEncoding;

            try
            {
                //ファイル単位のマルチスレッド作成
                Parallel.ForEach(this._files, (fileName) =>
                    {
                        if (File.Exists(fileName))
                        {
                            string writeFileName = fileName + ".RP$";

                            // 書き込み対象ファイルがすでに存在する場合は削除する
                            if (File.Exists(writeFileName))
                            {
                                File.Delete(writeFileName);
                            }

                            // 読み取りファイルを開く
                            using (FileStream fs = new FileStream(fileName, FileMode.Open))
                            {
                                bool bomExist = false;
                                int codePage;
                                int writeCodePage;

                                // 読み込みエンコーディングの有無で分岐
                                if (encoding == null)
                                {
                                    // エンコーディング指定が無い場合

                                    // 読み取りファイルの文字エンコーディングを判定する
                                    long fileLength = fs.Length;
                                    
                                    // ファイルサイズ検証：2GB以上のファイルはエラー
                                    if (fileLength > int.MaxValue)
                                    {
                                        throw new RmsmfException($"ファイル {fileName} が大きすぎます（最大 2GB）。");
                                    }
                                    
                                    int fileSize = (int)fileLength;
                                    byte[] buffer = new byte[fileSize];
                                    int readCount = fs.Read(buffer, 0, fileSize);
                                    
                                    // ファイルポジションを先頭に戻す（StreamReaderが正しく読めるようにする）
                                    fs.Position = 0;

                                    ByteOrderMarkJudgment bomJudg = new ByteOrderMarkJudgment();

                                    if (bomJudg.IsBOM(buffer))
                                    {
                                        bomExist = true;
                                        codePage = bomJudg.CodePage;
                                    }
                                    else
                                    {
                                        bomExist = false;

                                        if (this._encodingJudgmentMode == CommandOptions.EncodingJudgmentType.Normal)
                                        {
                                            EncodingInfomation encInfo =
                                            EncodingJudgmentControl.NormalJudgeEncoding(buffer);

                                            codePage = encInfo.CodePage;
                                        }
                                        else if (this._encodingJudgmentMode == CommandOptions.EncodingJudgmentType.FirstParty)
                                        {
                                            EncodingInfomation encInfo =
                                            EncodingJudgmentControl.JudgeEncoding(buffer);

                                            codePage = encInfo.CodePage;
                                        }
                                        else if (this._encodingJudgmentMode == CommandOptions.EncodingJudgmentType.ThirdParty)
                                        {
                                            EncodingInfomation encInfo =
                                            EncodingJudgmentControl.JudgeUtfUnknown(buffer);

                                            codePage = encInfo.CodePage;
                                        }
                                        else
                                        {
                                            EncodingInfomation encInfo =
                                            EncodingJudgmentControl.NormalJudgeEncoding(buffer);

                                            codePage = encInfo.CodePage;
                                        }
                                    }

                                    if (codePage > 0)
                                    {
                                        try
                                        {
                                            inEncoding = Encoding.GetEncoding(codePage);
                                        }
                                        catch (ArgumentException)
                                        {
                                            // サポートされていないコードページの場合はnullを設定
                                            // （例: EUC-TW (51950) は .NET Framework 4.8/4.8.1 でサポートされていない）
                                            inEncoding = null;
                                            Console.WriteLine($"Warning: Code page {codePage} is not supported. Skipping {fileName}");
                                        }
                                        catch (NotSupportedException)
                                        {
                                            inEncoding = null;
                                            Console.WriteLine($"Warning: Code page {codePage} is not supported. Skipping {fileName}");
                                        }
                                    }
                                    else
                                    {
                                        inEncoding = null;
                                    }
                                }
                                else
                                {
                                    // エンコーディング指定が有る場合

                                    inEncoding = encoding;

                                    byte[] bomBuffer = new byte[4] { 0xFF, 0xFF, 0xFF, 0xFF };
                                    fs.Read(bomBuffer, 0, 4);
                                    fs.Position = 0;

                                    ByteOrderMarkJudgment bomJudg = new ByteOrderMarkJudgment();

                                    if (bomJudg.IsBOM(bomBuffer))
                                    {
                                        bomExist = true;
                                        //codePage = bomJudg.CodePage; //オプション指定のコードページが間違っていた時どうする ?
                                        codePage = encoding.CodePage;
                                    }
                                    else
                                    {
                                        bomExist = false;
                                        codePage = encoding.CodePage;
                                    }
                                }

                                if (inEncoding == null)
                                {
                                    //読み込みエンコーディングが不明な場合、処理をスキップする。
                                    string dispBOM;
                                    string lineBreakType = "EOL Unknown";
                                    string encodingName = "encoding Unknown";

                                    if (bomExist == true)
                                    {
                                        dispBOM = "BOM exists";
                                    }
                                    else
                                    {
                                        dispBOM = "No BOM";
                                    }

                                    string dispLine = fileName + "\t," + encodingName + "\t," + lineBreakType + "\t," + dispBOM;
                                    Console.WriteLine("{0}", dispLine);
                                    return;
                                }

                                //書き込みエンコーディングの有無で分岐
                                if (writeEncoding == null)
                                {
                                    writeCodePage = codePage;
                                }
                                else
                                {
                                    writeCodePage = writeEncoding.CodePage;
                                }

                                // 書き込みエンコーディングの再作成
                                inWriteEncoding = GetWriteEncoding(writeCodePage, bomExist, this._enableBOM);

                                // エンコーディングを指定してテキストストリームを開く
                                using (var reader = new StreamReader(fs, inEncoding, true))
                                {
                                    // 置換メイン処理
                                    ReadWriteForReplace(reader, writeFileName, inEncoding, inWriteEncoding, writeNewLine);
                                }
                            }

                            // ファイル操作の原子性を確保
                            string backupFileName = fileName + ".BACKUP$";
                            bool backupCreated = false;
                            
                            try
                            {
                                // バックアップファイルを作成
                                File.Copy(fileName, backupFileName, true);
                                backupCreated = true;
                                
                                // 元のファイルを削除
                                File.Delete(fileName);
                                
                                // 新しいファイルを元のファイル名に変更
                                File.Move(writeFileName, fileName);
                                
                                // 成功したらバックアップを削除
                                if (File.Exists(backupFileName))
                                {
                                    File.Delete(backupFileName);
                                }
                            }
                            catch
                            {
                                // エラー時にバックアップから復元を試みる
                                if (backupCreated && File.Exists(backupFileName))
                                {
                                    try
                                    {
                                        // 元のファイルが削除されていたら復元
                                        if (!File.Exists(fileName))
                                        {
                                            File.Move(backupFileName, fileName);
                                        }
                                        
                                        // 一時ファイルがあれば削除
                                        if (File.Exists(writeFileName))
                                        {
                                            File.Delete(writeFileName);
                                        }
                                    }
                                    catch
                                    {
                                        // 復元に失敗した場合もエラーを再スロー
                                    }
                                }
                                throw;
                            }
                        }
                    }
                );
            }
            catch (UnauthorizedAccessException uae)
            {
                throw new RmsmfException(uae.Message, uae);
            }
            catch (AggregateException ae)
            {
                int errorCount = 0;
                foreach(var ie in ae.InnerExceptions)
                {
                    errorCount++;
                    Console.WriteLine(ie.Message);
                }

                throw new RmsmfException(errorCount + "件のエラーが発生しました。他のファイルは正常に処理しました。", ae);
            }

            return success;
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

            //BOMなし
            if (enableBOM == null)
            {
                //BOMは存在するか
                if (bomExist)
                {
                    //BOMは存在する

                    // BOM と同じエンコーディングで書き込みます。
                    writeEncoding = Encoding.GetEncoding(codePage);
                }
                else
                {
                    //BOMは存在しない
                    // reset writeEncoding
                    // utf-8
                    if (codePage == 65001)
                        writeEncoding = new UTF8Encoding(false);
                    // utf-16 Little En
                    else if (codePage == 1200)
                        writeEncoding = new UnicodeEncoding(false, false);
                    // utf-16 Big En
                    else if (codePage == 1201)
                        writeEncoding = new UnicodeEncoding(true, false);
                    // utf-32 Little En
                    else if (codePage == 12000)
                        writeEncoding = new UTF32Encoding(false, false);
                    // utf-32 Big En
                    else if (codePage == 12001)
                        writeEncoding = new UTF32Encoding(true, false);
                    else
                        writeEncoding = Encoding.GetEncoding(codePage);
                }
            }
            // BOM有り
            else
            {
                //reset writeEncoding and ByteOrderMark
                //書き込み文字エンコーディングとBOMを再設定する。

                bool existByteOrderMark = enableBOM == true ? true : false;

                //書き込みエンコーディングの指定が無い場合、BOM指定のみ再設定する

                // utf-8
                if (codePage == 65001)
                    writeEncoding = new UTF8Encoding(existByteOrderMark);
                // utf-16 Little En
                else if (codePage == 1200)
                    writeEncoding = new UnicodeEncoding(false, existByteOrderMark);
                // utf-16 Big En
                else if (codePage == 1201)
                    writeEncoding = new UnicodeEncoding(true, existByteOrderMark);
                // utf-32 Little En
                else if (codePage == 12000)
                    writeEncoding = new UTF32Encoding(false, existByteOrderMark);
                // utf-32 Big En
                else if (codePage == 12001)
                    writeEncoding = new UTF32Encoding(true, existByteOrderMark);
                else
                    writeEncoding = Encoding.GetEncoding(codePage);
            }

            return writeEncoding;
        }

    }
}
