using System;
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
    }
}
