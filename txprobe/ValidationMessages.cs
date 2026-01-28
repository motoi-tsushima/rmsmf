using System;

namespace rmsmf
{
    /// <summary>
    /// 多言語対応検証エラーメッセージ（txprobe用）
    /// </summary>
    public static class ValidationMessages
    {
        /// <summary>
        /// 現在の言語コードを取得
        /// </summary>
        private static string LanguageCode => HelpMessages.GetLanguageCode();

        // 必須パラメータ関連
        public static string MissingRequiredParameters => GetMessage("MissingRequiredParameters");
        public static string MissingTargetFileName => GetMessage("MissingTargetFileName");

        // ファイル指定関連
        public static string ConflictingFileSpecificationMethods => GetMessage("ConflictingFileSpecificationMethods");
        public static string ReplaceWordsRequiresTargetFiles => GetMessage("ReplaceWordsRequiresTargetFiles");
        public static string SearchWordsRequiresTargetFiles => GetMessage("SearchWordsRequiresTargetFiles");

        // エンコーディング関連
        public static string MissingEncodingName => GetMessage("MissingEncodingName");
        public static string InvalidEncodingName => GetMessage("InvalidEncodingName");
        public static string UnsupportedEncoding => GetMessage("UnsupportedEncoding");
        public static string UnknownEncoding => GetMessage("UnknownEncoding");

        // エンコーディングオプション依存関係
        public static string ReplaceWordsEncodingWithoutReplaceWords => GetMessage("ReplaceWordsEncodingWithoutReplaceWords");
        public static string SearchWordsEncodingWithoutSearchWords => GetMessage("SearchWordsEncodingWithoutSearchWords");
        public static string FileListEncodingWithoutFileList => GetMessage("FileListEncodingWithoutFileList");

        // 変換モード関連
        public static string ConversionRequiresOutputEncoding => GetMessage("ConversionRequiresOutputEncoding");

        // ファイル存在関連
        public static string FileNotFound => GetMessage("FileNotFound");
        public static string MissingOptionFileName => GetMessage("MissingOptionFileName");

        // ファイル内容関連
        public static string EmptyReplaceWords => GetMessage("EmptyReplaceWords");
        public static string EmptySearchWords => GetMessage("EmptySearchWords");
        public static string InvalidReplaceWordFormat => GetMessage("InvalidReplaceWordFormat");

        // カルチャー情報関連
        public static string InvalidCultureInfo => GetMessage("InvalidCultureInfo");
        
        // オプション用エンコーディング名
        public static string MissingEncodingNameForOption => GetMessage("MissingEncodingNameForOption");

        // エラー処理関連
        public static string ErrorsOccurred => GetMessage("ErrorsOccurred");
        public static string OtherFilesProcessedSuccessfully => GetMessage("OtherFilesProcessedSuccessfully");
        public static string SearchComplete => GetMessage("SearchComplete");
        public static string UnexpectedErrorOccurred => GetMessage("UnexpectedErrorOccurred");

