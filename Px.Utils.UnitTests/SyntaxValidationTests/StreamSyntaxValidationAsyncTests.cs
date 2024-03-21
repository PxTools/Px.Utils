using PxUtils.Validation.SyntaxValidation;
using PxUtils.UnitTests.SyntaxValidationTests.Fixtures;
using System.Text;
using PxUtils.PxFile;
using PxUtils.Validation;

namespace PxUtils.UnitTests.SyntaxValidationTests
{
    [TestClass]
    public class StreamSyntaxValidationAsyncTests
    {
        private readonly string filename = "foo";

        [TestMethod]
        public async Task ValidatePxFileSyntaxAsync_CalledWith_MINIMAL_UTF8_Returns_Valid_Result()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.MINIMAL_UTF8_N);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);

            // Act
            SyntaxValidationResult result = await SyntaxValidation.ValidatePxFileMetadataSyntaxAsync(stream, filename);

            Assert.AreEqual(8, result.Result.Count);
            Assert.AreEqual(0, result.Report.FeedbackItems?.Count);
        }

        [TestMethod]
        public async Task ValidatePxFileSyntaxAsync_CalledWith_UTF8_N_WITH_SPECIFIERS_Returns_Result_With_Right_Structure()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_SPECIFIERS);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);

            // Act
            SyntaxValidationResult result = await SyntaxValidation.ValidatePxFileMetadataSyntaxAsync(stream, filename);

            Assert.AreEqual(10, result.Result.Count);
            Assert.AreEqual("YES", result.Result[8].Value);
            Assert.AreEqual("NO", result.Result[9].Value);
            Assert.AreEqual("fi", result.Result[8].Key.Language);
            Assert.AreEqual("fi", result.Result[9].Key.Language);
            Assert.AreEqual("first_specifier", result.Result[8].Key.FirstSpecifier);
            Assert.AreEqual("first_specifier", result.Result[9].Key.FirstSpecifier);
            Assert.AreEqual("second_specifier", result.Result[8].Key.SecondSpecifier);
        }

        [TestMethod]
        public async Task ValidatePxFileSyntaxAsync_CalledWith_UNKNOWN_ENCODING_Returns_With_Error()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UNKNOWN_ENCODING);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);

            // Act
            SyntaxValidationResult result = await SyntaxValidation.ValidatePxFileMetadataSyntaxAsync(stream, filename);

            Assert.AreEqual(1, result.Report.FeedbackItems?.Count);
        }
    }
}
