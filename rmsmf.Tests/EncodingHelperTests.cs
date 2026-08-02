using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using rmsmf;
using SnowStack.EncodingProbe;

namespace rmsmf.Tests
{
    /// <summary>
    /// EncodingHelper クラスのテスト
    /// </summary>
    [TestClass]
    public class EncodingHelperTests
    {
        [TestMethod]
        public void DetectOrUseSpecifiedEncoding_WithUtf8BOM_DetectsUtf8()
        {
            // Arrange
            string tempFile = Path.GetTempFileName();
            try
            {
                // UTF-8 BOMありのファイルを作成
                File.WriteAllText(tempFile, "テストデータ", new UTF8Encoding(true));

                using (FileStream fs = new FileStream(tempFile, FileMode.Open))
                {
                    // Act
                    var result = EncodingHelper.DetectOrUseSpecifiedEncoding(
                        fs, tempFile, null, CommandOptions.EncodingDetectionType.Normal);

                    // Assert
                    Assert.IsNotNull(result);
                    Assert.IsNotNull(result.Encoding);
                    Assert.AreEqual(65001, result.CodePage); // UTF-8
                    Assert.IsTrue(result.BomExists);
                }
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void DetectOrUseSpecifiedEncoding_WithSpecifiedEncoding_UsesSpecifiedEncoding()
        {
            // Arrange
            string tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, "test data", Encoding.ASCII);
                var specifiedEncoding = Encoding.UTF8;

                using (FileStream fs = new FileStream(tempFile, FileMode.Open))
                {
                    // Act
                    var result = EncodingHelper.DetectOrUseSpecifiedEncoding(
                        fs, tempFile, specifiedEncoding, CommandOptions.EncodingDetectionType.Normal);

                    // Assert
                    Assert.IsNotNull(result);
                    Assert.AreEqual(specifiedEncoding, result.Encoding);
                    Assert.AreEqual(65001, result.CodePage);
                }
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void DetectOrUseSpecifiedEncoding_WithUtf8NoBOM_DetectsUtf8()
        {
            // Arrange
            string tempFile = Path.GetTempFileName();
            try
            {
                // UTF-8 BOMなしのファイルを作成
                File.WriteAllText(tempFile, "test data テスト", new UTF8Encoding(false));

                using (FileStream fs = new FileStream(tempFile, FileMode.Open))
                {
                    // Act
                    var result = EncodingHelper.DetectOrUseSpecifiedEncoding(
                        fs, tempFile, null, CommandOptions.EncodingDetectionType.Normal);

                    // Assert
                    Assert.IsNotNull(result);
                    Assert.IsNotNull(result.Encoding);
                    Assert.IsFalse(result.BomExists); // BOMなし
                }
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void GetEncodingName_WithValidEncoding_ReturnsEncodingName()
        {
            // Arrange
            var encoding = Encoding.UTF8;
            // EncodingInformationはSnowStack.EncodingProbe側でしか生成できないため、実際の判定結果を使用する
            // ASCII文字だけだと"us-ascii"と判定されてしまうため、多バイト文字を含める
            byte[] utf8Bytes = new UTF8Encoding(false).GetBytes("test data テスト");
            EncodingInformation encInfo = EncodingProbe.Detect(utf8Bytes);

            // Act
            string name = EncodingHelper.GetEncodingName(encoding, encInfo);

            // Assert
            Assert.AreEqual("utf-8", name);
        }

        [TestMethod]
        public void GetEncodingName_WithNullEncoding_ReturnsUnknown()
        {
            // Act
            string name = EncodingHelper.GetEncodingName(null, null);

            // Assert
            Assert.AreEqual("encoding Unknown", name);
        }

        [TestMethod]
        public void GetBomDisplayString_WithBomExists_ReturnsBomExists()
        {
            // Act
            string result = EncodingHelper.GetBomDisplayString(true);

            // Assert
            Assert.AreEqual("BOM exists", result);
        }

        [TestMethod]
        public void GetBomDisplayString_WithNoBom_ReturnsNoBOM()
        {
            // Act
            string result = EncodingHelper.GetBomDisplayString(false);

            // Assert
            Assert.AreEqual("No BOM", result);
        }

        [TestMethod]
        public void CreateUnknownEncodingDisplayLine_CreatesCorrectFormat()
        {
            // Arrange
            string fileName = "test.txt";
            var encodingResult = new EncodingDetectionResult
            {
                BomExists = true,
                CodePage = 65001,
                LineBreak = LineBreakType.None,
                EncodingInfo = null
            };

            // Act
            string result = EncodingHelper.CreateUnknownEncodingDisplayLine(fileName, encodingResult);

            // Assert
            Assert.IsTrue(result.Contains(fileName));
            Assert.IsTrue(result.Contains("BOM exists"));
        }

        [TestMethod]
        public void DetectOrUseSpecifiedEncoding_WithShiftJIS_DetectsCorrectly()
        {
            // Arrange
            string tempFile = Path.GetTempFileName();
            try
            {
                // Shift_JIS でファイルを作成
                var shiftJis = Encoding.GetEncoding(932);
                File.WriteAllText(tempFile, "テストデータ", shiftJis);

                using (FileStream fs = new FileStream(tempFile, FileMode.Open))
                {
                    // Act
                    var result = EncodingHelper.DetectOrUseSpecifiedEncoding(
                        fs, tempFile, null, CommandOptions.EncodingDetectionType.Normal);

                    // Assert
                    Assert.IsNotNull(result);
                    // エンコーディングが検出されること（Shift_JISまたはその互換）
                    Assert.IsNotNull(result.Encoding);
                }
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void GetEncodingName_WithEncInfoWithoutWebName_FallsBackToEncodingWebName()
        {
            // Arrange
            // EncodingInformationはSnowStack.EncodingProbe側でしか生成できないため、
            // どの独自判定にも一致しない1バイト（0x80）を使い、EncodingWebNameが未設定の
            // 実際の判定結果を作る
            var encoding = Encoding.UTF8;
            var options = new EncodingDetectorOptions { Strategy = DetectionStrategy.NativeOnly };
            EncodingInformation encInfo = EncodingProbe.Detect(new byte[] { 0x80 }, options);

            // Act
            string name = EncodingHelper.GetEncodingName(encoding, encInfo);

            // Assert
            // encInfo.EncodingWebNameが空の場合、encoding.WebNameにフォールバックする
            Assert.AreEqual(encoding.WebName, name);
        }
    }
}


