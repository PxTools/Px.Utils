
namespace Px.Utils.Validation.ContentValidation
{
    internal class ContentValidationResult(ValidationFeedbackItem[] feedbackItems, int dataRowLength, int dataRowAmount): IValidationResult
    {
        public ValidationFeedbackItem[] FeedbackItems { get; } = feedbackItems;
        public int DataRowLength { get; } = dataRowLength;
        public int DataRowAmount { get; } = dataRowAmount;
    }
}
