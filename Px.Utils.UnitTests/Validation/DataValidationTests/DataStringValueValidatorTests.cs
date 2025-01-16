using Px.Utils.Validation;
using Px.Utils.Validation.DataValidation;
using System.Text;

namespace Px.Utils.UnitTests.Validation.DataValidationTests
{
    [TestClass]
    public class DataStringValueValidatorTest
    {

        [TestMethod]
        [DataRow("\".\"")]
        [DataRow("\"..\"")]
        [DataRow("\"...\"")]
        [DataRow("\"....\"")]
        [DataRow("\".....\"")]
        [DataRow("\"......\"")]
        [DataRow("\"-\"")]
        [DataRow(".")]
        [DataRow("..")]
        [DataRow("...")]
        [DataRow("....")]
        [DataRow(".....")]
        [DataRow("......")]
        [DataRow("-")]
        public void AllowedStrings(string allowedValue)
        {
            DataStringValidator validator = new();
            Encoding encoding = Encoding.UTF8;
            List<byte> value = [.. encoding.GetBytes(allowedValue)];
            KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? nullableFeedback = validator.Validate(value, EntryType.DataItem, encoding, 0, 0, "foo");

            Assert.IsNull(nullableFeedback);
        }

        [TestMethod]
        [DataRow("\"...")]
        [DataRow("...\"")]
        [DataRow("\"\"")]
        [DataRow(" ... ")]
        [DataRow("foo")]
        [DataRow("\"foo\"")]
        [DataRow("\".......\"")]
        [DataRow("\"--\"")]
        public void NotAllowedStringValue(string notAllowedValue)
        {
            DataStringValidator validator = new();

            Encoding encoding = Encoding.UTF8;
            List<byte> value = [.. encoding.GetBytes(notAllowedValue)];

            KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? nullableFeedback = validator.Validate(value, EntryType.DataItem, encoding, 0, 0, "foo");

            Assert.IsNotNull(nullableFeedback);
            KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue> feedback = (KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>)nullableFeedback;
            Assert.AreEqual(ValidationFeedbackRule.DataValidationFeedbackInvalidString, feedback.Key.Rule);
            Assert.AreEqual(notAllowedValue, feedback.Value.AdditionalInfo);
            Assert.AreEqual(ValidationFeedbackLevel.Error, feedback.Key.Level);
        }
    }
}