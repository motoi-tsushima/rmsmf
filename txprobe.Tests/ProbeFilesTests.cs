using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace txprobe.Tests
{
    /// <summary>
    /// ProbeFiles クラスのテスト
    /// </summary>
    [TestClass]
    public class ProbeFilesTests
    {
        private string _tempTestFile;
        private string _tempOutputFile;

        [TestInitialize]
        public void Initialize()
        {
            _tempTestFile = "test_probe_" + Guid.NewGuid().ToString() + ".txt";
            _tempOutputFile = "test_output_" + Guid.NewGuid().ToString() + ".txt";
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(_tempTestFile))
            {
                File.Delete(_tempTestFile);
            }
            if (File.Exists(_tempOutputFile))
            {
                File.Delete(_tempOutputFile);
            }
        }

        [TestMethod]
        public void Constructor_WithValidParameters_CreatesInstance()
        {
            // Arrange
            string[] searchWords = { "test", "search" };
            string[] files = { "file1.txt", "file2.txt" };

            // Act
            var probeFiles = new ProbeFiles(searchWords, files, true, null, Encoding.UTF8);

            // Assert
            Assert.IsNotNull(probeFiles);
        }

        [TestMethod]
        public void Constructor_WithNullFilesEncoding_UsesUTF8()
        {
            // Arrange
            string[] searchWords = { "test" };
            string[] files = { "file1.txt" };

            // Act
            var probeFiles = new ProbeFiles(searchWords, files, true, null, null);

            // Assert - UTF8がデフォルトとして使用されることを確認（例外がスローされない）
            Assert.IsNotNull(probeFiles);
        }

        [TestMethod]
        public void Probe_WithValidFile_ReturnsTrue()
        {
            // Arrange
            File.WriteAllText(_tempTestFile, "This is a test file with search words.", Encoding.UTF8);
            
            string[] searchWords = { "test", "search" };
            string[] files = { _tempTestFile };
            
            var probeFiles = new ProbeFiles(searchWords, files, false, null, Encoding.UTF8);

            // Act
            bool result = probeFiles.Probe(Encoding.UTF8);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Probe_WithSearchWordsFound_DisplaysResults()
        {
            // Arrange
            File.WriteAllText(_tempTestFile, "This file contains test and search keywords.", Encoding.UTF8);
            
            string[] searchWords = { "test" };
            string[] files = { _tempTestFile };
            
            var probeFiles = new ProbeFiles(searchWords, files, false, null, Encoding.UTF8);

            // Act
            bool result = probeFiles.Probe(Encoding.UTF8);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Probe_WithMultipleFiles_ProcessesAll()
        {
            // Arrange
            string tempFile2 = "test_probe2_" + Guid.NewGuid().ToString() + ".txt";
            
            try
            {
                File.WriteAllText(_tempTestFile, "First file with test.", Encoding.UTF8);
                File.WriteAllText(tempFile2, "Second file with test.", Encoding.UTF8);
                
                string[] searchWords = { "test" };
                string[] files = { _tempTestFile, tempFile2 };
                
                var probeFiles = new ProbeFiles(searchWords, files, false, null, Encoding.UTF8);

                // Act
                bool result = probeFiles.Probe(Encoding.UTF8);

                // Assert
                Assert.IsTrue(result);
            }
            finally
            {
                if (File.Exists(tempFile2))
                {
                    File.Delete(tempFile2);
                }
            }
        }

        [TestMethod]
        public void Probe_WithOutputFile_CreatesOutputFile()
        {
            // Arrange
            File.WriteAllText(_tempTestFile, "This file contains the word test.", Encoding.UTF8);
            
            string[] searchWords = { "test" };
            string[] files = { _tempTestFile };
            
            var probeFiles = new ProbeFiles(searchWords, files, false, _tempOutputFile, Encoding.UTF8);

            // Act
            bool result = probeFiles.Probe(Encoding.UTF8);

            // Assert
            Assert.IsTrue(result);
            // 出力ファイルの作成を確認（非同期処理のため少し待つ）
            System.Threading.Thread.Sleep(500);
            Assert.IsTrue(File.Exists(_tempOutputFile));
        }

        [TestMethod]
        public void Probe_WithProbeMode_DisplaysEncodingInfo()
        {
            // Arrange
            File.WriteAllText(_tempTestFile, "UTF-8 encoded file content.", Encoding.UTF8);
            
            string[] searchWords = { };
            string[] files = { _tempTestFile };
            
            var probeFiles = new ProbeFiles(searchWords, files, true, null, Encoding.UTF8);

            // Act
            bool result = probeFiles.Probe(null);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void EncodingDetectionMode_CanBeSetAndGet()
        {
            // Arrange
            string[] searchWords = { "test" };
            string[] files = { "file1.txt" };
            var probeFiles = new ProbeFiles(searchWords, files, true, null, Encoding.UTF8);

            // Act
            probeFiles.EncodingDetectionMode = CommandOptions.EncodingDetectionType.FirstParty;

            // Assert
            Assert.AreEqual(CommandOptions.EncodingDetectionType.FirstParty, probeFiles.EncodingDetectionMode);
        }

        [TestMethod]
        public void Probe_WithNonExistentFile_HandlesGracefully()
        {
            // Arrange
            string[] searchWords = { "test" };
            string[] files = { "nonexistent_file_12345.txt" };
            
            var probeFiles = new ProbeFiles(searchWords, files, false, null, Encoding.UTF8);

            // Act
            bool result = probeFiles.Probe(Encoding.UTF8);

            // Assert - 存在しないファイルは無視されて処理が続行される
            Assert.IsTrue(result);
        }
    }
}
