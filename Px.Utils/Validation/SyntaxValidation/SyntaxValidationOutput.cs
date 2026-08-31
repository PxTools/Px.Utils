namespace Px.Utils.Validation.SyntaxValidation
{
    internal sealed record SyntaxValidationOutput(
        List<ValidationStructuredEntry> StructuredEntries,
        int DataStartRow,
        long DataStartStreamPosition);
}