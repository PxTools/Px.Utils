using Px.Utils.Validation;
using Px.Utils.Validation.DataValidation;
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
            ValidationFeedback feedbacks = [];
            DataStructureValidator validator = new();
            foreach (EntryType tokenType in tokenSequence)
            {
                KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? feedback = validator.Validate([], tokenType, Encoding.UTF8, 1, 1, "foo");
                if (feedback is not null)
                {
                    feedbacks.Add((KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>)feedback);
                }
            }
        
            Assert.IsTrue(feedbacks.Count == 0);
        }
    
    
        [TestMethod]
        [DataRow([EntryType.DataItem, EntryType.DataItemSeparator, EntryType.DataItemSeparator])]
        [DataRow([EntryType.DataItem, EntryType.LineSeparator])]
        [DataRow([EntryType.DataItem, EntryType.DataItemSeparator, EntryType.LineSeparator, EntryType.LineSeparator])]
        [DataRow([EntryType.DataItem, EntryType.EndOfData, EntryType.DataItem])]
        [DataRow([EntryType.DataItem, EntryType.DataItemSeparator, EntryType.EndOfData])]
        public void NotAllowedTokenSequences(params EntryType[] tokenSequence)
        {
            ValidationFeedback feedbacks = [];
            DataStructureValidator validator = new();
            foreach (EntryType tokenType in tokenSequence)
            {
                KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? feedback = validator.Validate([], tokenType, Encoding.UTF8, 1, 1, "foo");
                if (feedback is not null)
                {
                    feedbacks.Add((KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>)feedback);
                }
            }

            Assert.AreEqual(1, feedbacks.Count);
            Assert.AreEqual(ValidationFeedbackRule.DataValidationFeedbackInvalidStructure, feedbacks.First().Key.Rule);
            Assert.AreEqual(ValidationFeedbackLevel.Error, feedbacks.First().Key.Level);
            List<EntryType> expectedTokens = [ tokenSequence.Length > 1 ? tokenSequence[^2] : EntryType.Unknown, tokenSequence[^1] ];

            Assert.AreEqual(string.Join(",", expectedTokens), feedbacks.First().Value[0].AdditionalInfo);
        }
    }
}