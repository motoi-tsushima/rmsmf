using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using rmsmf;

namespace rmsmf.Tests
{
    /// <summary>
    /// OptionValidator クラスのテスト
    /// </summary>
    [TestClass]
    public class OptionValidatorTests
    {
        #region ValidateFileSpecificationNotConflicting Tests

        [TestMethod]
        public void ValidateFileSpecificationNotConflicting_WithNoConflict_DoesNotThrow()
        {
            // Arrange & Act & Assert - 例外がスローされないことを確認
            OptionValidator.ValidateFileSpecificationNotConflicting(hasFileListOption: false, parameterCount: 3);
            OptionValidator.ValidateFileSpecificationNotConflicting(hasFileListOption: true, parameterCount: 0);
        }

        [TestMethod]
        [ExpectedException(typeof(RmsmfException))]
        public void ValidateFileSpecificationNotConflicting_WithConflict_ThrowsException()
        {
            // Arrange & Act
            OptionValidator.ValidateFileSpecificationNotConflicting(hasFileListOption: true, parameterCount: 2);

            // Assert - Exception expected
        }

        #endregion

        #region ValidateEncodingOptionDependency Tests

        [TestMethod]
        public void ValidateEncodingOptionDependency_WithValidDependency_DoesNotThrow()
        {
            // Arrange & Act & Assert - 例外がスローされないことを確認
            OptionValidator.ValidateEncodingOptionDependency(
                hasMainOption: true,
                hasEncodingOption: true,
                errorMessage: "Test error");

            OptionValidator.ValidateEncodingOptionDependency(
                hasMainOption: false,
                hasEncodingOption: false,
                errorMessage: "Test error");
        }

        [TestMethod]
        [ExpectedException(typeof(RmsmfException))]
        public void ValidateEncodingOptionDependency_WithInvalidDependency_ThrowsException()
        {
            // Arrange & Act
            OptionValidator.ValidateEncodingOptionDependency(
                hasMainOption: false,
                hasEncodingOption: true,
                errorMessage: "Test error message");

            // Assert - Exception expected
        }

        [TestMethod]
        public void ValidateEncodingOptionDependency_ThrowsExceptionWithCorrectMessage()
        {
            // Arrange
            string expectedMessage = "Custom error message";

            try
            {
                // Act
                OptionValidator.ValidateEncodingOptionDependency(
                    hasMainOption: false,
                    hasEncodingOption: true,
                    errorMessage: expectedMessage);

                Assert.Fail("Expected RmsmfException was not thrown.");
            }
            catch (RmsmfException ex)
            {
                // Assert
                Assert.AreEqual(expectedMessage, ex.Message);
            }
        }

        #endregion

        #region ValidateAtLeastOneCondition Tests

        [TestMethod]
        public void ValidateAtLeastOneCondition_WithOneTrue_DoesNotThrow()
        {
            // Arrange & Act & Assert
            OptionValidator.ValidateAtLeastOneCondition(true);
            OptionValidator.ValidateAtLeastOneCondition(false, true, false);
            OptionValidator.ValidateAtLeastOneCondition(false, false, true);
        }

        [TestMethod]
        public void ValidateAtLeastOneCondition_WithMultipleTrue_DoesNotThrow()
        {
            // Arrange & Act & Assert
            OptionValidator.ValidateAtLeastOneCondition(true, true);
            OptionValidator.ValidateAtLeastOneCondition(true, true, true);
        }

        [TestMethod]
        [ExpectedException(typeof(RmsmfException))]
        public void ValidateAtLeastOneCondition_WithAllFalse_ThrowsException()
        {
            // Arrange & Act
            OptionValidator.ValidateAtLeastOneCondition(false, false, false);

            // Assert - Exception expected
        }

        [TestMethod]
        [ExpectedException(typeof(RmsmfException))]
        public void ValidateAtLeastOneCondition_WithSingleFalse_ThrowsException()
        {
            // Arrange & Act
            OptionValidator.ValidateAtLeastOneCondition(false);

            // Assert - Exception expected
        }

        [TestMethod]
        public void ValidateAtLeastOneCondition_ThrowsExceptionWithCorrectMessage()
        {
            try
            {
                // Act
                OptionValidator.ValidateAtLeastOneCondition(false, false);

                Assert.Fail("Expected RmsmfException was not thrown.");
            }
            catch (RmsmfException ex)
            {
                // Assert
                Assert.AreEqual(ValidationMessages.MissingRequiredParameters, ex.Message);
            }
        }

        #endregion
    }
}
