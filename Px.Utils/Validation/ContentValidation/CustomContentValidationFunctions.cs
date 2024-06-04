namespace Px.Utils.Validation.ContentValidation
{
    /// <summary>
    /// Object storing optional custom content validation functions
    /// </summary>
    /// <param name="contentValidationSearchFunctions">Functions that are executed for the whole set of entries. 
    /// This is used when specific entries are to be searched or stored during the validation. These are ran first in the validation process</param>
    /// <param name="contentValidationEntryFunctions">Functions that are executed for individual entries. Used when specific properties of each or specific entries are to be validated. 
    /// These functions are ran after the search functions in the validation process.</param>
    public class CustomContentValidationFunctions(
        List<ContentValidationFindKeywordValidator> contentValidationSearchFunctions,
        List<ContentValidationEntryValidator> contentValidationEntryFunctions
        )
    {
        public List<ContentValidationFindKeywordValidator> CustomContentValidationFindKeywordFunctions { get; } = contentValidationSearchFunctions;
        public List<ContentValidationEntryValidator> CustomContentValidationEntryFunctions { get; } = contentValidationEntryFunctions;
    }
}
