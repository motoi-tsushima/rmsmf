using Microsoft.VisualStudio.TestTools.UnitTesting;
using rmsmf;

namespace rmsmf.Tests
{
    /// <summary>
    /// FileConstants クラスのテスト
    /// </summary>
    [TestClass]
    public class FileConstantsTests
    {
        [TestMethod]
        public void MaxFileSize_IsCorrectValue()
        {
            // Assert
            Assert.AreEqual(int.MaxValue, FileConstants.MaxFileSize);
            Assert.AreEqual(2147483647, FileConstants.MaxFileSize);
        }

        [TestMethod]
        public void TempFileExtension_IsCorrectValue()
        {
            // Assert
            Assert.AreEqual(".RP$", FileConstants.TempFileExtension);
        }

        [TestMethod]
        public void BackupFileExtension_IsCorrectValue()
        {
            // Assert
            Assert.AreEqual(".BACKUP$", FileConstants.BackupFileExtension);
        }

        [TestMethod]
        public void BomBufferSize_IsCorrectValue()
        {
            // Assert
            Assert.AreEqual(4, FileConstants.BomBufferSize);
        }

        [TestMethod]
        public void MinFileContentSize_IsCorrectValue()
        {
            // Assert
            Assert.AreEqual(3, FileConstants.MinFileContentSize);
        }

        [TestMethod]
        public void AllConstants_AreNotNull()
        {
            // Assert - すべての定数が有効な値を持つことを確認
            Assert.IsNotNull(FileConstants.TempFileExtension);
            Assert.IsNotNull(FileConstants.BackupFileExtension);
            Assert.IsTrue(FileConstants.MaxFileSize > 0);
            Assert.IsTrue(FileConstants.BomBufferSize > 0);
            Assert.IsTrue(FileConstants.MinFileContentSize > 0);
        }
    }
}
