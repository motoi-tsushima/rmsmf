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
                    //"rmsmf (Replace Multiple Strings in Multiple Files)\n"
                    //,"  rmsmf <File name to replace words (wildcards allowed)>\n"
                    //," Options  "
                    //,"   /c:<Character set name of read file OR CodePage>"
                    //,"   /w:<Character set name of write file OR CodePage>"
                    //,"   /f:<Files LIst FileName>"
                    //,"   /fc:<Files LIst FileName Character set name>"
                    //,"   /r:<Specify the CSV file name of the list of words to be replaced>"
                    //,"   /rc:<CSV file Character set name>"
                    //,"   /b:<If you write a BOM, write true, otherwise write false>"
                    //,"\n"
                    //,"the list of words to be replaced. (words csv file)"
                    //,""
                    //,"Search word 1, replacement word 1"
                    //,"Search word 2, Replace word 2"
                    //,"Search word 3, Replace word 3"
                    //,".,."
                    //,".,."
                    //,"Search word n, replacement word n"
                    //,"\n"
                    //,"as an example.\n"
                    //,"rmsmf /r:words.csv .\\*.txt "
                    //,""
                    //,"rmsmf /r:words.csv .\\*.txt /c:utf-16 /w:utf-32 /b:true "
                    //,""
                    //,"rmsmf /r:words.csv .\\*.txt /c:shift_jis /w:utf-8 /b:false "
                    //,""
                    //,""
                    //,"An example of simply changing the character set.\n"
                    //,"rmsmf .\\*.txt /c:shift_jis /w:utf-8 /b:true "
                    //,"(The position of the option is free)"
                    //,""
                    "rmsmf（複数のファイルの複数の文字列を置き換える）\n"
                    ,  "rmsmf <単語を置き換えるファイル名（ワイルドカードを使用できます）> \n"
                    ,  "<オプション>"
                    ,  "/c:<読み込みファイルのCodePage又は文字エンコーディング名>"
                    ,  "/w:<書き込みファイルのCodePage又は文字エンコーディング名>"
                    ,  "/f:<ファイルリストのファイル名>"
                    ,  "/fc:<ファイルリストの文字エンコーディング名>"
                    ,  "/r:<置換単語リストCSVのファイル名>"
                    ,  "/rc:<置換単語リストCSVのファイルの文字エンコーディング名>"
                    ,  "/b:<BOMを作成する場合は, trueを記述し, そうでない場合はfalseを記述します>"
                    ,  "\n"
                    ,  "置換する単語のリスト。（置換単語リストCSVファイル）"
                    ,  ""
                    ,  "検索ワード1, 置換ワード1"
                    ,  "検索ワード2, 置換ワード2"
                    ,  "検索ワード3, 置換ワード3"
                    ,  ".,."
                    ,  ".,."
                    ,  "検索ワードn, 置換ワードn"
                    ,  "\n"
                    ,  "例として、\n"
                    ,  "rmsmf /r:words.csv .\\*.txt "
                    ,  ""
                    ,  "rmsmf /r:words.csv .\\*.txt /c:utf-16 /w:utf-32 /b:true "
                    ,  ""
                    ,  "rmsmf /r:words.csv .\\*.txt /c:shift_jis /w:utf-8 /b:false "
                    ,  ""
                    ,  ""
                    ,  "文字エンコーディング名を変更するだけの例。\n"
                    ,  "rmsmf .\\*.txt /c:shift_jis /w:utf-8 /b:true "
                    ,  "（オプションの位置は自由です）"
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
