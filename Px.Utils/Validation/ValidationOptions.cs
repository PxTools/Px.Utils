namespace Px.Utils.Validation
{
    /// <summary>
    /// Configures validation feedback retention.
    /// </summary>
    public sealed class ValidationOptions
    {
        /// <summary>
        /// Gets a configuration that retains every feedback item.
        /// </summary>
        public static ValidationOptions Unlimited { get; } = new() { MaxFeedbackItemsPerSignature = null };

        /// <summary>
        /// Gets or initializes the maximum number of feedback items retained for each filename, level, and rule signature. A null value retains all feedback items.
        /// </summary>
        public int? MaxFeedbackItemsPerSignature { get; init; } = 100;
    }
}
