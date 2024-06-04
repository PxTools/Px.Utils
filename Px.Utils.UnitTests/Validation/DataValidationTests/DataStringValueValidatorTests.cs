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
        public void AllowedStrings(string allowedValue)
        {
            DataStringValidator validator = new();
            Encoding encoding = Encoding.UTF8;
            List<byte> value = [.. encoding.GetBytes(allowedValue)];
            ValidationFeedback? nullableFeedback = validator.Validate(value, EntryType.DataItem, encoding, 0, 0);

            Assert.IsNull(nullableFeedback);
        }

        [TestMethod]
        [DataRow("\"...")]
        [DataRow("...\"")]
        [DataRow("\"\"")]
        [DataRow("...")]
        [DataRow("foo")]
        [DataRow("\"foo\"")]
        [DataRow("\".......\"")]
        [DataRow("\"--\"")]
        [DataRow("-")]
        public void NotAllowedStringValue(string notAllowedValue)
        {
            DataStringValidator validator = new();

            Encoding encoding = Encoding.UTF8;
            List<byte> value = [.. encoding.GetBytes(notAllowedValue)];

            ValidationFeedback? nullableFeedback = validator.Validate(value, EntryType.DataItem, encoding, 0, 0);

            Assert.IsNotNull(nullableFeedback);
            ValidationFeedback feedback = (ValidationFeedback)nullableFeedback;
            Assert.AreEqual(ValidationFeedbackRule.DataValidationFeedbackInvalidString, feedback.Rule);
            Assert.AreEqual(notAllowedValue, feedback.AdditionalInfo);
            Assert.AreEqual(ValidationFeedbackLevel.Error, feedback.Level);
        }
    }
}