using PxUtils.PxFile;
using PxUtils.Validation.SyntaxValidation;
using System.Text;
using System.Threading.Tasks;

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

        // TODO: Add summary
        public static async Task<ValidationFeedbackItem[]> ValidatePxFileContentAsync(
            string filename,
            ValidationStructuredEntry[] entries,
            PxFileSyntaxConf syntaxConf,
            List<ValidationFeedbackItem> feedbackItems,
            Encoding encoding,
            CustomContentValidationFunctions? customContentValidationFunctions = null,
            CancellationToken cancellationToken = default
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

            var searchTasks = contentValidationSearchFunctions.Select(searchFunction => Task.Run(() => searchFunction(entries, syntaxConf, ref contentValidationInfo)));

            await Task.WhenAll(searchTasks);

            searchTasks.Select(task => task.Result).ToList().ForEach(feedback => 
                feedbackItems.AddRange(feedback is not null ? feedback : []));

            var entryTasks = contentValidationEntryFunctions.SelectMany(entryFunction => entries.Select(entry => Task.Run(() => entryFunction(entry, syntaxConf, ref contentValidationInfo))));

            await Task.WhenAll(entryTasks);

            entryTasks.Select(task => task.Result).ToList().ForEach(feedback =>
                feedbackItems.AddRange(feedback is not null ? feedback : []));

            return [.. feedbackItems];
        }
    }
}
