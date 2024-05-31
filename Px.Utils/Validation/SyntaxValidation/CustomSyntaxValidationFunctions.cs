namespace Px.Utils.Validation.SyntaxValidation
{

    /// <summary>
    /// Collection of custom validation functions to be used during validation.
    /// </summary>
    public class CustomSyntaxValidationFunctions(
        List<EntryValidationFunction> stringValidationFunctions,
        List<KeyValuePairValidationFunction> keyValueValidationFunctions,
        List<StructuredValidationFunction> structuredValidationFunctions)
    {
        public List<EntryValidationFunction> CustomStringValidationFunctions { get; } = stringValidationFunctions;
        public List<KeyValuePairValidationFunction> CustomKeyValueValidationFunctions { get; } = keyValueValidationFunctions;
        public List<StructuredValidationFunction> CustomStructuredValidationFunctions { get; } = structuredValidationFunctions;
    }
}
