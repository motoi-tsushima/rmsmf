using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using rmsmf;

namespace rmsmf.Tests
{
    /// <summary>
    /// CommandOptions クラスのテスト
    /// </summary>
    [TestClass]
    public class CommandOptionsTests
    {
        [TestMethod]
        [ExpectedException(typeof(RmsmfException))]
        public void Constructor_WithNoParameters_ThrowsException()
        {
            // Arrange
            string[] args = { };

            // Act
            var options = new CommandOptions(args);

            // Assert - Exception expected (MissingTargetFileName)
        }

        [TestMethod]
        public void Constructor_WithValidFileParameter_CreatesInstance()
        {
            // Arrange
            string[] args = { "*.txt", "/w:UTF-8" };

            // Act
            var options = new CommandOptions(args);

            // Assert
            Assert.IsNotNull(options);
            Assert.IsNotNull(options.WriteEncoding);
        }

        [TestMethod]
        [ExpectedException(typeof(RmsmfException))]
        public void Constructor_WithMissingEncodingName_ThrowsException()
        {
            // Arrange
            string[] args = { "*.txt", "/c" };  // エンコーディング名が指定されていない

            // Act
            var options = new CommandOptions(args);

            // Assert - Exception expected
        }

        [TestMethod]
        [ExpectedException(typeof(RmsmfException))]
        public void Constructor_WithOnlyWriteOptionAndNoConversion_ThrowsException()
        {
            // Arrange
            string[] args = { "*.txt" };  // 変換オプションがない

            // Act
            var options = new CommandOptions(args);

            // Assert - Exception expected
        }

        [TestMethod]
        [ExpectedException(typeof(RmsmfException))]
        public void Constructor_WithFileListAndCommandLineFiles_ThrowsException()
        {
            // Arrange
            string[] args = { "*.txt", "/f:files.txt" };  // /f と コマンドラインファイルの競合

            // Act
            var options = new CommandOptions(args);

            // Assert - Exception expected
        }

        [TestMethod]
        public void Constructor_WithWriteCharacterSet_SetsEmptyWriteCharacterSetFalse()
        {
            // Arrange
            string[] args = { "*.txt", "/w:UTF-8" };

            // Act
            var options = new CommandOptions(args);

            // Assert
            Assert.IsFalse(options.EmptyWriteCharacterSet);
        }

        [TestMethod]
        public void Constructor_WithoutWriteCharacterSet_SetsEmptyWriteCharacterSetTrue()
        {
            // Arrange
            string[] args = { "*.txt", "/c:UTF-8", "/b:true" };

            // Act
            var options = new CommandOptions(args);

            // Assert
            Assert.IsTrue(options.EmptyWriteCharacterSet);
        }

        [TestMethod]
        public void Constructor_WithBomOptionTrue_SetsEnableBOMTrue()
        {
            // Arrange
            string[] args = { "*.txt", "/w:UTF-8", "/b:true" };

            // Act
            var options = new CommandOptions(args);

            // Assert
            Assert.IsTrue(options.EnableBOM.HasValue);
            Assert.IsTrue(options.EnableBOM.Value);
        }

        [TestMethod]
        public void Constructor_WithBomOptionFalse_SetsEnableBOMFalse()
        {
            // Arrange
            string[] args = { "*.txt", "/w:UTF-8", "/b:false" };

            // Act
            var options = new CommandOptions(args);

            // Assert
            Assert.IsTrue(options.EnableBOM.HasValue);
            Assert.IsFalse(options.EnableBOM.Value);
        }

        [TestMethod]
        public void Constructor_WithNewLineCRLF_SetsCorrectly()
        {
            // Arrange
            string[] args = { "*.txt", "/w:UTF-8", "/nl:crlf" };

            // Act
            var options = new CommandOptions(args);

            // Assert
            Assert.AreEqual(CommandOptions.NewLineCRLF, options.WriteNewLine);
        }

        [TestMethod]
        public void Constructor_WithNewLineLF_SetsCorrectly()
        {
            // Arrange
            string[] args = { "*.txt", "/w:UTF-8", "/nl:lf" };

            // Act
            var options = new CommandOptions(args);

            // Assert
            Assert.AreEqual(CommandOptions.NewLineLF, options.WriteNewLine);
        }

