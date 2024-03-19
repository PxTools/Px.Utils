namespace PxUtils.Validation.SyntaxValidation
{
    /// <summary>
    /// Represents the result of a syntax validation operation. This struct contains a validation report and a list of structured validation entries.
    /// </summary>
    /// <param name="report">The <see cref="ValidationReport"/> produced by the syntax validation operation.</param>
    /// <param name="structuredEntries">A list of <see cref="StructuredValidationEntry"/> objects produced by the syntax validation operation.</param>
    public readonly struct SyntaxValidationResult(ValidationReport report, List<StructuredValidationEntry> structuredEntries)
    {
        /// <summary>
        /// Gets the <see cref="ValidationReport"/> produced by the syntax validation operation.
        /// </summary>
        public ValidationReport Report { get; } = report;

        /// <summary>
        /// Gets the list of <see cref="StructuredValidationEntry"/> objects produced by the syntax validation operation.
        /// </summary>
        public List<StructuredValidationEntry> StructuredEntries { get; } = structuredEntries;
    }
}
