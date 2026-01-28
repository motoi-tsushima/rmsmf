using System;
using System.Globalization;

namespace rmsmf
{
    /// <summary>
    /// 多言語対応ヘルプメッセージ
    /// </summary>
    public static class HelpMessages
    {
        /// <summary>
        /// 現在のカルチャーに基づいて適切な言語を取得
        /// </summary>
        /// <returns>言語コード（ja, ko, zh-CN, zh-TW, en）</returns>
        public static string GetLanguageCode()
        {
            try
            {
                CultureInfo currentCulture = CultureInfo.CurrentCulture;
                string cultureName = currentCulture.Name;

                // 日本語
                if (cultureName.StartsWith("ja", StringComparison.OrdinalIgnoreCase))
                {
                    return "ja";
                }
                // 韓国語
                else if (cultureName.StartsWith("ko", StringComparison.OrdinalIgnoreCase))
                {
                    return "ko";
                }
                // 中国語（簡体字）
                else if (cultureName.Equals("zh-CN", StringComparison.OrdinalIgnoreCase) ||
                         cultureName.Equals("zh-Hans", StringComparison.OrdinalIgnoreCase) ||
                         cultureName.Equals("zh-SG", StringComparison.OrdinalIgnoreCase))
                {
                    return "zh-CN";
                }
                // 中国語（繁体字・台湾）
                else if (cultureName.Equals("zh-TW", StringComparison.OrdinalIgnoreCase) ||
                         cultureName.Equals("zh-Hant", StringComparison.OrdinalIgnoreCase) ||
                         cultureName.Equals("zh-HK", StringComparison.OrdinalIgnoreCase) ||
                         cultureName.Equals("zh-MO", StringComparison.OrdinalIgnoreCase))
                {
                    return "zh-TW";
                }

                // デフォルトは英語
                return "en";
            }
            catch
            {
                // エラーが発生した場合は英語を返す
                return "en";
            }
        }

        /// <summary>
        /// rmsmf のヘルプメッセージを取得
        /// </summary>
        /// <param name="languageCode">言語コード</param>
        /// <returns>ヘルプメッセージ配列</returns>
        public static string[] GetRmsmfHelpMessage(string languageCode)
        {
            switch (languageCode)
            {
                case "ja":
                    return GetRmsmfHelpJapanese();
                case "ko":
                    return GetRmsmfHelpKorean();
                case "zh-CN":
                    return GetRmsmfHelpChineseSimplified();
                case "zh-TW":
                    return GetRmsmfHelpChineseTraditional();
                default:
                    return GetRmsmfHelpEnglish();
            }
        }

        /// <summary>
        /// txprobe のヘルプメッセージを取得
        /// </summary>
        /// <param name="languageCode">言語コード</param>
        /// <returns>ヘルプメッセージ配列</returns>
        public static string[] GetTxprobeHelpMessage(string languageCode)
        {
            switch (languageCode)
            {
                case "ja":
                    return GetTxprobeHelpJapanese();
                case "ko":
                    return GetTxprobeHelpKorean();
                case "zh-CN":
                    return GetTxprobeHelpChineseSimplified();
                case "zh-TW":
                    return GetTxprobeHelpChineseTraditional();
                default:
                    return GetTxprobeHelpEnglish();
            }
        }

        #region rmsmf Help Messages

