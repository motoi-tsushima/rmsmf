using Microsoft.VisualStudio.TestTools.UnitTesting;
using rmsmf;

namespace rmsmf.Tests
{
    /// <summary>
    /// OptionConstants クラスのテスト
    /// </summary>
    [TestClass]
    public class OptionConstantsTests
    {
        [TestMethod]
        public void CharacterSetDetection_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual("Detection", OptionConstants.CharacterSetDetection);
        }

        [TestMethod]
        public void OptionHelp_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual("h", OptionConstants.OptionHelp);
        }

        [TestMethod]
        public void OptionVersion_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual("v", OptionConstants.OptionVersion);
        }

        [TestMethod]
        public void OptionCharacterSet_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual("c", OptionConstants.OptionCharacterSet);
        }

        [TestMethod]
        public void OptionWriteCharacterSet_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual("w", OptionConstants.OptionWriteCharacterSet);
        }

        [TestMethod]
        public void OptionFileNameList_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual("f", OptionConstants.OptionFileNameList);
        }

        [TestMethod]
        public void OptionFileNameListCharacterSet_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual("fc", OptionConstants.OptionFileNameListCharacterSet);
        }

        [TestMethod]
        public void OptionReplaceWords_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual("r", OptionConstants.OptionReplaceWords);
        }

        [TestMethod]
        public void OptionReplaceWordsCharacterSet_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual("rc", OptionConstants.OptionReplaceWordsCharacterSet);
        }

        [TestMethod]
        public void OptionSearchWords_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual("s", OptionConstants.OptionSearchWords);
        }

        [TestMethod]
        public void OptionSearchWordsCharacterSet_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual("sc", OptionConstants.OptionSearchWordsCharacterSet);
        }

        [TestMethod]
        public void OptionWriteByteOrderMark_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual("b", OptionConstants.OptionWriteByteOrderMark);
        }

        [TestMethod]
        public void OptionAllDirectories_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual("d", OptionConstants.OptionAllDirectories);
        }

        [TestMethod]
        public void OptionNewLine_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual("nl", OptionConstants.OptionNewLine);
        }

        [TestMethod]
        public void OptionDetectionMode_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual("det", OptionConstants.OptionDetectionMode);
        }

        [TestMethod]
        public void OptionCultureInfo_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual("ci", OptionConstants.OptionCultureInfo);
        }

        [TestMethod]
        public void OptionProbeMode_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual("p", OptionConstants.OptionProbeMode);
        }

        [TestMethod]
        public void OptionOutputFileNamelist_HasCorrectValue()
        {
            // Assert
            Assert.AreEqual("o", OptionConstants.OptionOutputFileNamelist);
        }

        [TestMethod]
        public void AllOptionConstants_AreNotNullOrEmpty()
        {
            // Assert - すべてのオプション定数が有効な値を持つことを確認
            Assert.IsFalse(string.IsNullOrEmpty(OptionConstants.CharacterSetDetection));
            Assert.IsFalse(string.IsNullOrEmpty(OptionConstants.OptionHelp));
            Assert.IsFalse(string.IsNullOrEmpty(OptionConstants.OptionVersion));
            Assert.IsFalse(string.IsNullOrEmpty(OptionConstants.OptionCharacterSet));
            Assert.IsFalse(string.IsNullOrEmpty(OptionConstants.OptionWriteCharacterSet));
            Assert.IsFalse(string.IsNullOrEmpty(OptionConstants.OptionFileNameList));
            Assert.IsFalse(string.IsNullOrEmpty(OptionConstants.OptionFileNameListCharacterSet));
            Assert.IsFalse(string.IsNullOrEmpty(OptionConstants.OptionReplaceWords));
            Assert.IsFalse(string.IsNullOrEmpty(OptionConstants.OptionReplaceWordsCharacterSet));
            Assert.IsFalse(string.IsNullOrEmpty(OptionConstants.OptionSearchWords));
            Assert.IsFalse(string.IsNullOrEmpty(OptionConstants.OptionSearchWordsCharacterSet));
            Assert.IsFalse(string.IsNullOrEmpty(OptionConstants.OptionWriteByteOrderMark));
            Assert.IsFalse(string.IsNullOrEmpty(OptionConstants.OptionAllDirectories));
            Assert.IsFalse(string.IsNullOrEmpty(OptionConstants.OptionNewLine));
            Assert.IsFalse(string.IsNullOrEmpty(OptionConstants.OptionDetectionMode));
            Assert.IsFalse(string.IsNullOrEmpty(OptionConstants.OptionCultureInfo));
            Assert.IsFalse(string.IsNullOrEmpty(OptionConstants.OptionProbeMode));
            Assert.IsFalse(string.IsNullOrEmpty(OptionConstants.OptionOutputFileNamelist));
        }

        [TestMethod]
        public void AllOptionKeys_AreUnique()
        {
            // Arrange
            var optionKeys = new[]
            {
                OptionConstants.OptionHelp,
                OptionConstants.OptionVersion,
                OptionConstants.OptionCharacterSet,
                OptionConstants.OptionWriteCharacterSet,
                OptionConstants.OptionFileNameList,
                OptionConstants.OptionFileNameListCharacterSet,
                OptionConstants.OptionReplaceWords,
                OptionConstants.OptionReplaceWordsCharacterSet,
                OptionConstants.OptionSearchWords,
                OptionConstants.OptionSearchWordsCharacterSet,
                OptionConstants.OptionWriteByteOrderMark,
                OptionConstants.OptionAllDirectories,
                OptionConstants.OptionNewLine,
                OptionConstants.OptionDetectionMode,
                OptionConstants.OptionCultureInfo,
                OptionConstants.OptionProbeMode,
                OptionConstants.OptionOutputFileNamelist
            };

            // Act & Assert
            var uniqueKeys = new System.Collections.Generic.HashSet<string>(optionKeys);
            Assert.AreEqual(optionKeys.Length, uniqueKeys.Count, "すべてのオプションキーは一意である必要があります");
        }
    }
}
