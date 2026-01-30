using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using rmsmf;

namespace txprobe
{
    /// <summary>
    /// txprobe 用　コマンドオプション解析クラス
    /// </summary>
    public class CommandOptions : Colipex
    {
        private bool _searchOptionAllDirectories = false; // AllDirectories オプション
        private string _cultureInfo = null; // CultureInfo オプション
        private bool _helpOrVersionDisplayed = false; // ヘルプまたはバージョンが表示されたかどうか

        /// <summary>
        /// コマンドオプション
        /// </summary>
        /// <param name="args"></param>
        public CommandOptions(string[] args) : base(args)
        {
            // 最初にカルチャー情報を設定（ヘルプやバージョン表示に反映させるため）
            this._cultureInfo = ParseCultureInfoOption();
            if (!string.IsNullOrEmpty(this._cultureInfo))
            {
                try
                {
                    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(this._cultureInfo);
                }
                catch (System.Globalization.CultureNotFoundException ex)
                {
                    throw new RmsmfException(string.Format(rmsmf.ValidationMessages.InvalidCultureInfo, this._cultureInfo) + "\n" + ex.Message, ex);
                }
                catch (ArgumentException ex)
                {
                    throw new RmsmfException(string.Format(rmsmf.ValidationMessages.InvalidCultureInfo, this._cultureInfo) + "\n" + ex.Message, ex);
                }
            }

            // ヘルプまたはバージョンオプションのチェック（カルチャー設定後）
            if (CheckAndDisplayHelpOrVersion())
            {
                this._helpOrVersionDisplayed = true;
                return; // ヘルプまたはバージョンを表示した場合は、以降の処理をスキップ
            }

            ValidateRequiredParameters();

            ParseEncodingOptions(
                out string readCharacterSet,
                out string searchWordsCharacterSet,
                out string filesCharacterSet);

            this._enableProbe = ParseProbeModeOption();
            this._searchOptionAllDirectories = ParseAllDirectoriesOption();
            this._outputFileNameListFileName = ParseOutputFileNameListOption();

            InitializeEncodings(
                readCharacterSet,
                searchWordsCharacterSet,
                filesCharacterSet);

            ValidateAndSetFileOptions();
            ValidateOptionConsistency();
        }

        /// <summary>
        /// 必須パラメータの検証
        /// </summary>
        private void ValidateRequiredParameters()
        {
            if (this.IsOption(rmsmf.OptionConstants.OptionFileNameList) == false)
            {
                if (this.Parameters.Count == 0 && this.Options.Count == 0)
                {
                    throw new RmsmfException(rmsmf.ValidationMessages.MissingTargetFileName);
                }
            }
        }

