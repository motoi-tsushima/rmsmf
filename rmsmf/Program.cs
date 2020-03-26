using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

/// <summary>
/// I replace multiple strings in multiple files.
/// </summary>
namespace rmsmf
{
    /// <summary>
    /// RMSMF (Replace Multiple Strings in Multiple Files)
    /// </summary>
    class Program
    {
        private static string characterSet;
        private static string[,] replaceWords;
        private static int replaceWordsCount;

        /// <summary>
        /// 複数単語複数ファイル置換
        /// </summary>
        /// <param name="args">(1)置換単語CSVファイル名,(2)置換対象ファイル名フォルダー名(ワイルドカード可),/c:文字コード(デフォルト utf-8)</param>
        /// <remarks>/c:で使用できる文字コードは、us-ascii,shift_jis,utf-8,utf-16,utf-32,iso-2022-jp,euc-jp,x-mac-japanese などEncoding.GetEncodingで使用できるもの。コードページも使用可能。
        /// BOMが存在する場合はBOMを優先し「/c」の指定は無視する。
        /// 改行コードについては.NETが処理系に合わせるはず。</remarks>
        static void Main(string[] args)
        {
            const string OptionCharacterSet = "c";

            Colipex colipex = new Colipex(args);

            //文字コード設定
            if (colipex.IsOption(OptionCharacterSet) == true)
            {
                Program.characterSet = colipex.Options[OptionCharacterSet];
            }
            else
            {
                Program.characterSet = "utf-8";
            }

            //置換単語CSVファイル読み込み
            List<string> wordsList = new List<string>();
            //コードページを取り出す。
            Encoding encoding;
            int codePage;
            if (int.TryParse(Program.characterSet, out codePage))
                encoding = Encoding.GetEncoding(codePage);
            else
                encoding = Encoding.GetEncoding(Program.characterSet);

            using (var reader = new StreamReader(colipex.Parameters[0], encoding, true))
            {
                while (!reader.EndOfStream)
                {
                    wordsList.Add(reader.ReadLine());
                }
            }

            //置換単語表へ登録
            Program.replaceWordsCount = wordsList.Count;
            Program.replaceWords = new string[2, Program.replaceWordsCount];
            for (int i = 0; i < Program.replaceWordsCount; i++)
            {
                string[] colmuns = wordsList[i].Split(',');
                Program.replaceWords[0, i] = colmuns[0];
                Program.replaceWords[1, i] = colmuns[1];
            }

            wordsList.Clear();
            wordsList = null;

            //置換対象ファイル検索
            string direcrtoryName = Path.GetDirectoryName(colipex.Parameters[1]);
            string searchWord = Path.GetFileName(colipex.Parameters[1]);

            string[] files = Directory.GetFileSystemEntries(direcrtoryName, searchWord, System.IO.SearchOption.AllDirectories);

            //ファイル名のループ
            foreach (string fileName in files)
            {
                if (!File.Exists(fileName))
                    continue;

                string writeFileName = fileName + ".RP$";

                //書き込みファイルが既に存在していたら削除
                if (!File.Exists(writeFileName))
                {
                    File.Delete(writeFileName);
                }

                //置換元ファイルを開く
                using (var reader = new StreamReader(fileName, encoding, true))
                {
                    //BOMを仮読みする
                    byte[] bom = new byte[4];
                    int readCount = reader.BaseStream.Read(bom, 0, 4);
                    reader.BaseStream.Position = 0;

                    Encoding writeEncoding;

                    if (IsBOM(bom))
                    {
                        writeEncoding = reader.CurrentEncoding;
                    }
                    else
                    {
                        // utf-8
                        if (reader.CurrentEncoding.CodePage == 65001)
                            writeEncoding = new UTF8Encoding(false);
                        // utf-16 Little En
                        else if (reader.CurrentEncoding.CodePage == 1200)
                            writeEncoding = new UnicodeEncoding(false, false);
                        // utf-16 Big En
                        else if (reader.CurrentEncoding.CodePage == 1201)
                            writeEncoding = new UnicodeEncoding(true, false);
                        // utf-32 Little En
                        else if (reader.CurrentEncoding.CodePage == 12000)
                            writeEncoding = new UTF32Encoding(false, false);
                        // utf-32 Big En
                        else if (reader.CurrentEncoding.CodePage == 12001)
                            writeEncoding = new UTF32Encoding(true, false);
                        else
                            writeEncoding = reader.CurrentEncoding;
                    }

                    //置換先ファイルを開く
                    using (var writer = new StreamWriter(writeFileName, true, writeEncoding))
                    {
                        //置換元ファイル読み込み
                        string readLine = reader.ReadToEnd();

                        //置換先ファイル書き込み
                        for (int i = 0; i < Program.replaceWordsCount; i++)
                        {
                            readLine = readLine.Replace(Program.replaceWords[0, i], Program.replaceWords[1, i]);
                        }

                        //置換先ファイル上書き
                        writer.Write(readLine);
                    }
                }

                //置換元ファイル削除
                File.Delete(fileName);

                //置換先ファイルを置換元ファイル名に改名
                File.Move(writeFileName, fileName);
            }
        }

        /// <summary>
        /// BOMであるか判定する
        /// </summary>
        /// <param name="bom">検査対象配列(4byte)</param>
        /// <returns>true=BOMである。</returns>
        static bool IsBOM(byte[] bomByte)
        {
            bool result = false;
            byte[] bomUTF8 = { 0xEF, 0xBB, 0xBF };
            byte[] bomUTF16Little = { 0xFF, 0xFE };
            byte[] bomUTF16Big = { 0xFE, 0xFF };
            byte[] bomUTF32Little = { 0xFF, 0xFE, 0x00, 0x00 };
            byte[] bomUTF32Big = { 0x00, 0x00, 0xFE, 0xFF };

            if (IsMatched(bomByte, bomUTF8))
                result = true;

            else if (IsMatched(bomByte, bomUTF16Little))
                result = true;

            else if (IsMatched(bomByte, bomUTF16Big))
                result = true;

            else if (IsMatched(bomByte, bomUTF32Little))
                result = true;

            else if (IsMatched(bomByte, bomUTF32Big))
                result = true;

            return result;
        }

        /// <summary>
        /// BOM配列比較
        /// </summary>
        /// <param name="data">検査対象配列</param>
        /// <param name="bom">BOM配列</param>
        /// <returns>true=一致する</returns>
        static bool IsMatched(byte[] data, byte[] bom)
        {
            bool result = true;

            for (int i = 0; i < bom.Length; i++)
            {
                if (bom[i] != data[i])
                    result = false;
            }

            return result;
        }

    }
}
