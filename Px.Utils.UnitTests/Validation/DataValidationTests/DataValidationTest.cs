using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using Px.Utils.UnitTests.Validation.Fixtures;
using Px.Utils.PxFile;
using Px.Utils.Validation;
using Px.Utils.Validation.DataValidation;

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
            DataValidator validator = new(stream, 5, 4, "foo", 1, Encoding.Default);

            ValidationFeedbackItem[] validationFeedbacks = validator.Validate().FeedbackItems;

            foreach (ValidationFeedbackItem validationFeedback in validationFeedbacks)
            {
                Logger.LogMessage($"Line {validationFeedback.Feedback.Line}, Char {validationFeedback.Feedback.Character}: " 
                                  + $"{validationFeedback.Feedback.Rule} {validationFeedback.Feedback.AdditionalInfo}");
            }
            Assert.AreEqual(0, validationFeedbacks.Length);
        }

        [TestMethod]
        public async Task TestValidateAsyncWithoutErrors()
        {
            using Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(DataStreamContents.SIMPLE_VALID_DATA));
            stream.Seek(6, 0);
            DataValidator validator = new(stream, 5, 4, "foo", 1, Encoding.Default);

            IValidationResult result = await validator.ValidateAsync();
            ValidationFeedbackItem[] validationFeedbacks = result.FeedbackItems;

            foreach (ValidationFeedbackItem validationFeedback in validationFeedbacks)
            {
                Logger.LogMessage($"Line {validationFeedback.Feedback.Line}, Char {validationFeedback.Feedback.Character}: "
                                  + $"{validationFeedback.Feedback.Rule} {validationFeedback.Feedback.AdditionalInfo}");
            }
            Assert.AreEqual(0, validationFeedbacks.Length);

        }

        [TestMethod]
        public void TestValidateWithErrors()
        {
            using Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(DataStreamContents.SIMPLE_INVALID_DATA));
            stream.Seek(6, 0); 
            DataValidator validator = new(stream, 5, 4, "foo", 1, Encoding.Default);

            ValidationFeedbackItem[] validationFeedbacks = validator.Validate().FeedbackItems;

            foreach (ValidationFeedbackItem validationFeedback in validationFeedbacks)
            {
                Logger.LogMessage($"Line {validationFeedback.Feedback.Line}, Char {validationFeedback.Feedback.Character}: "
                                  + $"{validationFeedback.Feedback.Rule} {validationFeedback.Feedback.AdditionalInfo}");
            }
            Assert.AreEqual(10, validationFeedbacks.Length);
        }

        [TestMethod]
        public async Task TestValidateAsyncWithErrors()
        {
            using Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(DataStreamContents.SIMPLE_INVALID_DATA));
            stream.Seek(6, 0);
            DataValidator validator = new(stream, 5, 4, "foo", 1, Encoding.Default);

            IValidationResult result = await validator.ValidateAsync();
            ValidationFeedbackItem[] validationFeedbacks = result.FeedbackItems;

            foreach (ValidationFeedbackItem validationFeedback in validationFeedbacks)
            {
                Logger.LogMessage($"Line {validationFeedback.Feedback.Line}, Char {validationFeedback.Feedback.Character}: "
                                  + $"{validationFeedback.Feedback.Rule} {validationFeedback.Feedback.AdditionalInfo}");
            }
            Assert.AreEqual(10, validationFeedbacks.Length);
        }
    }
}