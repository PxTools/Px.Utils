namespace Px.Utils.Validation.SyntaxValidation
{
    /// <summary>
    /// Represents the result of a syntax validation operation. This struct contains a validation report and a list of structured validation entries.
    /// </summary>
    /// <param name="feedbacks">A dictionary of <see cref="ValidationFeedbackKey"/> amd <see cref="ValidationFeedbackValue"/> objects produced by the syntax validation operation.</param>
    /// <param name="result">A list of <see cref="ValidationStructuredEntry"/> objects produced by the syntax validation operation.</param>
    /// <param name="dataStartRow">The row number where the data section starts in the file.</param>
    /// <param name="dataStartStreamPosition">The absolute raw byte offset of the first non-whitespace data value after the DATA keyword, 
    /// suitable for direct assignment to <see cref="Stream.Position"/>; otherwise -1 when the DATA keyword or a value cannot be found.</param>
    public class SyntaxValidationResult(ValidationFeedback feedbacks, List<ValidationStructuredEntry> result, int dataStartRow, long dataStartStreamPosition) : ValidationResult(feedbacks)
    {
        /// <summary>
        /// Gets the list of <see cref="ValidationStructuredEntry"/> objects produced by the syntax validation operation.
        /// </summary>
        public List<ValidationStructuredEntry> Result { get; } = result;

        /// <summary>
        /// Gets the row number where the data section starts in the file.
        /// </summary>
        public int DataStartRow { get; } = dataStartRow;

        /// <summary>
        /// Gets the absolute raw byte offset of the first non-whitespace data value after the DATA keyword.
        /// The value is suitable for direct assignment to <see cref="Stream.Position"/> and is -1 when the DATA keyword or a value cannot be found.
        /// </summary>
        public long DataStartStreamPosition { get; } = dataStartStreamPosition;
    }
}
