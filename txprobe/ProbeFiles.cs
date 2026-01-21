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
        /// 出力データキュー（マルチスレッド対応）
        /// </summary>
        private BlockingCollection<string> _outputQueue = null;

        public ProbeFiles(string[] searchWords, string[] files, bool enableProbe, string outputFileListName, Encoding filesEncoding) 
        { 
            _searchWords = searchWords;
            _files = files;
            _enableProbe = enableProbe;
            _output_filelist_filename = outputFileListName;
            _filesEncoding = filesEncoding ?? Encoding.UTF8;
        }

        /// <summary>
        /// _files に記載されたファイルの内容を探索する。
        /// </summary>
        /// <param name="encoding">読み取りファイルの文字エンコーディング</param>
        /// <returns>true = 探索成功 , false = 異常終了.</returns>
        public bool Probe(Encoding encoding)
        {
            Encoding inEncoding;

            try
            {
                // 出力キューの初期化
                _outputQueue = new BlockingCollection<string>();

                // 専用の書き込みスレッドを開始
                Task writerTask = null;
                if (this._output_filelist_filename != null)
                {
                   // 出力ファイルのフルパスを取得
                   string outputFullPath = Path.GetFullPath(this._output_filelist_filename);
                   
                   // 入力ファイルリストから出力ファイルを除外
                   this._files = this._files.Where(f => 
                       !string.Equals(Path.GetFullPath(f), outputFullPath, StringComparison.OrdinalIgnoreCase))
                       .ToArray();
                   
                    writerTask = Task.Run(() => FileWriterThread());
                }

                //ファイル単位のマルチスレッド作成
                Parallel.ForEach(this._files, (fileName) =>
                {
                    if (File.Exists(fileName))
                    {
                        // 読み取りファイルを開く
                        using (FileStream fs = new FileStream(fileName, FileMode.Open))
                        {
                            bool bomExist = false;
                            int codePage;

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
                                    else if(this._encodingJudgmentMode == CommandOptions.EncodingJudgmentType.ThirdParty)
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
                                    inEncoding = Encoding.GetEncoding(codePage);
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
                                    codePage = encoding.CodePage;
                                }
                                else
                                {
                                    bomExist = false;
                                    codePage = encoding.CodePage;
                                }
                            }

                            if(inEncoding == null)
                            {
                                // エンコーディングが不明な場合、処理をスキップする
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
                                return; // Parallel.ForEachではcontinueの代わりにreturn
                            }

                            // エンコーディングを指定してテキストストリームを開く
                            using (var reader = new StreamReader(fs, inEncoding, true))
                            {
                                // 検索メイン処理
                                ReadForSearch(fileName, reader, inEncoding, bomExist);
                            }
                        }
                    }
                });

                // キューへの追加完了を通知
                _outputQueue.CompleteAdding();

                // 書き込みスレッドの完了を待機
                if (writerTask != null)
                {
                    writerTask.Wait();
                }
            }
            catch (UnauthorizedAccessException uae)
            {
                throw new RmsmfException(uae.Message, uae);
            }
            catch (AggregateException ae)
            {
                int errorCount = 0;
                foreach (var ie in ae.InnerExceptions)
                {
                    errorCount++;
                    Console.WriteLine(ie.Message);
                }

                throw new RmsmfException(errorCount + "件のエラーが発生しました。他のファイルは正常に処理しました。", ae);
            }
            finally
            {
                // リソース解放
                if (_outputQueue != null)
                {
                    _outputQueue.Dispose();
                    _outputQueue = null;
                }
            }

            return true;
        }

        /// <summary>
        /// 専用の書き込みスレッド（Producer-Consumerパターンの消費者）
        /// </summary>
        private void FileWriterThread()
        {
            try
            {
                using (var ofs = new StreamWriter(this._output_filelist_filename, false, this._filesEncoding))
                {
                    // キューからデータを取得して書き込み
                    // GetConsumingEnumerable()はキューが空の場合は待機し、
                    // CompleteAdding()後にキューが空になるとループを抜ける
                    foreach (var line in _outputQueue.GetConsumingEnumerable())
                    {
                        ofs.WriteLine(line);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ファイル書き込みエラー: " + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// キューにデータを追加（Producer-Consumerパターンの生産者）
        /// </summary>
        /// <param name="line">追加する行</param>
        private void EnqueueOutput(string line)
        {
            if (_outputQueue != null)
            {
                try
                {
                    _outputQueue.Add(line);
                }
                catch (InvalidOperationException)
                {
                    // CompleteAdding()後の追加は無視
                }
            }
        }

        /// <summary>
        /// 文字列内の指定された部分文字列の出現回数をカウントする
        /// </summary>
        /// <param name="text">検索対象の文字列</param>
        /// <param name="searchString">検索する部分文字列</param>
        /// <returns>出現回数</returns>
        private int CountSubstring(string text, string searchString)
        {
            int count = 0;
            int index = 0;
            int searchLength = searchString.Length;

            do
            {
                if (text.Length - index < searchLength)
                {
                    break;
                }

                index = text.IndexOf(searchString, index, text.Length - index);
                if (index >= 0)
                {
                    count++;
                    index += searchLength;
                }

            } while (index >= 0);

            return count;
        }

        /// <summary>
        /// 改行コードの種類を判定する
        /// </summary>
        /// <param name="countCRLF">CR-LFの出現回数</param>
        /// <param name="countLF">LFの出現回数</param>
        /// <param name="countCR">CRの出現回数</param>
        /// <returns>改行コードの種類を示す文字列</returns>
        private string DetermineLineBreakType(int countCRLF, int countLF, int countCR)
        {
            if (countLF == 0 && countCR == 0 && countCRLF == 0)
            {
                return "No";
            }
            else if (countCRLF == countLF && countCRLF == countCR)
            {
                return "CR-LF";
            }
            else if (countLF > 0 && countCR == 0 && countCRLF == 0)
            {
                return "LF";
            }
            else if (countCR > 0 && countLF == 0 && countCRLF == 0)
            {
                return "CR";
            }
            else if (countLF > 0 && countCRLF > 0 && countLF != countCRLF && countCR == 0)
            {
                return "LF & CR-LF";
            }
            else if (countCR > 0 && countCRLF > 0 && countCR != countCRLF && countLF == 0)
            {
                return "CR & CR-LF";
            }
            else if (countCRLF == 0 && countCR > 0 && countLF > 0)
            {
                return "LF & CR";
            }
            else
            {
                return "LF & CR & CR-LF";
            }
        }

        /// <summary>
        /// Main processing For Replace
        /// 置換メイン処理
        /// </summary>
        /// <param name="fileName">Read File Name. 読み取りファイル名。</param>
        /// <param name="reader">Read File Stream. 読み取りファイルストリーム。</param>
        /// <param name="encoding">Read File Encoding. 読み取りファイルの文字エンコーディング。</param>
        /// <param name="bomExist">Read File BOM Existence Flag. 読み取りファイルのBOM有無フラグ。</param>
        /// <returns>正常終了=true</returns>
        public bool ReadForSearch(string fileName, StreamReader reader, Encoding encoding, bool bomExist)
        {
            bool rc = true;
            string dispBOM;
            string encodingName = "";

            if (bomExist == true)
            {
                dispBOM = "BOM exists";
            }
            else
            {
                dispBOM = "No BOM";
            }

            if(encoding == null)
            {
                encodingName = "encoding Unknown";
            }
            else
            {
                encodingName = encoding.WebName;
            }

            // 読み取りファイルを全て読み込む
            string readLine = reader.ReadToEnd();

            // 改行コードのカウント
            int countCRLF = CountSubstring(readLine, "\r\n");
            int countLF = CountSubstring(readLine, "\n");
            int countCR = CountSubstring(readLine, "\r");

            // 改行コードの種類を判定
            string lineBreakType = DetermineLineBreakType(countCRLF, countLF, countCR);

            if (this._searchWords != null)
            {
                //検索を実施します。
                //int searchWordsCount = this._searchWords.GetLength(1);
                int searchWordsCount = this._searchWords.GetLength(0);
                bool wordFound = false;

                for (int i = 0; i < searchWordsCount; i++)
                {
                    if (readLine.IndexOf(this._searchWords[i]) >= 0)
                    {
                        // 検索単語の探索
                        if (this._enableProbe == true)
                        {
                            // プローブ機能が有効の場合、検索単語を探索する。
                            // 検索単語が見つかった場合、結果を表示する。
                            string dispLine = fileName + "," + this._searchWords[i];
                            Console.WriteLine("{0}", dispLine);
                            wordFound = true;
                        }
                        else
                        {
                            // プローブ機能が無効の場合、検索結果のみ表示する。検索単語も表示する。
                            wordFound = true;
                        }
                    }
                }

                if (this._enableProbe == false && wordFound == true)
                {
                    string dispLine = fileName + "," + encodingName + "," + lineBreakType + "," + dispBOM;
                    Console.WriteLine("{0}", dispLine);

                    // スレッドセーフなキューへの追加
                    EnqueueOutput(dispLine);
                }
            }
            else
            {
                //検索単語が指定されていない場合、ファイル探索結果のみ表示する。
                string dispLine = fileName + "\t," + encodingName + "\t," + lineBreakType + "\t," + dispBOM;
                Console.WriteLine("{0}", dispLine);

                // スレッドセーフなキューへの追加
                EnqueueOutput(dispLine);
            }

            return rc;
        }

    }
}
