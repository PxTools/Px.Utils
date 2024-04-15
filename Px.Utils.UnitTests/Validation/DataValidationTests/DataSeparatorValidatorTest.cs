using PxUtils.Validation;
using PxUtils.Validation.DataValidation;

namespace Px.Utils.UnitTests.Validation.DataValidationTests
{
    [TestClass]
    public class DataSeparatorValidatorTest
    {

        [TestMethod]
        public void FirstSeparatorIsUsedAsReference()
        {
            DataSeparatorValidator validator = new();
            IEnumerable<ValidationFeedback> feedbacks = validator.Validate(new Token(TokenType.DataItemSeparator, " ", 1 ,1));

            Assert.IsFalse(feedbacks.Any());
        
        }
    
        [TestMethod]
        public void InconsistentSeparator()
        {
            DataSeparatorValidator validator = new();
            validator.Validate(new Token(TokenType.DataItemSeparator, " ", 1 ,1));

            IEnumerable<ValidationFeedback> feedbacks = validator.Validate(new Token(TokenType.DataItemSeparator, "\t", 1 ,1));

            ValidationFeedback[] validationFeedbacks = feedbacks as ValidationFeedback[] ?? feedbacks.ToArray();
            Assert.AreEqual(1, validationFeedbacks.Length);
            Assert.AreEqual(ValidationFeedbackRule.DataValidationFeedbackInconsistentSeparator, validationFeedbacks[0].Rule);
            Assert.AreEqual(ValidationFeedbackLevel.Warning, validationFeedbacks[0].Level);

        
        }
    
        [TestMethod]
        public void ConsistentSeparator()
        {
            DataSeparatorValidator validator = new();
            validator.Validate(new Token(TokenType.DataItemSeparator, " ", 1 ,1));

            IEnumerable<ValidationFeedback> feedbacks = validator.Validate(new Token(TokenType.DataItemSeparator, " ", 1 ,1));

            Assert.IsFalse(feedbacks.Any());
        }
    
    }
}