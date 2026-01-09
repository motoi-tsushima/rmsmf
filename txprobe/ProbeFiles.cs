using rmsmf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public ProbeFiles(string[] searchWords, string[] files, bool enableProbe) 
        { 
            _searchWords = searchWords;
            _files = files;
            _enableProbe = enableProbe;
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
                //ファイル単位のマルチスレッド作成
                Parallel.ForEach(this._files, (fileName) =>
                {
                    if (File.Exists(fileName))
                    {
                        //Open read file
                        //読み取りファイルを開く。
                        using (FileStream fs = new FileStream(fileName, FileMode.Open))
                        {
                            bool bomExist = false;
                            int codePage;

                            //読み込みエンコーディングの有無で分岐
                            if (encoding == null)
                            {
                                // エンコーディング指定が無い場合

                                //  読み取りファイルの文字エンコーディングを判定する
                                int fileSize = (int)fs.Length;
                                byte[] buffer = new byte[fileSize];
                                int readCount = fs.Read(buffer, 0, fileSize);

                                EncodingJudgment encJudgment = new EncodingJudgment(buffer);
                                EncodingInfomation encInfo = encJudgment.Judgment();

                                fs.Position = 0;

                                bomExist = encInfo.bom;
                                codePage = encInfo.codePage;

                                inEncoding = Encoding.GetEncoding(encInfo.codePage);
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

                            //エンコーディングを指定してテキストストリームを開く
                            using (var reader = new StreamReader(fs, inEncoding, true))
                            {
                                //検索メイン処理
                                ReadForSearch(fileName, reader, inEncoding, bomExist);
                            }
                        }
                    }
                }
                );
            }
            catch (UnauthorizedAccessException uae)
            {
                ExecutionState.isError = true;
                ExecutionState.isNormal = !ExecutionState.isError;
                ExecutionState.errorMessage = uae.Message;
                throw uae;
            }
            catch (AggregateException ae)
            {
                int errorCount = 0;
                foreach (var ie in ae.InnerExceptions)
                {
                    errorCount++;
                    Console.WriteLine(ie.Message);
                }

                ExecutionState.isError = true;
                ExecutionState.isNormal = !ExecutionState.isError;
                ExecutionState.errorMessage = errorCount + "件のエラーが発生しました。他のファイルは正常に処理しました。";

                throw ae;
            }

            return true;
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

            //Read Readfile.
            //読み取りファイルを全て読み込む。
            string readLine = reader.ReadToEnd();

            // CR-LF 探索
            int countCRLF = 0;
            int indexCRLF = 0;

            do
            {
                indexCRLF = readLine.IndexOf("\r\n", indexCRLF, readLine.Length);
                if (indexCRLF >= 0)
                {
                    countCRLF++;
                }

            } while (indexCRLF >= 0);

            // LF 探索
            int countLF = 0;
            int indexLF = 0;

            do
            {
                indexLF = readLine.IndexOf("\n", indexLF, readLine.Length);
                if (indexLF >= 0)
                {
                    countLF++;
                }

            } while (indexLF >= 0);

            // CR 探索
            int countCR = 0;
            int indexCR = 0;

            do
            {
                indexCRLF = readLine.IndexOf("\r", indexCR, readLine.Length);
                if (indexCR >= 0)
                {
                    countCR++;
                }

            } while (indexCR >= 0);

            // 判定結果
            string lineBreakType;

            if (countLF == 0 && countCR == 0 && countCRLF == 0)
            {
                lineBreakType = "No";
            }
            else if (countCRLF == countLF && countCRLF == countCR)
            {
                lineBreakType = "CR-LF";
            }
            else if (countLF > 0 && countCR == 0 && countCRLF == 0)
            {
                lineBreakType = "LF";
            }
            else if (countCR > 0 && countLF == 0 && countCRLF == 0)
            {
                lineBreakType = "CR";
            }
            else if (countLF > 0 && countCRLF > 0 && countLF != countCRLF && countCR == 0)
            {
                lineBreakType = "LF & CR-LF";
            }
            else if (countCR > 0 && countCRLF > 0 && countCR != countCRLF && countLF == 0)
            {
                lineBreakType = "CR & CR-LF";
            }
            else if (countCRLF == 0 && countCR > 0 && countLF > 0 )
            {
                lineBreakType = "LF & CR";
            }
            else
            {
                lineBreakType = "LF & CR & CR-LF";
            }

            if (this._searchWords != null)
            {
                //検索を実施します。
                int searchWordsCount = this._searchWords.GetLength(1);
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
                    string dispLine = fileName + "," + encoding.WebName + "," + lineBreakType + "," + bomExist;
                    Console.WriteLine("{0}", dispLine);
                }
            }
            else
            {
                //検索単語が指定されていない場合、ファイル探索結果のみ表示する。
                string dispLine = fileName + "," + encoding.WebName + "," + lineBreakType + "," + bomExist;
                Console.WriteLine("{0}", dispLine);
            }

            return rc;
        }

    }
}
