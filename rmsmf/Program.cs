using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

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
        private static string readCharacterSet;
        private static string replaceWordsCharacterSet;
        private static string filesCharacterSet;
        private static string writeCharacterSet;
        private static bool Empty_WriteCharacterSet;
        private static bool? existByteOrderMark;
        private static string[,] replaceWords;
        private static int replaceWordsCount;

        /// <summary>
        /// Multiple word multiple file replacement
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            const string OptionHelp = "h";
            const string OptionCharacterSet = "c";
            const string OptionWriteCharacterSet = "w";
            const string OptionFileNameList = "f";
            const string OptionFileNameListCharacterSet = "fc";
            const string OptionReplaceWords = "r";
            const string OptionReplaceWordsCharacterSet = "rc";
            const string OptionWriteByteOrderMark = "b";

            Colipex colipex = new Colipex(args);

            if(colipex.Parameters.Count == 0)
            {
                if(colipex.Options.Count == 0)
                {
                    Console.WriteLine("Please specify the target file name.");
                    return;
                }
                else if(colipex.IsOption(OptionHelp) == false 
                    && colipex.IsOption(OptionFileNameList) == false)
                {
                    return;
                }
            }

            Assembly thisAssem = typeof(Program).Assembly;
            AssemblyName thisAssemName = thisAssem.GetName();
            AssemblyCopyrightAttribute[] copyrightAttributes = (AssemblyCopyrightAttribute[])thisAssem.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);

            Version ver = thisAssemName.Version;
            String copyright = copyrightAttributes[0].Copyright;

            Console.WriteLine("{0}  version {1}  {2}", thisAssemName.Name, ver, copyright);


            //Hlep Option
            if (colipex.IsOption(OptionHelp) == true)
            {
                string[] HelpMessage =
                {
                    "rmsmf (Replace Multiple Strings in Multiple Files)\n"
                    ,"  rmsmf <File name to replace words (wildcards allowed)>\n"
                    ," Options  "
                    ,"   /c:<Character set name of read file OR CodePage>"
                    ,"   /w:<Character set name of write file OR CodePage>"
                    ,"   /f:<Files LIst FileName>"
                    ,"   /fc:<Files LIst FileName Character set name>"
                    ,"   /r:<Specify the CSV file name of the list of words to be replaced>"
                    ,"   /rc:<CSV file Character set name>"
                    ,"   /b:<If you write a BOM, write true, otherwise write false>"
                    ,"\n"
                    ,"the list of words to be replaced. (words csv file)"
                    ,""
                    ,"Search word 1, replacement word 1"
                    ,"Search word 2, Replace word 2"
                    ,"Search word 3, Replace word 3"
                    ,".,."
                    ,".,."
                    ,"Search word n, replacement word n"
                    ,"\n"
                    ,"as an example.\n"
                    ,"rmsmf /r:words.csv .\\*.txt "
                    ,""
                    ,"rmsmf /r:words.csv .\\*.txt /c:utf-16 /w:utf-32 /b:true "
                    ,""
                    ,"rmsmf /r:words.csv .\\*.txt /c:shift_jis /w:utf-8 /b:false "
                    ,""
                    ,""
                    ,"An example of simply changing the character set.\n"
                    ,"rmsmf .\\*.txt /c:shift_jis /w:utf-8 /b:true "
                    ,"(The position of the option is free)"
                    ,""
                };

                foreach(string message in HelpMessage)
                {
                    Console.WriteLine(message);
                }

                return;
            }

            //Setting Read CharacterSet 
            if (colipex.IsOption(OptionCharacterSet) == true)
            {
                Program.readCharacterSet = colipex.Options[OptionCharacterSet];
                if(Program.readCharacterSet == Colipex.NonValue)
                {
                    Console.WriteLine("Please specify the encoding name. (/c)");
                    return;
                }
            }
            else
            {
                Program.readCharacterSet = "utf-8";
            }

            //Setting Write CharacterSet 
            if (colipex.IsOption(OptionWriteCharacterSet) == true)
            {
                Program.writeCharacterSet = colipex.Options[OptionWriteCharacterSet];
                if (Program.writeCharacterSet == Colipex.NonValue)
                {
                    Console.WriteLine("Please specify the encoding name. (/w)");
                    return;
                }
                Program.Empty_WriteCharacterSet = false;
            }
            else
            {
                Program.writeCharacterSet = Program.readCharacterSet;
                Program.Empty_WriteCharacterSet = true;
            }

            //Setting ReplaceWords CharacterSet 
            if (colipex.IsOption(OptionReplaceWordsCharacterSet) == true)
            {
                Program.replaceWordsCharacterSet = colipex.Options[OptionReplaceWordsCharacterSet];
                if (Program.replaceWordsCharacterSet == Colipex.NonValue)
                {
                    Console.WriteLine("Please specify the encoding name. (/rc)");
                    return;
                }
            }
            else
            {
                Program.replaceWordsCharacterSet = Program.readCharacterSet;
            }

            //Setting FileNameList CharacterSet 
            if (colipex.IsOption(OptionFileNameListCharacterSet) == true)
            {
                Program.filesCharacterSet = colipex.Options[OptionFileNameListCharacterSet];
                if (Program.filesCharacterSet == Colipex.NonValue)
                {
                    Console.WriteLine("Please specify the encoding name. (/fc)");
                    return;
                }
            }
            else
            {
                Program.filesCharacterSet = Program.readCharacterSet;
            }

            //Setting ByteOrderMark
            if (colipex.IsOption(OptionWriteByteOrderMark) == true)
            {
                if( colipex.Options[OptionWriteByteOrderMark].ToLower() == "false" || 
                    colipex.Options[OptionWriteByteOrderMark].ToLower() == "no" ||
                    colipex.Options[OptionWriteByteOrderMark].ToLower() == "n" )
                    Program.existByteOrderMark = false;
                else
                    Program.existByteOrderMark = true;
            }
            else
            {
                Program.existByteOrderMark = null;
            }

            Encoding encoding = null;
            Encoding writeEncoding = null;
            Encoding repleaseEncoding = null;
            Encoding filesEncoding = null;
            int codePage;
            int writeCodePage;
            int repleaseCodePage;
            int filesCodePage;
            string errorEncoding = null;

            //Setting Encoding and Check error of Encoding
            try
            {
                //Setting Read Encoding
                errorEncoding = "Read Encoding";
                if (int.TryParse(Program.readCharacterSet, out codePage))
                    encoding = Encoding.GetEncoding(codePage);
                else
                    encoding = Encoding.GetEncoding(Program.readCharacterSet);

                //Setting Write Encoding
                errorEncoding = "Write Encoding";
                if (int.TryParse(Program.writeCharacterSet, out writeCodePage))
                    writeEncoding = Encoding.GetEncoding(writeCodePage);
                else
                    writeEncoding = Encoding.GetEncoding(Program.writeCharacterSet);

                //Setting Replease Encoding
                errorEncoding = "Replease Encoding";
                if (int.TryParse(Program.replaceWordsCharacterSet, out repleaseCodePage))
                    repleaseEncoding = Encoding.GetEncoding(repleaseCodePage);
                else
                    repleaseEncoding = Encoding.GetEncoding(Program.replaceWordsCharacterSet);

                //Setting Files Encoding
                errorEncoding = "Files Encoding";
                if (int.TryParse(Program.filesCharacterSet, out filesCodePage))
                    filesEncoding = Encoding.GetEncoding(filesCodePage);
                else
                    filesEncoding = Encoding.GetEncoding(Program.filesCharacterSet);
            }
            catch (Exception ex)
            {
                if (ex is ArgumentException)
                {
                    Console.WriteLine(errorEncoding +"Error !. " + ex.Message);
                }
                else if (ex is NotSupportedException)
                {
                    Console.WriteLine(errorEncoding + "Error !. " + ex.Message);
                }
                else if (ex is ArgumentOutOfRangeException)
                {
                    Console.WriteLine(errorEncoding + "Error !. " + ex.Message);
                }
                else
                {
                    Console.WriteLine(errorEncoding + "Error !. " + ex.Message);
                }
                return;
            }

            //Setting Replace Words
            List<string> wordsList = new List<string>();

            try
            {
                //There is a replacement
                if (colipex.IsOption(OptionReplaceWords) == true)
                {
                    if(colipex.Options[OptionReplaceWords] == Colipex.NonValue)
                    {
                        Console.WriteLine("Please specify a filename for the /r option.");
                        return;
                    }

                    //Read replacement word CSV file
                    using (var reader = new StreamReader(colipex.Options[OptionReplaceWords], repleaseEncoding, true))
                    {
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();

                            if (line.Length == 0) continue;
                            if (line.IndexOf(',') < 0) continue;

                            wordsList.Add(line);
                        }
                    }
                }

                //Register to the replacement word table
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

                //Search for files to replace
                string[] files = null;

                if (colipex.IsOption(OptionFileNameList) == true)
                {
                    if (!File.Exists(colipex.Options[OptionFileNameList]))
                    {
                        Console.WriteLine("{0} File not exists ", colipex.Options[OptionFileNameList]);
                        return;
                    }

                    List<string> filesList = new List<string>();

                    using (var reader = new StreamReader(colipex.Options[OptionFileNameList], filesEncoding, true))
                    {
                        while (!reader.EndOfStream)
                        {
                            string getFileName = reader.ReadLine();
                            if (!File.Exists(getFileName))
                            {
                                continue;
                            }
                            filesList.Add(getFileName);
                        }

                        files = filesList.ToArray();                   }
                }
                else
                {
                    string direcrtoryName = Path.GetDirectoryName(colipex.Parameters[0]);
                    string searchWord = Path.GetFileName(colipex.Parameters[0]);

                    files = Directory.GetFileSystemEntries(direcrtoryName, searchWord, System.IO.SearchOption.AllDirectories);
                }

                //Loop of files
                foreach (string fileName in files)
                {
                    if (!File.Exists(fileName))
                        continue;

                    string writeFileName = fileName + ".RP$";

                    //Delete the write file if it already exists
                    if (!File.Exists(writeFileName))
                    {
                        File.Delete(writeFileName);
                    }

                    //Open read file
                    using (var reader = new StreamReader(fileName, encoding, true))
                    {
                        //Main processing For Replace
                        ReadWriteForReplace(reader, writeFileName, encoding, writeEncoding);
                    }

                    //Delete read file
                    File.Delete(fileName);

                    //Read write file and rename to read file name
                    File.Move(writeFileName, fileName);
                }
            }
            catch (Exception ex)
            {
                if (ex is ArgumentException)
                {
                    Console.WriteLine(ex.Message);
                }
                else if (ex is NotSupportedException)
                {
                    Console.WriteLine(ex.Message);
                }
                else if (ex is ArgumentOutOfRangeException)
                {
                    Console.WriteLine(ex.Message);
                }
                else
                {
                    Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// Main processing For Replace
        /// </summary>
        /// <param name="reader">Read File Stream</param>
        /// <param name="writeFileName">Write File Name</param>
        /// <param name="encoding">Read File Encoding</param>
        /// <param name="writeEncoding">Write File Encoding</param>
        /// <returns>正常終了=true</returns>
        static bool ReadWriteForReplace(StreamReader reader, string writeFileName, Encoding encoding, Encoding writeEncoding)
        {
            bool rc = true;

            // I am read BOM of readfile.
            byte[] bom = new byte[4];
            int readCount = reader.BaseStream.Read(bom, 0, 4);
            reader.BaseStream.Position = 0;

            //Empty ByteOrderMark and WriteCharacterSet
            if (Program.existByteOrderMark == null && Program.Empty_WriteCharacterSet == true)
            {
                if (IsBOM(bom))
                {
                    // I follow BOM.
                    writeEncoding = reader.CurrentEncoding;
                }
                else
                {
                    // reset writeEncoding
                    // utf-8
                    if (writeEncoding.CodePage == 65001)
                        writeEncoding = new UTF8Encoding(false);
                    // utf-16 Little En
                    else if (writeEncoding.CodePage == 1200)
                        writeEncoding = new UnicodeEncoding(false, false);
                    // utf-16 Big En
                    else if (writeEncoding.CodePage == 1201)
                        writeEncoding = new UnicodeEncoding(true, false);
                    // utf-32 Little En
                    else if (writeEncoding.CodePage == 12000)
                        writeEncoding = new UTF32Encoding(false, false);
                    // utf-32 Big En
                    else if (writeEncoding.CodePage == 12001)
                        writeEncoding = new UTF32Encoding(true, false);
                    // If other writeEncoding, use as it is
                }
            }
            // ByteOrderMark or WriteCharacterSet specified
            else
            {
                //reset writeEncoding and ByteOrderMark
                int writeCodePage;
                bool existByteOrderMark = Program.existByteOrderMark == true ? true : false;

                if (int.TryParse(Program.writeCharacterSet, out writeCodePage))
                {
                    // utf-8
                    if (writeCodePage == 65001)
                        writeEncoding = new UTF8Encoding(existByteOrderMark);
                    // utf-16 Little En
                    else if (writeCodePage == 1200)
                        writeEncoding = new UnicodeEncoding(false, existByteOrderMark);
                    // utf-16 Big En
                    else if (writeCodePage == 1201)
                        writeEncoding = new UnicodeEncoding(true, existByteOrderMark);
                    // utf-32 Little En
                    else if (writeCodePage == 12000)
                        writeEncoding = new UTF32Encoding(false, existByteOrderMark);
                    // utf-32 Big En
                    else if (writeCodePage == 12001)
                        writeEncoding = new UTF32Encoding(true, existByteOrderMark);
                    else
                        writeEncoding = Encoding.GetEncoding(writeCodePage);
                }
                else
                {
                    // utf-8
                    if (Program.writeCharacterSet == "utf-8")
                        writeEncoding = new UTF8Encoding(existByteOrderMark);
                    // utf-16 Little En
                    else if (Program.writeCharacterSet == "utf-16")
                        writeEncoding = new UnicodeEncoding(false, existByteOrderMark);
                    // utf-16 Big En
                    else if (Program.writeCharacterSet == "unicodeFFFE")
                        writeEncoding = new UnicodeEncoding(true, existByteOrderMark);
                    // utf-32 Little En
                    else if (Program.writeCharacterSet == "utf-32")
                        writeEncoding = new UTF32Encoding(false, existByteOrderMark);
                    // utf-32 Big En
                    else if (Program.writeCharacterSet == "utf-32be")
                        writeEncoding = new UTF32Encoding(true, existByteOrderMark);
                    else
                        writeEncoding = Encoding.GetEncoding(Program.writeCharacterSet);
                }
            }

            if(Program.Empty_WriteCharacterSet == true)
            {
                Program.writeCharacterSet = writeEncoding.CodePage.ToString();
            }

            //Open Write File.
            using (var writer = new StreamWriter(writeFileName, true, writeEncoding))
            {
                //Read Readfile.
                string readLine = reader.ReadToEnd();

                //Words Replace at Line.
                for (int i = 0; i < Program.replaceWordsCount; i++)
                {
                    readLine = readLine.Replace(Program.replaceWords[0, i], Program.replaceWords[1, i]);
                }

                //Writefile Overwrite .
                writer.Write(readLine);
            }

            return rc;
        }


        /// <summary>
        /// Determine if it is BOM
        /// </summary>
        /// <param name="bom">Array to be inspected (4 bytes)</param>
        /// <returns>true=BOM.</returns>
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
        /// BOM sequence comparison
        /// </summary>
        /// <param name="data">Sequence to be inspected</param>
        /// <param name="bom">BOM array</param>
        /// <returns>true=match</returns>
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
