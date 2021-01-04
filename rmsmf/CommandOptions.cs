using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace rmsmf
{
    public class CommandOptions : Colipex
    {
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
        }

        private string[] _files = null;
        /// <summary>
        /// 置換対象ファイル名一覧
        /// </summary>
        public string[] Files
        {
            get { return this._files; }
        }
        public void SetFiles(string[] files)
        {
            this._files = files;
        }

        private bool _enableBOM = false;
        /// <summary>
        /// 書き込みBOM指定が有効です
        /// </summary>
        public bool EnableBOM
        {
            get { return this._enableBOM; }
        }
        public void SetEnableBOM(bool bomEnable)
        {
            this._enableBOM = bomEnable;
        }

        private string _replaceWordsFileName = null;
        /// <summary>
        /// 置換単語リストCSVのファイル名
        /// </summary>
        public string ReplaceWordsFileName
        {
            get { return this._replaceWordsFileName; }
        }

        private string _fileNameListFileName = null;
        /// <summary>
        /// ファイルリストのファイル名
        /// </summary>
        public string FileNameListFileName
        {
            get { return this._fileNameListFileName; }
        }


        public Encoding encoding = null;
        public Encoding writeEncoding = null;
        public Encoding repleaseEncoding = null;
        public Encoding filesEncoding = null;


    }
}