        private static string[] GetRmsmfHelpJapanese()
        {
            return new string[]
            {
                "rmsmf（複数のファイルの複数の文字列を置き換える）\n",
                "rmsmf <オプション>  <単語を置き換えるファイル名>\n",
                "",
                "・オプション (/r /f /b のいずれかが必須です)",
                "",
                "/d  サブディレクトリも検索対象に含める。",
                "/b:< true | false  > BOMを作成する場合は, trueを記述し, BOMを削除する場合はfalseを記述します。",
                "/nl:< crlf | lf | cr >  改行コードを変換する。",
                "/r:<置換単語リストCSVのファイル名>",
                "/rc:<置換単語リストCSVのファイルの文字エンコーディング名>",
                "",
                "/c:<読み込みファイルのCodePage又は文字エンコーディング名>",
                "/w:<書き込みファイルのCodePage又は文字エンコーディング名>",
                "/f:<ファイルリストのファイル名>",
                "/fc:<ファイルリストの文字エンコーディング名>",
                "/det:<0|1|3> エンコーディング自動判定モード(初期値は 0 )",
                "/ci:<カルチャー情報> システムのカルチャー情報を設定(例: en-US, ja-JP, zh-CN)",
                "\n",
                "置換する単語のリストはCSVに記述して /r: でそのファイル名を指定します。/r:置換単語リストCSV",
                "",
                "・置換単語リストCSVの内容の例、",
                "",
                "検索ワード1, 置換ワード1",
                "検索ワード2, 置換ワード2",
                "検索ワード3, 置換ワード3",
                ".,.",
                ".,.",
                "検索ワードn, 置換ワードn",
                "　",
                "※１　オプションの : の前後にはスペースを入れないでください。",
                "※２　検索ワードと置換ワードには \\r\\n の形で改行コードを記述することもできます。",
                "\n",
                "例として、\n",
                "・文字列を置換するときの使用例、",
                "",
                "rmsmf /r:words.csv *.txt ",
                "    (基本の使い方)",
                "",
                "rmsmf /r:words.csv *.txt /c:shift_jis /w:utf-8 /b:true ",
                "    (文字エンコーディングを shift_jis から utf-8 へ変更しBOMを付ける)",
                "",
                "rmsmf /r:words.csv *.txt /c:shift_jis /w:utf-8 /b:false ",
                "    (文字エンコーディングを shift_jis から utf-8 へ変更しBOMを外す)",
                "",
                "",
                "・文字エンコーディングを変更するだけの例。\n",
                "",
                "rmsmf *.txt /c:shift_jis /w:utf-8 /b:true ",
                "",
                "rmsmf *.txt /c:utf-8 /w:shift_jis ",
                "",
                "    （オプションの位置は自由です）",
                "",
                "・改行コードを変更するだけの例。",
                "",
                "rmsmf *.txt /nl:crlf ",
                "",
                "rmsmf *.txt /nl:lf ",
                "",
                "rmsmf *.txt /nl:win ",
                "",
                "rmsmf *.txt /nl:unix ",
                "",
                "・BOMを変更するだけの例。",
                "",
                "rmsmf *.txt /b:true ",
                "",
                "rmsmf *.txt /b:false ",
                "",
                "・ファイルリストに記載されるファイルのみ置換対象にする例。",
                "",
                "rmsmf /r:words.csv /f:filelist.txt ",
                "",
                "(ファイルリストは、ファイル名又はフルパス名が、一行づつ記述されたテキストファイルです)",
                "",
                "エンコーディング自動判定モード は、",
                "/det:0 が通常モード(独自処理で判定できない場合はサードパーティー製品で判定する)",
                "/det:1 が独自処理だけで自動判定",
                "/det:3 がサードパーティー製品(UTF.Unknown)のみによる自動判定",
                "となります。",
                "指定しない場合は通常モードになります。",
                "",
            };
        }

