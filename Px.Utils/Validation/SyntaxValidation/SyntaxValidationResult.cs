namespace Px.Utils.Validation.SyntaxValidation
{
    /// <summary>
    /// Represents the result of a syntax validation operation. This struct contains a validation report and a list of structured validation entries.
    /// </summary>
    /// <param name="feedbackItems">An array of <see cref="ValidationFeedbackItem"/> objects produced by the syntax validation operation.</param>
    /// <param name="result">A list of <see cref="ValidationStructuredEntry"/> objects produced by the syntax validation operation.</param>
    /// <param name="dataStartRow">The row number where the data section starts in the file.</param>
    /// <param name="dataStartStreamPosition">The stream position where the data section starts in the file.</param>
    public class SyntaxValidationResult(ValidationFeedbackItem[] feedbackItems, List<ValidationStructuredEntry> result, int dataStartRow, int dataStartStreamPosition) : ValidationResult(feedbackItems)
    {
        /// <summary>
        /// Gets the list of <see cref="ValidationStructuredEntry"/> objects produced by the syntax validation operation.
        /// </summary>
        public List<ValidationStructuredEntry> Result { get; } = result;
        public int DataStartRow { get; } = dataStartRow;
        public long DataStartStreamPosition { get; } = dataStartStreamPosition;
    }
}
