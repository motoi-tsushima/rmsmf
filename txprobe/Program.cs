using rmsmf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Text files probe.
/// 複数のファイルの以下の内容を確認します。
/// (1) BOMの有無
/// (2) 改行コードの種類（CRLF、LF、CR）
/// (3) 文字コード（UTF-8、UTF-16LE、UTF-16BE、Shift_JIS, JIS, EUC-JP, ASCIIなど） 
/// </summary>
namespace txprobe
{
    internal class Program
    {
        static void Main(string[] args)
        {
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

                commandOptions.ReadSearchWords();
                commandOptions.ReadFileNameList();

                //ファイルのプローブ実行
                ProbeFiles probe = new ProbeFiles(
                    commandOptions.SearchWords, commandOptions.Files, 
                    commandOptions.EnableProbe, commandOptions.OutputFileNameListFileName,
                    commandOptions.FilesEncoding);

                probe.Probe(commandOptions.ReadEncoding);

                //正常に処理を完了した。
                Console.WriteLine("");
                Console.WriteLine("Search complete.");
            }
            catch (RmsmfException ex)
            {
                Console.WriteLine(ex.Message);
#if DEBUG
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
#endif
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine("予期しないエラーが発生しました: " + ex.Message);
                Console.WriteLine(ex.ToString());
#if DEBUG
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
#endif
                return;
            }

#if DEBUG
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
#endif
        }
    }
}
