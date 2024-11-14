using Px.Utils.Validation;
using Px.Utils.Validation.DataValidation;
using System.Text;

namespace Px.Utils.UnitTests.Validation.DataValidationTests
{
    [TestClass]
    public class DataSeparatorValidatorTest
    {
        [TestMethod]
        public void FirstSeparatorIsUsedAsReference()
        {
            DataSeparatorValidator validator = new();
            List<byte> separator = [.. Encoding.UTF8.GetBytes(" ")];
            KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? nullableFeedback = validator.Validate(separator, EntryType.DataItemSeparator, Encoding.UTF8, 1, 1, "foo");

            Assert.IsNull(nullableFeedback);
        }
    
        [TestMethod]
        public void InconsistentSeparator()
        {
            DataSeparatorValidator validator = new();
            List<byte> separator = [.. Encoding.UTF8.GetBytes(" ")];
            validator.Validate(separator, EntryType.DataItemSeparator, Encoding.UTF8, 1, 1, "foo");
            List<byte> otherSeparator = [.. Encoding.UTF8.GetBytes("\t")];
            KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? nullableFeedback = validator.Validate(otherSeparator, EntryType.DataItemSeparator, Encoding.UTF8, 1, 1, "foo");

            Assert.IsNotNull(nullableFeedback);
            KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue> feedback = (KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>)nullableFeedback;
            Assert.AreEqual(ValidationFeedbackRule.DataValidationFeedbackInconsistentSeparator, feedback.Key.Rule);
            Assert.AreEqual(ValidationFeedbackLevel.Warning, feedback.Key.Level);

        
        }
    
        [TestMethod]
        public void ConsistentSeparator()
        {
            DataSeparatorValidator validator = new();
            List<byte> separator = [.. Encoding.UTF8.GetBytes(" ")];
            validator.Validate(separator, EntryType.DataItemSeparator, Encoding.UTF8, 1, 1, "foo");
            KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? nullableFeedback = validator.Validate(separator, EntryType.DataItemSeparator, Encoding.UTF8, 1, 1, "foo");
            Assert.IsNull(nullableFeedback);
        }
    }
}