        /// <summary>
        /// メッセージを取得
        /// </summary>
        private static string GetMessage(string key)
        {
            string lang = LanguageCode;
            
            switch (key)
            {
                case "MissingRequiredParameters":
                    switch (lang)
                    {
                        case "ja": return "必須パラメータが入力されていません。";
                        case "ko": return "필수 매개변수가 입력되지 않았습니다.";
                        case "zh-CN": return "未输入必需参数。";
                        case "zh-TW": return "未輸入必要參數。";
                        default: return "Required parameters are not specified.";
                    }

                case "MissingTargetFileName":
                    switch (lang)
                    {
                        case "ja": return "目的のファイル名を指定してください。(/h ヘルプ表示)";
                        case "ko": return "대상 파일 이름을 지정하십시오. (/h 도움말 표시)";
                        case "zh-CN": return "请指定目标文件名。(/h 显示帮助)";
                        case "zh-TW": return "請指定目標檔案名稱。(/h 顯示說明)";
                        default: return "Please specify the target file name. (/h for help)";
                    }

                case "ConflictingFileSpecificationMethods":
                    switch (lang)
                    {
                        case "ja": return "/f:オプションによるファイル指定と、コマンドラインでのファイル指定を、同時に使用する事はできません。";
                        case "ko": return "/f: 옵션에 의한 파일 지정과 명령줄에서의 파일 지정을 동시에 사용할 수 없습니다.";
                        case "zh-CN": return "不能同时使用 /f: 选项指定文件和命令行指定文件。";
                        case "zh-TW": return "無法同時使用 /f: 選項指定檔案與命令列指定檔案。";
                        default: return "Cannot use both /f: option file specification and command line file specification simultaneously.";
                    }

                case "ReplaceWordsRequiresTargetFiles":
                    switch (lang)
                    {
                        case "ja": return "置換対象となるファイルを指定してください。";
                        case "ko": return "교체 대상 파일을 지정하십시오.";
                        case "zh-CN": return "请指定要替换的文件。";
                        case "zh-TW": return "請指定要取代的檔案。";
                        default: return "Please specify the files to be replaced.";
                    }

                case "SearchWordsRequiresTargetFiles":
                    switch (lang)
                    {
                        case "ja": return "対象となるファイルを指定してください。";
                        case "ko": return "대상 파일을 지정하십시오.";
                        case "zh-CN": return "请指定目标文件。";
                        case "zh-TW": return "請指定目標檔案。";
                        default: return "Please specify the target files.";
                    }

                case "MissingEncodingName":
                    switch (lang)
                    {
                        case "ja": return "文字エンコーディング名を指定してください。 (/{0})";
                        case "ko": return "문자 인코딩 이름을 지정하십시오. (/{0})";
                        case "zh-CN": return "请指定字符编码名称。 (/{0})";
                        case "zh-TW": return "請指定字元編碼名稱。 (/{0})";
                        default: return "Please specify the character encoding name. (/{0})";
                    }

                case "InvalidEncodingName":
                    switch (lang)
                    {
                        case "ja": return "エンコーディング名が不正です。";
                        case "ko": return "인코딩 이름이 올바르지 않습니다.";
                        case "zh-CN": return "编码名称无效。";
                        case "zh-TW": return "編碼名稱無效。";
                        default: return "Invalid encoding name.";
                    }

                case "UnsupportedEncoding":
                    switch (lang)
                    {
                        case "ja": return "サポートされていないエンコーディングです。";
                        case "ko": return "지원되지 않는 인코딩입니다.";
                        case "zh-CN": return "不支持的编码。";
                        case "zh-TW": return "不支援的編碼。";
                        default: return "Unsupported encoding.";
                    }

                case "UnknownEncoding":
                    switch (lang)
                    {
                        case "ja": return "{0}の文字エンコーディングが分かりません。";
                        case "ko": return "{0}의 문자 인코딩을 알 수 없습니다.";
                        case "zh-CN": return "无法识别 {0} 的字符编码。";
                        case "zh-TW": return "無法識別 {0} 的字元編碼。";
                        default: return "Cannot determine the character encoding of {0}.";
                    }

                case "ReplaceWordsEncodingWithoutReplaceWords":
                    switch (lang)
                    {
                        case "ja": return "置換単語ファイルが指定されていないのに、置換単語ファイルのエンコーディングが指定されています。";
                        case "ko": return "교체 단어 파일이 지정되지 않았는데 교체 단어 파일의 인코딩이 지정되어 있습니다.";
                        case "zh-CN": return "未指定替换词文件,但指定了替换词文件的编码。";
                        case "zh-TW": return "未指定取代詞彙檔案,但指定了取代詞彙檔案的編碼。";
                        default: return "Encoding for replace words file is specified, but the replace words file is not specified.";
                    }

                case "SearchWordsEncodingWithoutSearchWords":
                    switch (lang)
                    {
                        case "ja": return "検索単語ファイルが指定されていないのに、検索単語ファイルのエンコーディングが指定されています。";
                        case "ko": return "검색 단어 파일이 지정되지 않았는데 검색 단어 파일의 인코딩이 지정되어 있습니다.";
                        case "zh-CN": return "未指定搜索词文件,但指定了搜索词文件的编码。";
                        case "zh-TW": return "未指定搜尋詞彙檔案,但指定了搜尋詞彙檔案的編碼。";
                        default: return "Encoding for search words file is specified, but the search words file is not specified.";
                    }

                case "FileListEncodingWithoutFileList":
                    switch (lang)
                    {
                        case "ja": return "ファイルリストが指定されていないのに、ファイルリストのエンコーディングが指定されています。";
                        case "ko": return "파일 목록이 지정되지 않았는데 파일 목록의 인코딩이 지정되어 있습니다.";
                        case "zh-CN": return "未指定文件列表,但指定了文件列表的编码。";
                        case "zh-TW": return "未指定檔案清單,但指定了檔案清單的編碼。";
                        default: return "Encoding for file list is specified, but the file list is not specified.";
                    }

                case "ConversionRequiresOutputEncoding":
                    switch (lang)
                    {
                        case "ja": return "文字エンコーディングの変換をする場合は、/w:により出力先の文字エンコーディングを指定してください。";
                        case "ko": return "문자 인코딩 변환을 수행하는 경우 /w:를 사용하여 출력 문자 인코딩을 지정하십시오.";
                        case "zh-CN": return "要转换字符编码,请使用 /w: 指定输出字符编码。";
                        case "zh-TW": return "要轉換字元編碼,請使用 /w: 指定輸出字元編碼。";
                        default: return "When converting character encoding, please specify the output character encoding using /w:.";
                    }

                case "FileNotFound":
                    switch (lang)
                    {
                        case "ja": return "{0} が存在しません。 ";
                        case "ko": return "{0}이(가) 존재하지 않습니다. ";
                        case "zh-CN": return "{0} 不存在。 ";
                        case "zh-TW": return "{0} 不存在。 ";
                        default: return "{0} does not exist. ";
                    }

                case "MissingOptionFileName":
                    switch (lang)
                    {
                        case "ja": return "/{0} オプションのファイル名を指定してください。";
                        case "ko": return "/{0} 옵션의 파일 이름을 지정하십시오.";
                        case "zh-CN": return "请指定 /{0} 选项的文件名。";
                        case "zh-TW": return "請指定 /{0} 選項的檔案名稱。";
                        default: return "Please specify the file name for /{0} option.";
                    }

                case "EmptyReplaceWords":
                    switch (lang)
                    {
                        case "ja": return "{0}の置換単語がゼロ件です。";
                        case "ko": return "{0}의 교체 단어가 0개입니다.";
                        case "zh-CN": return "{0} 的替换词数量为零。";
                        case "zh-TW": return "{0} 的取代詞彙數量為零。";
                        default: return "{0} has zero replace words.";
                    }

                case "EmptySearchWords":
                    switch (lang)
                    {
                        case "ja": return "{0}の検索単語がゼロ件です。";
                        case "ko": return "{0}의 검색 단어가 0개입니다.";
                        case "zh-CN": return "{0} 的搜索词数量为零。";
                        case "zh-TW": return "{0} 的搜尋詞彙數量為零。";
                        default: return "{0} has zero search words.";
                    }

                case "InvalidReplaceWordFormat":
                    switch (lang)
                    {
                        case "ja": return "置換単語ファイルの{0}行目が不正です。カンマ区切りで「検索文字列,置換文字列」の形式で指定してください。";
                        case "ko": return "교체 단어 파일의 {0}번째 줄이 올바르지 않습니다. 쉼표로 구분하여 \"검색 문자열,교체 문자열\" 형식으로 지정하십시오.";
                        case "zh-CN": return "替换词文件的第 {0} 行无效。请以逗号分隔的格式指定\"搜索字符串,替换字符串\"。";
                        case "zh-TW": return "取代詞彙檔案的第 {0} 行無效。請以逗號分隔的格式指定「搜尋字串,取代字串」。";
                        default: return "Line {0} of the replace words file is invalid. Please specify in the format \"search string,replace string\" separated by comma.";
                    }

                case "InvalidCultureInfo":
                    switch (lang)
                    {
                        case "ja": return "無効なカルチャー情報が指定されました: {0}";
                        case "ko": return "잘못된 문화권 정보가 지정되었습니다: {0}";
                        case "zh-CN": return "指定了无效的区域性信息: {0}";
                        case "zh-TW": return "指定了無效的文化特性資訊: {0}";
                        default: return "Invalid culture information specified: {0}";
                    }

                case "MissingEncodingNameForOption":
                    switch (lang)
                    {
                        case "ja": return "文字エンコーディング名を指定してください。 (/{0})";
                        case "ko": return "문자 인코딩 이름을 지정하십시오. (/{0})";
                        case "zh-CN": return "请指定字符编码名称。 (/{0})";
                        case "zh-TW": return "請指定字元編碼名稱。 (/{0})";
                        default: return "Please specify the character encoding name. (/{0})";
                    }

                case "ErrorsOccurred":
                    switch (lang)
                    {
                        case "ja": return "{0}件のエラーが発生しました。";
                        case "ko": return "{0}개의 오류가 발생했습니다.";
                        case "zh-CN": return "发生了 {0} 个错误。";
                        case "zh-TW": return "發生了 {0} 個錯誤。";
                        default: return "{0} error(s) occurred.";
                    }

                case "OtherFilesProcessedSuccessfully":
                    switch (lang)
                    {
                        case "ja": return "他のファイルは正常に処理しました。";
                        case "ko": return "다른 파일은 정상적으로 처리되었습니다.";
                        case "zh-CN": return "其他文件已正常处理。";
                        case "zh-TW": return "其他檔案已正常處理。";
                        default: return "Other files were processed successfully.";
                    }

                case "SearchComplete":
                    switch (lang)
                    {
                        case "ja": return "検索が完了しました。";
                        case "ko": return "검색이 완료되었습니다.";
                        case "zh-CN": return "搜索完成。";
                        case "zh-TW": return "搜尋完成。";
                        default: return "Search complete.";
                    }

                case "UnexpectedErrorOccurred":
                    switch (lang)
                    {
                        case "ja": return "予期しないエラーが発生しました: {0}";
                        case "ko": return "예기치 않은 오류가 발생했습니다: {0}";
                        case "zh-CN": return "发生了意外错误: {0}";
                        case "zh-TW": return "發生了意外錯誤: {0}";
                        default: return "An unexpected error occurred: {0}";
                    }

                default:
                    return key;
            }
        }
    }
}
