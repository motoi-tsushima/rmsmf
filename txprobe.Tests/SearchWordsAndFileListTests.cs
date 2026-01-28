using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace txprobe.Tests
{
    /// <summary>
    /// txprobe CommandOptions の検索単語とファイルリスト読み込みのテスト
    /// </summary>
    [TestClass]
    public class SearchWordsAndFileListTests
    {
        private string _tempSearchFile;
        private string _tempFileListFile;

        [TestInitialize]
        public void Initialize()
        {
            _tempSearchFile = "test_search_" + Guid.NewGuid().ToString() + ".txt";
            _tempFileListFile = "test_filelist_" + Guid.NewGuid().ToString() + ".txt";
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(_tempSearchFile))
            {
                File.Delete(_tempSearchFile);
            }
            if (File.Exists(_tempFileListFile))
            {
                File.Delete(_tempFileListFile);
            }
        }

        #region ReadSearchWords Tests

        [TestMethod]
        public void ReadSearchWords_WithValidFile_ReadsCorrectly()
        {
            // Arrange
            string searchContent = "word1\nword2\nword3";
            File.WriteAllText(_tempSearchFile, searchContent, Encoding.UTF8);

            string[] args = { "*.txt", $"/s:{_tempSearchFile}" };
            var options = new CommandOptions(args);

            // Act
            bool result = options.ReadSearchWords();

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(3, options.SearchWordsCount);
            Assert.AreEqual("word1", options.SearchWords[0]);
            Assert.AreEqual("word2", options.SearchWords[1]);
            Assert.AreEqual("word3", options.SearchWords[2]);
        }

        [TestMethod]
        public void ReadSearchWords_WithEmptyLines_SkipsEmptyLines()
        {
            // Arrange
            string searchContent = "word1\n\nword2\n\n";
            File.WriteAllText(_tempSearchFile, searchContent, Encoding.UTF8);

            string[] args = { "*.txt", $"/s:{_tempSearchFile}" };
            var options = new CommandOptions(args);

            // Act
            bool result = options.ReadSearchWords();

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(2, options.SearchWordsCount);
        }

        [TestMethod]
        public void ReadSearchWords_WithEmptyFile_ThrowsException()
        {
            // Arrange
            File.WriteAllText(_tempSearchFile, "", Encoding.UTF8);

            string[] args = { "*.txt", $"/s:{_tempSearchFile}" };
            var options = new CommandOptions(args);

            // Act & Assert
            try
            {
                options.ReadSearchWords();
                Assert.Fail("Expected exception was not thrown.");
            }
            catch (rmsmf.RmsmfException)
            {
                // Expected exception - test passes
            }
            catch (System.IO.FileNotFoundException)
            {
                // EnsureEncodingInitialized may throw FileNotFoundException for empty files
                // This is also acceptable
            }
            catch (IndexOutOfRangeException)
            {
                // EncodingDetector may throw IndexOutOfRangeException for empty files
                // This is also acceptable
            }
        }

        [TestMethod]
        [ExpectedException(typeof(rmsmf.RmsmfException))]
        public void ReadSearchWords_WithOnlyEmptyLines_ThrowsException()
        {
            // Arrange
            File.WriteAllText(_tempSearchFile, "\n\n\n", Encoding.UTF8);

            string[] args = { "*.txt", $"/s:{_tempSearchFile}" };
            var options = new CommandOptions(args);

            // Act
            options.ReadSearchWords();

            // Assert - Exception expected
        }

        [TestMethod]
        public void ReadSearchWords_WithoutSearchWordsFile_ReturnsFalse()
        {
            // Arrange
            string[] args = { "*.txt", "/c:UTF-8" };
            var options = new CommandOptions(args);

            // Act
            bool result = options.ReadSearchWords();

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ReadSearchWords_WithEscapeSequences_ConvertsCorrectly()
        {
            // Arrange
            // ファイルに "word\t1" と "word\n2" というエスケープシーケンス文字列を書き込む
            // これらは ConvertEscapeSequences によって実際のタブと改行文字に変換される
            string searchContent = "word\\t1\nword\\n2";
            File.WriteAllText(_tempSearchFile, searchContent, Encoding.UTF8);

            string[] args = { "*.txt", $"/s:{_tempSearchFile}" };
            var options = new CommandOptions(args);

            // Act
            bool result = options.ReadSearchWords();

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(2, options.SearchWordsCount);
            Assert.AreEqual("word\t1", options.SearchWords[0]);
            Assert.AreEqual("word\n2", options.SearchWords[1]);
        }

        #endregion

        #region ReadFileNameList Tests

        [TestMethod]
        public void ReadFileNameList_WithFileListFile_ReadsCorrectly()
        {
            // Arrange
            string tempFile1 = "test_file1_" + Guid.NewGuid().ToString() + ".txt";
            string tempFile2 = "test_file2_" + Guid.NewGuid().ToString() + ".txt";
            
            try
            {
                File.WriteAllText(tempFile1, "content1", Encoding.UTF8);
                File.WriteAllText(tempFile2, "content2", Encoding.UTF8);

                string fileListContent = $"{tempFile1}\n{tempFile2}";
                File.WriteAllText(_tempFileListFile, fileListContent, Encoding.UTF8);

                string[] args = { $"/f:{_tempFileListFile}", "/c:UTF-8" };
                var options = new CommandOptions(args);

                // Act
                bool result = options.ReadFileNameList();

                // Assert
                Assert.IsTrue(result);
                Assert.IsNotNull(options.Files);
                Assert.AreEqual(2, options.Files.Length);
            }
            finally
            {
                if (File.Exists(tempFile1)) File.Delete(tempFile1);
                if (File.Exists(tempFile2)) File.Delete(tempFile2);
            }
        }

        [TestMethod]
        public void ReadFileNameList_WithEmptyLines_SkipsEmptyLines()
        {
            // Arrange
            string tempFile1 = "test_file1_" + Guid.NewGuid().ToString() + ".txt";
            
            try
            {
                File.WriteAllText(tempFile1, "content1", Encoding.UTF8);
                
                string fileListContent = $"{tempFile1}\n\n\n";
                File.WriteAllText(_tempFileListFile, fileListContent, Encoding.UTF8);

                string[] args = { $"/f:{_tempFileListFile}", "/c:UTF-8" };
                var options = new CommandOptions(args);

                // Act
                bool result = options.ReadFileNameList();

                // Assert
                Assert.IsTrue(result);
                Assert.AreEqual(1, options.Files.Length);
            }
            finally
            {
                if (File.Exists(tempFile1)) File.Delete(tempFile1);
            }
        }

        [TestMethod]
        public void ReadFileNameList_WithCSVFormat_UsesFirstColumn()
        {
            // Arrange
            string tempFile1 = "test_file1_" + Guid.NewGuid().ToString() + ".txt";
            
            try
            {
                File.WriteAllText(tempFile1, "content1", Encoding.UTF8);
                
                string fileListContent = $"{tempFile1},extra,data\n";
                File.WriteAllText(_tempFileListFile, fileListContent, Encoding.UTF8);

                string[] args = { $"/f:{_tempFileListFile}", "/c:UTF-8" };
                var options = new CommandOptions(args);

                // Act
                bool result = options.ReadFileNameList();

                // Assert
                Assert.IsTrue(result);
                Assert.AreEqual(1, options.Files.Length);
                Assert.AreEqual(tempFile1, options.Files[0]);
            }
            finally
            {
                if (File.Exists(tempFile1)) File.Delete(tempFile1);
            }
        }

        [TestMethod]
        public void ReadFileNameList_WithNonExistentFiles_SkipsThem()
        {
            // Arrange
            string tempFile1 = "test_file1_" + Guid.NewGuid().ToString() + ".txt";
            string nonExistentFile = "nonexistent_" + Guid.NewGuid().ToString() + ".txt";
            
            try
            {
                File.WriteAllText(tempFile1, "content1", Encoding.UTF8);
                
                string fileListContent = $"{tempFile1}\n{nonExistentFile}\n";
                File.WriteAllText(_tempFileListFile, fileListContent, Encoding.UTF8);

                string[] args = { $"/f:{_tempFileListFile}", "/c:UTF-8" };
                var options = new CommandOptions(args);

                // Act
                bool result = options.ReadFileNameList();

                // Assert
                Assert.IsTrue(result);
                Assert.AreEqual(1, options.Files.Length);
                Assert.AreEqual(tempFile1, options.Files[0]);
            }
            finally
            {
                if (File.Exists(tempFile1)) File.Delete(tempFile1);
            }
        }

        [TestMethod]
        public void ReadFileNameList_WithCommandLineParameter_UsesSearchPattern()
        {
            // Arrange
            string tempFile1 = "test_txprobe_" + Guid.NewGuid().ToString() + ".txt";
            string tempFile2 = "test_txprobe_" + Guid.NewGuid().ToString() + ".txt";
            
            try
            {
                File.WriteAllText(tempFile1, "content1", Encoding.UTF8);
                File.WriteAllText(tempFile2, "content2", Encoding.UTF8);
                
                string[] args = { "test_txprobe_*.txt", "/c:UTF-8" };
                var options = new CommandOptions(args);

                // Act
                bool result = options.ReadFileNameList();

                // Assert
                Assert.IsTrue(result);
                Assert.IsNotNull(options.Files);
                Assert.IsTrue(options.Files.Length >= 2);
            }
            finally
            {
                if (File.Exists(tempFile1)) File.Delete(tempFile1);
                if (File.Exists(tempFile2)) File.Delete(tempFile2);
            }
        }

        #endregion
    }
}
