using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

            //Loop of files
            //ファイル単位のループ
            foreach (string fileName in this._files)
            {
                if (!File.Exists(fileName))
                    continue;

                string writeFileName = fileName + ".RP$";

                //Delete the write file if it already exists
                //書き込み対象ファイルがすでに存在する場合は削除します。
                if (!File.Exists(writeFileName))
                {
                    File.Delete(writeFileName);
                }

                //Open read file
                //読み取りファイルを開く。
                using (var reader = new StreamReader(fileName, encoding, true))
                {
                    //Main processing For Replace
                    //置換メイン処理
                    ReadWriteForReplace(reader, writeFileName, encoding, writeEncoding);
                }

                //Delete read file
                //読み取りファイルを削除する。
                File.Delete(fileName);

                //Read write file and rename to read file name
                //書き込み対象ファイルを読み取りファイル名に変更。
                File.Move(writeFileName, fileName);
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

            // I am read BOM of readfile.
            //  読み取りファイルのBOMを読みます。
            //byte[] bom = new byte[4];
            //int readCount = reader.BaseStream.Read(bom, 0, 4);
            //reader.BaseStream.Position = 0;

            //  読み取りファイルの文字エンコーディングを判定する
            byte[] buffer = new byte[EncodingJudgment.bufferSize];
            int readCount = reader.BaseStream.Read(buffer, 0, EncodingJudgment.bufferSize);

            EncodingJudgment encJudgment = new EncodingJudgment(buffer);
            EncodingInfomation encInfo = encJudgment.Judgment();

            reader.BaseStream.Position = 0;


            //Empty ByteOrderMark and WriteCharacterSet
            //BOMなし and 書き込み文字エンコーディング
            if (this._enableBOM == null && writeEncoding == null)
            {
                //BOMは存在するか
                if (encInfo.bom)
                {
                    //BOMは存在する

                    // BOM と同じエンコーディングで書き込みます。
                    writeEncoding = Encoding.GetEncoding(encInfo.codePage);
                }
                else
                {
                    //BOMは存在しない
                    // reset writeEncoding
                    // utf-8
                    if (encInfo.codePage == 65001)
                        writeEncoding = new UTF8Encoding(false);
                    // utf-16 Little En
                    else if (encInfo.codePage == 1200)
                        writeEncoding = new UnicodeEncoding(false, false);
                    // utf-16 Big En
                    else if (encInfo.codePage == 1201)
                        writeEncoding = new UnicodeEncoding(true, false);
                    // utf-32 Little En
                    else if (encInfo.codePage == 12000)
                        writeEncoding = new UTF32Encoding(false, false);
                    // utf-32 Big En
                    else if (encInfo.codePage == 12001)
                        writeEncoding = new UTF32Encoding(true, false);
                    // If other writeEncoding, use as it is
                    // 他のwriteEncodingの場合は、そのまま使用します
                }
            }
            // ByteOrderMark or WriteCharacterSet specified
            // BOM有り or 書き込み文字エンコーディング指定有り
            else
            {
                //reset writeEncoding and ByteOrderMark
                //書き込み文字エンコーディングとBOMを再設定する。

                bool existByteOrderMark = this._enableBOM == true ? true : false;

                if (writeEncoding == null)
                {
                    //書き込みエンコーディングの指定が無い場合、BOM指定のみ再設定する

                    // utf-8
                    if (encInfo.codePage == 65001)
                        writeEncoding = new UTF8Encoding(existByteOrderMark);
                    // utf-16 Little En
                    else if (encInfo.codePage == 1200)
                        writeEncoding = new UnicodeEncoding(false, existByteOrderMark);
                    // utf-16 Big En
                    else if (encInfo.codePage == 1201)
                        writeEncoding = new UnicodeEncoding(true, existByteOrderMark);
                    // utf-32 Little En
                    else if (encInfo.codePage == 12000)
                        writeEncoding = new UTF32Encoding(false, existByteOrderMark);
                    // utf-32 Big En
                    else if (encInfo.codePage == 12001)
                        writeEncoding = new UTF32Encoding(true, existByteOrderMark);
                    else
                        writeEncoding = Encoding.GetEncoding(encInfo.codePage);
                }
                else
                {
                    //書き込みエンコーディングの指定が有る場合

                    // utf-8
                    if (writeEncoding.CodePage == 65001)
                        writeEncoding = new UTF8Encoding(existByteOrderMark);
                    // utf-16 Little En
                    else if (writeEncoding.CodePage == 1200)
                        writeEncoding = new UnicodeEncoding(false, existByteOrderMark);
                    // utf-16 Big En
                    else if (writeEncoding.CodePage == 1201)
                        writeEncoding = new UnicodeEncoding(true, existByteOrderMark);
                    // utf-32 Little En
                    else if (writeEncoding.CodePage == 12000)
                        writeEncoding = new UTF32Encoding(false, existByteOrderMark);
                    // utf-32 Big En
                    else if (writeEncoding.CodePage == 12001)
                        writeEncoding = new UTF32Encoding(true, existByteOrderMark);
                    else
                        writeEncoding = Encoding.GetEncoding(writeEncoding.CodePage);
                }
            }

            //Open Write File.
            //書き込みファイルを開く。
            using (var writer = new StreamWriter(writeFileName, true, writeEncoding))
            {
                //Read Readfile.
                //読み取りファイルを全て読み込む。
                string readLine = reader.ReadToEnd();

                //Replace Performs a line-by-line replacement in the word list.
                //置換単語リストの行単位に置換を実施します。

                int replaceWordsCount = this._replaceWords.GetLength(1);

                for (int i = 0; i < replaceWordsCount; i++)
                {
                    readLine = readLine.Replace(this._replaceWords[0, i], this._replaceWords[1, i]);
                }

                //Writefile Overwrite .
                //書き込みファイルへ上書きします。
                writer.Write(readLine);
            }

            return rc;
        }


    }
}
