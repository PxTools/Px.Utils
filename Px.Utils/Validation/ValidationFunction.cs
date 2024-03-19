namespace PxUtils.Validation
{
    /// <summary>
    /// Defines a common interface for validation functions. A validation function is used to validate a validation entry and produce a validation feedback item.
    /// </summary>
    public interface IValidationFunction
    {

        /// <summary>
        /// Determines whether the validation function is relevant for the given validation entry.
        /// </summary>
        /// <param name="entry">The <see cref="IValidationEntry"/> to check for relevance.</param>
        /// <returns>A boolean indicating whether the validation function is relevant for the given validation entry.</returns>
        bool IsRelevant(IValidationEntry entry);

        /// <summary>
        /// Validates the given validation entry and produces a <see cref="ValidationFeedbackItem"/> if there are any validation issues.
        /// </summary>
        /// <param name="entry">The <see cref="IValidationEntry"/> to validate.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if there are any validation issues, null otherwise.</returns>
        ValidationFeedbackItem? Validate(IValidationEntry entry);
    }
}
