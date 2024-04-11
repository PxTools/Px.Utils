using PxUtils.Validation;
using PxUtils.Validation.DataValidation;

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
            var feedbacks = validator.Validate(new Token(TokenType.StringDataItem, allowedValue, 1, 1));
            Assert.IsFalse(feedbacks.Any());
        }
    
        [TestMethod]
        public void NotAllowedStringValue()
        {
            DataStringValidator validator = new();

            var feedbacks = validator.Validate(new Token(TokenType.StringDataItem, "X", 1, 1));
        
            Assert.AreEqual(1, feedbacks.Count());
            Assert.AreEqual(ValidationFeedbackRule.DataValidationFeedbackInvalidString, feedbacks.First().Rule);
            Assert.AreEqual("X", feedbacks.First().AdditionalInfo);
            Assert.AreEqual(ValidationFeedbackLevel.Error, feedbacks.First().Level);
        }
    }
}