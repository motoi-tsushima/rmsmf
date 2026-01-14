using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            ExecutionState.className = "Program.Main";
            ExecutionState.stepNumber = 1;

            //Show version
            Assembly thisAssem = typeof(Program).Assembly;
            AssemblyName thisAssemName = thisAssem.GetName();
            AssemblyCopyrightAttribute[] copyrightAttributes = (AssemblyCopyrightAttribute[])thisAssem.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);

            Version ver = thisAssemName.Version;
            String copyright = copyrightAttributes[0].Copyright;

            Console.WriteLine("{0}  version {1}  {2}\n", thisAssemName.Name, ver, copyright);


            CommandOptions commandOptions = null;

            try
            {
                //コマンドオプション取得
                commandOptions = new CommandOptions(args);

                //ヘルプ表示
                if (commandOptions.CallHelp == true)
                {
                    Help help = new Help();
                    help.Show();
                    //Console.WriteLine("\nPress any key to exit...");
                    //Console.ReadKey();
                    return;
                }

                commandOptions.ReadSearchWords();
                commandOptions.ReadFileNameList();

                //ファイルのプローブ実行
                ProbeFiles probe = new ProbeFiles(
                    commandOptions.SearchWords, commandOptions.Files, 
                    commandOptions.EnableProbe, commandOptions.OutputFileNameListFileName);

                probe.Probe(commandOptions.encoding);

                //正常に処理を完了した。
                Console.WriteLine("");
                Console.WriteLine("Search complete.");
            }
            catch (Exception ex)
            {
                if (ExecutionState.isError)
                {
                    Console.WriteLine(ExecutionState.errorMessage);
                    //Console.WriteLine("className = " + ExecutionState.className);
                    //Console.WriteLine("stepNumber = " + ExecutionState.stepNumber);
                }
                else
                {
                    Console.WriteLine(ExecutionState.errorMessage);
                    Console.WriteLine("className = " + ExecutionState.className);
                    Console.WriteLine("stepNumber = " + ExecutionState.stepNumber);
                    Console.WriteLine("管理されていないエラーが発生しました。" + ex.ToString());
                    throw ex;
                }

                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
