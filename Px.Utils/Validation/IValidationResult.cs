namespace Px.Utils.Validation
{
    /// <summary>
    /// Represents a feedback item that was produced during a px file validation operation.
    /// </summary>
    public class ValidationResult(ValidationFeedbackItem[] feedbackItems)
    {
        public ValidationFeedbackItem[] FeedbackItems { get; } = feedbackItems;
    }
}
