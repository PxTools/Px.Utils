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
            List<ValidationFeedback> feedbacks = [];
            validator.Validate(separator, EntryType.DataItemSeparator, Encoding.UTF8, 1, 1, ref feedbacks);

            Assert.IsTrue(feedbacks.Count == 0);
        }
    
        [TestMethod]
        public void InconsistentSeparator()
        {
            DataSeparatorValidator validator = new();
            List<byte> separator = [.. Encoding.UTF8.GetBytes(" ")];
            List<ValidationFeedback> feedbacks = [];
            validator.Validate(separator, EntryType.DataItemSeparator, Encoding.UTF8, 1, 1, ref feedbacks);
            List<byte> otherSeparator = [.. Encoding.UTF8.GetBytes("\t")];
            validator.Validate(otherSeparator, EntryType.DataItemSeparator, Encoding.UTF8, 1, 1, ref feedbacks);

            Assert.AreEqual(1, feedbacks.Count);
            Assert.AreEqual(ValidationFeedbackRule.DataValidationFeedbackInconsistentSeparator, feedbacks[0].Rule);
            Assert.AreEqual(ValidationFeedbackLevel.Warning, feedbacks[0].Level);

        
        }
    
        [TestMethod]
        public void ConsistentSeparator()
        {
            DataSeparatorValidator validator = new();
            List<byte> separator = [.. Encoding.UTF8.GetBytes(" ")];
            List<ValidationFeedback> feedbacks = [];
            validator.Validate(separator, EntryType.DataItemSeparator, Encoding.UTF8, 1, 1, ref feedbacks);
            validator.Validate(separator, EntryType.DataItemSeparator, Encoding.UTF8, 1, 1, ref feedbacks);
            Assert.IsTrue(feedbacks.Count == 0);
        }
    }
}