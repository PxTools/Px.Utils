namespace PxUtils.Validation
{
    /// <summary>
    /// Validation feedback represents the result of a validation operation.
    /// </summary>
    public readonly struct ValidationFeedback(ValidationFeedbackLevel level, ValidationFeedbackRule rule, int line = 0, int character = 0, string? additionalInfo = null)
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
        /// Index of the line the feedback is associated with.
        /// </summary>
        public int Line { get; } = line;

        /// <summary>
        /// Index of the character where the issue related to the feedback starts.
        /// </summary>
        public int Character { get; } = character;

        /// <summary>
        /// Any additional information can be stored into this property.
        /// </summary>
        public string? AdditionalInfo { get; } = additionalInfo;
    }
}
