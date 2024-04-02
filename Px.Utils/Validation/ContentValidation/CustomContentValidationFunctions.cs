

using PxUtils.Validation.SyntaxValidation;

namespace PxUtils.Validation.ContentValidation
{
    // TODO: Summaries
    public class CustomContentValidationFunctions(
        List<ContentValidationEntryFunctionDelegate> contentValidationEntryFunctions,
        List<ContentValidationSearchFunctionDelegate> contentValidationSearchFunctions
        )
    {
        public List<ContentValidationEntryFunctionDelegate> CustomContentValidationEntryFunctions { get; } = contentValidationEntryFunctions;
        public List<ContentValidationSearchFunctionDelegate> CustomContentValidationSearchFunctions { get; } = contentValidationSearchFunctions;
    }
}
