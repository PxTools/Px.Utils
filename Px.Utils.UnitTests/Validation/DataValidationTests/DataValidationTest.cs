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

            ValidationFeedback validationFeedbacks = validator.Validate().FeedbackItems;

            foreach (KeyValuePair<ValidationFeedbackKey, List<ValidationFeedbackValue>> validationFeedback in validationFeedbacks)
            {
                foreach (ValidationFeedbackValue instance in validationFeedback.Value)
                {
                    Logger.LogMessage($"Line {instance.Line}, Char {instance.Character}: " 
                        + $"{validationFeedback.Key.Rule} {instance.AdditionalInfo}");
                }
            }
            Assert.AreEqual(0, validationFeedbacks.Count);
        }

        [TestMethod]
        public async Task TestValidateAsyncWithoutErrors()
        {
            using Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(DataStreamContents.SIMPLE_VALID_DATA));
            stream.Seek(6, 0);
            DataValidator validator = new(stream, 5, 4, "foo", 1, Encoding.Default);

            ValidationResult result = await validator.ValidateAsync();
            ValidationFeedback validationFeedbacks = result.FeedbackItems;

            foreach (KeyValuePair<ValidationFeedbackKey, List<ValidationFeedbackValue>> validationFeedback in validationFeedbacks)
            {
                foreach (ValidationFeedbackValue instance in validationFeedback.Value)
                {
                    Logger.LogMessage($"Line {instance.Line}, Char {instance.Character}: "
                        + $"{validationFeedback.Key.Rule} {instance.AdditionalInfo}");
                }
            }
            Assert.AreEqual(0, validationFeedbacks.Count);

        }

        [TestMethod]
        public void TestValidateWithErrors()
        {
            using Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(DataStreamContents.SIMPLE_INVALID_DATA));
            stream.Seek(6, 0); 
            DataValidator validator = new(stream, 5, 4, "foo", 1, Encoding.Default);

            ValidationFeedback validationFeedbacks = validator.Validate().FeedbackItems;

            foreach (KeyValuePair<ValidationFeedbackKey, List<ValidationFeedbackValue>> validationFeedback in validationFeedbacks)
            {
                foreach (ValidationFeedbackValue instance in validationFeedback.Value)
                {
                    Logger.LogMessage($"Line {instance.Line}, Char {instance.Character}: "
                        + $"{validationFeedback.Key.Rule} {instance.AdditionalInfo}");
                }
            }

            Assert.AreEqual(7, validationFeedbacks.Count); // Unique feedbacks
            Assert.AreEqual(13, validationFeedbacks.Values.SelectMany(f => f).Count()); // Total feedbacks including duplicates
        }

        [TestMethod]
        public async Task TestValidateAsyncWithErrors()
        {
            using Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(DataStreamContents.SIMPLE_INVALID_DATA));
            stream.Seek(6, 0);
            DataValidator validator = new(stream, 5, 4, "foo", 1, Encoding.Default);

            ValidationResult result = await validator.ValidateAsync();
            ValidationFeedback validationFeedbacks = result.FeedbackItems;

            foreach (KeyValuePair<ValidationFeedbackKey, List<ValidationFeedbackValue>> validationFeedback in validationFeedbacks)
            {
                foreach (ValidationFeedbackValue instance in validationFeedback.Value)
                {
                    Logger.LogMessage($"Line {instance.Line}, Char {instance.Character}: "
                        + $"{validationFeedback.Key.Rule} {instance.AdditionalInfo}");
                }
            }

            Assert.AreEqual(7, validationFeedbacks.Count); // Unique feedbacks
            Assert.AreEqual(13, validationFeedbacks.Values.SelectMany(f => f).Count()); // Total feedbacks including duplicates
        }

        [TestMethod]
        public void ValidateWithoutDataReturnsErrors()
        {
            using Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(DataStreamContents.NO_DATA));
            DataValidator validator = new(stream, 5, 4, "foo", 1, Encoding.Default);

            ValidationFeedback validationFeedbacks = validator.Validate().FeedbackItems;

            foreach (KeyValuePair<ValidationFeedbackKey, List<ValidationFeedbackValue>> validationFeedback in validationFeedbacks)
            {
                foreach (ValidationFeedbackValue instance in validationFeedback.Value)
                {
                    Logger.LogMessage($"Line {instance.Line}, Char {instance.Character}: "
                        + $"{validationFeedback.Key.Rule} {instance.AdditionalInfo}");
                }
            }

            Assert.AreEqual(2, validationFeedbacks.Count);
        }

        [TestMethod]
        public async Task ValidateAsyncWithoutDataReturnsErrors()
        {
            using Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(DataStreamContents.NO_DATA));
            DataValidator validator = new(stream, 5, 4, "foo", 1, Encoding.Default);

            ValidationResult result = await validator.ValidateAsync();
            ValidationFeedback validationFeedbacks = result.FeedbackItems;

            foreach (KeyValuePair<ValidationFeedbackKey, List<ValidationFeedbackValue>> validationFeedback in validationFeedbacks)
            {
                foreach (ValidationFeedbackValue instance in validationFeedback.Value)
                {
                    Logger.LogMessage($"Line {instance.Line}, Char {instance.Character}: "
                        + $"{validationFeedback.Key.Rule} {instance.AdditionalInfo}");
                }
            }

            Assert.AreEqual(2, validationFeedbacks.Count);
        }

        [TestMethod]
        public void ValidateWithDataOnOnSingleRowReturnsErrors()
        {
            using Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(DataStreamContents.DATA_ON_SINGLE_ROW));
            DataValidator validator = new(stream, 5, 4, "foo", 1, Encoding.Default);

            ValidationFeedback validationFeedbacks = validator.Validate().FeedbackItems;

            foreach (KeyValuePair<ValidationFeedbackKey, List<ValidationFeedbackValue>> validationFeedback in validationFeedbacks)
            {
                foreach (ValidationFeedbackValue instance in validationFeedback.Value)
                {
                    Logger.LogMessage($"Line {instance.Line}, Char {instance.Character}: "
                        + $"{validationFeedback.Key.Rule} {instance.AdditionalInfo}");
                }
            }

            Assert.AreEqual(2, validationFeedbacks.Count); // Unique feedbacks
            Assert.AreEqual(6, validationFeedbacks.Values.SelectMany(f => f).Count()); // Total feedbacks including duplicates
        }

        [TestMethod]
        public async Task ValidateAsyncWithDataOnSingleRowReturnsErrors()
        {
            using Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(DataStreamContents.DATA_ON_SINGLE_ROW));
            DataValidator validator = new(stream, 5, 4, "foo", 1, Encoding.Default);

            ValidationResult result = await validator.ValidateAsync();
            ValidationFeedback validationFeedbacks = result.FeedbackItems;

            foreach (KeyValuePair<ValidationFeedbackKey, List<ValidationFeedbackValue>> validationFeedback in validationFeedbacks)
            {
                foreach (ValidationFeedbackValue instance in validationFeedback.Value)
                {
                    Logger.LogMessage($"Line {instance.Line}, Char {instance.Character}: "
                        + $"{validationFeedback.Key.Rule} {instance.AdditionalInfo}");
                }
            }

            Assert.AreEqual(2, validationFeedbacks.Count);// Unique feedbacks
            Assert.AreEqual(6, validationFeedbacks.Values.SelectMany(f => f).Count()); // Total feedbacks including duplicates
        }
    }
}