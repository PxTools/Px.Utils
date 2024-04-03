﻿using PxUtils.PxFile;
using PxUtils.Validation.SyntaxValidation;
using System.Text;

namespace PxUtils.Validation.ContentValidation
{
    // TODO: Add summary
    public static class ContentValidation
    {
        // TODO: Add summary
        public static void ValidatePxFileContent(
            string filename,
            ValidationStructuredEntry[] entries,
            PxFileSyntaxConf syntaxConf,
            ref List<ValidationFeedbackItem> feedbackItems,
            Encoding encoding,
            CustomContentValidationFunctions? customContentValidationFunctions = null
            )
        {
            ContentValidationInfo contentValidationInfo = new(filename, encoding);
            ContentValidationFunctions contentValidationFunctions = new();

            IEnumerable<ContentValidationEntryFunctionDelegate> contentValidationEntryFunctions = contentValidationFunctions.DefaultContentValidationEntryFunctions;
            IEnumerable<ContentValidationSearchFunctionDelegate> contentValidationSearchFunctions = contentValidationFunctions.DefaultContentValidationSearchFunctions;

            if (customContentValidationFunctions is not null)
            {
                contentValidationEntryFunctions = contentValidationEntryFunctions.Concat(customContentValidationFunctions.CustomContentValidationEntryFunctions);
                contentValidationSearchFunctions = contentValidationSearchFunctions.Concat(customContentValidationFunctions.CustomContentValidationSearchFunctions);
            }

            foreach(ContentValidationSearchFunctionDelegate searchFunction in contentValidationSearchFunctions)
            {
                ValidationFeedbackItem[]? feedback = searchFunction(entries, syntaxConf, ref contentValidationInfo);
                if (feedback is not null)
                {
                    feedbackItems.AddRange(feedback);
                }
            }
            foreach(ContentValidationEntryFunctionDelegate entryFunction in contentValidationEntryFunctions)
            {
                foreach(ValidationStructuredEntry entry in entries)
                {
                    ValidationFeedbackItem[]? feedback = entryFunction(entry, syntaxConf, ref contentValidationInfo);
                    if (feedback is not null)
                    {
                        feedbackItems.AddRange(feedback);
                    }
                }
            }
        }

        // TODO: Async version
    }
}