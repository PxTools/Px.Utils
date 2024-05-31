using Px.Utils.UnitTests.Validation.Fixtures;
using Px.Utils.Validation;
using System.Text;

namespace Px.Utils.UnitTests.Validation.PxFileValidationTests
{
    [TestClass]
    public class PxFileValidationTests
    {
        [TestMethod]
        public void ValidatePxFileWithMinimalPxFileShouldReturnValidResult()
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
    }
}
