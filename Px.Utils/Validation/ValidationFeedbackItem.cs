namespace PxUtils.Validation
{
    /// <summary>
    /// Represents a validation feedback item. A validation feedback item associates a validation entry with the feedback from validating that entry.
    /// </summary>
    /// <param name="entry">The <see cref="ValidationEntry"/> that this feedback item is associated with.</param>
    /// <param name="feedback">The <see cref="ValidationFeedback"/> from validating the entry.</param>
    public readonly struct ValidationFeedbackItem(ValidationEntry entry, ValidationFeedback feedback)
    {

        /// <summary>
        /// Gets the <see cref="ValidationEntry"/> that this feedback item is associated with.
        /// </summary>
        public ValidationEntry Entry { get; } = entry;


        /// <summary>
        /// Gets the <see cref="ValidationFeedback"/> from validating the entry.
        /// </summary>
        public ValidationFeedback Feedback { get; } = feedback;
    }
}
