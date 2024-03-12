using Px.Utils.Validation.SyntaxValidation;
using PxUtils.PxFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PxUtils.Validation.SyntaxValidation
{

    public class MultipleEntriesOnLineValidationFunction : IValidationFunction
    {
        public ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Warning;

        public bool IsRelevant(ValidationEntry entry)
        {
            StreamValidationEntry? streamEntry = entry as StreamValidationEntry;
            return streamEntry != null
                ? streamEntry.CurrentCharacter == streamEntry.SyntaxConf.Symbols.SectionSeparator
                : throw new ArgumentException("Entry is not of type StreamValidationEntry");
        }

        public ValidationFeedbackItem Validate(ValidationEntry entry)
        {
            StreamValidationEntry? streamEntry = entry as StreamValidationEntry;
            if (streamEntry == null)
            {
                throw new ArgumentException("Entry is not of type StreamValidationEntry");
            }

            if (streamEntry.NextCharacter != streamEntry.SyntaxConf.Symbols.LineSeparator && streamEntry.NextCharacter != streamEntry.SyntaxConf.Symbols.EndOfStream)
            {
                return new ValidationFeedbackItem(entry, new ValidationFeedbackMultipleEntriesOnLine());
            }
            else
            {
                return null;
            }
        }
    }
}
