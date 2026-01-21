using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace txprobe
{
    public class Help
    {
        private string[] _helpMessage =
        {
                    "txprobe（テキストファイル探索ツール）\n"
                    ,  "txprobe <オプション>  <探索するファイル名> \n"
                    ,  ""
                    ,  "・オプション"
                    ,  ""
                    ,  "/d  サブディレクトリも検索対象に含める。"
                    ,  "/c:<読み込みファイルのCodePage又は文字エンコーディング名>"
                    ,  "/f:<ファイルリストのファイル名>"
                    ,  "/fc:<ファイルリストの文字エンコーディング名>"
                    ,  "/s:<検索単語リストのファイル名>"
                    ,  "/sc:<検索単語リストのファイルの文字エンコーディング名>"
                    ,  "/p:<プローブモード有効（検索単語が見つかったファイルのみ表示）>"
                    ,  "/j:<0|1|3> エンコーディング自動判定モード(初期値は 0 )"
                    ,  "\n"
                    ,  "・検索する単語のリスト。（検索単語リストファイル）"
                    ,  ""
                    ,  "検索ワード1"
                    ,  "検索ワード2"
                    ,  "検索ワード3"
                    ,  "..."
                    ,  "検索ワードn"
                    ,  "\n"
                    ,  "例として、\n"
                    ,  "txprobe *.txt "
                    ,  "  （カレントディレクトリの全txtファイルの文字エンコーディングと改行コードを表示）"
                    ,  ""
                    ,  "txprobe /s:search.txt *.txt "
                    ,  "  （search.txtに記載された単語を検索し、見つかったファイルの情報を表示）"
                    ,  ""
                    ,  "txprobe /s:search.txt /p *.txt "
                    ,  "  （プローブモード：検索単語が見つかったファイル名と単語を表示）"
                    ,  ""
                    ,  "txprobe /d *.txt "
                    ,  "  （サブディレクトリも含めて検索）"
                    ,  ""
                    ,  "txprobe /c:utf-8 *.txt "
                    ,  "  （文字エンコーディングをUTF-8に指定して検索）"
                    ,  ""
                    ,  "（オプションの位置は自由です）"
                    ,  ""
                    ,  "エンコーディング自動判定モード は、"
                    ,  "/j:0 が通常モード(独自処理で判定できない場合はサードパーティー製品で判定する)"
                    ,  "/j:1 が独自処理だけで自動判定"
                    ,  "/j:3 がサードパーティー製品のみによる自動判定"
                    ,  "となります。"
                    ,  "指定しない場合は通常モードになります。"
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
