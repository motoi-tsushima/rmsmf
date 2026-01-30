using System;

namespace rmsmf
{
    /// <summary>
    /// コマンドラインオプション定数
    /// rmsmf と txprobe で共通使用する定数を定義
    /// </summary>
    public static class OptionConstants
    {
        /// <summary>エンコーディング自動判定を表す文字列</summary>
        public const string CharacterSetDetection = "Detection";

        // ===== オプションキー =====

        /// <summary>ヘルプオプション</summary>
        public const string OptionHelp = "h";

        /// <summary>バージョンオプション</summary>
        public const string OptionVersion = "v";

        /// <summary>読み取り文字エンコーディングオプション</summary>
        public const string OptionCharacterSet = "c";

        /// <summary>書き込み文字エンコーディングオプション</summary>
        public const string OptionWriteCharacterSet = "w";

        /// <summary>ファイルリストオプション</summary>
        public const string OptionFileNameList = "f";

        /// <summary>ファイルリスト文字エンコーディングオプション</summary>
        public const string OptionFileNameListCharacterSet = "fc";

        /// <summary>置換単語リストオプション</summary>
        public const string OptionReplaceWords = "r";

        /// <summary>置換単語リスト文字エンコーディングオプション</summary>
        public const string OptionReplaceWordsCharacterSet = "rc";

        /// <summary>検索単語リストオプション</summary>
        public const string OptionSearchWords = "s";

        /// <summary>検索単語リスト文字エンコーディングオプション</summary>
        public const string OptionSearchWordsCharacterSet = "sc";

        /// <summary>BOM書き込みオプション</summary>
        public const string OptionWriteByteOrderMark = "b";

        /// <summary>全ディレクトリ検索オプション</summary>
        public const string OptionAllDirectories = "d";

        /// <summary>改行コードオプション</summary>
        public const string OptionNewLine = "nl";

        /// <summary>エンコーディング判定モードオプション</summary>
        public const string OptionDetectionMode = "det";

        /// <summary>カルチャー情報オプション</summary>
        public const string OptionCultureInfo = "ci";

        /// <summary>プローブモードオプション</summary>
        public const string OptionProbeMode = "p";

        /// <summary>出力ファイルリストオプション</summary>
        public const string OptionOutputFileNamelist = "o";
    }
}
