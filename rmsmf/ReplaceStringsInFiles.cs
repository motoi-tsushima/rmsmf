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
        /// <returns></returns>
        public bool Replace(Encoding encoding, Encoding writeEncoding)
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

                            //Delete the write file if it already exists
                            //書き込み対象ファイルがすでに存在する場合は削除します。
                            if (!File.Exists(writeFileName))
                            {
                                File.Delete(writeFileName);
                            }

                            //Open read file
                            //読み取りファイルを開く。
                            using (FileStream fs = new FileStream(fileName, FileMode.Open))
                            {
                                bool bomExist = false;
                                int codePage;
                                int writeCodePage;

                                //読み込みエンコーディングの有無で分岐
                                if (encoding == null)
                                {
                                    // エンコーディング指定が無い場合

                                    //  読み取りファイルの文字エンコーディングを判定する
                                    byte[] buffer = new byte[EncodingJudgment.bufferSize];
                                    int readCount = fs.Read(buffer, 0, EncodingJudgment.bufferSize);

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
                                        //codePage = bomJudg.CodePage; //オプション指定のコードページが間違っていた時どうする ?
                                        codePage = encoding.CodePage;
                                    }
                                    else
                                    {
                                        bomExist = false;
                                        codePage = encoding.CodePage;
                                    }
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

                                //書き込みエンコーディングの再作成
                                inWriteEncoding = GetWriteEncoding(writeCodePage, bomExist, this._enableBOM);

                                //エンコーディングを指定してテキストストリームを開く
                                using (var reader = new StreamReader(fs, inEncoding, true))
                                {
                                    //Main processing For Replace
                                    //置換メイン処理
                                    ReadWriteForReplace(reader, writeFileName, inEncoding, inWriteEncoding);
                                }
                            }

                            //Delete read file
                            //読み取りファイルを削除する。
                            File.Delete(fileName);

                            //Read write file and rename to read file name
                            //書き込み対象ファイルを読み取りファイル名に変更。
                            File.Move(writeFileName, fileName);
                        }
                    }
                );
            }
            catch (UnauthorizedAccessException uae)
            {
                Console.WriteLine(uae.Message);
            }
            catch (AggregateException ae)
            {
                int errorCount = 0;
                foreach(var ie in ae.InnerExceptions)
                {
                    errorCount++;
                    Console.WriteLine(ie.Message);
                }

                ExecutionState.isError = true;
                ExecutionState.isNormal = !ExecutionState.isError;
                ExecutionState.errorMessage = errorCount + "件のエラーが発生しました。他のファイルは正常に処理しました。";

                throw ae;
            }

            return success;
        }

        /// <summary>
        /// Main processing For Replace
        /// 置換メイン処理
        /// </summary>
        /// <param name="reader">Read File Stream. 読み取りファイルストリーム。</param>
        /// <param name="writeFileName">Write File Name. 書き込みファイルストリーム。</param>
        /// <param name="encoding">Read File Encoding. 読み取りファイルの文字エンコーディング。</param>
        /// <param name="writeEncoding">Write File Encoding. 書き込みファイルの文字エンコーディング。</param>
        /// <returns>正常終了=true</returns>
        public bool ReadWriteForReplace(StreamReader reader, string writeFileName, Encoding encoding, Encoding writeEncoding)
        {
            bool rc = true;

            //Open Write File.
            //書き込みファイルを開く。
            using (var writer = new StreamWriter(writeFileName, true, writeEncoding))
            {
                //Read Readfile.
                //読み取りファイルを全て読み込む。
                string readLine = reader.ReadToEnd();

                if(this._replaceWords != null)
                {
                    //Replace Performs a line-by-line replacement in the word list.
                    //置換単語リストの行単位に置換を実施します。
                    int replaceWordsCount = this._replaceWords.GetLength(1);

                    for (int i = 0; i < replaceWordsCount; i++)
                    {
                        readLine = readLine.Replace(this._replaceWords[0, i], this._replaceWords[1, i]);
                    }
                }

                //Writefile Overwrite .
                //書き込みファイルへ上書きします。
                writer.Write(readLine);
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