        private static string[] GetRmsmfHelpEnglish()
        {
            return new string[]
            {
                "rmsmf (Replace Multiple Strings in Multiple Files)\n",
                "rmsmf <options>  <file name to replace words>\n",
                "",
                "・Options (at least one of /r /f /b is required)",
                "",
                "/d  Include subdirectories in the search.",
                "/b:< true | false  > Write true to add BOM, false to remove BOM.",
                "/nl:< crlf | lf | cr >  Convert newline codes.",
                "/r:<replacement word list CSV file name>",
                "/rc:<character encoding name of replacement word list CSV file>",
                "",
                "/c:<CodePage or character encoding name of input file>",
                "/w:<CodePage or character encoding name of output file>",
                "/f:<file list file name>",
                "/fc:<character encoding name of file list>",
                "/det:<0|1|3> Encoding auto-detection mode (default is 0)",
                "/ci:<culture info> Set system culture information (e.g., en-US, ja-JP, zh-CN)",
                "\n",
                "List of words to replace is written in CSV and specify the file name with /r: . /r:replacement word list CSV",
                "",
                "・Example of replacement word list CSV content:",
                "",
                "search word1, replace word1",
                "search word2, replace word2",
                "search word3, replace word3",
                ".,.",
                ".,.",
                "search wordn, replace wordn",
                "　",
                "※1  Do not put spaces before or after the : in options.",
                "※2  You can also write newline codes in the form \\r\\n in search and replace words.",
                "\n",
                "For example,\n",
                "・Usage example when replacing strings:",
                "",
                "rmsmf /r:words.csv *.txt ",
                "    (Basic usage)",
                "",
                "rmsmf /r:words.csv *.txt /c:shift_jis /w:utf-8 /b:true ",
                "    (Change character encoding from shift_jis to utf-8 and add BOM)",
                "",
                "rmsmf /r:words.csv *.txt /c:shift_jis /w:utf-8 /b:false ",
                "    (Change character encoding from shift_jis to utf-8 and remove BOM)",
                "",
                "",
                "・Example of just changing character encoding:\n",
                "",
                "rmsmf *.txt /c:shift_jis /w:utf-8 /b:true ",
                "",
                "rmsmf *.txt /c:utf-8 /w:shift_jis ",
                "",
                "    (Option positions are flexible)",
                "",
                "・Example of just changing newline codes:",
                "",
                "rmsmf *.txt /nl:crlf ",
                "",
                "rmsmf *.txt /nl:lf ",
                "",
                "rmsmf *.txt /nl:win ",
                "",
                "rmsmf *.txt /nl:unix ",
                "",
                "・Example of just changing BOM:",
                "",
                "rmsmf *.txt /b:true ",
                "",
                "rmsmf *.txt /b:false ",
                "",
                "・Example of targeting only files listed in file list:",
                "",
                "rmsmf /r:words.csv /f:filelist.txt ",
                "",
                "(File list is a text file with file names or full path names written one per line)",
                "",
                "Encoding auto-detection mode:",
                "/det:0 Normal mode (use third-party product for detection if unable to detect with proprietary processing)",
                "/det:1 Auto-detect with proprietary processing only",
                "/det:3 Auto-detect with third-party product (UTF.Unknown) only",
                "",
                "If not specified, normal mode will be used.",
                "",
            };
        }

