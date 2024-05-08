using PxUtils.Validation;
using PxUtils.Validation.DataValidation;
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
        public void NotAllowedStringValue()
        {
            DataStringValidator validator = new();

            Encoding encoding = Encoding.UTF8;
            List<byte> value = [.. encoding.GetBytes("X")];

            ValidationFeedback? nullableFeedback = validator.Validate(value, EntryType.DataItem, encoding, 0, 0);

            Assert.IsNotNull(nullableFeedback);
            ValidationFeedback feedback = (ValidationFeedback)nullableFeedback;
            Assert.AreEqual(ValidationFeedbackRule.DataValidationFeedbackInvalidString, feedback.Rule);
            Assert.AreEqual("X", feedback.AdditionalInfo);
            Assert.AreEqual(ValidationFeedbackLevel.Error, feedback.Level);
        }
    }
}