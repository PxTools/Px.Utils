namespace Px.Utils.Validation
{
    /// <summary>
    /// Represents the result of a validation operation. This interface contains a list of feedback items that were produced during a validation operation.
    /// </summary>
    public interface IValidationResult
    {
        public ValidationFeedbackItem[] FeedbackItems { get; }
    }

    /// <summary>
    /// Represents a feedback item that was produced during a full Px file validation operation.
    /// </summary>
    public class PxFileValidationResult(ValidationFeedbackItem[] feedbackItems): IValidationResult
    {
        public ValidationFeedbackItem[] FeedbackItems { get; } = feedbackItems;
    }
}
