using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace rmsmf
{
    /// <summary>
    /// Command line parameter extraction. (Co li p ex)
    /// </summary>
    public class Colipex
    {
        public const string NonValue = "NonValueOption";
        public const char OptionSeparatorColon = ':';
        public const char OptionSeparatorEqual = '=';

        public Colipex(string[] args)
        {
            char optionSeparator = (char)0;

            try
            {
                this.options = new Dictionary<string, string>();
                this.parameters = new List<string>();

                foreach(string parameter in args)
                {
                    if(parameter[0] == '-' || parameter[0] == '/')
                    {
                        string optionWord = parameter.Substring(1).TrimEnd(new char[] { '\x0a', '\x0d' });
                        if(optionWord.Contains(OptionSeparatorColon))
                        {
                            optionSeparator = OptionSeparatorColon;
                        }
                        else if(optionWord.Contains(OptionSeparatorEqual))
                        {
                            optionSeparator = OptionSeparatorEqual;
                        }
                        else
                        {
                            optionSeparator = OptionSeparatorColon;
                        }
                        string[] optionValue = optionWord.Split(optionSeparator);
                        if(optionValue.Length == 1)
                        {
                            this.options.Add(optionWord, NonValue);
                        }
                        else if(1 < optionValue.Length)
                        {
                            this.options.Add(optionValue[0], optionValue[1]);
                        }
                    }
                    else
                    {
                        this.parameters.Add(parameter);
                    }
                }

                this.args = this.parameters.ToArray();
            }
            catch(ArgumentException ex)
            {
                throw new RmsmfException("同じオプションが複数回入力されています。", ex);
            }
        }

        private Dictionary<string, string> options;

        private string[] args;

        private List<string> parameters;

        public Dictionary<string, string> Options
        {
            get
            {
                return this.options;
            }
        }

        public bool IsOption(string optionKey)
        {
            return this.options.ContainsKey(optionKey);
        }

        public string[] Args
        {
            get
            {
                return this.args;
            }
        }

        public List<string> Parameters
        {
            get
            {
                return this.parameters;
            }
        }

        /// <summary>
        /// エスケープシーケンス変換
        /// </summary>
        /// <param name="input">変換対象文字列</param>
        /// <returns>変換後文字列</returns>
        protected string ConvertEscapeSequences(string input)
        {
            return input
                .Replace("\\r\\n", "\r\n")
                .Replace("\\r", "\r")
                .Replace("\\n", "\n")
                .Replace("\\t", "\t")
                .Replace("\\\\", "\\");
        }

        /// <summary>
        /// エンコーディング名またはコードページからEncodingオブジェクトを取得
        /// </summary>
        /// <param name="encodingNameOrCodePage">エンコーディング名またはコードページ</param>
        /// <returns>Encodingオブジェクト、"Judgment"の場合はnull</returns>
        protected Encoding ResolveEncoding(string encodingNameOrCodePage, string judgmentKeyword = "Judgment")
        {
            if (encodingNameOrCodePage == judgmentKeyword)
            {
                return null;
            }

            if (int.TryParse(encodingNameOrCodePage, out int codePage))
            {
                return Encoding.GetEncoding(codePage);
            }

            return Encoding.GetEncoding(encodingNameOrCodePage);
        }

        /// <summary>
        /// エンコーディングが未設定の場合、ファイルから判定して設定する
        /// </summary>
        /// <param name="encoding">エンコーディング（参照渡し）</param>
        /// <param name="fileName">判定対象のファイル名</param>
        protected void EnsureEncodingInitialized(ref Encoding encoding, string fileName)
        {
            if (encoding == null)
            {
                EncodingJudgment encJudg = new EncodingJudgment(0);
                EncodingInfomation encInfo = encJudg.Judgment(fileName);

                if (encInfo.CodePage > 0)
                {
                    encoding = Encoding.GetEncoding(encInfo.CodePage);
                }
                else
                {
                    // string.Formatを使わず、直接文字列連結
                    throw new RmsmfException(fileName + "の文字エンコーディングが分かりません。");
                }
            }
        }

        /// <summary>
        /// ファイルから行を読み込み、エスケープシーケンスを変換する
        /// </summary>
        /// <param name="fileName">読み込むファイル名</param>
        /// <param name="encoding">使用するエンコーディング</param>
        /// <returns>読み込んだ行のリスト</returns>
        protected List<string> LoadLinesFromFile(string fileName, Encoding encoding)
        {
            List<string> lines = new List<string>();

            using (var reader = new StreamReader(fileName, encoding, true))
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
    }
}
