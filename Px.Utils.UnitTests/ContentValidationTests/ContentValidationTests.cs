using PxUtils.PxFile;
using PxUtils.UnitTests.ContentValidationTests.Fixtures;
using PxUtils.Validation;
using PxUtils.Validation.ContentValidation;
using PxUtils.Validation.SyntaxValidation;
using System.Text;

namespace PxUtils.UnitTests.ContentValidationTests
{
    [TestClass]
    public class ContentValidationTests
    {
        private readonly string filename = "foo";
        private readonly PxFileSyntaxConf syntaxConf = PxFileSyntaxConf.Default;
        private List<ValidationFeedbackItem> feedback = [];
        private readonly Encoding encoding = Encoding.UTF8;

        [TestMethod]
        public void ValidatePxFileContent_Called_With_MINIMAL_STRUCTURED_ENTRY_ARRAY_Returns_Valid_Result()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.MINIMAL_STRUCTURED_ENTRY_ARRAY;

            // Act
            ContentValidation.ValidatePxFileContent(
                filename,
                entries,
                syntaxConf,
                ref feedback,
                encoding
                );

            // Assert
            Assert.AreEqual(0, feedback.Count);
        }
    }
}
