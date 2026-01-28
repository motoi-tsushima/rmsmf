using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using rmsmf;

namespace rmsmf.Tests
{
    /// <summary>
    /// Colipex クラスのテスト
    /// </summary>
    [TestClass]
    public class ColipexTests
    {
        /// <summary>
        /// Colipexの保護メソッドをテストするためのテストヘルパークラス
        /// </summary>
        private class ColipexTestHelper : Colipex
        {
            public ColipexTestHelper(string[] args) : base(args) { }

            public string TestConvertEscapeSequences(string input)
            {
                return ConvertEscapeSequences(input);
            }

            public Encoding TestResolveEncoding(string encodingNameOrCodePage, string judgmentKeyword = "Detection")
            {
                return ResolveEncoding(encodingNameOrCodePage, judgmentKeyword);
            }

            public void TestEnsureEncodingInitialized(ref Encoding encoding, string fileName)
            {
                EnsureEncodingInitialized(ref encoding, fileName);
            }
        }
        [TestMethod]
        public void Constructor_WithSimpleArguments_ParsesCorrectly()
        {
            // Arrange
            string[] args = { "file.txt", "-o", "/c:UTF-8" };

            // Act
            var colipex = new Colipex(args);

            // Assert
            Assert.AreEqual(1, colipex.Parameters.Count);
            Assert.AreEqual("file.txt", colipex.Parameters[0]);
            Assert.AreEqual(2, colipex.Options.Count);
            Assert.IsTrue(colipex.IsOption("o"));
            Assert.IsTrue(colipex.IsOption("c"));
            Assert.AreEqual("UTF-8", colipex.Options["c"]);
        }

        [TestMethod]
        public void Constructor_WithColonSeparator_ParsesCorrectly()
        {
            // Arrange
            string[] args = { "/c:shift_jis", "-w:UTF-8" };

            // Act
            var colipex = new Colipex(args);

            // Assert
            Assert.AreEqual(2, colipex.Options.Count);
            Assert.AreEqual("shift_jis", colipex.Options["c"]);
            Assert.AreEqual("UTF-8", colipex.Options["w"]);
        }

        [TestMethod]
        public void Constructor_WithEqualsSeparator_ParsesCorrectly()
        {
            // Arrange
            string[] args = { "/c=UTF-8", "-w=shift_jis" };

            // Act
            var colipex = new Colipex(args);

            // Assert
            Assert.AreEqual(2, colipex.Options.Count);
            Assert.AreEqual("UTF-8", colipex.Options["c"]);
            Assert.AreEqual("shift_jis", colipex.Options["w"]);
        }

        [TestMethod]
        public void Constructor_WithNonValueOption_StoresNonValue()
        {
            // Arrange
            string[] args = { "-h", "/d" };

            // Act
            var colipex = new Colipex(args);

            // Assert
            Assert.AreEqual(2, colipex.Options.Count);
            Assert.AreEqual(Colipex.NonValue, colipex.Options["h"]);
            Assert.AreEqual(Colipex.NonValue, colipex.Options["d"]);
        }

        [TestMethod]
        public void IsOption_WithExistingOption_ReturnsTrue()
        {
            // Arrange
            string[] args = { "-h", "/c:UTF-8" };
            var colipex = new Colipex(args);

            // Act & Assert
            Assert.IsTrue(colipex.IsOption("h"));
            Assert.IsTrue(colipex.IsOption("c"));
        }

        [TestMethod]
        public void IsOption_WithNonExistingOption_ReturnsFalse()
        {
            // Arrange
            string[] args = { "-h" };
            var colipex = new Colipex(args);

            // Act & Assert
            Assert.IsFalse(colipex.IsOption("c"));
            Assert.IsFalse(colipex.IsOption("w"));
        }

        [TestMethod]
        public void Args_ReturnsParametersAsArray()
        {
            // Arrange
            string[] args = { "file1.txt", "file2.txt", "-o" };
            var colipex = new Colipex(args);

            // Act
            string[] result = colipex.Args;

            // Assert
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("file1.txt", result[0]);
            Assert.AreEqual("file2.txt", result[1]);
        }

        [TestMethod]
        [ExpectedException(typeof(RmsmfException))]
        public void Constructor_WithDuplicateOptions_ThrowsException()
        {
            // Arrange
            string[] args = { "/c:UTF-8", "-c:shift_jis" };

            // Act
            var colipex = new Colipex(args);

            // Assert - Exception expected
        }

        [TestMethod]
        public void Constructor_WithMixedParametersAndOptions_ParsesCorrectly()
        {
            // Arrange
            string[] args = { "file1.txt", "-o", "file2.txt", "/c:UTF-8", "file3.txt" };

            // Act
            var colipex = new Colipex(args);

            // Assert
            Assert.AreEqual(3, colipex.Parameters.Count);
            Assert.AreEqual("file1.txt", colipex.Parameters[0]);
            Assert.AreEqual("file2.txt", colipex.Parameters[1]);
            Assert.AreEqual("file3.txt", colipex.Parameters[2]);
            Assert.AreEqual(2, colipex.Options.Count);
        }

        [TestMethod]
        public void Constructor_WithEmptyArgs_CreatesEmptyCollections()
        {
            // Arrange
            string[] args = { };

            // Act
            var colipex = new Colipex(args);

            // Assert
            Assert.AreEqual(0, colipex.Parameters.Count);
            Assert.AreEqual(0, colipex.Options.Count);
            Assert.AreEqual(0, colipex.Args.Length);
        }

