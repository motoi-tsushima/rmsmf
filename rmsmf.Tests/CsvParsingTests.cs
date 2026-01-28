using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using rmsmf;

namespace rmsmf.Tests
{
    /// <summary>
    /// CSV解析機能のテスト
    /// </summary>
    [TestClass]
    public class CsvParsingTests
    {
        /// <summary>
        /// テスト用の一時ファイルパス
        /// </summary>
        private string _tempCsvFile;

        /// <summary>
        /// 各テストの初期化
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            // 絶対パスの : がオプションパーサーと競合しないよう、相対パスで一時ファイルを作成
            _tempCsvFile = "test_" + Guid.NewGuid().ToString() + ".csv";
        }

        /// <summary>
        /// 各テストのクリーンアップ
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(_tempCsvFile))
            {
                File.Delete(_tempCsvFile);
            }
        }

        #region ParseCsvLine メソッドのテスト

        [TestMethod]
        public void ParseCsvLine_SimpleFields_ParsesCorrectly()
        {
            // Arrange
            var options = CreateCommandOptionsWithDummyFile();
            string line = "search,replace";

            // Act
            string[] result = options.ParseCsvLine(line);

            // Assert
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("search", result[0]);
            Assert.AreEqual("replace", result[1]);
        }

        [TestMethod]
        public void ParseCsvLine_QuotedFieldWithComma_ParsesCorrectly()
        {
            // Arrange
            var options = CreateCommandOptionsWithDummyFile();
            string line = "\"search,text\",replace";

            // Act
            string[] result = options.ParseCsvLine(line);

            // Assert
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("search,text", result[0]);
            Assert.AreEqual("replace", result[1]);
        }

        [TestMethod]
        public void ParseCsvLine_BothFieldsQuotedWithComma_ParsesCorrectly()
        {
            // Arrange
            var options = CreateCommandOptionsWithDummyFile();
            string line = "\"search,text\",\"replace,text\"";

            // Act
            string[] result = options.ParseCsvLine(line);

            // Assert
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("search,text", result[0]);
            Assert.AreEqual("replace,text", result[1]);
        }

        [TestMethod]
        public void ParseCsvLine_EscapedQuotes_ParsesCorrectly()
        {
            // Arrange
            var options = CreateCommandOptionsWithDummyFile();
            string line = "\"say \"\"Hello\"\"\",\"reply \"\"Hi\"\"\"";

            // Act
            string[] result = options.ParseCsvLine(line);

            // Assert
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("say \"Hello\"", result[0]);
            Assert.AreEqual("reply \"Hi\"", result[1]);
        }

        [TestMethod]
        public void ParseCsvLine_QuotesOnly_ParsesCorrectly()
        {
            // Arrange
            var options = CreateCommandOptionsWithDummyFile();
            string line = "\"\"\"\",\"\"\"\"";

            // Act
            string[] result = options.ParseCsvLine(line);

            // Assert
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("\"", result[0]);
            Assert.AreEqual("\"", result[1]);
        }

        [TestMethod]
        public void ParseCsvLine_MixedQuotedAndUnquoted_ParsesCorrectly()
        {
            // Arrange
            var options = CreateCommandOptionsWithDummyFile();
            string line = "simple,\"quoted,field\",another";

            // Act
            string[] result = options.ParseCsvLine(line);

            // Assert
            Assert.AreEqual(3, result.Length);
            Assert.AreEqual("simple", result[0]);
            Assert.AreEqual("quoted,field", result[1]);
            Assert.AreEqual("another", result[2]);
        }

        [TestMethod]
        public void ParseCsvLine_EmptyFields_ParsesCorrectly()
        {
            // Arrange
            var options = CreateCommandOptionsWithDummyFile();
            string line = ",";

            // Act
            string[] result = options.ParseCsvLine(line);

            // Assert
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("", result[0]);
            Assert.AreEqual("", result[1]);
        }

        [TestMethod]
        public void ParseCsvLine_QuotedEmptyFields_ParsesCorrectly()
        {
            // Arrange
            var options = CreateCommandOptionsWithDummyFile();
            string line = "\"\",\"\"";

            // Act
            string[] result = options.ParseCsvLine(line);

            // Assert
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("", result[0]);
            Assert.AreEqual("", result[1]);
        }

        [TestMethod]
        public void ParseCsvLine_ComplexExample_ParsesCorrectly()
        {
            // Arrange
            var options = CreateCommandOptionsWithDummyFile();
            string line = "\"He said, \"\"Hello, World\"\"\",\"She replied, \"\"Hi there\"\"\"";

            // Act
            string[] result = options.ParseCsvLine(line);

            // Assert
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("He said, \"Hello, World\"", result[0]);
            Assert.AreEqual("She replied, \"Hi there\"", result[1]);
        }

        #endregion

        #region ReadReplaceWords 統合テスト

        [TestMethod]
        public void ReadReplaceWords_SimpleCSV_ReadsCorrectly()
        {
            // Arrange
            string csvContent = "search1,replace1\nsearch2,replace2\nsearch3,replace3";
            File.WriteAllText(_tempCsvFile, csvContent, Encoding.UTF8);

            string[] args = { "*.txt", "/w:UTF-8", $"/r:{_tempCsvFile}" };
            var options = new CommandOptions(args);

            // Act
            bool result = options.ReadReplaceWords();

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(3, options.ReplaceWordsCount);
            Assert.AreEqual("search1", options.ReplaceWords[0, 0]);
            Assert.AreEqual("replace1", options.ReplaceWords[1, 0]);
            Assert.AreEqual("search2", options.ReplaceWords[0, 1]);
            Assert.AreEqual("replace2", options.ReplaceWords[1, 1]);
        }

        [TestMethod]
        public void ReadReplaceWords_CSVWithCommasInFields_ReadsCorrectly()
        {
            // Arrange
            string csvContent = "\"search,1\",\"replace,1\"\n\"search,2\",\"replace,2\"";
            File.WriteAllText(_tempCsvFile, csvContent, Encoding.UTF8);

            string[] args = { "*.txt", "/w:UTF-8", $"/r:{_tempCsvFile}" };
            var options = new CommandOptions(args);

            // Act
            bool result = options.ReadReplaceWords();

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(2, options.ReplaceWordsCount);
            Assert.AreEqual("search,1", options.ReplaceWords[0, 0]);
            Assert.AreEqual("replace,1", options.ReplaceWords[1, 0]);
            Assert.AreEqual("search,2", options.ReplaceWords[0, 1]);
            Assert.AreEqual("replace,2", options.ReplaceWords[1, 1]);
        }

        [TestMethod]
        public void ReadReplaceWords_CSVWithQuotesInFields_ReadsCorrectly()
        {
            // Arrange
            string csvContent = "\"say \"\"Hello\"\"\",\"reply \"\"Hi\"\"\"\n\"value \"\"quoted\"\"\",\"replaced \"\"text\"\"\"";
            File.WriteAllText(_tempCsvFile, csvContent, Encoding.UTF8);

            string[] args = { "*.txt", "/w:UTF-8", $"/r:{_tempCsvFile}" };
            var options = new CommandOptions(args);

            // Act
            bool result = options.ReadReplaceWords();

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(2, options.ReplaceWordsCount);
            Assert.AreEqual("say \"Hello\"", options.ReplaceWords[0, 0]);
            Assert.AreEqual("reply \"Hi\"", options.ReplaceWords[1, 0]);
            Assert.AreEqual("value \"quoted\"", options.ReplaceWords[0, 1]);
            Assert.AreEqual("replaced \"text\"", options.ReplaceWords[1, 1]);
        }

        [TestMethod]
        public void ReadReplaceWords_MixedQuotedAndUnquoted_ReadsCorrectly()
        {
            // Arrange
            string csvContent = "simple,replace\n\"quoted,field\",replace2\nsearch3,\"replace,3\"";
            File.WriteAllText(_tempCsvFile, csvContent, Encoding.UTF8);

            string[] args = { "*.txt", "/w:UTF-8", $"/r:{_tempCsvFile}" };
            var options = new CommandOptions(args);

            // Act
            bool result = options.ReadReplaceWords();

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(3, options.ReplaceWordsCount);
            Assert.AreEqual("simple", options.ReplaceWords[0, 0]);
            Assert.AreEqual("replace", options.ReplaceWords[1, 0]);
            Assert.AreEqual("quoted,field", options.ReplaceWords[0, 1]);
            Assert.AreEqual("replace2", options.ReplaceWords[1, 1]);
            Assert.AreEqual("search3", options.ReplaceWords[0, 2]);
            Assert.AreEqual("replace,3", options.ReplaceWords[1, 2]);
        }

        [TestMethod]
        public void ReadReplaceWords_CSVWithEmptyLines_SkipsEmptyLines()
        {
            // Arrange
            string csvContent = "search1,replace1\n\nsearch2,replace2\n\n";
            File.WriteAllText(_tempCsvFile, csvContent, Encoding.UTF8);

            string[] args = { "*.txt", "/w:UTF-8", $"/r:{_tempCsvFile}" };
            var options = new CommandOptions(args);

            // Act
            bool result = options.ReadReplaceWords();

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(2, options.ReplaceWordsCount);
        }

        [TestMethod]
        [ExpectedException(typeof(RmsmfException))]
        public void ReadReplaceWords_EmptyFile_ThrowsException()
        {
            // Arrange
            File.WriteAllText(_tempCsvFile, "", Encoding.UTF8);

            string[] args = { "*.txt", "/w:UTF-8", $"/r:{_tempCsvFile}" };
            var options = new CommandOptions(args);

            // Act
            options.ReadReplaceWords();

            // Assert - Exception expected
        }

        [TestMethod]
        public void ReadReplaceWords_LinesWithoutComma_SkipsThoseLines()
        {
            // Arrange
            // カンマがない行はスキップされ、有効な行のみが読み込まれる
            string csvContent = "noseparator\nvalid,data\nanother invalid line\nsecond,valid";
            File.WriteAllText(_tempCsvFile, csvContent, Encoding.UTF8);

            string[] args = { "*.txt", "/w:UTF-8", $"/r:{_tempCsvFile}" };
            var options = new CommandOptions(args);

            // Act
            bool result = options.ReadReplaceWords();

            // Assert - 有効な2行のみが読み込まれる
            Assert.IsTrue(result);
            Assert.AreEqual(2, options.ReplaceWordsCount);
            Assert.AreEqual("valid", options.ReplaceWords[0, 0]);
            Assert.AreEqual("data", options.ReplaceWords[1, 0]);
            Assert.AreEqual("second", options.ReplaceWords[0, 1]);
            Assert.AreEqual("valid", options.ReplaceWords[1, 1]);
        }

        [TestMethod]
        [ExpectedException(typeof(RmsmfException))]
        public void ReadReplaceWords_OnlyInvalidLines_ThrowsException()
        {
            // Arrange
            // 全ての行にカンマがない場合、空ファイルと同じ扱いで例外が発生する
            string csvContent = "noseparator\nanother invalid line\nno comma here either";
            File.WriteAllText(_tempCsvFile, csvContent, Encoding.UTF8);

            string[] args = { "*.txt", "/w:UTF-8", $"/r:{_tempCsvFile}" };
            var options = new CommandOptions(args);

            // Act
            options.ReadReplaceWords();

            // Assert - Exception expected
        }

        #endregion

        #region ReadFileNameList 統合テスト

        [TestMethod]
        public void ReadFileNameList_WithFileListFile_ReadsCorrectly()
        {
            // Arrange
            string fileListContent = "file1.txt\nfile2.txt";
            File.WriteAllText(_tempCsvFile, fileListContent, Encoding.UTF8);

            // テスト用ファイルを作成
            string tempFile1 = "test_file1_" + Guid.NewGuid().ToString() + ".txt";
            string tempFile2 = "test_file2_" + Guid.NewGuid().ToString() + ".txt";
            
            try
            {
                File.WriteAllText(tempFile1, "content1", Encoding.UTF8);
                File.WriteAllText(tempFile2, "content2", Encoding.UTF8);

                // ファイルリストを実際のファイル名で更新
                File.WriteAllText(_tempCsvFile, $"{tempFile1}\n{tempFile2}", Encoding.UTF8);

                string[] args = { $"/f:{_tempCsvFile}", "/w:UTF-8" };
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
                File.WriteAllText(_tempCsvFile, fileListContent, Encoding.UTF8);

                string[] args = { $"/f:{_tempCsvFile}", "/w:UTF-8" };
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
                File.WriteAllText(_tempCsvFile, fileListContent, Encoding.UTF8);

                string[] args = { $"/f:{_tempCsvFile}", "/w:UTF-8" };
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
                File.WriteAllText(_tempCsvFile, fileListContent, Encoding.UTF8);

                string[] args = { $"/f:{_tempCsvFile}", "/w:UTF-8" };
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
            string tempFile1 = "test_csv_" + Guid.NewGuid().ToString() + ".csv";
            string tempFile2 = "test_csv_" + Guid.NewGuid().ToString() + ".csv";
            
            try
            {
                File.WriteAllText(tempFile1, "content1", Encoding.UTF8);
                File.WriteAllText(tempFile2, "content2", Encoding.UTF8);
                
                string[] args = { "test_csv_*.csv", "/w:UTF-8" };
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

        #region ヘルパーメソッド

        /// <summary>
        /// テスト用のダミーファイルを持つCommandOptionsインスタンスを作成
        /// ParseCsvLineメソッドをテストするためのヘルパー（ファイルは削除しない）
        /// </summary>
        private CommandOptions CreateCommandOptionsWithDummyFile()
        {
            // ParseCsvLineメソッドはインスタンスメソッドなので、
            // CommandOptionsインスタンスが必要だが、ファイルの読み込みは不要
            // 最小限のオプションでインスタンスを作成
            string[] args = { "*.txt", "/w:UTF-8" };
            return new CommandOptions(args);
        }

        #endregion
    }
}
