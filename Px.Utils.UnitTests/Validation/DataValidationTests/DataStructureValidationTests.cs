using PxUtils.Validation;
using PxUtils.Validation.DataValidation;

namespace Px.Utils.UnitTests.Validation.DataValidationTests;

[TestClass]
public class DataStructureValidatorTest
{

    [TestMethod]
    [DataRow([TokenType.StringDataItem])]
    [DataRow([TokenType.NumDataItem])]

    [DataRow([TokenType.StringDataItem, TokenType.DataItemSeparator])]
    [DataRow([TokenType.NumDataItem, TokenType.DataItemSeparator])]

    [DataRow([TokenType.NumDataItem, TokenType.DataItemSeparator, TokenType.NumDataItem])]
    [DataRow([TokenType.NumDataItem, TokenType.DataItemSeparator, TokenType.StringDataItem])]
    [DataRow([TokenType.NumDataItem, TokenType.DataItemSeparator, TokenType.LineSeparator])]

    [DataRow([TokenType.NumDataItem, TokenType.DataItemSeparator, TokenType.LineSeparator, TokenType.NumDataItem])]
    [DataRow([TokenType.NumDataItem, TokenType.DataItemSeparator, TokenType.LineSeparator, TokenType.StringDataItem])]
    [DataRow([TokenType.NumDataItem, TokenType.EndOfData])]

    public void AllowedTokenSequences(params TokenType[] tokenSequence)
    {
        var feedbacks = new List<ValidationFeedback>();
        var validator = new DataStructureValidator();
        foreach (var tokenType in tokenSequence)
        {
            feedbacks.AddRange(validator.Validate(new Token(tokenType, " ", 1, 1)));
        }
        
        Assert.IsFalse(feedbacks.Any());
    }
    
    
    [TestMethod]
    [DataRow(TokenType.InvalidDataChar)]
    [DataRow([TokenType.StringDataItem, TokenType.InvalidDataChar])]
    [DataRow([TokenType.NumDataItem, TokenType.InvalidDataChar])]
    [DataRow([TokenType.StringDataItem, TokenType.DataItemSeparator, TokenType.DataItemSeparator])]
    [DataRow([TokenType.StringDataItem, TokenType.LineSeparator])]
    [DataRow([TokenType.StringDataItem, TokenType.DataItemSeparator, TokenType.LineSeparator, TokenType.LineSeparator])]
    [DataRow([TokenType.StringDataItem, TokenType.EndOfStream])]
    [DataRow([TokenType.StringDataItem, TokenType.DataItemSeparator, TokenType.EndOfData])]
    public void NotAllowedTokenSequences(params TokenType[] tokenSequence)
    {
        var feedbacks = new List<ValidationFeedback>();
        var validator = new DataStructureValidator();
        foreach (var tokenType in tokenSequence)
        {
            feedbacks.AddRange(validator.Validate(new Token(tokenType, " ", 1, 1)));
        }
        
        Assert.AreEqual(1, feedbacks.Count);
        Assert.AreEqual(ValidationFeedbackRule.DataValidationFeedbackInvalidStructure, feedbacks[0].Rule);
        Assert.AreEqual(ValidationFeedbackLevel.Error, feedbacks[0].Level);
        var expectedTokens = new List<TokenType> { tokenSequence.Length>1?tokenSequence[^2]:TokenType.EmptyToken, tokenSequence[^1] };
        Assert.AreEqual(string.Join(",", expectedTokens), feedbacks[0].AdditionalInfo);
    }

}