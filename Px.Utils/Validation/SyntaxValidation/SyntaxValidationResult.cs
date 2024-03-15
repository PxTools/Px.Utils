namespace PxUtils.Validation.SyntaxValidation
{
    public readonly struct SyntaxValidationResult(ValidationReport report, List<StructuredValidationEntry> structuredEntries)
    {
        public ValidationReport Report { get; } = report;
        public List<StructuredValidationEntry> StructuredEntries { get; } = structuredEntries;
    }
}