        private static string[] GetRmsmfHelpKorean()
        {
            return new string[]
            {
                "rmsmf (여러 파일의 여러 문자열 바꾸기)\n",
                "rmsmf <옵션>  <단어를 바꿀 파일 이름>\n",
                "",
                "・옵션 (/r /f /b 중 하나 이상 필수)",
                "",
                "/d  하위 디렉터리도 검색 대상에 포함합니다.",
                "/b:< true | false  > BOM을 추가하려면 true, BOM을 제거하려면 false를 작성합니다.",
                "/nl:< crlf | lf | cr >  줄 바꿈 코드를 변환합니다.",
                "/r:<바꿀 단어 목록 CSV 파일 이름>",
                "/rc:<바꿀 단어 목록 CSV 파일의 문자 인코딩 이름>",
                "",
                "/c:<입력 파일의 CodePage 또는 문자 인코딩 이름>",
                "/w:<출력 파일의 CodePage 또는 문자 인코딩 이름>",
                "/f:<파일 목록 파일 이름>",
                "/fc:<파일 목록의 문자 인코딩 이름>",
                "/det:<0|1|3> 인코딩 자동 감지 모드 (기본값은 0)",
                "/ci:<문화권 정보> 시스템 문화권 정보 설정 (예: en-US, ja-JP, zh-CN)",
                "\n",
                "바꿀 단어 목록은 CSV로 작성하고 /r:로 파일 이름을 지정합니다. /r:바꿀 단어 목록 CSV",
                "",
                "・바꿀 단어 목록 CSV 내용의 예:",
                "",
                "검색 단어1, 바꿀 단어1",
                "검색 단어2, 바꿀 단어2",
                "검색 단어3, 바꿀 단어3",
                ".,.",
                ".,.",
                "검색 단어n, 바꿀 단어n",
                "　",
                "※1  옵션의 : 앞뒤에 공백을 넣지 마십시오.",
                "※2  검색 단어와 바꿀 단어에 \\r\\n 형식으로 줄 바꿈 코드를 작성할 수도 있습니다.",
                "\n",
                "예를 들어,\n",
                "・문자열을 바꿀 때의 사용 예:",
                "",
                "rmsmf /r:words.csv *.txt ",
                "    (기본 사용법)",
                "",
                "rmsmf /r:words.csv *.txt /c:shift_jis /w:utf-8 /b:true ",
                "    (문자 인코딩을 shift_jis에서 utf-8로 변경하고 BOM 추가)",
                "",
                "rmsmf /r:words.csv *.txt /c:shift_jis /w:utf-8 /b:false ",
                "    (문자 인코딩을 shift_jis에서 utf-8로 변경하고 BOM 제거)",
                "",
                "",
                "・문자 인코딩만 변경하는 예:\n",
                "",
                "rmsmf *.txt /c:shift_jis /w:utf-8 /b:true ",
                "",
                "rmsmf *.txt /c:utf-8 /w:shift_jis ",
                "",
                "    (옵션 위치는 자유롭습니다)",
                "",
                "・줄 바꿈 코드만 변경하는 예:",
                "",
                "rmsmf *.txt /nl:crlf ",
                "",
                "rmsmf *.txt /nl:lf ",
                "",
                "rmsmf *.txt /nl:win ",
                "",
                "rmsmf *.txt /nl:unix ",
                "",
                "・BOM만 변경하는 예:",
                "",
                "rmsmf *.txt /b:true ",
                "",
                "rmsmf *.txt /b:false ",
                "",
                "・파일 목록에 나열된 파일만 대상으로 하는 예:",
                "",
                "rmsmf /r:words.csv /f:filelist.txt ",
                "",
                "(파일 목록은 파일 이름 또는 전체 경로 이름이 한 줄씩 작성된 텍스트 파일입니다)",
                "",
                "인코딩 자동 감지 모드:",
                "/det:0 일반 모드 (자체 처리로 감지할 수 없는 경우 타사 제품으로 감지)",
                "/det:1 자체 처리만으로 자동 감지",
                "/det:3 타사 제품(UTF.Unknown)만으로 자동 감지",
                "",
                "지정하지 않으면 일반 모드가 사용됩니다.",
                "",
            };
        }

        private static string[] GetRmsmfHelpChineseSimplified()
        {
            return new string[]
            {
                "rmsmf（替换多个文件中的多个字符串）\n",
                "rmsmf <选项>  <要替换单词的文件名>\n",
                "",
                "・选项（/r /f /b 中至少需要一个）",
                "",
                "/d  将子目录也包含在搜索范围内。",
                "/b:< true | false  > 添加BOM时写true，删除BOM时写false。",
                "/nl:< crlf | lf | cr >  转换换行符。",
                "/r:<替换单词列表CSV文件名>",
                "/rc:<替换单词列表CSV文件的字符编码名称>",
                "",
                "/c:<输入文件的CodePage或字符编码名称>",
                "/w:<输出文件的CodePage或字符编码名称>",
                "/f:<文件列表文件名>",
                "/fc:<文件列表的字符编码名称>",
                "/det:<0|1|3> 编码自动检测模式（默认值为0）",
                "/ci:<区域性信息> 设置系统区域性信息（例如：en-US、ja-JP、zh-CN）",
                "\n",
                "要替换的单词列表以CSV格式编写，并使用/r:指定文件名。/r:替换单词列表CSV",
                "",
                "・替换单词列表CSV内容示例：",
                "",
                "搜索词1, 替换词1",
                "搜索词2, 替换词2",
                "搜索词3, 替换词3",
                ".,.",
                ".,.",
                "搜索词n, 替换词n",
                "　",
                "※1  选项的:前后不要加空格。",
                "※2  搜索词和替换词也可以用\\r\\n的形式编写换行符。",
                "\n",
                "例如，\n",
                "・替换字符串时的使用示例：",
                "",
                "rmsmf /r:words.csv *.txt ",
                "    （基本用法）",
                "",
                "rmsmf /r:words.csv *.txt /c:shift_jis /w:utf-8 /b:true ",
                "    （将字符编码从shift_jis更改为utf-8并添加BOM）",
                "",
                "rmsmf /r:words.csv *.txt /c:shift_jis /w:utf-8 /b:false ",
                "    （将字符编码从shift_jis更改为utf-8并删除BOM）",
                "",
                "",
                "・仅更改字符编码的示例：\n",
                "",
                "rmsmf *.txt /c:shift_jis /w:utf-8 /b:true ",
                "",
                "rmsmf *.txt /c:utf-8 /w:shift_jis ",
                "",
                "    （选项位置可以灵活调整）",
                "",
                "・仅更改换行符的示例：",
                "",
                "rmsmf *.txt /nl:crlf ",
                "",
                "rmsmf *.txt /nl:lf ",
                "",
                "rmsmf *.txt /nl:win ",
                "",
                "rmsmf *.txt /nl:unix ",
                "",
                "・仅更改BOM的示例：",
                "",
                "rmsmf *.txt /b:true ",
                "",
                "rmsmf *.txt /b:false ",
                "",
                "・仅针对文件列表中列出的文件的示例：",
                "",
                "rmsmf /r:words.csv /f:filelist.txt ",
                "",
                "（文件列表是逐行编写文件名或完整路径名的文本文件）",
                "",
                "编码自动检测模式：",
                "/det:0 普通模式（如果无法通过自有处理检测，则使用第三方产品检测）",
                "/det:1 仅使用自有处理进行自动检测",
                "/det:3 仅使用第三方产品（UTF.Unknown）进行自动检测",
                "",
                "如果不指定，将使用普通模式。",
                "",
            };
        }

