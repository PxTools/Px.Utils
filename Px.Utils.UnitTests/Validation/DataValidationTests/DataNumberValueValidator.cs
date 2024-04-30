using PxUtils.Validation;
using PxUtils.Validation.DataValidation;
using System.Text;

namespace Px.Utils.UnitTests.Validation.DataValidationTests
{
    [TestClass]
    public class DataNumberValueValidatorTest
    {

        [TestMethod]
        [DataRow("0")]
        [DataRow("0.1")]
        [DataRow("1.1")]
        [DataRow("-1.1")]
        [DataRow("1234567890")]
        [DataRow("-9999999999999999999999999999")]
        [DataRow("9999999999999999999999999999")]
        [DataRow("-79228162514264337593543950335")]
        [DataRow("79228162514264337593543950335")]
        public void AllowedNumberValues(string allowedValue)
        {
            DataNumberValidator validator = new();
            Encoding encoding = Encoding.UTF8;
            List<byte> value = [.. encoding.GetBytes(allowedValue)];
            IEnumerable<ValidationFeedback> feedbacks = validator.Validate(value, EntryType.DataItem, encoding, 0, 0);
            Assert.IsFalse(feedbacks.Any());
        }
    
        [TestMethod]
        [DataRow("-0")]
        [DataRow("0.")]
        [DataRow("00")]
        [DataRow("01")]
        [DataRow(".11")]
        [DataRow("11.")]
        [DataRow("1.1.1")]
        [DataRow("1\"1")]
        [DataRow("1-2")]
        [DataRow("-1-2")]
        [DataRow("-79228162514264337593543950336")]
        [DataRow("79228162514264337593543950336")]
        public void NotAllowedNumberValue(string notAllowedValue)
        {
            DataNumberValidator validator = new();

            Encoding encoding = Encoding.UTF8;
            List<byte> value = [.. encoding.GetBytes(notAllowedValue)];
            IEnumerable<ValidationFeedback> feedbacks = validator.Validate(value, EntryType.DataItem, encoding, 0, 0);

        
            Assert.AreEqual(1, feedbacks.Count());
            Assert.AreEqual(ValidationFeedbackRule.DataValidationFeedbackInvalidNumber, feedbacks.First().Rule);
            Assert.AreEqual(notAllowedValue, feedbacks.First().AdditionalInfo);
            Assert.AreEqual(ValidationFeedbackLevel.Error, feedbacks.First().Level);
        }


    }
}