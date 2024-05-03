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
            List<ValidationFeedback> feedbacks = [];
            validator.Validate(value, EntryType.DataItem, encoding, 0, 0, ref feedbacks);
            Assert.IsTrue(feedbacks.Count == 0);
        }
    
        [TestMethod]
        public void NotAllowedStringValue()
        {
            DataStringValidator validator = new();

            Encoding encoding = Encoding.UTF8;
            List<byte> value = [.. encoding.GetBytes("X")];

            List<ValidationFeedback> feedbacks = [];
           validator.Validate(value, EntryType.DataItem, encoding, 0, 0, ref feedbacks);

            Assert.AreEqual(1, feedbacks.Count);
            Assert.AreEqual(ValidationFeedbackRule.DataValidationFeedbackInvalidString, feedbacks[0].Rule);
            Assert.AreEqual("X", feedbacks[0].AdditionalInfo);
            Assert.AreEqual(ValidationFeedbackLevel.Error, feedbacks[0].Level);
        }
    }
}