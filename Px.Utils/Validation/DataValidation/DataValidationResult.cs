namespace Px.Utils.Validation.DataValidation
{
    internal class DataValidationResult(ValidationFeedbackItem[] feedbackItems): IValidationResult
    {
        public ValidationFeedbackItem[] FeedbackItems { get; } = feedbackItems;
    }
}
