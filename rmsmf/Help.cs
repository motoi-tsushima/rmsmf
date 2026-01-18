using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rmsmf
{

    public class Help
    {
        private string[] _helpMessage =
        {
                    "rmsmf（複数のファイルの複数の文字列を置き換える）\n"
                    ,  "rmsmf <オプション>  <単語を置き換えるファイル名>\n"
                    ,  ""
                    ,  "・オプション (/r /f /b のいずれかが必須です)"
                    ,  ""
                    ,  "/d  サブディレクトリも検索対象に含める。"
                    ,  "/b:< true | false  > BOMを作成する場合は, trueを記述し, そうでない場合はfalseを記述します。"
                    ,  "/nl:< crlf | lf | cr >  改行コードを変換する。"
                    ,  "/r:<置換単語リストCSVのファイル名>"
                    ,  "/rc:<置換単語リストCSVのファイルの文字エンコーディング名>"
                    ,  ""
                    ,  "/c:<読み込みファイルのCodePage又は文字エンコーディング名>"
                    ,  "/w:<書き込みファイルのCodePage又は文字エンコーディング名>"
                    ,  "/f:<ファイルリストのファイル名>"
                    ,  "/fc:<ファイルリストの文字エンコーディング名>"
                    ,  "\n"
                    ,  "置換する単語のリストはCSVに記述して /r: でそのファイル名を指定します。/r:置換単語リストCSV"
                    ,  ""
                    ,  "・置換単語リストCSVの内容の例、"
                    ,  ""
                    ,  "検索ワード1, 置換ワード1"
                    ,  "検索ワード2, 置換ワード2"
                    ,  "検索ワード3, 置換ワード3"
                    ,  ".,."
                    ,  ".,."
                    ,  "検索ワードn, 置換ワードn"
                    ,  "　"
                    ,  "※１　オプションの : の前後にはスペースを入れないでください。"
                    ,  "※２　検索ワードと置換ワードには \\r\\n の形で改行コードを記述することもできます。"
                    ,  "\n"
                    ,  "例として、\n"
                    ,  "・文字列を置換するときの使用例、"
                    ,  ""
                    ,  "rmsmf /r:words.csv *.txt "
                    ,  "    (基本の使い方)"
                    ,  ""
                    ,  "rmsmf /r:words.csv *.txt /c:shift_jis /w:utf-8 /b:true "
                    ,  "    (文字エンコーディングを shift_jis から utf-8 へ変更しBOMを付ける)"
                    ,  ""
                    ,  "rmsmf /r:words.csv *.txt /c:shift_jis /w:utf-8 /b:false "
                    ,  "    (文字エンコーディングを shift_jis から utf-8 へ変更しBOMを外す)"
                    ,  ""
                    ,  ""
                    ,  "・文字エンコーディングを変更するだけの例。\n"
                    ,  ""
                    ,  "rmsmf *.txt /c:shift_jis /w:utf-8 /b:true "
                    ,  ""
                    ,  "rmsmf *.txt /c:utf-8 /w:shift_jis "
                    ,  ""
                    ,  "    （オプションの位置は自由です）"
                    ,  ""
                    ,  ""
                    ,  "・ファイルリストに記載されるファイルのみ置換対象にする例。"
                    ,  ""
                    ,  "rmsmf /r:words.csv /f:filelist.txt "
                    ,  ""
                    ,  "(ファイルリストは、ファイル名又はフルパス名が、一行づつ記述されたテキストファイルです)"
                    ,  ""
                };

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Help() {}

        /// <summary>
        /// ヘルプを表示する
        /// </summary>
        public void Show()
        {
            foreach (string message in this._helpMessage)
            {
                Console.WriteLine(message);
            }
        }

    }
}
