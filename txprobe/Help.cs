using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rmsmf;

namespace txprobe
{
    public class Help
    {
        private string[] _helpMessage;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Help()
        {
            // カルチャー情報に基づいて適切な言語のヘルプメッセージを取得
            string languageCode = HelpMessages.GetLanguageCode();
            this._helpMessage = HelpMessages.GetTxprobeHelpMessage(languageCode);
        }

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
