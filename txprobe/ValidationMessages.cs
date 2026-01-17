using System;

namespace rmsmf
{
    /// <summary>
    /// 検証エラーメッセージの定数クラス
    /// </summary>
    public static class ValidationMessages
    {
        // 必須パラメータ関連
        public const string MissingRequiredParameters = "必須パラメータが入力されていません。";
        public const string MissingTargetFileName = "目的のファイル名を指定してください。(/h ヘルプ表示)";

        // ファイル指定関連
        public const string ConflictingFileSpecificationMethods = "/f:オプションによるファイル指定と、コマンドラインでのファイル指定を、同時に使用する事はできません。";
        public const string ReplaceWordsRequiresTargetFiles = "置換対象となるファイルを指定してください。";
        public const string SearchWordsRequiresTargetFiles = "対象となるファイルを指定してください。";

        // エンコーディング関連
        public const string MissingEncodingName = "文字エンコーディング名を指定してください。 (/{0})";
        public const string InvalidEncodingName = "エンコーディング名が不正です。";
        public const string UnsupportedEncoding = "サポートされていないエンコーディングです。";
        public const string UnknownEncoding = "{0}の文字エンコーディングが分かりません。";

        // エンコーディングオプション依存関係
        public const string ReplaceWordsEncodingWithoutReplaceWords = "置換単語ファイルが指定されていないのに、置換単語ファイルのエンコーディングが指定されています。";
        public const string SearchWordsEncodingWithoutSearchWords = "検索単語ファイルが指定されていないのに、検索単語ファイルのエンコーディングが指定されています。";
        public const string FileListEncodingWithoutFileList = "ファイルリストが指定されていないのに、ファイルリストのエンコーディングが指定されています。";

        // 変換モード関連
        public const string ConversionRequiresOutputEncoding = "文字エンコーディングの変換をする場合は、/w:により出力先の文字エンコーディングを指定してください。";

        // ファイル存在関連
        public const string FileNotFound = "{0} が存在しません。 ";
        public const string MissingOptionFileName = "/{0} オプションのファイル名を指定してください。";

        // ファイル内容関連
        public const string EmptyReplaceWords = "{0}の置換?語がゼロ件です。";
        public const string EmptySearchWords = "{0}の検索単語がゼロ件です。";
        public const string InvalidReplaceWordFormat = "置換単語ファイルの{0}行目が不正です。カンマ区切りで「検索文字列,置換文字列」の形式で指定してください。";
    }
}
