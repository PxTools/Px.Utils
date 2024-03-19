namespace PxUtils.Validation
{
    /// <summary>
    /// Represents a validation report. A validation report contains a list of validation feedback items that were produced during a validation operation.
    /// </summary>
    public class ValidationReport
    {

        /// <summary>
        /// Gets or sets the list of <see cref="ValidationFeedbackItem"/> objects that were produced during a validation operation.
        /// </summary>
        public List<ValidationFeedbackItem> FeedbackItems { get; set; } = [];
    }
}
