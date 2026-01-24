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
                "/j:<0|1|3> エンコーディング自動判定モード(初期値は 0 )",
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
                "/j:0 が通常モード(独自処理で判定できない場合はサードパーティー製品で判定する)",
                "/j:1 が独自処理だけで自動判定",
                "/j:3 がサードパーティー製品(UTF.Unknown)のみによる自動判定",
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
                "/j:<0|1|3> Encoding auto-detection mode (default is 0)",
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
                "/j:0 Normal mode (use third-party product for detection if unable to detect with proprietary processing)",
                "/j:1 Auto-detect with proprietary processing only",
                "/j:3 Auto-detect with third-party product (UTF.Unknown) only",
                "",
                "If not specified, normal mode will be used.",
                "",
            };
        }

        private static string[] GetRmsmfHelpKorean()
        {
            return new string[]
            {
                "rmsmf (?? ??? ?? ??? ???)\n",
                "rmsmf <??>  <??? ?? ?? ??>\n",
                "",
                "・?? (/r /f /b ? ?? ?? ??)",
                "",
                "/d  ?? ????? ?? ??? ?????.",
                "/b:< true | false  > BOM? ????? true, BOM? ????? false? ?????.",
                "/nl:< crlf | lf | cr >  ? ?? ??? ?????.",
                "/r:<?? ?? ?? CSV ?? ??>",
                "/rc:<?? ?? ?? CSV ??? ?? ??? ??>",
                "",
                "/c:<?? ??? CodePage ?? ?? ??? ??>",
                "/w:<?? ??? CodePage ?? ?? ??? ??>",
                "/f:<?? ?? ?? ??>",
                "/fc:<?? ??? ?? ??? ??>",
                "/j:<0|1|3> ??? ?? ?? ?? (???? 0)",
                "\n",
                "?? ?? ??? CSV? ???? /r:? ?? ??? ?????. /r:?? ?? ?? CSV",
                "",
                "・?? ?? ?? CSV ??? ?:",
                "",
                "?? ??1, ?? ??1",
                "?? ??2, ?? ??2",
                "?? ??3, ?? ??3",
                ".,.",
                ".,.",
                "?? ??n, ?? ??n",
                "　",
                "※1  ??? : ??? ??? ?? ????.",
                "※2  ?? ??? ?? ??? \\r\\n ???? ? ?? ??? ??? ?? ????.",
                "\n",
                "?? ??,\n",
                "・???? ?? ?? ?? ?:",
                "",
                "rmsmf /r:words.csv *.txt ",
                "    (?? ???)",
                "",
                "rmsmf /r:words.csv *.txt /c:shift_jis /w:utf-8 /b:true ",
                "    (?? ???? shift_jis?? utf-8? ???? BOM ??)",
                "",
                "rmsmf /r:words.csv *.txt /c:shift_jis /w:utf-8 /b:false ",
                "    (?? ???? shift_jis?? utf-8? ???? BOM ??)",
                "",
                "",
                "・?? ???? ???? ?:\n",
                "",
                "rmsmf *.txt /c:shift_jis /w:utf-8 /b:true ",
                "",
                "rmsmf *.txt /c:utf-8 /w:shift_jis ",
                "",
                "    (?? ??? ??????)",
                "",
                "・? ?? ??? ???? ?:",
                "",
                "rmsmf *.txt /nl:crlf ",
                "",
                "rmsmf *.txt /nl:lf ",
                "",
                "rmsmf *.txt /nl:win ",
                "",
                "rmsmf *.txt /nl:unix ",
                "",
                "・BOM? ???? ?:",
                "",
                "rmsmf *.txt /b:true ",
                "",
                "rmsmf *.txt /b:false ",
                "",
                "・?? ??? ??? ??? ???? ?? ?:",
                "",
                "rmsmf /r:words.csv /f:filelist.txt ",
                "",
                "(?? ??? ?? ?? ?? ?? ?? ??? ? ?? ??? ??? ?????)",
                "",
                "??? ?? ?? ??:",
                "/j:0 ?? ?? (?? ??? ??? ? ?? ?? ?? ???? ??)",
                "/j:1 ?? ????? ?? ??",
                "/j:3 ?? ??(UTF.Unknown)??? ?? ??",
                "",
                "???? ??? ?? ??? ?????.",
                "",
            };
        }

        private static string[] GetRmsmfHelpChineseSimplified()
        {
            return new string[]
            {
                "rmsmf（替?多个文件中的多个字符串）\n",
                "rmsmf <??>  <要替???的文件名>\n",
                "",
                "・??（/r /f /b 中至少需要一个）",
                "",
                "/d  将子目?也包含在搜索范?内。",
                "/b:< true | false  > 添加BOM?写true，?除BOM?写false。",
                "/nl:< crlf | lf | cr >  ???行符。",
                "/r:<替???列表CSV文件名>",
                "/rc:<替???列表CSV文件的字符??名称>",
                "",
                "/c:<?入文件的CodePage或字符??名称>",
                "/w:<?出文件的CodePage或字符??名称>",
                "/f:<文件列表文件名>",
                "/fc:<文件列表的字符??名称>",
                "/j:<0|1|3> ??自???模式（默???0）",
                "\n",
                "要替?的??列表以CSV格式?写，并使用/r:指定文件名。/r:替???列表CSV",
                "",
                "・替???列表CSV内容示例：",
                "",
                "搜索?1, 替??1",
                "搜索?2, 替??2",
                "搜索?3, 替??3",
                ".,.",
                ".,.",
                "搜索?n, 替??n",
                "　",
                "※1  ??的:前后不要加空格。",
                "※2  搜索?和替??也可以用\\r\\n的形式?写?行符。",
                "\n",
                "例如，\n",
                "・替?字符串?的使用示例：",
                "",
                "rmsmf /r:words.csv *.txt ",
                "    （基本用法）",
                "",
                "rmsmf /r:words.csv *.txt /c:shift_jis /w:utf-8 /b:true ",
                "    （将字符??从shift_jis更改?utf-8并添加BOM）",
                "",
                "rmsmf /r:words.csv *.txt /c:shift_jis /w:utf-8 /b:false ",
                "    （将字符??从shift_jis更改?utf-8并?除BOM）",
                "",
                "",
                "・?更改字符??的示例：\n",
                "",
                "rmsmf *.txt /c:shift_jis /w:utf-8 /b:true ",
                "",
                "rmsmf *.txt /c:utf-8 /w:shift_jis ",
                "",
                "    （??位置可以?活?整）",
                "",
                "・?更改?行符的示例：",
                "",
                "rmsmf *.txt /nl:crlf ",
                "",
                "rmsmf *.txt /nl:lf ",
                "",
                "rmsmf *.txt /nl:win ",
                "",
                "rmsmf *.txt /nl:unix ",
                "",
                "・?更改BOM的示例：",
                "",
                "rmsmf *.txt /b:true ",
                "",
                "rmsmf *.txt /b:false ",
                "",
                "・???文件列表中列出的文件的示例：",
                "",
                "rmsmf /r:words.csv /f:filelist.txt ",
                "",
                "（文件列表是逐行?写文件名或完整路径名的文本文件）",
                "",
                "??自???模式：",
                "/j:0 普通模式（如果无法通?自有?理??，?使用第三方?品??）",
                "/j:1 ?使用自有?理?行自???",
                "/j:3 ?使用第三方?品（UTF.Unknown）?行自???",
                "",
                "如果不指定，将使用普通模式。",
                "",
            };
        }

        private static string[] GetRmsmfHelpChineseTraditional()
        {
            return new string[]
            {
                "rmsmf（取代多個?案中的多個字串）\n",
                "rmsmf <選項>  <要取代單字的?案名稱>\n",
                "",
                "・選項（/r /f /b 中至少需要一個）",
                "",
                "/d  將子目?也包含在搜尋範圍?。",
                "/b:< true | false  > 新增BOM時寫true，刪除BOM時寫false。",
                "/nl:< crlf | lf | cr >  轉換換行符號。",
                "/r:<取代單字清單CSV?案名稱>",
                "/rc:<取代單字清單CSV?案的字元編碼名稱>",
                "",
                "/c:<輸入?案的CodePage或字元編碼名稱>",
                "/w:<輸出?案的CodePage或字元編碼名稱>",
                "/f:<?案清單?案名稱>",
                "/fc:<?案清單的字元編碼名稱>",
                "/j:<0|1|3> 編碼自動偵測模式（預設?為0）",
                "\n",
                "要取代的單字清單以CSV格式編寫，並使用/r:指定?案名稱。/r:取代單字清單CSV",
                "",
                "・取代單字清單CSV?容範例：",
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
                "・僅針對?案清單中列出的?案的範例：",
                "",
                "rmsmf /r:words.csv /f:filelist.txt ",
                "",
                "（?案清單是逐行編寫?案名稱或完整路徑名稱的文字?案）",
                "",
                "編碼自動偵測模式：",
                "/j:0 普通模式（如果無法透過自有處理偵測，則使用第三方?品偵測）",
                "/j:1 僅使用自有處理進行自動偵測",
                "/j:3 僅使用第三方?品（UTF.Unknown）進行自動偵測",
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
                "/j:<0|1|3> エンコーディング自動判定モード(初期値は 0 )",
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
                "  （カレントディレクトリの全txt???イルの文字エンコーディングと改行コードを表示）",
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
                "/j:0 が通常モード(独自処理で判定できない場合はサードパーティー製品で判定する)",
                "/j:1 が独自処理だけで自動判定",
                "/j:3 がサードパーティー製品(UTF.Unknown)のみによる自動判定",
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
                "/j:<0|1|3> Encoding auto-detection mode (default is 0)",
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
                "/j:0 Normal mode (use third-party product for detection if unable to detect with proprietary processing)",
                "/j:1 Auto-detect with proprietary processing only",
                "/j:3 Auto-detect with third-party product (UTF.Unknown) only",
                "",
                "If not specified, normal mode will be used.",
                "",
            };
        }

        private static string[] GetTxprobeHelpKorean()
        {
            return new string[]
            {
                "txprobe（??? ?? ?? ??）\n",
                "txprobe <??>  <??? ?? ??> \n",
                "",
                "・??",
                "",
                "/d  ?? ????? ?? ??? ?????.",
                "/c:<?? ??? CodePage ?? ?? ??? ??>",
                "/f:<?? ?? ?? ??>",
                "/fc:<?? ??? ?? ??? ??>",
                "/s:<?? ?? ?? ?? ??>",
                "/sc:<?? ?? ?? ??? ?? ??? ??>",
                "/p:<?? ?? ????（?? ??? ??? ??? ??）>",
                "/j:<0|1|3> ??? ?? ?? ?? (???? 0)",
                "\n",
                "・??? ?? ??。（?? ?? ?? ??）",
                "",
                "?? ??1",
                "?? ??2",
                "?? ??3",
                "...",
                "?? ??n",
                "\n",
                "?? ??,\n",
                "txprobe *.txt ",
                "  （?? ????? ?? txt ??? ?? ???? ? ?? ?? ??）",
                "",
                "txprobe /s:search.txt *.txt ",
                "  （search.txt? ??? ??? ???? ??? ?? ?? ??）",
                "",
                "txprobe /s:search.txt /p *.txt ",
                "  （?? ??: ?? ??? ??? ?? ??? ?? ??）",
                "",
                "txprobe /d *.txt ",
                "  （?? ????? ???? ??）",
                "",
                "txprobe /c:utf-8 *.txt ",
                "  （?? ???? UTF-8? ???? ??）",
                "",
                "（?? ??? ??????）",
                "",
                "??? ?? ?? ??:",
                "/j:0 ?? ?? (?? ??? ??? ? ?? ?? ?? ???? ??)",
                "/j:1 ?? ????? ?? ??",
                "/j:3 ?? ??(UTF.Unknown)??? ?? ??",
                "",
                "???? ??? ?? ??? ?????.",
                "",
            };
        }

        private static string[] GetTxprobeHelpChineseSimplified()
        {
            return new string[]
            {
                "txprobe（文本文件探?工具）\n",
                "txprobe <??>  <要探?的文件名> \n",
                "",
                "・??",
                "",
                "/d  将子目?也包含在搜索范?内。",
                "/c:<?入文件的CodePage或字符??名称>",
                "/f:<文件列表文件名>",
                "/fc:<文件列表的字符??名称>",
                "/s:<搜索??列表文件名>",
                "/sc:<搜索??列表文件的字符??名称>",
                "/p:<禁用探?模式（??示找到搜索??的文件）>",
                "/j:<0|1|3> ??自???模式（默???0）",
                "\n",
                "・要搜索的??列表。（搜索??列表文件）",
                "",
                "搜索?1",
                "搜索?2",
                "搜索?3",
                "...",
                "搜索?n",
                "\n",
                "例如，\n",
                "txprobe *.txt ",
                "  （?示当前目?中所有txt文件的字符??和?行符）",
                "",
                "txprobe /s:search.txt *.txt ",
                "  （搜索search.txt中列出的??并?示找到的文件信息）",
                "",
                "txprobe /s:search.txt /p *.txt ",
                "  （探?模式：?示找到搜索??的文件名和??）",
                "",
                "txprobe /d *.txt ",
                "  （包括子目??行搜索）",
                "",
                "txprobe /c:utf-8 *.txt ",
                "  （指定UTF-8作?字符???行搜索）",
                "",
                "（??位置可以?活?整）",
                "",
                "??自???模式：",
                "/j:0 普通模式（如果无法通?自有?理??，?使用第三方?品??）",
                "/j:1 ?使用自有?理?行自???",
                "/j:3 ?使用第三方?品（UTF.Unknown）?行自???",
                "",
                "如果不指定，将使用普通模式。",
                "",
            };
        }

        private static string[] GetTxprobeHelpChineseTraditional()
        {
            return new string[]
            {
                "txprobe（文字?案探測工具）\n",
                "txprobe <選項>  <要探測的?案名稱> \n",
                "",
                "・選項",
                "",
                "/d  將子目?也包含在搜尋範圍?。",
                "/c:<輸入?案的CodePage或字元編碼名稱>",
                "/f:<?案清單?案名稱>",
                "/fc:<?案清單的字元編碼名稱>",
                "/s:<搜尋單字清單?案名稱>",
                "/sc:<搜尋單字清單?案的字元編碼名稱>",
                "/p:<停用探測模式（僅顯示找到搜尋單字的?案）>",
                "/j:<0|1|3> 編碼自動偵測模式（預設?為0）",
                "\n",
                "・要搜尋的單字清單。（搜尋單字清單?案）",
                "",
                "搜尋詞1",
                "搜尋詞2",
                "搜尋詞3",
                "...",
                "搜尋詞n",
                "\n",
                "例如，\n",
                "txprobe *.txt ",
                "  （顯示目前目?中所有txt?案的字元編碼和換行符號）",
                "",
                "txprobe /s:search.txt *.txt ",
                "  （搜尋search.txt中列出的單字並顯示找到的?案資訊）",
                "",
                "txprobe /s:search.txt /p *.txt ",
                "  （探測模式：顯示找到搜尋單字的?案名稱和單字）",
                "",
                "txprobe /d *.txt ",
                "  （包括子目?進行搜尋）",
                "",
                "txprobe /c:utf-8 *.txt ",
                "  （指定UTF-8作為字元編碼進行搜尋）",
                "",
                "（選項位置可以靈活調整）",
                "",
                "編碼自動偵測模式：",
                "/j:0 普通模式（如果無法透過自有處理偵測，則使用第三方?品偵測）",
                "/j:1 僅使用自有處理進行自動偵測",
                "/j:3 僅使用第三方?品（UTF.Unknown）進行自動偵測",
                "",
                "如果不指定，將使用普通模式。",
                "",
            };
        }

        #endregion
    }
}
