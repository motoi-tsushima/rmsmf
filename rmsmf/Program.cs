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
                    return;
                }

                commandOptions.ReadReplaceWords();
                commandOptions.ReadFileNameList();

                //ファイルの文字列置換処理の実行
                ReplaceStringsInFiles replace = new ReplaceStringsInFiles(commandOptions.ReplaceWords, commandOptions.Files, commandOptions.EnableBOM);

                replace.Replace(commandOptions.encoding, commandOptions.writeEncoding);

                //正常に処理を完了した。
                Console.WriteLine("Successful.");
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

                return;
            }

        }
    }
}
