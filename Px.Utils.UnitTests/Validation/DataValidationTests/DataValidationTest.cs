using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using Px.Utils.UnitTests.Validation.Fixtures;
using PxUtils.PxFile;
using PxUtils.Validation;
using PxUtils.Validation.DataValidation;

namespace Px.Utils.UnitTests.Validation.DataValidationTests
{
    [TestClass]
    public class DataValidationTest
    {

        [TestMethod]
        public void TestValidateWithoutErrors()
        {
            using Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(DataStreamContents.SIMPLE_VALID_DATA));
            stream.Seek(6, 0);

            IEnumerable<ValidationFeedback> validationFeedbacks =
                DataValidation.Validate(stream, 5, 4, 1, Encoding.UTF8, PxFileSyntaxConf.Default);

            foreach (ValidationFeedback validationFeedback in validationFeedbacks)
            {
                Logger.LogMessage($"Line {validationFeedback.Line}, Char {validationFeedback.Character}: " 
                                  + $"{validationFeedback.Rule} {validationFeedback.AdditionalInfo}");
            }
            Assert.AreEqual(0, validationFeedbacks.Count());
        }

        [TestMethod]
        public async Task TestValidateAsyncWithoutErrors()
        {
            using Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(DataStreamContents.SIMPLE_VALID_DATA));
            stream.Seek(6, 0);

            IEnumerable<ValidationFeedback> validationFeedbacks =
                await DataValidation.ValidateAsync(stream, 5, 4, 1, Encoding.UTF8, PxFileSyntaxConf.Default);

            foreach (ValidationFeedback validationFeedback in validationFeedbacks)
            {
                Logger.LogMessage($"Line {validationFeedback.Line}, Char {validationFeedback.Character}: " 
                                  + $"{validationFeedback.Rule} {validationFeedback.AdditionalInfo}");
            }
            Assert.AreEqual(0, validationFeedbacks.Count());

        }

        [TestMethod]
        public void TestValidateWithErrors()
        {
            using Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(DataStreamContents.SIMPLE_INVALID_DATA));
            stream.Seek(6, 0);

            IEnumerable<ValidationFeedback> validationFeedbacks =
                DataValidation.Validate(stream, 5, 4, 1, Encoding.UTF8, PxFileSyntaxConf.Default);

            foreach (ValidationFeedback validationFeedback in validationFeedbacks)
            {
                Logger.LogMessage($"Line {validationFeedback.Line}, Char {validationFeedback.Character}: " 
                                  + $"{validationFeedback.Rule} {validationFeedback.AdditionalInfo}");
            }
            Assert.AreEqual(10, validationFeedbacks.Count());
        }

        [TestMethod]
        public async Task TestValidateAsyncWithErrors()
        {
            using Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(DataStreamContents.SIMPLE_INVALID_DATA));
            stream.Seek(6, 0);

            IEnumerable<ValidationFeedback> validationFeedbacks =
                await DataValidation.ValidateAsync(stream, 5, 4, 1, Encoding.UTF8, PxFileSyntaxConf.Default);

            foreach (ValidationFeedback validationFeedback in validationFeedbacks)
            {
                Logger.LogMessage($"Line {validationFeedback.Line}, Char {validationFeedback.Character}: " 
                                  + $"{validationFeedback.Rule} {validationFeedback.AdditionalInfo}");
            }

            Assert.AreEqual(10, validationFeedbacks.Count());
        }
    }
}