        private static string[] GetRmsmfHelpChineseTraditional()
        {
            return new string[]
            {
                "rmsmf（取代多個檔案中的多個字串）\n",
                "rmsmf <選項>  <要取代單字的檔案名稱>\n",
                "",
                "・選項（/r /f /b 中至少需要一個）",
                "",
                "/d  將子目錄也包含在搜尋範圍內。",
                "/b:< true | false  > 新增BOM時寫true，刪除BOM時寫false。",
                "/nl:< crlf | lf | cr >  轉換換行符號。",
                "/r:<取代單字清單CSV檔案名稱>",
                "/rc:<取代單字清單CSV檔案的字元編碼名稱>",
                "",
                "/c:<輸入檔案的CodePage或字元編碼名稱>",
                "/w:<輸出檔案的CodePage或字元編碼名稱>",
                "/f:<檔案清單檔案名稱>",
                "/fc:<檔案清單的字元編碼名稱>",
                "/det:<0|1|3> 編碼自動偵測模式（預設值為0）",
                "/ci:<文化特性資訊> 設定系統文化特性資訊（例如：en-US、ja-JP、zh-CN）",
                "\n",
                "要取代的單字清單以CSV格式編寫，並使用/r:指定檔案名稱。/r:取代單字清單CSV",
                "",
                "・取代單字清單CSV內容範例：",
                "",
                "搜尋詞1, 取代詞1",
                "搜尋詞2, 取代詞2",
                "搜尋詞3, 取代詞3",
                ".,.",
                ".,.",
                "搜尋詞n, 取代詞n",
                "　",
                "※1  選項的:前後不要加空格。",
                "※2  搜尋詞和取代詞也可以用\\r\\n的形式編寫換行符號。",
                "\n",
                "例如，\n",
                "・取代字串時的使用範例：",
                "",
                "rmsmf /r:words.csv *.txt ",
                "    （基本用法）",
                "",
                "rmsmf /r:words.csv *.txt /c:shift_jis /w:utf-8 /b:true ",
                "    （將字元編碼從shift_jis變更為utf-8並新增BOM）",
                "",
                "rmsmf /r:words.csv *.txt /c:shift_jis /w:utf-8 /b:false ",
                "    （將字元編碼從shift_jis變更為utf-8並刪除BOM）",
                "",
                "",
                "・僅變更字元編碼的範例：\n",
                "",
                "rmsmf *.txt /c:shift_jis /w:utf-8 /b:true ",
                "",
                "rmsmf *.txt /c:utf-8 /w:shift_jis ",
                "",
                "    （選項位置可以靈活調整）",
                "",
                "・僅變更換行符號的範例：",
                "",
                "rmsmf *.txt /nl:crlf ",
                "",
                "rmsmf *.txt /nl:lf ",
                "",
                "rmsmf *.txt /nl:win ",
                "",
                "rmsmf *.txt /nl:unix ",
                "",
                "・僅變更BOM的範例：",
                "",
                "rmsmf *.txt /b:true ",
                "",
                "rmsmf *.txt /b:false ",
                "",
                "・僅針對檔案清單中列出的檔案的範例：",
                "",
                "rmsmf /r:words.csv /f:filelist.txt ",
                "",
                "（檔案清單是逐行編寫檔案名稱或完整路徑名稱的文字檔案）",
                "",
                "編碼自動偵測模式：",
                "/det:0 普通模式（如果無法透過自有處理偵測，則使用第三方產品偵測）",
                "/det:1 僅使用自有處理進行自動偵測",
                "/det:3 僅使用第三方產品（UTF.Unknown）進行自動偵測",
                "",
                "如果不指定，將使用普通模式。",
                "",
            };
        }