        #region ConvertEscapeSequences Tests

        [TestMethod]
        public void ConvertEscapeSequences_WithCRLF_ConvertsCorrectly()
        {
            // Arrange
            var helper = new ColipexTestHelper(new string[] { });
            string input = "line1\\r\\nline2";

            // Act
            string result = helper.TestConvertEscapeSequences(input);

            // Assert
            Assert.AreEqual("line1\r\nline2", result);
        }

        [TestMethod]
        public void ConvertEscapeSequences_WithLF_ConvertsCorrectly()
        {
            // Arrange
            var helper = new ColipexTestHelper(new string[] { });
            string input = "line1\\nline2";

            // Act
            string result = helper.TestConvertEscapeSequences(input);

            // Assert
            Assert.AreEqual("line1\nline2", result);
        }

        [TestMethod]
        public void ConvertEscapeSequences_WithCR_ConvertsCorrectly()
        {
            // Arrange
            var helper = new ColipexTestHelper(new string[] { });
            string input = "line1\\rline2";

            // Act
            string result = helper.TestConvertEscapeSequences(input);

            // Assert
            Assert.AreEqual("line1\rline2", result);
        }

        [TestMethod]
        public void ConvertEscapeSequences_WithTab_ConvertsCorrectly()
        {
            // Arrange
            var helper = new ColipexTestHelper(new string[] { });
            string input = "col1\\tcol2";

            // Act
            string result = helper.TestConvertEscapeSequences(input);

            // Assert
            Assert.AreEqual("col1\tcol2", result);
        }

        [TestMethod]
        public void ConvertEscapeSequences_WithBackslash_ConvertsCorrectly()
        {
            // Arrange
            var helper = new ColipexTestHelper(new string[] { });
            string input = "path\\\\folder";

            // Act
            string result = helper.TestConvertEscapeSequences(input);

            // Assert
            Assert.AreEqual("path\\folder", result);
        }

        [TestMethod]
        public void ConvertEscapeSequences_WithMultipleEscapes_ConvertsCorrectly()
        {
            // Arrange
            var helper = new ColipexTestHelper(new string[] { });
            string input = "a\\tb\\nc\\rd";

            // Act
            string result = helper.TestConvertEscapeSequences(input);

            // Assert
            Assert.AreEqual("a\tb\nc\rd", result);
        }

        #endregion

        #region ResolveEncoding Tests

        [TestMethod]
        public void ResolveEncoding_WithDetectionKeyword_ReturnsNull()
        {
            // Arrange
            var helper = new ColipexTestHelper(new string[] { });

            // Act
            Encoding result = helper.TestResolveEncoding("Detection");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ResolveEncoding_WithEncodingName_ReturnsEncoding()
        {
            // Arrange
            var helper = new ColipexTestHelper(new string[] { });

            // Act
            Encoding result = helper.TestResolveEncoding("UTF-8");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("utf-8", result.WebName);
        }

        [TestMethod]
        public void ResolveEncoding_WithCodePage_ReturnsEncoding()
        {
            // Arrange
            var helper = new ColipexTestHelper(new string[] { });

            // Act
            Encoding result = helper.TestResolveEncoding("65001");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(65001, result.CodePage);
        }

        [TestMethod]
        public void ResolveEncoding_WithShiftJIS_ReturnsEncoding()
        {
            // Arrange
            var helper = new ColipexTestHelper(new string[] { });

            // Act
            Encoding result = helper.TestResolveEncoding("shift_jis");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("shift_jis", result.WebName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ResolveEncoding_WithInvalidEncodingName_ThrowsException()
        {
            // Arrange
            var helper = new ColipexTestHelper(new string[] { });

            // Act
            helper.TestResolveEncoding("InvalidEncoding");

            // Assert - Exception expected
        }

        #endregion

        #region EnsureEncodingInitialized Tests

        [TestMethod]
        public void EnsureEncodingInitialized_WithNullEncodingAndValidFile_InitializesEncoding()
        {
            // Arrange
            var helper = new ColipexTestHelper(new string[] { });
            string tempFile = Path.GetTempFileName();
            
            try
            {
                File.WriteAllText(tempFile, "test content", Encoding.UTF8);
                Encoding encoding = null;

                // Act
                helper.TestEnsureEncodingInitialized(ref encoding, tempFile);

                // Assert
                Assert.IsNotNull(encoding);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        [TestMethod]
        public void EnsureEncodingInitialized_WithExistingEncoding_DoesNotChange()
        {
            // Arrange
            var helper = new ColipexTestHelper(new string[] { });
            string tempFile = Path.GetTempFileName();
            
            try
            {
                File.WriteAllText(tempFile, "test content", Encoding.UTF8);
                Encoding encoding = Encoding.UTF8;
                Encoding originalEncoding = encoding;

                // Act
                helper.TestEnsureEncodingInitialized(ref encoding, tempFile);

                // Assert
                Assert.AreSame(originalEncoding, encoding);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void EnsureEncodingInitialized_WithNonExistentFile_ThrowsException()
        {
            // Arrange
            var helper = new ColipexTestHelper(new string[] { });
            Encoding encoding = null;

            // Act
            helper.TestEnsureEncodingInitialized(ref encoding, "nonexistent_file.txt");

            // Assert - Exception expected
        }

        #endregion
    }
}
