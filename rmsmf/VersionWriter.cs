using System;

namespace rmsmf
{
    /// <summary>
    /// バージョンとライセンス情報の表示を行うユーティリティクラス
    /// </summary>
    public static class VersionWriter
    {
        /// <summary>
        /// バージョンとライセンスの表示
        /// </summary>
        /// <param name="showLicense">ライセンス表示をする</param>
        /// <param name="asmName">アセンブリ名</param>
        /// <param name="version">本体バージョン</param>
        /// <param name="copyright">著作権表示</param>
        public static void WriteVersion(bool showLicense, string asmName, Version version, string copyright)
        {
            string[] thirdParty = {
                "This software includes the following third-party components:\n",
                "UTF.Unknown",
                "Copyright (c) 2018 Nikolay Pultsin",
                "Licensed under MIT License",
                "https://github.com/CharsetDetector/UTF-unknown",
                ""
            };

            Console.WriteLine("{0}  version {1}  {2}\n", asmName, version, copyright);
            Console.WriteLine("{0} is licensed under MIT License.", asmName);
            Console.WriteLine("https://github.com/motoi-tsushima/rmsmf");
            Console.WriteLine("https://snow-stack.net/ \n");

            if (showLicense)
            {
                foreach (string s in thirdParty)
                {
                    Console.WriteLine(s);
                }
            }
            else
            {
                Console.WriteLine("/h : Help options.  /v : Option to display version and license. \n");
            }
        }
    }
}
