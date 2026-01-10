using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace txprobe
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
            ExecutionState.className = "Colipex.Colipex";
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
                ExecutionState.isError = true;
                ExecutionState.isNormal = !ExecutionState.isError;
                ExecutionState.errorMessage = "同じオプションが複数回入力されています。";
                throw ex;
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
    }
}
