using PxUtils.Validation;
using PxUtils.Validation.DataValidation;

namespace Px.Utils.UnitTests.Validation.DataValidationTests
{
    [TestClass]
    public class DataStructureValidatorTest
    {
        /*
        [TestMethod]
        [DataRow([EntryType.StringDataItem])]
        [DataRow([EntryType.DataItem])]

        [DataRow([EntryType.StringDataItem, EntryType.DataItemSeparator])]
        [DataRow([EntryType.DataItem, EntryType.DataItemSeparator])]

        [DataRow([EntryType.DataItem, EntryType.DataItemSeparator, EntryType.DataItem])]
        [DataRow([EntryType.DataItem, EntryType.DataItemSeparator, EntryType.StringDataItem])]
        [DataRow([EntryType.DataItem, EntryType.DataItemSeparator, EntryType.LineSeparator])]

        [DataRow([EntryType.DataItem, EntryType.DataItemSeparator, EntryType.LineSeparator, EntryType.DataItem])]
        [DataRow([EntryType.DataItem, EntryType.DataItemSeparator, EntryType.LineSeparator, EntryType.StringDataItem])]
        [DataRow([EntryType.DataItem, EntryType.EndOfData])]

        public void AllowedTokenSequences(params EntryType[] tokenSequence)
        {
            List<ValidationFeedback> feedbacks = [];
            DataStructureValidator validator = new();
            foreach (EntryType tokenType in tokenSequence)
            {
                feedbacks.AddRange(validator.Validate(new Token(tokenType, " ", 1, 1)));
            }
        
            Assert.IsFalse(feedbacks.Any());
        }
    
    
        [TestMethod]
        [DataRow(EntryType.InvalidDataChar)]
        [DataRow([EntryType.StringDataItem, EntryType.InvalidDataChar])]
        [DataRow([EntryType.DataItem, EntryType.InvalidDataChar])]
        [DataRow([EntryType.StringDataItem, EntryType.DataItemSeparator, EntryType.DataItemSeparator])]
        [DataRow([EntryType.StringDataItem, EntryType.LineSeparator])]
        [DataRow([EntryType.StringDataItem, EntryType.DataItemSeparator, EntryType.LineSeparator, EntryType.LineSeparator])]
        [DataRow([EntryType.StringDataItem, EntryType.EndOfStream])]
        [DataRow([EntryType.StringDataItem, EntryType.DataItemSeparator, EntryType.EndOfData])]
        public void NotAllowedTokenSequences(params EntryType[] tokenSequence)
        {
            List<ValidationFeedback> feedbacks = [];
            DataStructureValidator validator = new();
            foreach (EntryType tokenType in tokenSequence)
            {
                feedbacks.AddRange(validator.Validate(new Token(tokenType, " ", 1, 1)));
            }
        
            Assert.AreEqual(1, feedbacks.Count);
            Assert.AreEqual(ValidationFeedbackRule.DataValidationFeedbackInvalidStructure, feedbacks[0].Rule);
            Assert.AreEqual(ValidationFeedbackLevel.Error, feedbacks[0].Level);
            List<EntryType> expectedTokens = new List<EntryType> { tokenSequence.Length>1?tokenSequence[^2]:EntryType.EmptyToken, tokenSequence[^1] };
            Assert.AreEqual(string.Join(",", expectedTokens), feedbacks[0].AdditionalInfo);
        }
        */
    }
}