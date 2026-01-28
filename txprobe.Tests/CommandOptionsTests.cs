using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace txprobe.Tests
{
    /// <summary>
    /// txprobe CommandOptions クラスのテスト
    /// </summary>
    [TestClass]
    public class CommandOptionsTests
    {
        [TestMethod]
        [ExpectedException(typeof(rmsmf.RmsmfException))]
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
            string[] args = { "*.txt", "/c:UTF-8" };

            // Act
            var options = new CommandOptions(args);

            // Assert
            Assert.IsNotNull(options);
            Assert.IsNotNull(options.ReadEncoding);
        }

        [TestMethod]
        [ExpectedException(typeof(rmsmf.RmsmfException))]
        public void Constructor_WithMissingEncodingName_ThrowsException()
        {
            // Arrange
            string[] args = { "*.txt", "/c" };  // エンコーディング名が指定されていない

            // Act
            var options = new CommandOptions(args);

            // Assert - Exception expected
        }

        [TestMethod]
        [ExpectedException(typeof(rmsmf.RmsmfException))]
        public void Constructor_WithFileListAndCommandLineFiles_ThrowsException()
        {
            // Arrange
            string[] args = { "*.txt", "/f:files.txt" };  // /f と コマンドラインファイルの競合

            // Act
            var options = new CommandOptions(args);

            // Assert - Exception expected
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
            string[] args = { "*.txt", "/c:UTF-8", "/ci:en-US" };

            // Act
            var options = new CommandOptions(args);

            // Assert
            Assert.IsNotNull(options);
            Assert.AreEqual("en-US", options.CultureInfo);
        }

        [TestMethod]
        [ExpectedException(typeof(rmsmf.RmsmfException))]
        public void Constructor_WithInvalidCultureInfo_ThrowsException()
        {
            // Arrange
            string[] args = { "*.txt", "/c:UTF-8", "/ci:invalid-culture" };

            // Act
            var options = new CommandOptions(args);

            // Assert - Exception expected
        }

        [TestMethod]
        [ExpectedException(typeof(rmsmf.RmsmfException))]
        public void Constructor_WithNonExistentSearchFile_ThrowsException()
        {
            // Arrange
            string[] args = { "*.txt", "/s:nonexistent.txt" };

            // Act
            var options = new CommandOptions(args);

            // Assert - Exception expected
        }

        [TestMethod]
        [ExpectedException(typeof(rmsmf.RmsmfException))]
        public void Constructor_WithNonExistentFileList_ThrowsException()
        {
            // Arrange
            string[] args = { "/f:nonexistent.txt", "/c:UTF-8" };

            // Act
            var options = new CommandOptions(args);

            // Assert - Exception expected
        }

        [TestMethod]
        [ExpectedException(typeof(rmsmf.RmsmfException))]
        public void Constructor_WithSearchWordsButNoTargetFiles_ThrowsException()
        {
            // Arrange
            // 検索単語ファイルは指定されているが、対象ファイルが指定されていない
            string tempFile = "test_search_" + System.Guid.NewGuid().ToString() + ".txt";
            
            try
            {
                System.IO.File.WriteAllText(tempFile, "search", System.Text.Encoding.UTF8);
                string[] args = { $"/s:{tempFile}" };

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
        [ExpectedException(typeof(rmsmf.RmsmfException))]
        public void Constructor_WithSearchWordsEncodingButNoSearchFile_ThrowsException()
        {
            // Arrange
            string[] args = { "*.txt", "/c:UTF-8", "/sc:UTF-8" };

            // Act
            var options = new CommandOptions(args);

            // Assert - Exception expected
        }

        [TestMethod]
        [ExpectedException(typeof(rmsmf.RmsmfException))]
        public void Constructor_WithFileListEncodingButNoFileList_ThrowsException()
        {
            // Arrange
            string[] args = { "*.txt", "/c:UTF-8", "/fc:UTF-8" };

            // Act
            var options = new CommandOptions(args);

            // Assert - Exception expected
        }

        [TestMethod]
        public void Constructor_WithJudgmentModeNormal_SetsCorrectly()
        {
            // Arrange
            string[] args = { "*.txt", "/c:UTF-8", "/j" };

            // Act
            var options = new CommandOptions(args);

            // Assert
            Assert.AreEqual(CommandOptions.EncodingJudgmentType.Normal, options.EncodingJudgmentMode);
        }

        [TestMethod]
        public void Constructor_WithJudgmentModeFirstParty_SetsCorrectly()
        {
            // Arrange
            string[] args = { "*.txt", "/c:UTF-8", "/j:1" };

            // Act
            var options = new CommandOptions(args);

            // Assert
            Assert.AreEqual(CommandOptions.EncodingJudgmentType.FirstParty, options.EncodingJudgmentMode);
        }

        [TestMethod]
        public void Constructor_WithJudgmentModeThirdParty_SetsCorrectly()
        {
            // Arrange
            string[] args = { "*.txt", "/c:UTF-8", "/j:3" };

            // Act
            var options = new CommandOptions(args);

            // Assert
            Assert.AreEqual(CommandOptions.EncodingJudgmentType.ThirdParty, options.EncodingJudgmentMode);
        }

        [TestMethod]
        public void Constructor_WithProbeMode_EnablesProbeModeExplicitly()
        {
            // Arrange
            string[] args = { "*.txt", "/c:UTF-8", "/p" };

            // Act
            var options = new CommandOptions(args);

            // Assert
            Assert.IsTrue(options.EnableProbe);
        }

        [TestMethod]
        public void Constructor_WithoutSearchWords_EnablesProbeModeImplicitly()
        {
            // Arrange
            string[] args = { "*.txt", "/c:UTF-8" };

            // Act
            var options = new CommandOptions(args);

            // Assert
            Assert.IsTrue(options.EnableProbe);
        }

        [TestMethod]
        public void Constructor_WithSearchWordsFile_SetsSearchWordsFileName()
        {
            // Arrange
            string tempFile = "test_search_" + System.Guid.NewGuid().ToString() + ".txt";
            
            try
            {
                System.IO.File.WriteAllText(tempFile, "search", System.Text.Encoding.UTF8);
                string[] args = { "*.txt", $"/s:{tempFile}" };

                // Act
                var options = new CommandOptions(args);

                // Assert
                Assert.AreEqual(tempFile, options.SearchWordsFileName);
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
        public void Constructor_WithFileList_SetsFileNameListFileName()
        {
            // Arrange
            string tempFile = "test_filelist_" + System.Guid.NewGuid().ToString() + ".txt";
            
            try
            {
                System.IO.File.WriteAllText(tempFile, "file1.txt", System.Text.Encoding.UTF8);
                string[] args = { $"/f:{tempFile}", "/c:UTF-8" };

                // Act
                var options = new CommandOptions(args);

                // Assert
                Assert.AreEqual(tempFile, options.FileNameListFileName);
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
        public void Constructor_WithOutputFileNameList_SetsOutputFileNameListFileName()
        {
            // Arrange
            string[] args = { "*.txt", "/c:UTF-8", "/o:output.txt" };

            // Act
            var options = new CommandOptions(args);

            // Assert
            Assert.AreEqual("output.txt", options.OutputFileNameListFileName);
        }

        [TestMethod]
        public void Constructor_WithOutputFileNameListNoValue_UsesDefaultFileName()
        {
            // Arrange
            string[] args = { "*.txt", "/c:UTF-8", "/o" };

            // Act
            var options = new CommandOptions(args);

            // Assert
            Assert.AreEqual("output_filelist.txt", options.OutputFileNameListFileName);
        }

        [TestMethod]
        public void Constructor_WithAllDirectoriesOption_CreatesInstance()
        {
            // Arrange
            string[] args = { "*.txt", "/c:UTF-8", "/d" };

            // Act
            var options = new CommandOptions(args);

            // Assert
            Assert.IsNotNull(options);
            Assert.IsNotNull(options.ReadEncoding);
        }

        [TestMethod]
        public void Constructor_WithReadEncoding_SetsEncodingsCorrectly()
        {
            // Arrange
            string[] args = { "*.txt", "/c:UTF-8" };

            // Act
            var options = new CommandOptions(args);

            // Assert
            Assert.IsNotNull(options.ReadEncoding);
            Assert.AreEqual("utf-8", options.ReadEncoding.WebName);
        }

        [TestMethod]
        public void Constructor_WithCodePageEncoding_SetsEncodingsCorrectly()
        {
            // Arrange
            string[] args = { "*.txt", "/c:65001" };

            // Act
            var options = new CommandOptions(args);

            // Assert
            Assert.IsNotNull(options.ReadEncoding);
            Assert.AreEqual(65001, options.ReadEncoding.CodePage);
        }
    }
}