        /// <summary>
        /// ヘルプまたはバージョンオプションをチェックして表示
        /// </summary>
        /// <returns>ヘルプまたはバージョンを表示した場合は true</returns>
        private bool CheckAndDisplayHelpOrVersion()
        {
            // バージョンオプションのチェック
            if (this.IsOption("v"))
            {
                System.Reflection.Assembly thisAssem = typeof(CommandOptions).Assembly;
                System.Reflection.AssemblyName thisAssemName = thisAssem.GetName();
                System.Reflection.AssemblyCopyrightAttribute[] copyrightAttributes = 
                    (System.Reflection.AssemblyCopyrightAttribute[])thisAssem.GetCustomAttributes(typeof(System.Reflection.AssemblyCopyrightAttribute), false);

                Version ver = thisAssemName.Version;
                String copyright = copyrightAttributes[0].Copyright;

                VersionWriter.WriteVersion(true, thisAssemName.Name, ver, copyright);
                return true;
            }

            // ヘルプオプションのチェック
            if (this.IsOption(rmsmf.OptionConstants.OptionHelp))
            {
                string helpOption = this.Options[rmsmf.OptionConstants.OptionHelp];
                
                // /h:cul - カルチャー情報の一覧を表示
                if (helpOption != null && helpOption.Trim().ToLower() == "cul")
                {
                    Help help = new Help();
                    help.ShowAvailableCultures();
                    return true;
                }
                // /h:enc - エンコーディング情報の一覧を表示
                else if (helpOption != null && helpOption.Trim().ToLower() == "enc")
                {
                    Help help = new Help();
                    help.ShowAvailableEncodings();
                    return true;
                }
                // /h - 通常のヘルプを表示
                else
                {
                    Help help = new Help();
                    help.Show();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// エンコーディング関連オプションの解析
        /// </summary>
        private void ParseEncodingOptions(
            out string readCharacterSet,
            out string searchWordsCharacterSet,
            out string filesCharacterSet)
        {
            //Setting Read CharacterSet 
            //読み取り文字エンコーディング名を設定する。
            if (this.IsOption(rmsmf.OptionConstants.OptionCharacterSet))
            {
                readCharacterSet = this.Options[rmsmf.OptionConstants.OptionCharacterSet].TrimEnd(new char[] { '\x0a', '\x0d' });
                if (readCharacterSet == Colipex.NonValue)
                {
                    throw new RmsmfException(string.Format(rmsmf.ValidationMessages.MissingEncodingName, "c"));
                }
            }
            else
            {
                readCharacterSet = rmsmf.OptionConstants.CharacterSetDetection;
            }

            //Setting SearchWords CharacterSet 
            //検索単語リストの文字エンコーディングの設定する。
            if (this.IsOption(rmsmf.OptionConstants.OptionSearchWordsCharacterSet))
            {
                searchWordsCharacterSet = this.Options[rmsmf.OptionConstants.OptionSearchWordsCharacterSet].TrimEnd(new char[] { '\x0a', '\x0d' });
                if (searchWordsCharacterSet == Colipex.NonValue)
                {
                    throw new RmsmfException(string.Format(rmsmf.ValidationMessages.MissingEncodingName, "sc"));
                }
            }
            else
            {
                searchWordsCharacterSet = readCharacterSet;
            }

            //Setting FileNameList CharacterSet 
            //ファイルリストの文字エンコーディングを設定する。
            if (this.IsOption(rmsmf.OptionConstants.OptionFileNameListCharacterSet))
            {
                filesCharacterSet = this.Options[rmsmf.OptionConstants.OptionFileNameListCharacterSet].TrimEnd(new char[] { '\x0a', '\x0d' });
                if (filesCharacterSet == Colipex.NonValue)
                {
                    throw new RmsmfException(string.Format(rmsmf.ValidationMessages.MissingEncodingName, "fc"));
                }
            }
            else
            {
                filesCharacterSet = readCharacterSet;
            }

            //Setting Encoding Detection Mode
            //文字エンコーディング自動判定モードを設定する。
            if (this.IsOption(rmsmf.OptionConstants.OptionDetectionMode))
            {
                string encJMode = string.Empty;
                encJMode = this.Options[rmsmf.OptionConstants.OptionDetectionMode].TrimEnd(new char[] { '\x0a', '\x0d' });
                if (encJMode == Colipex.NonValue)
                {
                    _encodingDetectionMode = EncodingDetectionType.Normal;
                }
                else
                {
                    if(encJMode == "1")
                    {
                        _encodingDetectionMode = EncodingDetectionType.FirstParty;
                    }
                    else if(encJMode == "3")
                    {
                        _encodingDetectionMode = EncodingDetectionType.ThirdParty;
                    }
                    else
                    {
                        _encodingDetectionMode = EncodingDetectionType.Normal;
                    }
                }
            }
            else
            {
                _encodingDetectionMode = EncodingDetectionType.Normal;
            }
        }

        /// <summary>
        /// Probemodeオプションの解析
        /// </summary>
        /// <returns>Probemodeが有効かどうか</returns>
        private bool ParseProbeModeOption()
        {
            if (this.IsOption(rmsmf.OptionConstants.OptionProbeMode))
            {
                return true;
            }
            else
            {
                // 検索文字列が無い場合は Probe Mode を有効にする
                return this.IsOption(rmsmf.OptionConstants.OptionSearchWords) == false;
            }
        }

        /// <summary>
        /// AllDirectoriesオプションの解析
        /// </summary>
        /// <returns>AllDirectoriesが有効かどうか</returns>
        private bool ParseAllDirectoriesOption()
        {
            return this.IsOption(rmsmf.OptionConstants.OptionAllDirectories);
        }

        /// <summary>
        /// 出力ファイルリストオプションの解析
        /// </summary>
        /// <returns>出力ファイルリストのファイル名（null=指定なし）</returns>
        private string ParseOutputFileNameListOption()
        {
            if (this.IsOption(rmsmf.OptionConstants.OptionOutputFileNamelist))
            {
                if (this.Options[rmsmf.OptionConstants.OptionOutputFileNamelist] == Colipex.NonValue)
                {
                    return "output_filelist.txt";
                }
                else
                {
                    return this.Options[rmsmf.OptionConstants.OptionOutputFileNamelist].TrimEnd(new char[] { '\x0a', '\x0d' });
                }
            }

            return null;
        }

        /// <summary>
        /// カルチャー情報オプションの解析
        /// </summary>
        /// <returns>カルチャー情報文字列（null=指定なし）</returns>
        private string ParseCultureInfoOption()
        {
            if (this.IsOption(rmsmf.OptionConstants.OptionCultureInfo))
            {
                string cultureInfo = this.Options[rmsmf.OptionConstants.OptionCultureInfo].TrimEnd(new char[] { '\x0a', '\x0d' });
                if (cultureInfo != Colipex.NonValue && !string.IsNullOrWhiteSpace(cultureInfo))
                {
                    return cultureInfo;
                }
            }

            return null;
        }

        /// <summary>
        /// Encodingオブジェクトの初期化
        /// </summary>
        private void InitializeEncodings(
            string readCharacterSet,
            string searchWordsCharacterSet,
            string filesCharacterSet)
        {
            try
            {
                this.ReadEncoding = ResolveEncoding(readCharacterSet, rmsmf.OptionConstants.CharacterSetDetection);
                this.ReplaceEncoding = ResolveEncoding(searchWordsCharacterSet, rmsmf.OptionConstants.CharacterSetDetection);
                this.FilesEncoding = ResolveEncoding(filesCharacterSet, rmsmf.OptionConstants.CharacterSetDetection);
            }
            catch (ArgumentException ex)
            {
                throw new RmsmfException(rmsmf.ValidationMessages.InvalidEncodingName, ex);
            }
            catch (NotSupportedException ex)
            {
                throw new RmsmfException(rmsmf.ValidationMessages.UnsupportedEncoding, ex);
            }
        }

        /// <summary>
        /// ファイル関連オプションの検証と設定
        /// </summary>
        private void ValidateAndSetFileOptions()
        {
            // 検索単語リストファイルオプションの設定
            if (this.IsOption(rmsmf.OptionConstants.OptionSearchWords))
            {
                if (this.Options[rmsmf.OptionConstants.OptionSearchWords] == Colipex.NonValue)
                {
                    throw new RmsmfException(string.Format(rmsmf.ValidationMessages.MissingOptionFileName, "s"));
                }

                this._searchWordsFileName = this.Options[rmsmf.OptionConstants.OptionSearchWords].TrimEnd(new char[] { '\x0a', '\x0d' });

                if (!File.Exists(this._searchWordsFileName))
                {
                    throw new RmsmfException(string.Format(rmsmf.ValidationMessages.FileNotFound, this._searchWordsFileName));
                }
            }

            // ファイルリストファイルオプションの設定
            if (this.IsOption(rmsmf.OptionConstants.OptionFileNameList))
            {
                if (this.Options[rmsmf.OptionConstants.OptionFileNameList] == Colipex.NonValue)
                {
                    throw new RmsmfException(string.Format(rmsmf.ValidationMessages.MissingOptionFileName, "f"));
                }

                this._fileNameListFileName = this.Options[rmsmf.OptionConstants.OptionFileNameList].TrimEnd(new char[] { '\x0a', '\x0d' });

                if (!File.Exists(this._fileNameListFileName))
                {
                    throw new RmsmfException(string.Format(rmsmf.ValidationMessages.FileNotFound, this._fileNameListFileName));
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
            ValidateSearchWordsHasTargetFiles();
            ValidateEncodingOptionsDependencies();
        }

        /// <summary>
        /// 必須パラメータが最低1つ指定されているか検証
        /// </summary>
        private void ValidateRequiredParametersProvided()
        {
            rmsmf.OptionValidator.ValidateAtLeastOneCondition(
                this.IsOption(rmsmf.OptionConstants.OptionSearchWords),
                this.IsOption(rmsmf.OptionConstants.OptionFileNameList),
                this.Parameters.Count > 0
            );
        }

        /// <summary>
        /// ファイル指定方法が競合していないか検証
        /// </summary>
        private void ValidateFileSpecificationMethod()
        {
            rmsmf.OptionValidator.ValidateFileSpecificationNotConflicting(
                this.IsOption(rmsmf.OptionConstants.OptionFileNameList),
                this.Parameters.Count
            );
        }

        /// <summary>
        /// 検索単語オプション使用時に対象ファイルが指定されているか検証
        /// </summary>
        private void ValidateSearchWordsHasTargetFiles()
        {
            if (this.IsOption(rmsmf.OptionConstants.OptionSearchWords) && 
                this.IsOption(rmsmf.OptionConstants.OptionFileNameList) == false && 
                this.Parameters.Count == 0)
            {
                throw new RmsmfException(rmsmf.ValidationMessages.SearchWordsRequiresTargetFiles);
            }
        }

        /// <summary>
        /// エンコーディング関連オプションの依存関係を検証
        /// </summary>
        private void ValidateEncodingOptionsDependencies()
        {
            // 検索単語ファイルのエンコーディングは、検索単語ファイルオプションが必要
            rmsmf.OptionValidator.ValidateEncodingOptionDependency(
                this.IsOption(rmsmf.OptionConstants.OptionSearchWords),
                this.IsOption(rmsmf.OptionConstants.OptionSearchWordsCharacterSet),
                rmsmf.ValidationMessages.SearchWordsEncodingWithoutSearchWords
            );

            // ファイルリストのエンコーディングは、ファイルリスト(/f)または出力ファイルリスト(/o)オプションが必要
            if (this.IsOption(rmsmf.OptionConstants.OptionFileNameListCharacterSet))
            {
                if (!this.IsOption(rmsmf.OptionConstants.OptionFileNameList) && 
                    !this.IsOption(rmsmf.OptionConstants.OptionOutputFileNamelist))
                {
                    throw new RmsmfException(rmsmf.ValidationMessages.FileListEncodingWithoutFileList);
                }
            }
        }

        /// <summary>
        /// 検索単語テーブル初期化
        /// </summary>
        /// <returns>true=正常に初期化した</returns>
        public bool ReadSearchWords()
        {
            if (string.IsNullOrEmpty(this._searchWordsFileName))
            {
                return false;
            }

            // エンコーディングの判定と設定
            EnsureEncodingInitialized(
                ref this._replaceEncoding, 
                this._searchWordsFileName);

            // ファイルから行を読み込み
            List<string> lines = LoadSearchWordsFromFile();

            // 空チェック
            if (lines.Count == 0)
            {
                throw new RmsmfException(string.Format(rmsmf.ValidationMessages.EmptySearchWords, this._searchWordsFileName));
            }

            // 検索単語テーブルへ登録
            ParseAndStoreSearchWords(lines);

            return true;
        }

        /// <summary>
        /// 検索単語リストファイルから行を読み込む
        /// </summary>
        /// <returns>読み込んだ行のリスト</returns>
        private List<string> LoadSearchWordsFromFile()
        {
            List<string> lines = new List<string>();

            using (var reader = new StreamReader(this._searchWordsFileName, this.ReplaceEncoding, true))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    line = this.ConvertEscapeSequences(line);

                    if (line.Length == 0) continue;

                    lines.Add(line);
                }
            }

            return lines;
        }

        /// <summary>
        /// 読み込んだ行をパースして検索単語テーブルに格納
        /// </summary>
        /// <param name="lines">読み込んだ行のリスト</param>
        private void ParseAndStoreSearchWords(List<string> lines)
        {
            this._searchWordsCount = lines.Count;
            this._searchWords = new string[this._searchWordsCount];

            for (int i = 0; i < this._searchWordsCount; i++)
            {
                this._searchWords[i] = lines[i];
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
                    if (this._searchOptionAllDirectories == true)
                    {
                        searchOption = System.IO.SearchOption.AllDirectories;
                    }

                    this._files = Directory.GetFileSystemEntries(directoryName, searchWord, searchOption);
                }
                catch(System.ArgumentException ex)
                {
                    throw new RmsmfException(path + " が存在しないか、検索キーワードとして無効です。", ex);
                }

                normal = true;
                return normal;
            }

            //ファイル名リストファイルが有る場合

            if(this.FilesEncoding == null)
            {
                //ファイル名リストファイルの文字エンコーディングを判定する。
                EncodingDetector encDetec = new EncodingDetector(0);
                EncodingInfomation encInfo = encDetec.Detection(this._fileNameListFileName);

                if (encInfo.CodePage > 0)
                {
                    this.FilesEncoding = Encoding.GetEncoding(encInfo.CodePage);
                }
                else
                {
                    throw new RmsmfException(string.Format(rmsmf.ValidationMessages.UnknownEncoding, this._fileNameListFileName));
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
        /// 検索単語テーブル
        /// </summary>
        private string[] _searchWords;

        /// <summary>
        /// 検索単語テーブルプロパティ
        /// </summary>
        public string[] SearchWords
        {
            get { return this._searchWords; }
        }

        /// <summary>
        /// 検索単語件数
        /// </summary>
        private int _searchWordsCount;

        /// <summary>
        /// 検索単語件数プロパティ
        /// </summary>
        public int SearchWordsCount
        {
            get { return this._searchWordsCount; }
        }

        /// <summary>
        /// 検索対象ファイル名一覧
        /// </summary>
        private string[] _files = null;

        /// <summary>
        /// 検索対象ファイル名一覧プロパティ
        /// </summary>
        public string[] Files
        {
            get { return this._files; }
        }


        /// <summary>
        /// Probe Mode が有効です
        /// </summary>
        private bool _enableProbe = false;

        /// <summary>
        /// Probe Mode が有効です。プロパティ
        /// </summary>
        public bool EnableProbe
        {
            get { return this._enableProbe; }
        }


        /// <summary>
        /// 検索単語リストCSVのファイル名
        /// </summary>
        private string _searchWordsFileName = null;

        /// <summary>
        /// 検索単語リストCSVのファイル名
        /// </summary>
        public string SearchWordsFileName
        {
            get { return this._searchWordsFileName; }
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
        /// 出力ファイルリストのファイル名
        /// </summary>
        private string _outputFileNameListFileName = null;

        /// <summary>
        /// 出力ファイルリストのファイル名
        /// </summary>
        public string OutputFileNameListFileName
        {
            get { return this._outputFileNameListFileName; }
        }

        /// <summary>
        /// カルチャー情報
        /// </summary>
        public string CultureInfo
        {
            get { return this._cultureInfo; }
        }

        /// <summary>
        /// ヘルプまたはバージョンが表示されたかどうか
        /// </summary>
        public bool HelpOrVersionDisplayed
        {
            get { return this._helpOrVersionDisplayed; }
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
        /// 検索単語リストCSV文字エンコーディング
        /// </summary>
        private Encoding _replaceEncoding = null;

        /// <summary>
        /// 検索単語リストCSV文字エンコーディング
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
        private EncodingDetectionType _encodingDetectionMode = EncodingDetectionType.Normal;
        /// <summary>
        /// 文字エンコーディングの自動判定モード
        /// </summary>
        public EncodingDetectionType EncodingDetectionMode
        {
            get { return _encodingDetectionMode; }
            private set { _encodingDetectionMode = value; }
        }

        /// <summary>
        /// 文字エンコーディングの自動判定モードタイプ
        /// </summary>
        public enum EncodingDetectionType
        {
            Normal = 0,
            FirstParty = 1,
            ThirdParty = 3
        }
    }
}
