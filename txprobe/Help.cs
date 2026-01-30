using System;
using System.Collections.Generic;
using System.Globalization;
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

        /// <summary>
        /// 使用可能なカルチャー情報の一覧を表示する
        /// </summary>
        public void ShowAvailableCultures()
        {
            Console.WriteLine("Available Culture Information for /ci: option:");
            Console.WriteLine("==============================================");
            Console.WriteLine();

            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            var sortedCultures = cultures
                .Where(c => !string.IsNullOrEmpty(c.Name))
                .OrderBy(c => c.Name)
                .ToList();

            // Display in 4 columns
            int columns = 4;
            int rows = (sortedCultures.Count + columns - 1) / columns;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    int index = row + col * rows;
                    if (index < sortedCultures.Count)
                    {
                        Console.Write(sortedCultures[index].Name.PadRight(20));
                    }
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// 使用可能なエンコーディング情報の一覧を表示する
        /// </summary>
        public void ShowAvailableEncodings()
        {
            Console.WriteLine("Available Encodings for /c: and /w: options:");
            Console.WriteLine("=============================================");
            Console.WriteLine();

            EncodingInfo[] encodings = Encoding.GetEncodings();
            var sortedEncodings = encodings.OrderBy(e => e.Name).ToList();

            // Display in 3 columns with encoding name and code page
            int columns = 3;
            int rows = (sortedEncodings.Count + columns - 1) / columns;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    int index = row + col * rows;
                    if (index < sortedEncodings.Count)
                    {
                        EncodingInfo enc = sortedEncodings[index];
                        string displayText = $"{enc.Name} ({enc.CodePage})";
                        Console.Write(displayText.PadRight(30));
                    }
                }
                Console.WriteLine();
            }
        }

    }
}
