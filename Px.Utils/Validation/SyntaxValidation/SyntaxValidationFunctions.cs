using PxUtils.PxFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PxUtils.Validation.SyntaxValidation
{

    public class MultipleEntriesOnLine : IValidationFunction
    {
        public bool IsRelevant(ValidationEntry entry)
        {
            StringValidationEntry? stringEntry = entry as StringValidationEntry ?? throw new ArgumentException("Entry is not of type StringValidationEntry");

            // Validation is not relevant for first entry
            return stringEntry.EntryIndex > 0;
        }

        public ValidationFeedbackItem? Validate(ValidationEntry entry)
        {
            StringValidationEntry? stringEntry = entry as StringValidationEntry ?? throw new ArgumentException("Entry is not of type StringValidationEntry");

            // If the entry does not start with a line separator, it is not on its own line
            if (!stringEntry.EntryString.StartsWith(stringEntry.SyntaxConf.Symbols.LineSeparator))
            {
                return new ValidationFeedbackItem(entry, new SyntaxValidationFeedbackMultipleEntriesOnLine());
            }
            else
            {
                return null;
            }
        }
    }

    public class MoreThanOneLanguageParameter : IValidationFunction
    {
        public bool IsRelevant(ValidationEntry entry)
        {
            return true;
        }

        public ValidationFeedbackItem? Validate(ValidationEntry entry)
        {
            KeyValuePairValidationEntry? keyValueValidationEntry = entry as KeyValuePairValidationEntry ?? throw new ArgumentException("Entry is not of type KeyValueValidationEntry");

            bool hasMultipleParameters = SyntaxValidationUtilityMethods.HasMoreThanOneParameter(keyValueValidationEntry.KeyValueEntry.Key, keyValueValidationEntry.SyntaxConf.Symbols.Key.LangParamStart);
            
            if (hasMultipleParameters)
            {
                return new ValidationFeedbackItem(entry, new SyntaxValidationFeedbackMoreThanOneLanguage());
            }
            else
            {
                return null;
            }
        }
    }

    public class MoreThanOneSpecifierParameter : IValidationFunction
    {
        public bool IsRelevant(ValidationEntry entry)
        {
            return true;
        }

        public ValidationFeedbackItem? Validate(ValidationEntry entry)
        {
            KeyValuePairValidationEntry? keyValueValidationEntry = entry as KeyValuePairValidationEntry ?? throw new ArgumentException("Entry is not of type KeyValueValidationEntry");

            bool hasMultipleParameters = SyntaxValidationUtilityMethods.HasMoreThanOneParameter(keyValueValidationEntry.KeyValueEntry.Key, keyValueValidationEntry.SyntaxConf.Symbols.Key.SpecifierParamStart);

            if (hasMultipleParameters)
            {
                return new ValidationFeedbackItem(entry, new SyntaxValidationFeedbackMoreThanOneSpecifier());
            }
            else
            {
                return null;
            }
        }
    }

    public class WrongKeyOrder : IValidationFunction
    {
        public bool IsRelevant(ValidationEntry entry)
        {
            return true;
        }

        public ValidationFeedbackItem? Validate(ValidationEntry entry)
        {
            KeyValuePairValidationEntry? keyValueValidationEntry = entry as KeyValuePairValidationEntry ?? throw new ArgumentException("Entry is not of type KeyValueValidationEntry");

            string key = keyValueValidationEntry.KeyValueEntry.Key;
            PxFileSyntaxConf syntaxConf = keyValueValidationEntry.SyntaxConf;

            int langParamStartIndex = key.IndexOf(syntaxConf.Symbols.Key.LangParamStart);
            int specifierParamStartIndex = key.IndexOf(syntaxConf.Symbols.Key.SpecifierParamStart);

            if (langParamStartIndex == -1 && specifierParamStartIndex == -1)
            {
                return null;
            }
            else
            {
                if (langParamStartIndex > specifierParamStartIndex)
                {
                    return new ValidationFeedbackItem(entry, new SyntaxValidationFeedbackKeyHasWrongOrder());
                }
            }
            return null;
        }
    }
}
