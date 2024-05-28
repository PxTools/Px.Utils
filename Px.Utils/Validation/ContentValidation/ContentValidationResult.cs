
namespace Px.Utils.Validation.ContentValidation
{
    internal class ContentValidationResult(ValidationFeedbackItem[] feedbackItems): IValidationResult
    {
        public ValidationFeedbackItem[] FeedbackItems { get; } = feedbackItems;
    }
}
