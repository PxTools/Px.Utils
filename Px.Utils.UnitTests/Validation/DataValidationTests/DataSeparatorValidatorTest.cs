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
            ValidationFeedback? nullableFeedback = validator.Validate(separator, EntryType.DataItemSeparator, Encoding.UTF8, 1, 1);

            Assert.IsNull(nullableFeedback);
        }
    
        [TestMethod]
        public void InconsistentSeparator()
        {
            DataSeparatorValidator validator = new();
            List<byte> separator = [.. Encoding.UTF8.GetBytes(" ")];
            validator.Validate(separator, EntryType.DataItemSeparator, Encoding.UTF8, 1, 1);
            List<byte> otherSeparator = [.. Encoding.UTF8.GetBytes("\t")];
            ValidationFeedback? nullableFeedback = validator.Validate(otherSeparator, EntryType.DataItemSeparator, Encoding.UTF8, 1, 1);

            Assert.IsNotNull(nullableFeedback);
            ValidationFeedback feedback = (ValidationFeedback)nullableFeedback;
            Assert.AreEqual(ValidationFeedbackRule.DataValidationFeedbackInconsistentSeparator, feedback.Rule);
            Assert.AreEqual(ValidationFeedbackLevel.Warning, feedback.Level);

        
        }
    
        [TestMethod]
        public void ConsistentSeparator()
        {
            DataSeparatorValidator validator = new();
            List<byte> separator = [.. Encoding.UTF8.GetBytes(" ")];
            validator.Validate(separator, EntryType.DataItemSeparator, Encoding.UTF8, 1, 1);
            ValidationFeedback? nullableFeedback = validator.Validate(separator, EntryType.DataItemSeparator, Encoding.UTF8, 1, 1);
            Assert.IsNull(nullableFeedback);
        }
    }
}