using Px.Utils.UnitTests.Validation.ContentValidationTests;
using Px.Utils.UnitTests.Validation.Fixtures;
using Px.Utils.UnitTests.Validation.SyntaxValidationTests;
using Px.Utils.Validation;
using System.Text;

namespace Px.Utils.UnitTests.Validation.PxFileValidationTests
{
    [TestClass]
    public class PxFileValidationTests
    {
        [TestMethod]
        public void ValidatePxFileWithMinimalPxFileReturnsValidResult()
        {
            // Arrange
            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(PxFileFixtures.MINIMAL_PX_FILE));
            PxFileValidator validator = new (stream, "foo", Encoding.UTF8);

            // Act
            ValidationResult result = validator.Validate();

            // Assert
            Assert.IsNotNull(result, "Validation result should not be null");
            Assert.AreEqual(0, result.FeedbackItems.Count);
        }

        [TestMethod]
        public async Task ValidatePxFileAsyncWithMinimalPxFileReturnsValidResult()
        {
            // Arrange
            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(PxFileFixtures.MINIMAL_PX_FILE));
            PxFileValidator validator = new(stream, "foo", Encoding.UTF8);

            // Act
            ValidationResult result = await validator.ValidateAsync();

            // Assert
            Assert.IsNotNull(result, "Validation result should not be null");
            Assert.AreEqual(0, result.FeedbackItems.Count);
        }

        [TestMethod]
        public async Task ValidatePxFileWithInvalidPxFileReturnsFeedbacks()
        {
            // Arrange
            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(PxFileFixtures.INVALID_PX_FILE));
            PxFileValidator validator = new(stream, "foo", Encoding.UTF8);

            // Act
            ValidationResult result = await validator.ValidateAsync();

            // Assert
            Assert.IsNotNull(result, "Validation result should not be null");
            Assert.AreEqual(9, result.FeedbackItems.Count); // Unique feedbacks
            Assert.AreEqual(11, result.FeedbackItems.Values.SelectMany(f => f).Count()); // Total feedbacks including duplicates
        }

        [TestMethod]
        public void ValidatePxFileWithCustomValidatorsReturnsValidResult()
        {
            // Arrange
            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(PxFileFixtures.MINIMAL_PX_FILE));
            PxFileValidator validator = new(stream, "foo", Encoding.UTF8);
            validator.SetCustomValidators([new MockCustomValidators()]);

            // Act
            ValidationResult result = validator.Validate();

            // Assert
            Assert.IsNotNull(result, "Validation result should not be null");
            Assert.AreEqual(0, result.FeedbackItems.Count);
        }

        [TestMethod]
        public void ValidatePxFileWithoutDataReturnsError()
        {
            // Arrange
            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(PxFileFixtures.PX_FILE_WITHOUT_DATA));
            PxFileValidator validator = new(stream, "foo", Encoding.UTF8);

            // Act
            ValidationResult result = validator.Validate();

            // Assert
            Assert.IsNotNull(result, "Validation result should not be null");
            Assert.AreEqual(1, result.FeedbackItems.Count);
        }

        [TestMethod]
        public async Task ValidatePxFileAsyncWithoutDataReturnsError()
        {
            // Arrange
            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(PxFileFixtures.PX_FILE_WITHOUT_DATA));
            PxFileValidator validator = new(stream, "foo", Encoding.UTF8);

            // Act
            ValidationResult result = await validator.ValidateAsync();

            // Assert
            Assert.IsNotNull(result, "Validation result should not be null");
            Assert.AreEqual(1, result.FeedbackItems.Count);
        }

        [TestMethod]
        public async Task ValidatePxFileAsyncWithCustomValidatorsReturnsValidResult()
        {
            // Arrange
            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(PxFileFixtures.MINIMAL_PX_FILE));
            PxFileValidator validator = new(stream, "foo", Encoding.UTF8);
            validator.SetCustomValidators(null, [new MockCustomValidators()]);

            // Act
            ValidationResult result = await validator.ValidateAsync();

            // Assert
            Assert.IsNotNull(result, "Validation result should not be null");
            Assert.AreEqual(0, result.FeedbackItems.Count);
        }

        [TestMethod]
        public void ValidatePxFileWithCustomValidationFunctionsReturnsValidResult()
        {
            // Arrange
            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(PxFileFixtures.MINIMAL_PX_FILE));
            PxFileValidator validator = new(stream, "foo", Encoding.UTF8);
            validator.SetCustomValidatorFunctions(new MockCustomSyntaxValidationFunctions(), new MockCustomContentValidationFunctions());

            // Act
            ValidationResult result = validator.Validate();

            // Assert
            Assert.IsNotNull(result, "Validation result should not be null");
            Assert.AreEqual(0, result.FeedbackItems.Count);
        }
    }
}
