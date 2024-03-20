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
        /// Enum that defines the type of validation feedback rule. Can be used to categorize feedback by rule type or for translations.
        /// </summary>
        public ValidationFeedbackRule Rule { get; } = rule;

        /// <summary>
        /// Any additional information can be stored into this property.
        /// </summary>
        public string? AdditionalInfo { get; } = additionalInfo;
    }
}
