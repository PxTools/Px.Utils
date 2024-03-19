namespace PxUtils.Validation
{
    /// <summary>
    /// Defines a common interface for validation feedback. Validation feedback represents the result of a validation operation.
    /// </summary>
    public interface IValidationFeedback
    {
        /// <summary>
        /// Enum that gets the level of the feedback. This can be used to categorize feedback items by severity.
        /// </summary>
        public ValidationFeedbackLevel Level { get; }

        /// <summary>
        /// Gets the description of the rule that was applied during validation. This can be used to identify which validation rule produced the feedback.
        /// </summary>
        public string Rule { get; }
    }
}
