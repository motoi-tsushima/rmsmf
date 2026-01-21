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
        private const string OptionJudgmentMode = "j";

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
            ValidateRequiredParameters();

            ParseEncodingOptions(
                out string readCharacterSet,
                out string writeCharacterSet,
                out string replaceWordsCharacterSet,
                out string filesCharacterSet);

            this._enableBOM = ParseBomOption();
            this.searchOptionAllDirectories = ParseAllDirectoriesOption();
            this._writeNewLine = ParseNewLineOption();

            InitializeEncodings(
                readCharacterSet,
                writeCharacterSet,
                replaceWordsCharacterSet,
                filesCharacterSet);

            ValidateAndSetFileOptions();
            ValidateOptionConsistency();
        }

        /// <summary>
        /// 必須パラメータの検証
        /// </summary>
        private void ValidateRequiredParameters()
        {
            if (this.IsOption(OptionFileNameList) == false)
            {
                if (this.Parameters.Count == 0 && this.Options.Count == 0)
                {
                    throw new RmsmfException(ValidationMessages.MissingTargetFileName);
                }
            }
        }

        /// <summary>
        /// エンコーディング関連オプションの解析
        /// </summary>
        private void ParseEncodingOptions(
            out string readCharacterSet,
            out string writeCharacterSet,
            out string replaceWordsCharacterSet,
            out string filesCharacterSet)
        {
            //Setting Read CharacterSet 
            //読み取り文字エンコーディング名を設定する。
            if (this.IsOption(OptionCharacterSet))
            {
                readCharacterSet = this.Options[OptionCharacterSet].TrimEnd(new char[] { '\x0a', '\x0d' });
                if (readCharacterSet == Colipex.NonValue)
                {
                    throw new RmsmfException(string.Format(ValidationMessages.MissingEncodingName, "c"));
                }
            }
            else
            {
                readCharacterSet = CharacterSetJudgment;
            }

            //Setting Write CharacterSet 
            //書き込み文字エンコーディング名の設定する。
            if (this.IsOption(OptionWriteCharacterSet))
            {
                writeCharacterSet = this.Options[OptionWriteCharacterSet].TrimEnd(new char[] { '\x0a', '\x0d' });
                if (writeCharacterSet == Colipex.NonValue)
                {
                    throw new RmsmfException(string.Format(ValidationMessages.MissingEncodingName, "w"));
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
            if (this.IsOption(OptionReplaceWordsCharacterSet))
            {
                replaceWordsCharacterSet = this.Options[OptionReplaceWordsCharacterSet].TrimEnd(new char[] { '\x0a', '\x0d' });
                if (replaceWordsCharacterSet == Colipex.NonValue)
                {
                    throw new RmsmfException(string.Format(ValidationMessages.MissingEncodingName, "rc"));
                }
            }
            else
            {
                replaceWordsCharacterSet = readCharacterSet;
            }

            //Setting FileNameList CharacterSet 
            //ファイルリストの文字エンコーディングを設定する。
            if (this.IsOption(OptionFileNameListCharacterSet))
            {
                filesCharacterSet = this.Options[OptionFileNameListCharacterSet].TrimEnd(new char[] { '\x0a', '\x0d' });
                if (filesCharacterSet == Colipex.NonValue)
                {
                    throw new RmsmfException(string.Format(ValidationMessages.MissingEncodingName, "fc"));
                }
            }
            else
            {
                filesCharacterSet = readCharacterSet;
            }

            //Setting Encoding Judgment Mode
            //文字エンコーディング自動判定モードを設定する。
            if (this.IsOption(OptionJudgmentMode))
            {
                string encJMode = string.Empty;
                encJMode = this.Options[OptionJudgmentMode].TrimEnd(new char[] { '\x0a', '\x0d' });
                if (encJMode == Colipex.NonValue)
                {
                    _encodingJudgmentMode = EncodingJudgmentType.Normal;
                }
                else
                {
                    if (encJMode == "1")
                    {
                        _encodingJudgmentMode = EncodingJudgmentType.FirstParty;
                    }
                    else if (encJMode == "3")
                    {
                        _encodingJudgmentMode = EncodingJudgmentType.ThirdParty;
                    }
                    else
                    {
                        _encodingJudgmentMode = EncodingJudgmentType.Normal;
                    }
                }
            }
            else
            {
                _encodingJudgmentMode = EncodingJudgmentType.Normal;
            }

        }

        /// <summary>
        /// BOMオプションの解析
        /// </summary>
        /// <returns>BOM設定（null=指定なし、true=有効、false=無効）</returns>
        private bool? ParseBomOption()
        {
            if (this.IsOption(OptionWriteByteOrderMark))
            {
                string optionBOM = this.Options[OptionWriteByteOrderMark].TrimEnd(new char[] { '\x0a', '\x0d' }).ToLower();

                if (optionBOM == "false" || optionBOM == "no" || optionBOM == "n")
                    return false;
                else
                    return true;
            }

            return null;
        }

        /// <summary>
        /// AllDirectoriesオプションの解析
        /// </summary>
        /// <returns>AllDirectoriesが有効かどうか</returns>
        private bool ParseAllDirectoriesOption()
        {
            return this.IsOption(OptionAllDirectories);
        }

        /// <summary>
        /// 改行コードオプションの解析
        /// </summary>
        /// <returns>改行コード設定（null=指定なし）</returns>
        private string ParseNewLineOption()
        {
            if (this.IsOption(OptionNewLine))
            {
                string optionNewLine = this.Options[OptionNewLine].TrimEnd(new char[] { '\x0a', '\x0d' }).ToLower();

                if (optionNewLine == "win" || optionNewLine == "windows" || optionNewLine == "w" || optionNewLine == "crlf")
                    return NewLineCRLF;
                else if (optionNewLine == "unix" || optionNewLine == "u"
                    || optionNewLine == "linux" || optionNewLine == "l"
                    || optionNewLine == "mac" || optionNewLine == "m"
                    || optionNewLine == "lf")
                    return NewLineLF;
                else if (optionNewLine == "oldmac" || optionNewLine == "cr" || optionNewLine == "old")
                    return NewLineCR;
                else
                    return NewLineCRLF;
            }

            return null;
        }

        /// <summary>
        /// Encodingオブジェクトの初期化
        /// </summary>
        private void InitializeEncodings(
            string readCharacterSet,
            string writeCharacterSet,
            string replaceWordsCharacterSet,
            string filesCharacterSet)
        {
            try
            {
                this.ReadEncoding = ResolveEncoding(readCharacterSet, CharacterSetJudgment);
                this.WriteEncoding = ResolveEncoding(writeCharacterSet, CharacterSetJudgment);
                this.ReplaceEncoding = ResolveEncoding(replaceWordsCharacterSet, CharacterSetJudgment);
                this.FilesEncoding = ResolveEncoding(filesCharacterSet, CharacterSetJudgment);
            }
            catch (ArgumentException ex)
            {
                throw new RmsmfException(ValidationMessages.InvalidEncodingName, ex);
            }
            catch (NotSupportedException ex)
            {
                throw new RmsmfException(ValidationMessages.UnsupportedEncoding, ex);
            }
        }

        /// <summary>
        /// ファイル関連オプションの検証と設定
        /// </summary>
        private void ValidateAndSetFileOptions()
        {
            // 置換単語リストファイルオプションの設定
            if (this.IsOption(OptionReplaceWords))
            {
                if (this.Options[OptionReplaceWords] == Colipex.NonValue)
                {
                    throw new RmsmfException(string.Format(ValidationMessages.MissingOptionFileName, "r"));
                }

                this._replaceWordsFileName = this.Options[OptionReplaceWords].TrimEnd(new char[] { '\x0a', '\x0d' });

                if (!File.Exists(this._replaceWordsFileName))
                {
                    throw new RmsmfException(string.Format(ValidationMessages.FileNotFound, this._replaceWordsFileName));
                }
            }

            // ファイルリストファイルオプションの設定
            if (this.IsOption(OptionFileNameList))
            {
                if (this.Options[OptionFileNameList] == Colipex.NonValue)
                {
                    throw new RmsmfException(string.Format(ValidationMessages.MissingOptionFileName, "f"));
                }

                this._fileNameListFileName = this.Options[OptionFileNameList].TrimEnd(new char[] { '\x0a', '\x0d' });

                if (!File.Exists(this._fileNameListFileName))
                {
                    throw new RmsmfException(string.Format(ValidationMessages.FileNotFound, this._fileNameListFileName));
                }
            }
        }

        /// <summary>
        /// オプションの組み合わせの正当性確認
        /// </summary>
        private void ValidateOptionConsistency()
        {
            ValidateRequiredParametersProvided();
            ValidateFileSpecificationMethod();
            ValidateReplaceWordsHasTargetFiles();
            ValidateEncodingOptionsDependencies();
            ValidateConversionModeRequirements();
        }

        /// <summary>
        /// 必須パラメータが最低1つ指定されているか検証
        /// </summary>
        private void ValidateRequiredParametersProvided()
        {
            OptionValidator.ValidateAtLeastOneCondition(
                this.IsOption(OptionReplaceWords),
                this.IsOption(OptionFileNameList),
                this.Parameters.Count > 0
            );
        }

        /// <summary>
        /// ファイル指定方法が競合していないか検証
        /// </summary>
        private void ValidateFileSpecificationMethod()
        {
            OptionValidator.ValidateFileSpecificationNotConflicting(
                this.IsOption(OptionFileNameList),
                this.Parameters.Count
            );
        }

        /// <summary>
        /// 置換単語オプション使用時に対象ファイルが指定されているか検証
        /// </summary>
        private void ValidateReplaceWordsHasTargetFiles()
        {
            if (this.IsOption(OptionReplaceWords) && 
                this.IsOption(OptionFileNameList) == false && 
                this.Parameters.Count == 0)
            {
                throw new RmsmfException(ValidationMessages.ReplaceWordsRequiresTargetFiles);
            }
        }

        /// <summary>
        /// エンコーディング関連オプションの依存関係を検証
        /// </summary>
        private void ValidateEncodingOptionsDependencies()
        {
            // 置換単語ファイルのエンコーディングは、置換単語ファイルオプションが必要
            OptionValidator.ValidateEncodingOptionDependency(
                this.IsOption(OptionReplaceWords),
                this.IsOption(OptionReplaceWordsCharacterSet),
                ValidationMessages.ReplaceWordsEncodingWithoutReplaceWords
            );

            // ファイルリストのエンコーディングは、ファイルリストオプションが必要
            OptionValidator.ValidateEncodingOptionDependency(
                this.IsOption(OptionFileNameList),
                this.IsOption(OptionFileNameListCharacterSet),
                ValidationMessages.FileListEncodingWithoutFileList
            );
        }

        /// <summary>
        /// 変換モード時の必須オプションを検証
        /// </summary>
        private void ValidateConversionModeRequirements()
        {
            // 置換単語オプションが無い場合、少なくとも1つの変換オプションが必要
            if (this.IsOption(OptionReplaceWords) == false)
            {
                bool hasConversionOption = 
                    this.IsOption(OptionCharacterSet) ||
                    this.IsOption(OptionWriteCharacterSet) ||
                    this.IsOption(OptionWriteByteOrderMark) ||
                    this.IsOption(OptionNewLine);

                if (!hasConversionOption)
                {
                    throw new RmsmfException(ValidationMessages.ConversionRequiresOutputEncoding);
                }
            }
        }

        /// <summary>
        /// 置換単語テーブル初期化
        /// </summary>
        /// <returns>true=正常に初期化した</returns>
        public bool ReadReplaceWords()
        {
            if (string.IsNullOrEmpty(this._replaceWordsFileName))
            {
                return false;
            }

            // エンコーディングの判定と設定
            EnsureEncodingInitialized(
                ref this._replaceEncoding, 
                this._replaceWordsFileName, 
                ValidationMessages.UnknownEncoding);

            // ファイルから行を読み込み
            List<string> lines = LoadReplaceWordsFromFile();

            // 空チェック
            if (lines.Count == 0)
            {
                throw new RmsmfException(string.Format(ValidationMessages.EmptyReplaceWords, this._replaceWordsFileName));
            }

            // 置換単語テーブルへ登録
            ParseAndStoreReplaceWords(lines);

            return true;
        }

        /// <summary>
        /// 置換単語リストファイルから行を読み込む
        /// </summary>
        /// <returns>読み込んだ行のリスト（カンマを含む行のみ）</returns>
        private List<string> LoadReplaceWordsFromFile()
        {
            List<string> lines = new List<string>();

            using (var reader = new StreamReader(this._replaceWordsFileName, this.ReplaceEncoding, true))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    line = this.ConvertEscapeSequences(line);

                    if (line.Length == 0) continue;
                    if (line.IndexOf(',') < 0) continue;  // カンマがない行はスキップ

                    lines.Add(line);
                }
            }

            return lines;
        }

        /// <summary>
        /// 読み込んだ行をパースして置換単語テーブルに格納
        /// </summary>
        /// <param name="lines">読み込んだ行のリスト</param>
        private void ParseAndStoreReplaceWords(List<string> lines)
        {
            this._replaceWordsCount = lines.Count;
            this._replaceWords = new string[2, this._replaceWordsCount];

            for (int i = 0; i < this._replaceWordsCount; i++)
            {
                string[] columns = lines[i].Split(',');

                // 入力検証: カンマ区切りで2つの要素が必要
                if (columns.Length < 2)
                {
                    throw new RmsmfException(string.Format(ValidationMessages.InvalidReplaceWordFormat, i + 1));
                }

                this._replaceWords[0, i] = columns[0];
                this._replaceWords[1, i] = columns[1];
            }
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
                    throw new RmsmfException(string.Format(ValidationMessages.UnknownEncoding, this._fileNameListFileName));
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


        /// <summary>
        /// 文字エンコーディングの自動判定モード
        /// </summary>
        private EncodingJudgmentType _encodingJudgmentMode = EncodingJudgmentType.Normal;
        /// <summary>
        /// 文字エンコーディングの自動判定モード
        /// </summary>
        public EncodingJudgmentType EncodingJudgmentMode
        {
            get { return _encodingJudgmentMode; }
            private set { _encodingJudgmentMode = value; }
        }

        /// <summary>
        /// 文字エンコーディングの自動判定モードタイプ
        /// </summary>
        public enum EncodingJudgmentType
        {
            Normal = 0,
            FirstParty = 1,
            ThirdParty = 3
        }

    }
}