        [TestMethod]
        public void Constructor_WithNewLineCR_SetsCorrectly()
        {
            // Arrange
            string[] args = { "*.txt", "/w:UTF-8", "/nl:cr" };

            // Act
            var options = new CommandOptions(args);

            // Assert
            Assert.AreEqual(CommandOptions.NewLineCR, options.WriteNewLine);
        }

        [TestMethod]
        public void Constructor_WithAllDirectoriesOption_CreatesInstance()
        {
            // Arrange
            string[] args = { "*.txt", "/w:UTF-8", "/d" };

            // Act
            var options = new CommandOptions(args);

            // Assert
            Assert.IsNotNull(options);
            Assert.IsNotNull(options.WriteEncoding);
        }

        [TestMethod]
        public void Constructor_WithHelpOption_SetsHelpOrVersionDisplayed()
        {
            // Arrange
            string[] args = { "/h" };

            // Act
            var options = new CommandOptions(args);

            // Assert
            Assert.IsTrue(options.HelpOrVersionDisplayed);
        }

        [TestMethod]
        public void Constructor_WithVersionOption_SetsHelpOrVersionDisplayed()
        {
            // Arrange
            string[] args = { "/v" };

            // Act
            var options = new CommandOptions(args);

            // Assert
            Assert.IsTrue(options.HelpOrVersionDisplayed);
        }

        [TestMethod]
        public void Constructor_WithCultureInfoOption_DoesNotThrow()
        {
            // Arrange
            string[] args = { "*.txt", "/w:UTF-8", "/ci:en-US" };

            // Act
            var options = new CommandOptions(args);

            // Assert
            Assert.IsNotNull(options);
            Assert.AreEqual("en-US", options.CultureInfo);
        }

        [TestMethod]
        [ExpectedException(typeof(RmsmfException))]
        public void Constructor_WithInvalidCultureInfo_ThrowsException()
        {
            // Arrange
            string[] args = { "*.txt", "/w:UTF-8", "/ci:invalid-culture" };

            // Act
            var options = new CommandOptions(args);

            // Assert - Exception expected
        }

        [TestMethod]
        [ExpectedException(typeof(RmsmfException))]
        public void Constructor_WithNonExistentReplaceFile_ThrowsException()
        {
            // Arrange
            string[] args = { "*.txt", "/r:nonexistent.csv" };

            // Act
            var options = new CommandOptions(args);

            // Assert - Exception expected
        }

        [TestMethod]
        [ExpectedException(typeof(RmsmfException))]
        public void Constructor_WithNonExistentFileList_ThrowsException()
        {
            // Arrange
            string[] args = { "/f:nonexistent.txt", "/w:UTF-8" };

            // Act
            var options = new CommandOptions(args);

            // Assert - Exception expected
        }

        [TestMethod]
        [ExpectedException(typeof(RmsmfException))]
        public void Constructor_WithReplaceWordsButNoTargetFiles_ThrowsException()
        {
            // Arrange
            // 置換単語ファイルは指定されているが、対象ファイルが指定されていない
            string tempFile = System.IO.Path.GetTempFileName();
            
            try
            {
                System.IO.File.WriteAllText(tempFile, "search,replace", System.Text.Encoding.UTF8);
                string[] args = { $"/r:{tempFile}" };

                // Act
                var options = new CommandOptions(args);

                // Assert - Exception expected
            }
            finally
            {
                if (System.IO.File.Exists(tempFile))
                {
                    System.IO.File.Delete(tempFile);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(RmsmfException))]
        public void Constructor_WithReplaceWordsEncodingButNoReplaceFile_ThrowsException()
        {
            // Arrange
            string[] args = { "*.txt", "/w:UTF-8", "/rc:UTF-8" };

            // Act
            var options = new CommandOptions(args);

            // Assert - Exception expected
        }

        [TestMethod]
        [ExpectedException(typeof(RmsmfException))]
        public void Constructor_WithFileListEncodingButNoFileList_ThrowsException()
        {
            // Arrange
            string[] args = { "*.txt", "/w:UTF-8", "/fc:UTF-8" };

            // Act
            var options = new CommandOptions(args);

            // Assert - Exception expected
        }
    }
}
