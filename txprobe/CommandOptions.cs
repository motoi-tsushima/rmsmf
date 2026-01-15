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
            ExecutionState.className = "CommandOptions.CommandOptions";

            string readCharacterSet;
            string searchWordsCharacterSet;
            string filesCharacterSet;

            string errorEncoding = null;

            //ヘルプオプションの場合
            if (this.IsOption(OptionHelp) == true)
            {
                this._callHelp = true;
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

                //Setting SearchWords CharacterSet 
                //検索単語リストの文字エンコーディングの設定する。
                if (this.IsOption(OptionSearchWordsCharacterSet) == true)
                {
                    searchWordsCharacterSet = this.Options[OptionSearchWordsCharacterSet].TrimEnd(new char[] { '\x0a', '\x0d' });
                    if (searchWordsCharacterSet == Colipex.NonValue)
                    {
                        //Console.WriteLine("Please specify the encoding name. (/rc)");
                        Console.WriteLine("文字エンコーディング名を指定してください。 (/rc)");
                        return;
                    }
                }
                else
                {
                    searchWordsCharacterSet = readCharacterSet;
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

                //Probe Mode
                if (this.IsOption(OptionProbeMode) == true)
                {
                    this._enableProbe = true;
                }
                else
                {
                    if (this.IsOption(OptionSearchWords) == false)
                    {
                        // 検索文字列が無い場合は Probe Mode を有効にする
                        this._enableProbe = true;
                    }
                    else
                    {
                        this._enableProbe = false;
                    }
                }

                // AllDirectories を有効にする
                if (this.IsOption(OptionAllDirectories) == true)
                {
                    this.searchOptionAllDirectories = true;
                }
                else
                {
                    this.searchOptionAllDirectories = false;
                }

                // 出力ファイルリスト・オプションを設定する
                if(this.IsOption(OptionOutputFileNamelist) == true)
                {
                    if (this.Options[OptionOutputFileNamelist] == Colipex.NonValue)
                    {
                        this._outputFileNameListFileName = "output_filelist.txt";
                    }
                    else 
                    {                         //出力ファイルリストのファイル名を保存する
                        this._outputFileNameListFileName = this.Options[OptionOutputFileNamelist].TrimEnd(new char[] { '\x0a', '\x0d' });
                    }
                }

                //-----------------------------------------------------------
                //Setting Encoding and Check error of Encoding
                //エンコーディングの設定とエンコーディングのエラーの確認をする。
                //-
                int codePage;
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


                //Setting Replease Encoding
                errorEncoding = "Replease Encoding";
                if (searchWordsCharacterSet == CharacterSetJudgment)
                    this.repleaseEncoding = null;
                else if (int.TryParse(searchWordsCharacterSet, out repleaseCodePage))
                    this.repleaseEncoding = Encoding.GetEncoding(repleaseCodePage);
                else
                    this.repleaseEncoding = Encoding.GetEncoding(searchWordsCharacterSet);

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
                if(errorEncoding == null)
                    ExecutionState.errorMessage = "管理下のエラー:ArgumentException" + errorEncoding;
                else
                {
                    ExecutionState.errorMessage = errorEncoding + " のエンコーディング名が不正です。";
                }

                throw ex;
            }
            catch (Exception ex)
            {
                ExecutionState.isNormal = false;
                throw ex;
            }

            //------------------------------------------------------------
            // 検索単語リストファイルオプションの設定
            //-
            //検索単語リストが存在する。
            if (this.IsOption(OptionSearchWords) == true)
            {
                if (this.Options[OptionSearchWords] == Colipex.NonValue)
                {
                    ExecutionState.isError = true;
                    ExecutionState.isNormal = !ExecutionState.isError;
                    ExecutionState.errorMessage = "/r オプションのファイル名を指定してください。";

                    throw new Exception(ExecutionState.errorMessage);
                }

                //検索単語リストファイル名を保存する
                this._searchWordsFileName = this.Options[OptionSearchWords].TrimEnd(new char[] { '\x0a', '\x0d' }); 

                //検索単語リストファイル名の存在確認
                if (!File.Exists(this._searchWordsFileName))
                {
                    ExecutionState.isError = true;
                    ExecutionState.isNormal = !ExecutionState.isError;
                    ExecutionState.errorMessage = this._searchWordsFileName + " が存在しません。 ";

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
                this._fileNameListFileName = this.Options[OptionFileNameList].TrimEnd(new char[] { '\x0a', '\x0d' }); 

                if (!File.Exists(this._fileNameListFileName))
                {
                    ExecutionState.isError = true;
                    ExecutionState.isNormal = !ExecutionState.isError;
                    ExecutionState.errorMessage = this._fileNameListFileName + " が存在しません。 ";

                    throw new Exception(ExecutionState.errorMessage);
                }
            }

            //------------------------------------------------------------
            // オプションの正当性確認
            //-
            if(this.IsOption(OptionSearchWords) == false && this.IsOption(OptionFileNameList) == false && this.Parameters.Count == 0)
            {
                ExecutionState.isError = true;
                ExecutionState.isNormal = !ExecutionState.isError;
                ExecutionState.errorMessage = "必須パラメータが入力されていません。";

                throw new Exception(ExecutionState.errorMessage);
            }

            if (this.IsOption(OptionFileNameList) == true && this.Parameters.Count > 0)
            {
                ExecutionState.isError = true;
                ExecutionState.isNormal = !ExecutionState.isError;
                ExecutionState.errorMessage = "/f:オプションによるファイル指定と、コマンドラインでのファイル指定を、同時に使用する事はできません。";

                throw new Exception(ExecutionState.errorMessage);
            }

            if (this.IsOption(OptionSearchWords) == true && this.IsOption(OptionFileNameList) == false && this.Parameters.Count == 0)
            {
                ExecutionState.isError = true;
                ExecutionState.isNormal = !ExecutionState.isError;
                ExecutionState.errorMessage = "対象となるファイルを指定してください。";

                throw new Exception(ExecutionState.errorMessage);
            }

            if(this.IsOption(OptionSearchWords) == false && this.IsOption(OptionSearchWordsCharacterSet) == true)
            {
                ExecutionState.isError = true;
                ExecutionState.isNormal = !ExecutionState.isError;
                ExecutionState.errorMessage = "検索単語ファイルが指定されていないのに、検索単語ファイルのエンコーディングが指定されています。";

                throw new Exception(ExecutionState.errorMessage);
            }

            if (this.IsOption(OptionFileNameList) == false && this.IsOption(OptionFileNameListCharacterSet) == true)
            {
                ExecutionState.isError = true;
                ExecutionState.isNormal = !ExecutionState.isError;
                ExecutionState.errorMessage = "ファイルリストが指定されていないのに、ファイルリストのエンコーディングが指定されています。";

                throw new Exception(ExecutionState.errorMessage);
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
        /// 検索単語テーブル初期化
        /// </summary>
        /// <returns>true=正常に初期化した</returns>
        public bool ReadSearchWords()
        {
            bool normal = true;

            if (string.IsNullOrEmpty(this._searchWordsFileName))
            {
                normal = false;
                return normal;
            }

            //検索単語の設定をする。
            List<string> wordsList = new List<string>();

            if(this.repleaseEncoding == null)
            {
                //検索単語リストファイルの文字エンコーディングを判定する。
                EncodingJudgment encJudg = new EncodingJudgment(0);
                EncodingInfomation encInfo = encJudg.Judgment(this._searchWordsFileName);

                if(encInfo.codePage > 0)
                {
                    this.repleaseEncoding = Encoding.GetEncoding(encInfo.codePage);
                }
                else
                {
                    ExecutionState.isError = true;
                    ExecutionState.isNormal = !ExecutionState.isError;
                    ExecutionState.errorMessage = this._searchWordsFileName + "の文字エンコーディングが分かりません。";

                    throw new Exception(ExecutionState.errorMessage);
                }
            }

            //Read searchment word CSV file
            //検索単語リストCSVを読み取る。
            using (var reader = new StreamReader(this._searchWordsFileName, this.repleaseEncoding, true))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    line = this.ConvertEscapeSequences(line);

                    if (line.Length == 0) continue;
                    //if (line.IndexOf(',') < 0) continue;

                    wordsList.Add(line);
                }
            }

            if(wordsList.Count == 0)
            {
                ExecutionState.isError = true;
                ExecutionState.isNormal = !ExecutionState.isError;
                ExecutionState.errorMessage = this._searchWordsFileName + "の検索単語がゼロ件です。";

                throw new Exception(ExecutionState.errorMessage);
            }

            //検索単語テーブルへ登録する。
            this._searchWordsCount = wordsList.Count;
            this._searchWords = new string[this._searchWordsCount];
            for (int i = 0; i < this._searchWordsCount; i++)
            {
                this._searchWords[i] = wordsList[i];
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
                if (direcrtoryName != null) 
                { 
                    if (direcrtoryName.Length == 0)
                    {
                        direcrtoryName = ".";
                    }
                    else if (direcrtoryName.Length == 1)
                    {
                        if (direcrtoryName[0] != '.' && direcrtoryName[0] != '\\')
                        {
                            direcrtoryName = ".\\" + direcrtoryName;
                        }
                    }
                    else if (direcrtoryName.Length > 1)
                    {
                        if (direcrtoryName[0] != '.' && direcrtoryName[0] != '\\'
                            && direcrtoryName[1] != ':')
                        {
                            direcrtoryName = ".\\" + direcrtoryName;
                        }
                    }
                }
                else
                {
                    direcrtoryName = ".";
                }

                string searchWord = Path.GetFileName(path);

                try
                {
                    System.IO.SearchOption searchOption = System.IO.SearchOption.TopDirectoryOnly;
                    if (this.searchOptionAllDirectories == true)
                    {
                        searchOption = System.IO.SearchOption.AllDirectories;
                    }

                    this._files = Directory.GetFileSystemEntries(direcrtoryName, searchWord, searchOption);
                }
                catch(System.ArgumentException ex)
                {
                    ExecutionState.isError = true;
                    ExecutionState.isNormal = !ExecutionState.isError;
                    ExecutionState.errorMessage = path + " が存在しないか、検索キーワードとして無効です。";
                    ExecutionState.className = "CommandOptions.ReadFileNameList";
                    throw ex;
                }

                normal = true;
                return normal;
            }

            //ファイル名リストファイルが有る場合

            if(this.filesEncoding == null)
            {
                //ファイル名リストファイルの文字エンコーディングを判定する。
                EncodingJudgment encJudg = new EncodingJudgment(0);
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
        /// 検索単語リストCSV文字エンコーディング
        /// </summary>
        public Encoding repleaseEncoding = null;

        /// <summary>
        /// ファイルリスト文字エンコーディング
        /// </summary>
        public Encoding filesEncoding = null;


    }
}
