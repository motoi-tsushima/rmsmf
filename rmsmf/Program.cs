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
            //Show version
            Assembly thisAssem = typeof(Program).Assembly;
            AssemblyName thisAssemName = thisAssem.GetName();
            AssemblyCopyrightAttribute[] copyrightAttributes = (AssemblyCopyrightAttribute[])thisAssem.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);

            Version ver = thisAssemName.Version;
            String copyright = copyrightAttributes[0].Copyright;

            CommandOptions commandOptions = null;

            try
            {
                // バージョンオプションの事前チェック
                if (args.Length == 0 ||
                    (args.Length > 0 && (args[0] == "-v" || args[0] == "/v" ||
                    args.Any(arg => arg == "-v" || arg == "/v")))
                    )
                {
                    Console.WriteLine("{0}  version {1}  {2}\n", thisAssemName.Name, ver, copyright);
                    Console.WriteLine("{0} is licensed under MIT License.", thisAssemName.Name);
                    Console.WriteLine("https://github.com/motoi-tsushima/rmsmf");
                    Console.WriteLine("");
                    Console.WriteLine("This software includes the following third-party components:\n");
                    Console.WriteLine("UTF.Unknown");
                    Console.WriteLine("Copyright (c) 2018 Nikolay Pultsin");
                    Console.WriteLine("Licensed under MIT License");
                    Console.WriteLine("https://github.com/CharsetDetector/UTF-unknown");
#if DEBUG
                    Console.WriteLine("\nPress any key to exit...");
                    Console.ReadKey();
#endif
                    return;
                }
                // ヘルプオプションの事前チェック
                if (args.Length > 0 && (args[0] == "-h" || args[0] == "/h" || 
                    args.Any(arg => arg == "-h" || arg == "/h")))
                {
                    Help help = new Help();
                    help.Show();
#if DEBUG
                    Console.WriteLine("\nPress any key to exit...");
                    Console.ReadKey();
#endif
                    return;
                }

                //コマンドオプション取得
                commandOptions = new CommandOptions(args);

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
