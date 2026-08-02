using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace txprobe.Tests
{
    /// <summary>
    /// Program.Run が CommandOptions.EncodingDetectionMode を ProbeFiles へ正しく連携しているかのテスト
    /// （/det: オプションが実処理に反映されない回帰を防止する）
    /// </summary>
    [TestClass]
    public class ProgramEncodingDetectionModeTests
    {
        private string _tempFile;

        [TestInitialize]
        public void Initialize()
        {
            _tempFile = "test_greek_" + Guid.NewGuid().ToString() + ".txt";

            // Windows-1253（ギリシャ語）でファイルを作成する
            // rmsmf/txprobeの独自判定（NativeOnly）はギリシャ語系エンコーディングに一切対応していないため、
            // /det:1（FirstParty=NativeOnly）指定時は必ず「判定不可」になるはずである
            var greekEncoding = Encoding.GetEncoding(1253);
            string greekText = "Καλημέρα κόσμε. Αυτό είναι ένα τεστ κειμένου.";
            File.WriteAllBytes(_tempFile, greekEncoding.GetBytes(greekText));
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(_tempFile))
            {
                File.Delete(_tempFile);
            }
        }

        [TestMethod]
        public void Run_WithDetectionModeFirstParty_TreatsLegacyGreekEncodingAsUnknown()
        {
            // Arrange
            string[] args = { _tempFile, "/det:1" };
            var commandOptions = new CommandOptions(args);

            // Act
            string output = RunAndCaptureConsoleOutput(commandOptions);

            // Assert
            // NativeOnly（独自実装のみ）ではギリシャ語系エンコーディングを検出できないため、
            // 「encoding Unknown」として表示されなければならない
            Assert.IsTrue(output.Contains("encoding Unknown"),
                "「/det:1」指定時はNativeOnly判定のみが使われるべきだが、実際の出力は: " + output);
        }

        [TestMethod]
        public void Run_WithDefaultDetectionMode_DetectsLegacyGreekEncoding()
        {
            // Arrange
            // オプション未指定＝デフォルトのNormal（Combined = 独自実装 + UtfUnknownフォールバック）
            string[] args = { _tempFile };
            var commandOptions = new CommandOptions(args);

            // Act
            string output = RunAndCaptureConsoleOutput(commandOptions);

            // Assert
            // Combined戦略ではUtfUnknownによるフォールバックが働き、
            // ギリシャ語エンコーディングとして検出できるはずである（Unknownにはならない）
            Assert.IsFalse(output.Contains("encoding Unknown"),
                "デフォルト（Combined）判定ではエンコーディングを検出できるはずだが、実際の出力は: " + output);
        }

        [TestMethod]
        public void Run_WithDetectionModeThirdParty_DetectsLegacyGreekEncoding()
        {
            // Arrange
            string[] args = { _tempFile, "/det:3" };
            var commandOptions = new CommandOptions(args);

            // Act
            string output = RunAndCaptureConsoleOutput(commandOptions);

            // Assert
            // ThirdParty（UtfUnknownOnly）でもギリシャ語エンコーディングを検出できるはずである
            Assert.IsFalse(output.Contains("encoding Unknown"),
                "「/det:3」指定時はUtfUnknownOnly判定でエンコーディングを検出できるはずだが、実際の出力は: " + output);
        }

        /// <summary>
        /// Program.Run を実行し、コンソール出力をキャプチャして返す
        /// </summary>
        private string RunAndCaptureConsoleOutput(CommandOptions commandOptions)
        {
            TextWriter originalOut = Console.Out;
            var writer = new StringWriter();
            try
            {
                Console.SetOut(writer);
                Program.Run(commandOptions);
            }
            finally
            {
                Console.SetOut(originalOut);
            }

            return writer.ToString();
        }
    }
}