        #endregion

        #region txprobe Help Messages

        private static string[] GetTxprobeHelpJapanese()
        {
            return new string[]
            {
                "txprobe（テキストファイル探査ツール）\n",
                "txprobe <オプション>  <探査するファイル名> \n",
                "",
                "・オプション",
                "",
                "/d  サブディレクトリも検索対象に含める。",
                "/c:<読み込みファイルのCodePage又は文字エンコーディング名>",
                "/f:<ファイルリストのファイル名>",
                "/fc:<ファイルリストの文字エンコーディング名>",
                "/s:<検索単語リストのファイル名>",
                "/sc:<検索単語リストのファイルの文字エンコーディング名>",
                "/p:<プローブモード無効（検索単語が見つかったファイルのみ表示）>",
                "/det:<0|1|3> エンコーディング自動判定モード(初期値は 0 )",
                "/ci:<カルチャー情報> システムのカルチャー情報を設定(例: en-US, ja-JP, zh-CN)",
                "\n",
                "・検索する単語のリスト。（検索単語リストファイル）",
                "",
                "検索ワード1",
                "検索ワード2",
                "検索ワード3",
                "...",
                "検索ワードn",
                "\n",
                "例として、\n",
                "txprobe *.txt ",
                "  （カレントディレクトリの全txtファイルの文字エンコーディングと改行コードを表示）",
                "",
                "txprobe /s:search.txt *.txt ",
                "  （search.txtに記載された単語を検索し、見つかったファイルの情報を表示）",
                "",
                "txprobe /s:search.txt /p *.txt ",
                "  （プローブモード：検索単語が見つかったファイル名と単語を表示）",
                "",
                "txprobe /d *.txt ",
                "  （サブディレクトリも含めて検索）",
                "",
                "txprobe /c:utf-8 *.txt ",
                "  （文字エンコーディングをUTF-8に指定して検索）",
                "",
                "（オプションの位置は自由です）",
                "",
                "エンコーディング自動判定モード は、",
                "/det:0 が通常モード(独自処理で判定できない場合はサードパーティー製品で判定する)",
                "/det:1 が独自処理だけで自動判定",
                "/det:3 がサードパーティー製品(UTF.Unknown)のみによる自動判定",
                "となります。",
                "指定しない場合は通常モードになります。",
                "",
            };
        }

