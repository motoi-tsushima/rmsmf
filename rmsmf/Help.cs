using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rmsmf
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
            this._helpMessage = HelpMessages.GetRmsmfHelpMessage(languageCode);
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
