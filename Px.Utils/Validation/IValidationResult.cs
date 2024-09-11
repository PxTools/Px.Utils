namespace Px.Utils.Validation
{
    /// <summary>
    /// Represents a feedback item that was produced during a px file validation operation.
    /// </summary>
    /// <param name="feedbackItems">The feedback items that were produced during the validation operation.</param>
    public class ValidationResult(ValidationFeedback feedbackItems)
    {
        public ValidationFeedback FeedbackItems { get; } = feedbackItems;
    }
}
