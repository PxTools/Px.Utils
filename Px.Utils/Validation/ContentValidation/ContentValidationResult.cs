﻿
namespace Px.Utils.Validation.ContentValidation
{
    /// <summary>
    /// Represents the result of a px file metadata content validation operation. Contains a validation report and information about the expected dimensions of the data section.
    /// </summary>
    /// <param name="feedbackItems">A <see cref="ValidationFeedback"/> object containing information about rule violations gathered during the metadata content validation process.</param>
    /// <param name="dataRowLength">Expected length of each data row.</param>
    /// <param name="dataRowAmount">Expected data column length/amount of data rows.</param>
    public sealed class ContentValidationResult(ValidationFeedback feedbackItems, int dataRowLength, int dataRowAmount) : ValidationResult(feedbackItems)
    {
        public int DataRowLength { get; } = dataRowLength;
        public int DataRowAmount { get; } = dataRowAmount;
    }
}