        private static string[] GetTxprobeHelpEnglish()
        {
            return new string[]
            {
                "txprobe (Text File Probe Tool)\n",
                "txprobe <options>  <file name to probe> \n",
                "",
                "・Options",
                "",
                "/d  Include subdirectories in the search.",
                "/c:<CodePage or character encoding name of input file>",
                "/f:<file list file name>",
                "/fc:<character encoding name of file list>",
                "/s:<search word list file name>",
                "/sc:<character encoding name of search word list file>",
                "/p:<disable probe mode (show only files where search words are found)>",
                "/det:<0|1|3> Encoding auto-detection mode (default is 0)",
                "/ci:<culture info> Set system culture information (e.g., en-US, ja-JP, zh-CN)",
                "\n",
                "・List of words to search. (search word list file)",
                "",
                "search word1",
                "search word2",
                "search word3",
                "...",
                "search wordn",
                "\n",
                "For example,\n",
                "txprobe *.txt ",
                "  (Display character encoding and newline codes for all txt files in current directory)",
                "",
                "txprobe /s:search.txt *.txt ",
                "  (Search for words listed in search.txt and display information on found files)",
                "",
                "txprobe /s:search.txt /p *.txt ",
                "  (Probe mode: Display file names and words where search words are found)",
                "",
                "txprobe /d *.txt ",
                "  (Search including subdirectories)",
                "",
                "txprobe /c:utf-8 *.txt ",
                "  (Search specifying UTF-8 as character encoding)",
                "",
                "(Option positions are flexible)",
                "",
                "Encoding auto-detection mode:",
                "/det:0 Normal mode (use third-party product for detection if unable to detect with proprietary processing)",
                "/det:1 Auto-detect with proprietary processing only",
                "/det:3 Auto-detect with third-party product (UTF.Unknown) only",
                "",
                "If not specified, normal mode will be used.",
                "",
            };
        }

        private static string[] GetTxprobeHelpKorean()
        {
            return new string[]
            {
                "txprobe（텍스트 파일 탐색 도구）\n",
                "txprobe <옵션>  <탐색할 파일 이름> \n",
                "",
                "・옵션",
                "",
                "/d  하위 디렉터리도 검색 대상에 포함합니다.",
                "/c:<입력 파일의 CodePage 또는 문자 인코딩 이름>",
                "/f:<파일 목록 파일 이름>",
                "/fc:<파일 목록의 문자 인코딩 이름>",
                "/s:<검색 단어 목록 파일 이름>",
                "/sc:<검색 단어 목록 파일의 문자 인코딩 이름>",
                "/p:<탐색 모드 비활성화（검색 단어가 발견된 파일만 표시）>",
                "/det:<0|1|3> 인코딩 자동 감지 모드 (기본값은 0)",
                "/ci:<문화권 정보> 시스템 문화권 정보 설정 (예: en-US, ja-JP, zh-CN)",
                "\n",
                "・검색할 단어 목록。（검색 단어 목록 파일）",
                "",
                "검색 단어1",
                "검색 단어2",
                "검색 단어3",
                "...",
                "검색 단어n",
                "\n",
                "예를 들어,\n",
                "txprobe *.txt ",
                "  （현재 디렉터리의 모든 txt 파일의 문자 인코딩과 줄 바꿈 코드 표시）",
                "",
                "txprobe /s:search.txt *.txt ",
                "  （search.txt에 나열된 단어를 검색하고 발견된 파일 정보 표시）",
                "",
                "txprobe /s:search.txt /p *.txt ",
                "  （탐색 모드: 검색 단어가 발견된 파일 이름과 단어 표시）",
                "",
                "txprobe /d *.txt ",
                "  （하위 디렉터리를 포함하여 검색）",
                "",
                "txprobe /c:utf-8 *.txt ",
                "  （문자 인코딩을 UTF-8로 지정하여 검색）",
                "",
                "（옵션 위치는 자유롭습니다）",
                "",
                "인코딩 자동 감지 모드:",
                "/det:0 일반 모드 (자체 처리로 감지할 수 없는 경우 타사 제품으로 감지)",
                "/det:1 자체 처리만으로 자동 감지",
                "/det:3 타사 제품(UTF.Unknown)만으로 자동 감지",
                "",
                "지정하지 않으면 일반 모드가 사용됩니다.",
                "",
            };
        }

