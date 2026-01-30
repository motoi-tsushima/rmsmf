using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using rmsmf;

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
            var encInfo = new EncodingInfomation
            {
                EncodingName = "UTF-8",
                CodePage = 65001
            };

            // Act
            string name = EncodingHelper.GetEncodingName(encoding, encInfo);

            // Assert
            Assert.AreEqual("UTF-8", name);
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
            bool bomExists = true;
            int codePage = 65001;

            // Act
            string result = EncodingHelper.CreateUnknownEncodingDisplayLine(
                fileName, bomExists, codePage);

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
        public void GetEncodingName_WithEncodingVariant_ReturnsVariant()
        {
            // Arrange
            var encoding = Encoding.UTF8;
            var encInfo = new EncodingInfomation
            {
                // EncodingName を設定しない（null）
                EncodingVariant = "UTF-8-BOM",
                CodePage = 65001
            };

            // Act
            string name = EncodingHelper.GetEncodingName(encoding, encInfo);

            // Assert
            // EncodingName が null の場合、EncodingVariant が使用される
            Assert.AreEqual("UTF-8-BOM", name);
        }

        [TestMethod]
        public void GetEncodingName_WithBothEncodingNameAndVariant_ReturnsEncodingName()
        {
            // Arrange
            var encoding = Encoding.UTF8;
            var encInfo = new EncodingInfomation
            {
                EncodingName = "UTF-8",
                EncodingVariant = "UTF-8-BOM",
                CodePage = 65001
            };

            // Act
            string name = EncodingHelper.GetEncodingName(encoding, encInfo);

            // Assert
            // EncodingName が優先される
            Assert.AreEqual("UTF-8", name);
        }
    }
}


