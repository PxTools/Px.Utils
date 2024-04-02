using PxUtils.PxFile;
using PxUtils.Validation.SyntaxValidation;

namespace PxUtils.Validation.ContentValidation
{
    internal static class ContentValidationUtilityMethods
    {
        internal static Dictionary<KeyValuePair<string, string>, string[]> FindVariableValues(
            ValidationStructuredEntry[] entries,
            PxFileSyntaxConf syntaxConf,
            Dictionary<string, string[]>? variables,
            ref List<ValidationFeedbackItem> feedbackItems,
            string filename)
        {
            if (variables is null)
            {
                return [];
            }

            Dictionary<KeyValuePair<string, string>, string[]> variableValues = [];

            foreach (string language in variables.Keys)
            {
                foreach (string dimension in variables[language])
                {
                    ValidationStructuredEntry? valuesEntry = Array.Find(entries,
                        e => e.Key.FirstSpecifier is not null &&
                        e.Key.FirstSpecifier.Equals(dimension));
                    if (valuesEntry is not null)
                    {
                        variableValues.Add(
                            new KeyValuePair<string, string> ( language, dimension ),
                            valuesEntry.Value.Split(syntaxConf.Symbols.Value.ListSeparator)
                            );
                    }
                    else
                    {
                        feedbackItems.Add(new ValidationFeedbackItem(
                            new ContentValidationObject(filename, 0, []),
                            new ValidationFeedback(
                                ValidationFeedbackLevel.Error,
                                ValidationFeedbackRule.VariableValuesMissing,
                                0,
                                0,
                                $"{dimension}, {language}"
                                )
                            ));
                    }
                }
            }

            return variableValues;
        }
    }
}
