using PxUtils.Validation;
using PxUtils.Validation.DataValidation;

namespace Px.Utils.UnitTests.Validation.DataValidationTests;

[TestClass]
public class DataRowLengthValidatorTest
{
    
    [TestMethod]
    public void RowLengthIsCorrect()
    {
        var validator = new DataRowLengthValidator(3);
        var feedbacks = validator.Validate(new Token(TokenType.DataItemSeparator, " ", 1, 1));
        Assert.IsFalse(feedbacks.Any());
        feedbacks = validator.Validate(new Token(TokenType.DataItemSeparator, " ", 1, 1));
        Assert.IsFalse(feedbacks.Any());
        feedbacks = validator.Validate(new Token(TokenType.DataItemSeparator, " ", 1, 1));
        Assert.IsFalse(feedbacks.Any());
        feedbacks = validator.Validate(new Token(TokenType.LineSeparator, " ", 1, 1));
        Assert.IsFalse(feedbacks.Any());
    }
    
    [TestMethod]
    public void TooFewDataItems()
    {
        var validator = new DataRowLengthValidator(3);
        validator.Validate(new Token(TokenType.DataItemSeparator, " ", 1, 1));
        validator.Validate(new Token(TokenType.DataItemSeparator, " ", 1, 1));
        var feedbacks = validator.Validate(new Token(TokenType.LineSeparator, " ", 1, 1));
        
        Assert.AreEqual(1, feedbacks.Count());
        Assert.AreEqual(ValidationFeedbackRule.DataValidationFeedbackInvalidRowLength, feedbacks.First().Rule);
        Assert.AreEqual("3,2", feedbacks.First().AdditionalInfo);
        Assert.AreEqual(ValidationFeedbackLevel.Error, feedbacks.First().Level);

    }

    [TestMethod]
    public void TooManyDataItems()
    {
        var validator = new DataRowLengthValidator(3);
        validator.Validate(new Token(TokenType.DataItemSeparator, " ", 1, 1));
        validator.Validate(new Token(TokenType.DataItemSeparator, " ", 1, 1));
        validator.Validate(new Token(TokenType.DataItemSeparator, " ", 1, 1));
        validator.Validate(new Token(TokenType.DataItemSeparator, " ", 1, 1));
        var feedbacks = validator.Validate(new Token(TokenType.LineSeparator, " ", 1, 1));

        Assert.AreEqual(1, feedbacks.Count());
        Assert.AreEqual(ValidationFeedbackRule.DataValidationFeedbackInvalidRowLength, feedbacks.First().Rule);
        Assert.AreEqual("3,4", feedbacks.First().AdditionalInfo);
        Assert.AreEqual(ValidationFeedbackLevel.Error, feedbacks.First().Level);

    }

}