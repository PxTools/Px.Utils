using PxUtils.Validation;
using PxUtils.Validation.DataValidation;
using System.Text;

namespace Px.Utils.UnitTests.Validation.DataValidationTests
{
    [TestClass]
    public class DataStructureValidatorTest
    {
        [TestMethod]
        [DataRow([EntryType.DataItem])]

        [DataRow([EntryType.DataItem, EntryType.DataItemSeparator])]
        [DataRow([EntryType.DataItem, EntryType.DataItemSeparator])]

        [DataRow([EntryType.DataItem, EntryType.DataItemSeparator, EntryType.DataItem])]
        [DataRow([EntryType.DataItem, EntryType.DataItemSeparator, EntryType.LineSeparator])]

        [DataRow([EntryType.DataItem, EntryType.DataItemSeparator, EntryType.LineSeparator, EntryType.DataItem])]
        [DataRow([EntryType.DataItem, EntryType.EndOfData])]

        public void AllowedTokenSequences(params EntryType[] tokenSequence)
        {
            List<ValidationFeedback> feedbacks = [];
            DataStructureValidator validator = new();
            foreach (EntryType tokenType in tokenSequence)
            {
                feedbacks.AddRange(validator.Validate([], tokenType, Encoding.UTF8, 1, 1));
            }
        
            Assert.IsFalse(feedbacks.Any());
        }
    
    
        [TestMethod]
        [DataRow([EntryType.DataItem, EntryType.DataItemSeparator, EntryType.DataItemSeparator])]
        [DataRow([EntryType.DataItem, EntryType.LineSeparator])]
        [DataRow([EntryType.DataItem, EntryType.DataItemSeparator, EntryType.LineSeparator, EntryType.LineSeparator])]
        [DataRow([EntryType.DataItem, EntryType.EndOfData, EntryType.DataItem])]
        [DataRow([EntryType.DataItem, EntryType.DataItemSeparator, EntryType.EndOfData])]
        public void NotAllowedTokenSequences(params EntryType[] tokenSequence)
        {
            List<ValidationFeedback> feedbacks = [];
            DataStructureValidator validator = new();
            foreach (EntryType tokenType in tokenSequence)
            {
                feedbacks.AddRange(validator.Validate([], tokenType, Encoding.UTF8, 1, 1));
            }

            Assert.AreEqual(1, feedbacks.Count);
            Assert.AreEqual(ValidationFeedbackRule.DataValidationFeedbackInvalidStructure, feedbacks[0].Rule);
            Assert.AreEqual(ValidationFeedbackLevel.Error, feedbacks[0].Level);
            List<EntryType> expectedTokens = [ tokenSequence.Length > 1 ? tokenSequence[^2] : EntryType.Unknown, tokenSequence[^1] ];
            Assert.AreEqual(string.Join(",", expectedTokens), feedbacks[0].AdditionalInfo);
        }
    }
}