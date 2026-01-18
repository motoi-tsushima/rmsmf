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
        private const string CharacterSetJudgment = "Judgment";
        private const string OptionHelp = "h";
        private const string OptionCharacterSet = "c";
        private const string OptionFileNameList = "f";
        private const string OptionFileNameListCharacterSet = "fc";
        private const string OptionSearchWords = "s";
        private const string OptionSearchWordsCharacterSet = "sc";
        private const string OptionProbeMode = "p";
        private const string OptionAllDirectories = "d";
        private const string OptionOutputFileNamelist = "o";

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
                out string searchWordsCharacterSet,
                out string filesCharacterSet);

            this._enableProbe = ParseProbeModeOption();
            this.searchOptionAllDirectories = ParseAllDirectoriesOption();
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
            if (this.IsOption(OptionFileNameList) == false)
            {
                if (this.Parameters.Count == 0 && this.Options.Count == 0)
                {
                    throw new RmsmfException(rmsmf.ValidationMessages.MissingTargetFileName);
                }
            }
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
            if (this.IsOption(OptionCharacterSet))
            {
                readCharacterSet = this.Options[OptionCharacterSet].TrimEnd(new char[] { '\x0a', '\x0d' });
                if (readCharacterSet == Colipex.NonValue)
                {
                    throw new RmsmfException(string.Format(rmsmf.ValidationMessages.MissingEncodingName, "c"));
                }
            }
            else
            {
                readCharacterSet = CharacterSetJudgment;
            }

            //Setting SearchWords CharacterSet 
            //検索単語リストの文字エンコーディングの設定する。
            if (this.IsOption(OptionSearchWordsCharacterSet))
            {
                searchWordsCharacterSet = this.Options[OptionSearchWordsCharacterSet].TrimEnd(new char[] { '\x0a', '\x0d' });
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
            if (this.IsOption(OptionFileNameListCharacterSet))
            {
                filesCharacterSet = this.Options[OptionFileNameListCharacterSet].TrimEnd(new char[] { '\x0a', '\x0d' });
                if (filesCharacterSet == Colipex.NonValue)
                {
                    throw new RmsmfException(string.Format(rmsmf.ValidationMessages.MissingEncodingName, "fc"));
                }
            }
            else
            {
                filesCharacterSet = readCharacterSet;
            }
        }

        /// <summary>
        /// Probemodeオプションの解析
        /// </summary>
        /// <returns>Probemodeが有効かどうか</returns>
        private bool ParseProbeModeOption()
        {
            if (this.IsOption(OptionProbeMode))
            {
                return true;
            }
            else
            {
                // 検索文字列が無い場合は Probe Mode を有効にする
                return this.IsOption(OptionSearchWords) == false;
            }
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
        /// 出力ファイルリストオプションの解析
        /// </summary>
        /// <returns>出力ファイルリストのファイル名（null=指定なし）</returns>
        private string ParseOutputFileNameListOption()
        {
            if (this.IsOption(OptionOutputFileNamelist))
            {
                if (this.Options[OptionOutputFileNamelist] == Colipex.NonValue)
                {
                    return "output_filelist.txt";
                }
                else
                {
                    return this.Options[OptionOutputFileNamelist].TrimEnd(new char[] { '\x0a', '\x0d' });
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
                this.ReadEncoding = ResolveEncoding(readCharacterSet, CharacterSetJudgment);
                this.ReplaceEncoding = ResolveEncoding(searchWordsCharacterSet, CharacterSetJudgment);
                this.FilesEncoding = ResolveEncoding(filesCharacterSet, CharacterSetJudgment);
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
            if (this.IsOption(OptionSearchWords))
            {
                if (this.Options[OptionSearchWords] == Colipex.NonValue)
                {
                    throw new RmsmfException(string.Format(rmsmf.ValidationMessages.MissingOptionFileName, "s"));
                }

                this._searchWordsFileName = this.Options[OptionSearchWords].TrimEnd(new char[] { '\x0a', '\x0d' });

                if (!File.Exists(this._searchWordsFileName))
                {
                    throw new RmsmfException(string.Format(rmsmf.ValidationMessages.FileNotFound, this._searchWordsFileName));
                }
            }

            // ファイルリストファイルオプションの設定
            if (this.IsOption(OptionFileNameList))
            {
                if (this.Options[OptionFileNameList] == Colipex.NonValue)
                {
                    throw new RmsmfException(string.Format(rmsmf.ValidationMessages.MissingOptionFileName, "f"));
                }

                this._fileNameListFileName = this.Options[OptionFileNameList].TrimEnd(new char[] { '\x0a', '\x0d' });

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
                this.IsOption(OptionSearchWords),
                this.IsOption(OptionFileNameList),
                this.Parameters.Count > 0
            );
        }

        /// <summary>
        /// ファイル指定方法が競合していないか検証
        /// </summary>
        private void ValidateFileSpecificationMethod()
        {
            rmsmf.OptionValidator.ValidateFileSpecificationNotConflicting(
                this.IsOption(OptionFileNameList),
                this.Parameters.Count
            );
        }

        /// <summary>
        /// 検索単語オプション使用時に対象ファイルが指定されているか検証
        /// </summary>
        private void ValidateSearchWordsHasTargetFiles()
        {
            if (this.IsOption(OptionSearchWords) && 
                this.IsOption(OptionFileNameList) == false && 
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
                this.IsOption(OptionSearchWords),
                this.IsOption(OptionSearchWordsCharacterSet),
                rmsmf.ValidationMessages.SearchWordsEncodingWithoutSearchWords
            );

            // ファイルリストのエンコーディングは、ファイルリスト(/f)または出力ファイルリスト(/o)オプションが必要
            if (this.IsOption(OptionFileNameListCharacterSet))
            {
                if (!this.IsOption(OptionFileNameList) && !this.IsOption(OptionOutputFileNamelist))
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
                this._searchWordsFileName, 
                rmsmf.ValidationMessages.UnknownEncoding);

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
                    if (this.searchOptionAllDirectories == true)
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
                EncodingJudgment encJudg = new EncodingJudgment(0);
                EncodingInfomation encInfo = encJudg.Judgment(this._fileNameListFileName);

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


    }
}