        private static string[] GetTxprobeHelpChineseSimplified()
        {
            return new string[]
            {
                "txprobe（文本文件探测工具）\n",
                "txprobe <选项>  <要探测的文件名> \n",
                "",
                "・选项",
                "",
                "/d  将子目录也包含在搜索范围内。",
                "/c:<输入文件的CodePage或字符编码名称>",
                "/f:<文件列表文件名>",
                "/fc:<文件列表的字符编码名称>",
                "/s:<搜索单词列表文件名>",
                "/sc:<搜索单词列表文件的字符编码名称>",
                "/p:<禁用探测模式（仅显示找到搜索单词的文件）>",
                "/det:<0|1|3> 编码自动检测模式（默认值为0）",
                "/ci:<区域性信息> 设置系统区域性信息（例如：en-US、ja-JP、zh-CN）",
                "\n",
                "・要搜索的单词列表。（搜索单词列表文件）",
                "",
                "搜索词1",
                "搜索词2",
                "搜索词3",
                "...",
                "搜索词n",
                "\n",
                "例如，\n",
                "txprobe *.txt ",
                "  （显示当前目录中所有txt文件的字符编码和换行符）",
                "",
                "txprobe /s:search.txt *.txt ",
                "  （搜索search.txt中列出的单词并显示找到的文件信息）",
                "",
                "txprobe /s:search.txt /p *.txt ",
                "  （探测模式：显示找到搜索单词的文件名和单词）",
                "",
                "txprobe /d *.txt ",
                "  （包括子目录进行搜索）",
                "",
                "txprobe /c:utf-8 *.txt ",
                "  （指定UTF-8作为字符编码进行搜索）",
                "",
                "（选项位置可以灵活调整）",
                "",
                "编码自动检测模式：",
                "/det:0 普通模式（如果无法通过自有处理检测，则使用第三方产品检测）",
                "/det:1 仅使用自有处理进行自动检测",
                "/det:3 仅使用第三方产品（UTF.Unknown）进行自动检测",
                "",
                "如果不指定，将使用普通模式。",
                "",
            };
        }

        private static string[] GetTxprobeHelpChineseTraditional()
        {
            return new string[]
            {
                "txprobe（文字檔案探測工具）\n",
                "txprobe <選項>  <要探測的檔案名稱> \n",
                "",
                "・選項",
                "",
                "/d  將子目錄也包含在搜尋範圍內。",
                "/c:<輸入檔案的CodePage或字元編碼名稱>",
                "/f:<檔案清單檔案名稱>",
                "/fc:<檔案清單的字元編碼名稱>",
                "/s:<搜尋單字清單檔案名稱>",
                "/sc:<搜尋單字清單檔案的字元編碼名稱>",
                "/p:<停用探測模式（僅顯示找到搜尋單字的檔案）>",
                "/det:<0|1|3> 編碼自動偵測模式（預設值為0）",
                "/ci:<文化特性資訊> 設定系統文化特性資訊（例如：en-US、ja-JP、zh-CN）",
                "\n",
                "・要搜尋的單字清單。（搜尋單字清單檔案）",
                "",
                "搜尋詞1",
                "搜尋詞2",
                "搜尋詞3",
                "...",
                "搜尋詞n",
                "\n",
                "例如，\n",
                "txprobe *.txt ",
                "  （顯示目前目錄中所有txt檔案的字元編碼和換行符號）",
                "",
                "txprobe /s:search.txt *.txt ",
                "  （搜尋search.txt中列出的單字並顯示找到的檔案資訊）",
                "",
                "txprobe /s:search.txt /p *.txt ",
                "  （探測模式：顯示找到搜尋單字的檔案名稱和單字）",
                "",
                "txprobe /d *.txt ",
                "  （包括子目錄進行搜尋）",
                "",
                "txprobe /c:utf-8 *.txt ",
                "  （指定UTF-8作為字元編碼進行搜尋）",
                "",
                "（選項位置可以靈活調整）",
                "",
                "編碼自動偵測模式：",
                "/det:0 普通模式（如果無法透過自有處理偵測，則使用第三方產品偵測）",
                "/det:1 僅使用自有處理進行自動偵測",
                "/det:3 僅使用第三方產品（UTF.Unknown）進行自動偵測",
                "",
                "如果不指定，將使用普通模式。",
                "",
            };
        }

        #endregion
    }
}
