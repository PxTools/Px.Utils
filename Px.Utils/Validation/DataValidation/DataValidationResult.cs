namespace Px.Utils.Validation.DataValidation
{
    public sealed class DataValidationResult(ValidationFeedbackItem[] feedbackItems): IValidationResult
    {
        public ValidationFeedbackItem[] FeedbackItems { get; } = feedbackItems;
    }
}
