using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace rmsmf
{
    public class CommandOptions : Colipex
    {
        private const string CharacterSetJudgment = "Judgment";
        private const string OptionHelp = "h";
        private const string OptionCharacterSet = "c";
        private const string OptionWriteCharacterSet = "w";
        private const string OptionFileNameList = "f";
        private const string OptionFileNameListCharacterSet = "fc";
        private const string OptionReplaceWords = "r";
        private const string OptionReplaceWordsCharacterSet = "rc";
        private const string OptionWriteByteOrderMark = "b";

        /// <summary>
        /// コマンドオプション
        /// </summary>
        /// <param name="args"></param>
        public CommandOptions(string[] args) : base(args)
        {
            ExecutionState.className = "CommandOptions.CommandOptions";

            string readCharacterSet;
            string replaceWordsCharacterSet;
            string filesCharacterSet;
            string writeCharacterSet;
            bool? existByteOrderMark;

            string errorEncoding = null;

            //ヘルプオプションの場合
            if (this.IsOption(OptionHelp) == true)
            {
                this._callHelp = false;
                return;
            }

            //パラメータがない場合は終了
            if(this.IsOption(OptionFileNameList) == false)
            {
                if(this.Parameters.Count == 0 && this.Options.Count == 0)
                {
                    ExecutionState.isError = true;
                    ExecutionState.isNormal = !ExecutionState.isError;
                    ExecutionState.errorMessage = "目的のファイル名を指定してください。(/h ヘルプ表示)";
                    throw new Exception(ExecutionState.errorMessage);
                }
            }

            try
            {
                //Setting Read CharacterSet 
                //読み取り文字エンコーディング名を設定する。
                if (this.IsOption(OptionCharacterSet) == true)
                {
                    readCharacterSet = this.Options[OptionCharacterSet];
                    if (readCharacterSet == Colipex.NonValue)
                    {
                        //Console.WriteLine("Please specify the encoding name. (/c)");
                        Console.WriteLine("文字エンコーディング名を指定してください。 (/c)");
                        return;
                    }
                }
                else
                {
                    readCharacterSet = CharacterSetJudgment;
                }

                //Setting Write CharacterSet 
                //書き込み文字エンコーディング名の設定する。
                if (this.IsOption(OptionWriteCharacterSet) == true)
                {
                    writeCharacterSet = this.Options[OptionWriteCharacterSet];
                    if (writeCharacterSet == Colipex.NonValue)
                    {
                        //Console.WriteLine("Please specify the encoding name. (/w)");
                        Console.WriteLine("文字エンコーディング名を指定してください。 (/w)");
                        return;
                    }
                    this.Empty_WriteCharacterSet = false;
                }
                else
                {
                    writeCharacterSet = readCharacterSet;
                    this.Empty_WriteCharacterSet = true;
                }

                //Setting ReplaceWords CharacterSet 
                //置換単語リストの文字エンコーディングの設定する。
                if (this.IsOption(OptionReplaceWordsCharacterSet) == true)
                {
                    replaceWordsCharacterSet = this.Options[OptionReplaceWordsCharacterSet];
                    if (replaceWordsCharacterSet == Colipex.NonValue)
                    {
                        //Console.WriteLine("Please specify the encoding name. (/rc)");
                        Console.WriteLine("文字エンコーディング名を指定してください。 (/rc)");
                        return;
                    }
                }
                else
                {
                    replaceWordsCharacterSet = readCharacterSet;
                }

                //Setting FileNameList CharacterSet 
                //ファイルリストの文字エンコーディングを設定する。
                if (this.IsOption(OptionFileNameListCharacterSet) == true)
                {
                    filesCharacterSet = this.Options[OptionFileNameListCharacterSet];
                    if (filesCharacterSet == Colipex.NonValue)
                    {
                        //Console.WriteLine("Please specify the encoding name. (/fc)");
                        Console.WriteLine("文字エンコーディング名を指定してください。 (/fc)");
                        return;
                    }
                }
                else
                {
                    filesCharacterSet = readCharacterSet;
                }

                //Setting ByteOrderMark
                //BOM を設定する。
                if (this.IsOption(OptionWriteByteOrderMark) == true)
                {
                    if (this.Options[OptionWriteByteOrderMark].ToLower() == "false" ||
                        this.Options[OptionWriteByteOrderMark].ToLower() == "no" ||
                        this.Options[OptionWriteByteOrderMark].ToLower() == "n")
                        existByteOrderMark = false;
                    else
                        existByteOrderMark = true;
                }
                else
                {
                    existByteOrderMark = null;
                }

                this._enableBOM = existByteOrderMark;

                //-----------------------------------------------------------
                //Setting Encoding and Check error of Encoding
                //エンコーディングの設定とエンコーディングのエラーの確認をする。
                //-
                int codePage;
                int writeCodePage;
                int repleaseCodePage;
                int filesCodePage;

                //Setting Read Encoding
                errorEncoding = "Read Encoding";
                if (readCharacterSet == CharacterSetJudgment)
                    this.encoding = null;
                else if (int.TryParse(readCharacterSet, out codePage))
                    this.encoding = Encoding.GetEncoding(codePage);
                else
                    this.encoding = Encoding.GetEncoding(readCharacterSet);

                //Setting Write Encoding
                errorEncoding = "Write Encoding";
                if (writeCharacterSet == CharacterSetJudgment)
                    this.writeEncoding = null;
                else if (int.TryParse(writeCharacterSet, out writeCodePage))
                    this.writeEncoding = Encoding.GetEncoding(writeCodePage);
                else
                    this.writeEncoding = Encoding.GetEncoding(writeCharacterSet);

                //Setting Replease Encoding
                errorEncoding = "Replease Encoding";
                if (replaceWordsCharacterSet == CharacterSetJudgment)
                    this.repleaseEncoding = null;
                else if (int.TryParse(replaceWordsCharacterSet, out repleaseCodePage))
                    this.repleaseEncoding = Encoding.GetEncoding(repleaseCodePage);
                else
                    this.repleaseEncoding = Encoding.GetEncoding(replaceWordsCharacterSet);

                //Setting Files Encoding
                errorEncoding = "Files Encoding";
                if (filesCharacterSet == CharacterSetJudgment)
                    this.filesEncoding = null;
                else if (int.TryParse(filesCharacterSet, out filesCodePage))
                    this.filesEncoding = Encoding.GetEncoding(filesCodePage);
                else
                    this.filesEncoding = Encoding.GetEncoding(filesCharacterSet);

            }
            catch (DirectoryNotFoundException ex)
            {
                ExecutionState.isError = true;
                ExecutionState.isNormal = !ExecutionState.isError;
                ExecutionState.errorMessage = "指定のパスが存在しません。";
                throw ex;
            }
            catch (NotSupportedException ex)
            {
                ExecutionState.isError = true;
                ExecutionState.isNormal = !ExecutionState.isError;
                ExecutionState.errorMessage = "管理下のエラー:NotSupportedException" + errorEncoding;
                throw ex;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                ExecutionState.isError = true;
                ExecutionState.isNormal = !ExecutionState.isError;
                ExecutionState.errorMessage = "管理下のエラー:ArgumentOutOfRangeException" + errorEncoding;
                throw ex;
            }
            catch (ArgumentException ex)
            {
                ExecutionState.isError = true;
                ExecutionState.isNormal = !ExecutionState.isError;
                ExecutionState.errorMessage = "管理下のエラー:ArgumentException" + errorEncoding;
                throw ex;
            }
            catch (Exception ex)
            {
                ExecutionState.isNormal = false;
                throw ex;
            }

            //------------------------------------------------------------
            // 置換単語リストファイルオプションの設定
            //-
            //置換単語リストが存在する。
            if (this.IsOption(OptionReplaceWords) == true)
            {
                if (this.Options[OptionReplaceWords] == Colipex.NonValue)
                {
                    ExecutionState.isError = true;
                    ExecutionState.isNormal = !ExecutionState.isError;
                    ExecutionState.errorMessage = "/r オプションのファイル名を指定してください。";

                    throw new Exception(ExecutionState.errorMessage);
                }

                //置換単語リストファイル名を保存する
                this._replaceWordsFileName = this.Options[OptionReplaceWords];

                //置換単語リストファイル名の存在確認
                if (!File.Exists(this._replaceWordsFileName))
                {
                    ExecutionState.isError = true;
                    ExecutionState.isNormal = !ExecutionState.isError;
                    ExecutionState.errorMessage = this._replaceWordsFileName + " が存在しません。 ";

                    throw new Exception(ExecutionState.errorMessage);
                }
            }

            //------------------------------------------------------------
            // ファイルリストファイルオプションの設定
            //-
            if (this.IsOption(OptionFileNameList) == true)
            {
                if (this.Options[OptionFileNameList] == Colipex.NonValue)
                {
                    ExecutionState.isError = true;
                    ExecutionState.isNormal = !ExecutionState.isError;
                    ExecutionState.errorMessage = "/f オプションのファイル名を指定してください。";

                    throw new Exception(ExecutionState.errorMessage);
                }

                //ファイルリストファイル名を保存する
                this._fileNameListFileName = this.Options[OptionFileNameList];

                if (!File.Exists(this._fileNameListFileName))
                {
                    ExecutionState.isError = true;
                    ExecutionState.isNormal = !ExecutionState.isError;
                    ExecutionState.errorMessage = this._fileNameListFileName + " が存在しません。 ";

                    throw new Exception(ExecutionState.errorMessage);
                }
            }
        }

        /// <summary>
        /// 置換単語テーブル初期化
        /// </summary>
        /// <returns>true=正常に初期化した</returns>
        public bool ReadReplaceWords()
        {
            bool normal = true;

            if (string.IsNullOrEmpty(this._replaceWordsFileName))
            {
                normal = false;
                return normal;
            }

            //置換単語の設定をする。
            List<string> wordsList = new List<string>();

            if(this.repleaseEncoding == null)
            {
                //置換単語リストファイルの文字エンコーディングを判定する。
                EncodingJudgment encJudg = new EncodingJudgment();
                EncodingInfomation encInfo = encJudg.Judgment(this._replaceWordsFileName);

                if(encInfo.codePage > 0)
                {
                    this.repleaseEncoding = Encoding.GetEncoding(encInfo.codePage);
                }
                else
                {
                    ExecutionState.isError = true;
                    ExecutionState.isNormal = !ExecutionState.isError;
                    ExecutionState.errorMessage = this._replaceWordsFileName + "の文字エンコーディングが分かりません。";

                    throw new Exception(ExecutionState.errorMessage);
                }
            }

            //Read replacement word CSV file
            //置換単語リストCSVを読み取る。
            using (var reader = new StreamReader(this._replaceWordsFileName, this.repleaseEncoding, true))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    if (line.Length == 0) continue;
                    if (line.IndexOf(',') < 0) continue;

                    wordsList.Add(line);
                }
            }

            if(wordsList.Count == 0)
            {
                ExecutionState.isError = true;
                ExecutionState.isNormal = !ExecutionState.isError;
                ExecutionState.errorMessage = this._replaceWordsFileName + "の置換単語がゼロ件です。";

                throw new Exception(ExecutionState.errorMessage);
            }

            //置換単語テーブルへ登録する。
            this._replaceWordsCount = wordsList.Count;
            this._replaceWords = new string[2, this._replaceWordsCount];
            for (int i = 0; i < this._replaceWordsCount; i++)
            {
                string[] colmuns = wordsList[i].Split(',');
                this._replaceWords[0, i] = colmuns[0];
                this._replaceWords[1, i] = colmuns[1];
            }

            return normal;
        }

        /// <summary>
        /// ファイルリストの初期化
        /// </summary>
        /// <returns>true=正常に初期化した</returns>
        public bool ReadFileNameList()
        {
            bool normal = true;

            if (string.IsNullOrEmpty(this._fileNameListFileName))
            {
                //ファイル名リストファイルが無い場合はコマンドパラメータのファイル名を検索する

                string path = this.Parameters[0].TrimEnd(new char[] { '\x0a', '\x0d' });
                string direcrtoryName = Path.GetDirectoryName(path);
                string searchWord = Path.GetFileName(path);

                this._files = Directory.GetFileSystemEntries(direcrtoryName, searchWord, System.IO.SearchOption.AllDirectories);

                normal = true;
                return normal;
            }

            //ファイル名リストファイルが有る場合

            if(this.filesEncoding == null)
            {
                //ファイル名リストファイルの文字エンコーディングを判定する。
                EncodingJudgment encJudg = new EncodingJudgment();
                EncodingInfomation encInfo = encJudg.Judgment(this._fileNameListFileName);

                if (encInfo.codePage > 0)
                {
                    this.filesEncoding = Encoding.GetEncoding(encInfo.codePage);
                }
                else
                {
                    ExecutionState.isError = true;
                    ExecutionState.isNormal = !ExecutionState.isError;
                    ExecutionState.errorMessage = this._fileNameListFileName + "の文字エンコーディングが分かりません。";

                    throw new Exception(ExecutionState.errorMessage);
                }
            }

            List<string> filesList = new List<string>();

            using (var reader = new StreamReader(this._fileNameListFileName, this.filesEncoding, true))
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

                this._files = filesList.ToArray();
            }


            return normal;
        }

        /// <summary>
        /// ヘルプモード
        /// </summary>
        private bool _callHelp = false;

        /// <summary>
        /// ヘルプモード
        /// </summary>
        public bool CallHelp
        {
            get { return this._callHelp; }
        }

        /// <summary>
        /// 置換単語テーブル
        /// </summary>
        private string[,] _replaceWords;

        /// <summary>
        /// 置換単語テーブルプロパティ
        /// </summary>
        public string[,] ReplaceWords
        {
            get { return this._replaceWords; }
        }

        /// <summary>
        /// 置換単語件数
        /// </summary>
        private int _replaceWordsCount;

        /// <summary>
        /// 置換単語件数プロパティ
        /// </summary>
        public int ReplaceWordsCount
        {
            get { return this._replaceWordsCount; }
        }

        /// <summary>
        /// 置換対象ファイル名一覧
        /// </summary>
        private string[] _files = null;

        /// <summary>
        /// 置換対象ファイル名一覧プロパティ
        /// </summary>
        public string[] Files
        {
            get { return this._files; }
        }


        /// <summary>
        /// 書き込みBOM指定が有効です
        /// </summary>
        private bool? _enableBOM = false;

        /// <summary>
        /// 書き込みBOM指定が有効です。プロパティ
        /// </summary>
        public bool? EnableBOM
        {
            get { return this._enableBOM; }
        }


        /// <summary>
        /// 置換単語リストCSVのファイル名
        /// </summary>
        private string _replaceWordsFileName = null;

        /// <summary>
        /// 置換単語リストCSVのファイル名
        /// </summary>
        public string ReplaceWordsFileName
        {
            get { return this._replaceWordsFileName; }
        }

        /// <summary>
        /// ファイルリストのファイル名
        /// </summary>
        private string _fileNameListFileName = null;

        /// <summary>
        /// ファイルリストのファイル名
        /// </summary>
        public string FileNameListFileName
        {
            get { return this._fileNameListFileName; }
        }

        /// <summary>
        /// 読み取り文字エンコーディング
        /// </summary>
        public Encoding encoding = null;

        /// <summary>
        /// 書き込み文字エンコーディング
        /// </summary>
        public Encoding writeEncoding = null;

        /// <summary>
        /// 書き込み文字エンコーディングが指定されていない。
        /// </summary>
        public bool Empty_WriteCharacterSet = false;

        /// <summary>
        /// 置換単語リストCSV文字エンコーディング
        /// </summary>
        public Encoding repleaseEncoding = null;

        /// <summary>
        /// ファイルリスト文字エンコーディング
        /// </summary>
        public Encoding filesEncoding = null;


    }
}
