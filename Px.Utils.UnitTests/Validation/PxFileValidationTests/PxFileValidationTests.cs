﻿using Px.Utils.UnitTests.Validation.Fixtures;
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
            IValidationResult result = validator.Validate();

            // Assert
            Assert.IsNotNull(result, "Validation result should not be null");
            Assert.AreEqual(0, result.FeedbackItems.Length);
        }

        [TestMethod]
        public async Task ValidatePxFileAsyncWithMinimalPxFileReturnsValidResult()
        {
            // Arrange
            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(PxFileFixtures.MINIMAL_PX_FILE));
            PxFileValidator validator = new(stream, "foo", Encoding.UTF8);

            // Act
            IValidationResult result = await validator.ValidateAsync();

            // Assert
            Assert.IsNotNull(result, "Validation result should not be null");
            Assert.AreEqual(0, result.FeedbackItems.Length);
        }

        [TestMethod]
        public async Task ValidatePxFileWithINvalidPxFileReturnsFeedbacks()
        {
            // Arrange
            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(PxFileFixtures.INVALID_PX_FILE));
            PxFileValidator validator = new(stream, "foo", Encoding.UTF8);

            // Act
            IValidationResult result = await validator.ValidateAsync();

            // Assert
            Assert.IsNotNull(result, "Validation result should not be null");
            Assert.AreEqual(14, result.FeedbackItems.Length);
        }
    }
}
