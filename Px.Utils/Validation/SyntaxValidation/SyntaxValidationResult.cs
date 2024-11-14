namespace Px.Utils.Validation.SyntaxValidation
{
    /// <summary>
    /// Represents the result of a syntax validation operation. This struct contains a validation report and a list of structured validation entries.
    /// </summary>
    /// <param name="feedbacks">A dictionary of <see cref="ValidationFeedbackKey"/> amd <see cref="ValidationFeedbackValue"/> objects produced by the syntax validation operation.</param>
    /// <param name="result">A list of <see cref="ValidationStructuredEntry"/> objects produced by the syntax validation operation.</param>
    /// <param name="dataStartRow">The row number where the data section starts in the file.</param>
    /// <param name="dataStartStreamPosition">The stream position where the data section starts in the file.</param>
    public class SyntaxValidationResult(ValidationFeedback feedbacks, List<ValidationStructuredEntry> result, int dataStartRow, int dataStartStreamPosition) : ValidationResult(feedbacks)
    {
        /// <summary>
        /// Gets the list of <see cref="ValidationStructuredEntry"/> objects produced by the syntax validation operation.
        /// </summary>
        public List<ValidationStructuredEntry> Result { get; } = result;
        public int DataStartRow { get; } = dataStartRow;
        public long DataStartStreamPosition { get; } = dataStartStreamPosition;
    }
}
