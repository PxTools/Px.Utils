using PxUtils.Validation;
using PxUtils.Validation.DataValidation;
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
            IEnumerable<ValidationFeedback> feedbacks = validator.Validate(separator, EntryType.DataItemSeparator, Encoding.UTF8, 1, 1);

            Assert.IsFalse(feedbacks.Any());
        }
    
        [TestMethod]
        public void InconsistentSeparator()
        {
            DataSeparatorValidator validator = new();
            List<byte> separator = [.. Encoding.UTF8.GetBytes(" ")];
            validator.Validate(separator, EntryType.DataItemSeparator, Encoding.UTF8, 1, 1);
            List<byte> otherSeparator = [.. Encoding.UTF8.GetBytes("\t")];
            IEnumerable<ValidationFeedback> feedbacks = validator.Validate(otherSeparator, EntryType.DataItemSeparator, Encoding.UTF8, 1, 1);

            ValidationFeedback[] validationFeedbacks = feedbacks as ValidationFeedback[] ?? feedbacks.ToArray();
            Assert.AreEqual(1, validationFeedbacks.Length);
            Assert.AreEqual(ValidationFeedbackRule.DataValidationFeedbackInconsistentSeparator, validationFeedbacks[0].Rule);
            Assert.AreEqual(ValidationFeedbackLevel.Warning, validationFeedbacks[0].Level);

        
        }
    
        [TestMethod]
        public void ConsistentSeparator()
        {
            DataSeparatorValidator validator = new();
            List<byte> separator = [.. Encoding.UTF8.GetBytes(" ")];
            validator.Validate(separator, EntryType.DataItemSeparator, Encoding.UTF8, 1, 1);
            IEnumerable<ValidationFeedback> feedbacks = validator.Validate(separator, EntryType.DataItemSeparator, Encoding.UTF8, 1, 1);
            Assert.IsFalse(feedbacks.Any());
        }
    }
}