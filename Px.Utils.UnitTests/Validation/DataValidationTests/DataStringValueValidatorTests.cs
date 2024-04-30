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
            IEnumerable<ValidationFeedback> feedbacks = validator.Validate(value, EntryType.StringDataItem, encoding, 0, 0);
            Assert.IsFalse(feedbacks.Any());
        }
    
        [TestMethod]
        public void NotAllowedStringValue()
        {
            DataStringValidator validator = new();

            Encoding encoding = Encoding.UTF8;
            List<byte> value = [.. encoding.GetBytes("X")];

            IEnumerable<ValidationFeedback> feedbacks = validator.Validate(value, EntryType.StringDataItem, encoding, 0, 0);

            Assert.AreEqual(1, feedbacks.Count());
            Assert.AreEqual(ValidationFeedbackRule.DataValidationFeedbackInvalidString, feedbacks.First().Rule);
            Assert.AreEqual("X", feedbacks.First().AdditionalInfo);
            Assert.AreEqual(ValidationFeedbackLevel.Error, feedbacks.First().Level);
        }
    }
}