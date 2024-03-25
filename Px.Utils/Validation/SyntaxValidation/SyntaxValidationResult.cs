namespace PxUtils.Validation.SyntaxValidation
{
    /// <summary>
    /// Represents the result of a syntax validation operation. This struct contains a validation report and a list of structured validation entries.
    /// </summary>
    /// <param name="report">The <see cref="ValidationReport"/> produced by the syntax validation operation.</param>
    /// <param name="result">A list of <see cref="ValidationStructuredEntry"/> objects produced by the syntax validation operation.</param>
    public class SyntaxValidationResult(ValidationFeedbackItem[] feedbackItems, List<ValidationStructuredEntry> result)
    {

        /// <summary>
        /// Gets or sets the list of <see cref="ValidationFeedbackItem"/> objects that were produced during a validation operation.
        /// </summary>
        public IReadOnlyList<ValidationFeedbackItem> FeedbackItems { get; } = feedbackItems;

        /// <summary>
        /// Gets the list of <see cref="ValidationStructuredEntry"/> objects produced by the syntax validation operation.
        /// </summary>
        public List<ValidationStructuredEntry> Result { get; } = result;
    }
}
