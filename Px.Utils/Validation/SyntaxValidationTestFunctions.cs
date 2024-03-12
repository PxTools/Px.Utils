using PxUtils.PxFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PxUtils.Validation
{
    public interface IStreamValidationFunction : IValidationFunction
    {
        ValidationFeedbackItem Validate(char nextCharacter, PxFileSyntaxConf symbolsConf, long line, int character, string filename);
        bool IsRelevant(char currentCharacter, PxFileSyntaxConf symbolsConf);
    }

    public class MultipleEntriesOnLineValidationFunction : IStreamValidationFunction
    {
        public ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Warning;
        public bool IsRelevant(char currentCharacter, PxFileSyntaxConf symbolsConf)
        {
            return currentCharacter == symbolsConf.Symbols.SectionSeparator;
        }

        public ValidationFeedbackItem Validate(char nextCharacter, PxFileSyntaxConf symbolsConf, long line, int character, string filename)
        {
            if (nextCharacter != '\n' && nextCharacter != '\0')
            {
                return new ValidationFeedbackItem(filename, line, character, new ValidationFeedbackMultipleEntriesOnLine());
            }
            else
            {
                return null;
            }
        }
    }
}
