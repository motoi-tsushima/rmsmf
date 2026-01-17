using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
        private const string OptionAllDirectories = "d";
        private const string OptionNewLine = "nl";

        public static readonly string NewLineCRLF = "CRLF";
        public static readonly string NewLineLF = "LF";
        public static readonly string NewLineCR = "CR";

        private bool searchOptionAllDirectories = false; // AllDirectories オプション

        /// <summary>
        /// コマンドオプション
        /// </summary>
        /// <param name="args"></param>
        public CommandOptions(string[] args) : base(args)
        {

            string readCharacterSet;
            string replaceWordsCharacterSet;
            string filesCharacterSet;
            string writeCharacterSet;
            bool? existByteOrderMark;
            string writeNewLine;

            string errorEncoding = null;

            //ヘルプオプションの場合
            if (this.IsOption(OptionHelp) == true)
            {
                this._callHelp = true;
                return;
            }

            //パラメータがない場合は終了
            if (this.IsOption(OptionFileNameList) == false)
            {
                if (this.Parameters.Count == 0 && this.Options.Count == 0)
                {
                    throw new RmsmfException("目的のファイル名を指定してください。(/h ヘルプ表示)");
                }
            }

            try
            {
                //Setting Read CharacterSet 
                //読み取り文字エンコーディング名を設定する。
                if (this.IsOption(OptionCharacterSet) == true)
                {
                    readCharacterSet = this.Options[OptionCharacterSet].TrimEnd(new char[] { '\x0a', '\x0d' });
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
                    writeCharacterSet = this.Options[OptionWriteCharacterSet].TrimEnd(new char[] { '\x0a', '\x0d' });
                    if (writeCharacterSet == Colipex.NonValue)
                    {
                        //Console.WriteLine("Please specify the encoding name. (/w)");
                        Console.WriteLine("文字エンコーディング名を指定してください。 (/w)");
                        return;
                    }
                    this.EmptyWriteCharacterSet = false;
                }
                else
                {
                    writeCharacterSet = readCharacterSet;
                    this.EmptyWriteCharacterSet = true;
                }

                //Setting ReplaceWords CharacterSet 
                //置換単語リストの文字エンコーディングの設定する。
                if (this.IsOption(OptionReplaceWordsCharacterSet) == true)
                {
                    replaceWordsCharacterSet = this.Options[OptionReplaceWordsCharacterSet].TrimEnd(new char[] { '\x0a', '\x0d' });
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
                    filesCharacterSet = this.Options[OptionFileNameListCharacterSet].TrimEnd(new char[] { '\x0a', '\x0d' });
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
                    string optionBOM = this.Options[OptionWriteByteOrderMark].TrimEnd(new char[] { '\x0a', '\x0d' }).ToLower();

                    if (optionBOM == "false" || optionBOM == "no" || optionBOM == "n")
                        existByteOrderMark = false;
                    else
                        existByteOrderMark = true;
                }
                else
                {
                    existByteOrderMark = null;
                }

                this._enableBOM = existByteOrderMark;

                // AllDirectories を有効にする
                if (this.IsOption(OptionAllDirectories) == true)
                {
                    this.searchOptionAllDirectories = true;
                }
                else
                {
                    this.searchOptionAllDirectories = false;
                }

                //Setting New Line
                //改行コード を設定する。
                if (this.IsOption(OptionNewLine) == true)
                {
                    string optionNewLine = this.Options[OptionNewLine].TrimEnd(new char[] { '\x0a', '\x0d' }).ToLower();

                    if (optionNewLine == "win" || optionNewLine == "windows" || optionNewLine == "w" || optionNewLine == "crlf")
                        writeNewLine = NewLineCRLF;
                    else if (optionNewLine == "unix" || optionNewLine == "u" 
                        || optionNewLine == "linux" || optionNewLine == "l" 
                        || optionNewLine == "mac" || optionNewLine == "m"
                        || optionNewLine == "lf")
                        writeNewLine = NewLineLF;
                    else if (optionNewLine == "oldmac" || optionNewLine == "cr" || optionNewLine == "old")
                        writeNewLine = NewLineCR;
                    else
                        writeNewLine = NewLineCRLF;
                }
                else
                {
                    writeNewLine = null;
                }

                this._writeNewLine = writeNewLine;


                //-----------------------------------------------------------
                //Setting Encoding and Check error of Encoding
                //エンコーディングの設定とエンコーディングのエラーの確認をする。
                //-

                //Setting Read Encoding
                errorEncoding = "Read Encoding";
                this.ReadEncoding = ResolveEncoding(readCharacterSet, CharacterSetJudgment);

                //Setting Write Encoding
                errorEncoding = "Write Encoding";
                this.WriteEncoding = ResolveEncoding(writeCharacterSet, CharacterSetJudgment);

                //Setting Replace Encoding
                errorEncoding = "Replace Encoding";
                this.ReplaceEncoding = ResolveEncoding(replaceWordsCharacterSet, CharacterSetJudgment);

                //Setting Files Encoding
                errorEncoding = "Files Encoding";
                this.FilesEncoding = ResolveEncoding(filesCharacterSet, CharacterSetJudgment);

            }
            catch (DirectoryNotFoundException ex)
            {
                throw new RmsmfException("指定のパスが存在しません。", ex);
            }
            catch (NotSupportedException ex)
            {
                throw new RmsmfException("管理下のエラー:NotSupportedException" + errorEncoding, ex);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw new RmsmfException("管理下のエラー:ArgumentOutOfRangeException" + errorEncoding, ex);
            }
            catch (ArgumentException ex)
            {
                if (errorEncoding == null)
                    throw new RmsmfException("管理下のエラー:ArgumentException" + errorEncoding, ex);
                else
                {
                    throw new RmsmfException(errorEncoding + " のエンコーディング名が不正です。", ex);
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            //------------------------------------------------------------
            // 置換単語リストファイルオプションの設定
            //-
            //置換単語リストが存在する。
            if (this.IsOption(OptionReplaceWords) == true)
            {
                if (this.Options[OptionReplaceWords] == Colipex.NonValue)
                {
                    throw new RmsmfException("/r オプションのファイル名を指定してください。");
                }

                //置換単語リストファイル名を保存する
                this._replaceWordsFileName = this.Options[OptionReplaceWords].TrimEnd(new char[] { '\x0a', '\x0d' });

                //置換単語リストファイル名の存在確認
                if (!File.Exists(this._replaceWordsFileName))
                {
                    throw new RmsmfException(this._replaceWordsFileName + " が存在しません。 ");
                }
            }

            //------------------------------------------------------------
            // ファイルリストファイルオプションの設定
            //-
            if (this.IsOption(OptionFileNameList) == true)
            {
                if (this.Options[OptionFileNameList] == Colipex.NonValue)
                {
                    throw new RmsmfException("/f オプションのファイル名を指定してください。");
                }

                //ファイルリストファイル名を保存する
                this._fileNameListFileName = this.Options[OptionFileNameList].TrimEnd(new char[] { '\x0a', '\x0d' });

                if (!File.Exists(this._fileNameListFileName))
                {
                    throw new RmsmfException(this._fileNameListFileName + " が存在しません。 ");
                }
            }

            //------------------------------------------------------------
            // オプションの正当性確認
            //-
            if (this.IsOption(OptionReplaceWords) == false && this.IsOption(OptionFileNameList) == false && this.Parameters.Count == 0)
            {
                throw new RmsmfException("必須パラメータが入力されていません。");
            }

            if (this.IsOption(OptionReplaceWords) == false)
            {
                if (this.IsOption(OptionCharacterSet) == false && this.IsOption(OptionWriteCharacterSet) == false 
                    && this.IsOption(OptionWriteByteOrderMark) == false
                    && this.IsOption(OptionNewLine) == false)
                {
                    throw new RmsmfException("文字エンコーディングの変換をする場合は、/w:により出力先の文字エンコーディングを指定してください。");
                }
            }

            if (this.IsOption(OptionFileNameList) == true && this.Parameters.Count > 0)
            {
                throw new RmsmfException("/f:オプションによるファイル指定と、コマンドラインでのファイル指定を、同時に使用する事はできません。");
            }

            if (this.IsOption(OptionReplaceWords) == true && this.IsOption(OptionFileNameList) == false && this.Parameters.Count == 0)
            {
                throw new RmsmfException("置換対象となるファイルを指定してください。");
            }

            if (this.IsOption(OptionReplaceWords) == false && this.IsOption(OptionReplaceWordsCharacterSet) == true)
            {
                throw new RmsmfException("置換単語ファイルが指定されていないのに、置換単語ファイルのエンコーディングが指定されています。");
            }

            if (this.IsOption(OptionFileNameList) == false && this.IsOption(OptionFileNameListCharacterSet) == true)
            {
                throw new RmsmfException("ファイルリストが指定されていないのに、ファイルリストのエンコーディングが指定されています。");
            }
        }

        /// <summary>
        /// エスケープシーケンス変換
        /// </summary>
        /// <param name="input">変換対象文字列</param>
        /// <returns>変換後文字列</returns>
        private string ConvertEscapeSequences(string input)
        {
            return input
                .Replace("\\r\\n", "\r\n")
                .Replace("\\r", "\r")
                .Replace("\\n", "\n")
                .Replace("\\t", "\t")
                .Replace("\\\\", "\\");
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

            if (this.ReplaceEncoding == null)
            {
                //置換単語リストファイルの文字エンコーディングを判定する。
                EncodingJudgment encJudg = new EncodingJudgment(0);
                EncodingInfomation encInfo = encJudg.Judgment(this._replaceWordsFileName);

                if (encInfo.CodePage > 0)
                {
                    this.ReplaceEncoding = Encoding.GetEncoding(encInfo.CodePage);
                }
                else
                {
                    throw new RmsmfException(this._replaceWordsFileName + "の文字エンコーディングが分かりません。");
                }
            }

            //Read replacement word CSV file
            //置換単語リストCSVを読み取る。
            using (var reader = new StreamReader(this._replaceWordsFileName, this.ReplaceEncoding, true))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    line = this.ConvertEscapeSequences(line);

                    if (line.Length == 0) continue;
                    if (line.IndexOf(',') < 0) continue;

                    wordsList.Add(line);
                }
            }

            if (wordsList.Count == 0)
            {
                throw new RmsmfException(this._replaceWordsFileName + "の置換单語がゼロ件です。");
            }

            // 置換単語テーブルへ登録する
            this._replaceWordsCount = wordsList.Count;
            this._replaceWords = new string[2, this._replaceWordsCount];
            for (int i = 0; i < this._replaceWordsCount; i++)
            {
                string[] columns = wordsList[i].Split(',');
                
                // 入力検証: カンマ区切りで2つの要素が必要
                if (columns.Length < 2)
                {
                    throw new RmsmfException($"置換単語ファイルの{i + 1}行目が不正です。カンマ区切りで「検索文字列,置換文字列」の形式で指定してください。");
                }
                
                this._replaceWords[0, i] = columns[0];
                this._replaceWords[1, i] = columns[1];
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

                string directoryName = Path.GetDirectoryName(path);
                if (directoryName != null)
                {
                    if (directoryName.Length == 0)
                    {
                        directoryName = ".";
                    }
                    else if (directoryName.Length == 1)
                    {
                        if (directoryName[0] != '.' && directoryName[0] != '\\')
                        {
                            directoryName = ".\\" + directoryName;
                        }
                    }
                    else if (directoryName.Length > 1)
                    {
                        if (directoryName[0] != '.' && directoryName[0] != '\\'
                            && directoryName[1] != ':')
                        {
                            directoryName = ".\\" + directoryName;
                        }
                    }
                }
                else
                {
                    directoryName = ".";
                }

                string searchWord = Path.GetFileName(path);

                try
                {
                    System.IO.SearchOption searchOption = System.IO.SearchOption.TopDirectoryOnly;
                    if (this.searchOptionAllDirectories == true)
                        searchOption = System.IO.SearchOption.AllDirectories;

                    this._files = Directory.GetFileSystemEntries(directoryName, searchWord, searchOption);
                }
                catch (System.ArgumentException ex)
                {
                    throw new RmsmfException(path + " が存在しないか、検索キーワードとして無効です。", ex);
                }

                normal = true;
                return normal;
            }

            //ファイル名リストファイルが有る場合

            if (this.FilesEncoding == null)
            {
                //ファイル名リストファイルの文字エンコーディングを判定する。
                EncodingJudgment encJudg = new EncodingJudgment(0);
                EncodingInfomation encInfo = encJudg.Judgment(this._fileNameListFileName);

                if (encInfo.CodePage > 0)
                {
                    this.FilesEncoding = Encoding.GetEncoding(encInfo.CodePage);
                }
                else
                {
                    throw new RmsmfException(this._fileNameListFileName + "の文字エンコーディングが分かりません。");
                }
            }

            List<string> filesList = new List<string>();

            using (var reader = new StreamReader(this._fileNameListFileName, this.FilesEncoding, true))
            {
                while (!reader.EndOfStream)
                {
                    string getLine = reader.ReadLine();
                    
                    // 空行やnullをスキップ
                    if (string.IsNullOrWhiteSpace(getLine))
                    {
                        continue;
                    }
                    
                    string getFileName = getLine.Trim();
                    if (getLine.Contains(","))
                    {
                        // カンマ区切りの場合は、最初の項目をファイル名とする
                        string[] columns = getLine.Split(',');
                        
                        // 入力検証: 配列が空でないことを確認
                        if (columns.Length > 0 && !string.IsNullOrWhiteSpace(columns[0]))
                        {
                            getFileName = columns[0].Trim();
                        }
                        else
                        {
                            continue;
                        }
                    }
                    
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
        /// 書き込み用改行コード
        /// </summary>
        private string _writeNewLine = null;

        /// <summary>
        /// 書き込み用改行コード
        /// </summary>
        public string WriteNewLine
        {
            get { return this._writeNewLine; }
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
        private Encoding _readEncoding = null;

        /// <summary>
        /// 読み取り文字エンコーディング
        /// </summary>
        public Encoding ReadEncoding
        {
            get { return _readEncoding; }
            private set { _readEncoding = value; }
        }

        /// <summary>
        /// 書き込み文字エンコーディング
        /// </summary>
        private Encoding _writeEncoding = null;

        /// <summary>
        /// 書き込み文字エンコーディング
        /// </summary>
        public Encoding WriteEncoding
        {
            get { return _writeEncoding; }
            private set { _writeEncoding = value; }
        }

        /// <summary>
        /// 書き込み文字エンコーディングが指定されていない。
        /// </summary>
        private bool _emptyWriteCharacterSet = false;

        /// <summary>
        /// 書き込み文字エンコーディングが指定されていない。
        /// </summary>
        public bool EmptyWriteCharacterSet
        {
            get { return _emptyWriteCharacterSet; }
            private set { _emptyWriteCharacterSet = value; }
        }

        /// <summary>
        /// 置換単語リストCSV文字エンコーディング
        /// </summary>
        private Encoding _replaceEncoding = null;

        /// <summary>
        /// 置換単語リストCSV文字エンコーディング
        /// </summary>
        public Encoding ReplaceEncoding
        {
            get { return _replaceEncoding; }
            private set { _replaceEncoding = value; }
        }

        /// <summary>
        /// ファイルリスト文字エンコーディング
        /// </summary>
        private Encoding _filesEncoding = null;

        /// <summary>
        /// ファイルリスト文字エンコーディング
        /// </summary>
        public Encoding FilesEncoding
        {
            get { return _filesEncoding; }
            private set { _filesEncoding = value; }
        }


    }
}
