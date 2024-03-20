namespace PxUtils.Validation
{
    /// <summary>
    /// Validation feedback represents the result of a validation operation.
    /// </summary>
    public readonly struct ValidationFeedback(ValidationFeedbackLevel level, ValidationFeedbackRule rule, string? additionalInfo = null)
    {
        /// <summary>
        /// Enum that gets the level of the feedback. This can be used to categorize feedback items by severity.
        /// </summary>
        public ValidationFeedbackLevel Level { get; } = level;

        /// <summary>
        /// Gets the description of the rule that was applied during validation. This can be used to identify which validation rule produced the feedback.
        /// </summary>
        public ValidationFeedbackRule Rule { get; } = rule;

        /// <summary>
        /// Any additional information can be stored into this property.
        /// </summary>
        public string? AdditionalInfo { get; } = additionalInfo;
    }
}
