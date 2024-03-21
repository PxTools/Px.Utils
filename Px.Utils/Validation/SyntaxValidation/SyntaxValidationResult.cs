namespace PxUtils.Validation.SyntaxValidation
{
    /// <summary>
    /// Represents the result of a syntax validation operation. This struct contains a validation report and a list of structured validation entries.
    /// </summary>
    /// <param name="report">The <see cref="ValidationReport"/> produced by the syntax validation operation.</param>
    /// <param name="result">A list of <see cref="ValidationStruct"/> objects produced by the syntax validation operation.</param>
    public readonly struct SyntaxValidationResult(ValidationReport report, List<ValidationStruct> result)
    {
        /// <summary>
        /// Gets the <see cref="ValidationReport"/> produced by the syntax validation operation.
        /// </summary>
        public ValidationReport Report { get; } = report;

        /// <summary>
        /// Gets the list of <see cref="ValidationStruct"/> objects produced by the syntax validation operation.
        /// </summary>
        public List<ValidationStruct> Result { get; } = result;
    }
}
