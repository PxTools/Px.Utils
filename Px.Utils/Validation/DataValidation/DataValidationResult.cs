namespace Px.Utils.Validation.DataValidation
{
    internal sealed class DataValidationResult(ValidationFeedbackItem[] feedbackItems): IValidationResult
    {
        public ValidationFeedbackItem[] FeedbackItems { get; } = feedbackItems;
    }
}
