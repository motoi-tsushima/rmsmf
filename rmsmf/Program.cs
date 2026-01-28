using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

/// <summary>
/// I replace multiple strings in multiple files.
/// 複数のファイルの複数の文字列を置き換えます。
/// </summary>
namespace rmsmf
{
    /// <summary>
    /// RMSMF (Replace Multiple Strings in Multiple Files)
    /// 複数のファイルの複数の文字列を置き換える。
    /// </summary>
    class Program
    {
        /// <summary>
        /// Multiple word multiple file replacement
        /// 複数の単語の複数のファイルの置換。
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // System.Text.Encoding.CodePagesパッケージのエンコーディングプロバイダーを登録
            // これにより、EUC-KR (51949), Shift_JIS (932), GB18030など
            // .NET Frameworkでデフォルトでサポートされていないエンコーディングが使用可能になる
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            #region Change culture for testing
            //// テスト用にカルチャーを変更
            //Console.OutputEncoding = System.Text.Encoding.UTF8;
            //string testCulture = "en-US";
            ////string testCulture = "ko";

            ////string testCulture = "zh-CN";
            ////string testCulture = "zh-Hans";

            ////string testCulture = "zh-TW";
            ////string testCulture = "zh-HK";
            //System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(testCulture);
            #endregion

            //Show version
            Assembly thisAssem = typeof(Program).Assembly;
            AssemblyName thisAssemName = thisAssem.GetName();
            AssemblyCopyrightAttribute[] copyrightAttributes = (AssemblyCopyrightAttribute[])thisAssem.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);

            Version ver = thisAssemName.Version;
            String copyright = copyrightAttributes[0].Copyright;

            CommandOptions commandOptions = null;


            try
            {
                // 引数なしの場合は簡易バージョン表示
                if (args.Length == 0)
                {
                    VersionWriter.WriteVersion(false, thisAssemName.Name, ver, copyright);
#if DEBUG
                    Console.WriteLine("\nPress any key to exit...");
                    Console.ReadKey();
#endif
                    return;
                }

                //コマンドオプション取得（カルチャー設定、ヘルプ・バージョン表示を含む）
                commandOptions = new CommandOptions(args);

                // ヘルプまたはバージョンが表示された場合は終了
                if (commandOptions.HelpOrVersionDisplayed)
                {
#if DEBUG
                    Console.WriteLine("\nPress any key to exit...");
                    Console.ReadKey();
#endif
                    return;
                }

                commandOptions.ReadReplaceWords();
                commandOptions.ReadFileNameList();

                //ファイルの文字列置換処理の実行
                ReplaceStringsInFiles replace = new ReplaceStringsInFiles(commandOptions.ReplaceWords, commandOptions.Files, commandOptions.EnableBOM);

                replace.Replace(commandOptions.ReadEncoding, commandOptions.WriteEncoding, commandOptions.WriteNewLine);

                //正常に処理を完了した。
                Console.WriteLine("Successful.");
            }
            catch (RmsmfException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine("予期しないエラーが発生しました: " + ex.Message);
                Console.WriteLine(ex.ToString());
                return;
            }
        }
    }
}
