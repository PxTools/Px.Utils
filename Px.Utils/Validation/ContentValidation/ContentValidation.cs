using PxUtils.PxFile;
using PxUtils.Validation.SyntaxValidation;
using System.Text;

namespace PxUtils.Validation.ContentValidation
{
    /// <summary>
    /// Methods for validating the content of Px file metadata
    /// </summary>
    public static class ContentValidation
    {
        /// <summary>
        /// Blocking function for validating contents of a Px file metadata
        /// </summary>
        /// <param name="filename">Name of the file being validated</param>
        /// <param name="entries">Array of <see cref="ValidationStructuredEntry"/> objects that represent entries of the Px file metadata</param>
        /// <param name="syntaxConf"><see cref="PxFileSyntaxConf"/> object that defines keywords and symbols for Px file syntax</param>
        /// <param name="feedbackItems">List of <see cref="ValidationFeedbackItem"/> objects. Any issues found during the validation process will be added to this list</param>
        /// <param name="encoding">Encoding format of the Px file</param>
        /// <param name="customContentValidationFunctions"><see cref="ContentValidationFunctions"/> object that contains any optional additional validation functions</param>
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

            foreach (ContentValidationSearchFunctionDelegate searchFunction in contentValidationSearchFunctions)
            {
                ValidationFeedbackItem[]? feedback = searchFunction(entries, syntaxConf, ref contentValidationInfo);
                if (feedback is not null)
                {
                    feedbackItems.AddRange(feedback);
                }
            }
            foreach (ContentValidationEntryFunctionDelegate entryFunction in contentValidationEntryFunctions)
            {
                foreach (ValidationStructuredEntry entry in entries)
                {
                    ValidationFeedbackItem[]? feedback = entryFunction(entry, syntaxConf, ref contentValidationInfo);
                    if (feedback is not null)
                    {
                        feedbackItems.AddRange(feedback);
                    }
                }
            }
        }

        /// <summary>
        /// Asynchronous function for validating contents of a Px file metadata
        /// </summary>
        /// <param name="filename">Name of the file being validated</param>
        /// <param name="entries">Array of <see cref="ValidationStructuredEntry"/> objects that represent entries of the Px file metadata</param>
        /// <param name="syntaxConf"><see cref="PxFileSyntaxConf"/> object that defines keywords and symbols for Px file syntax</param>
        /// <param name="feedbackItems">List of <see cref="ValidationFeedbackItem"/> objects. Any issues found during the validation process will be added to this list</param>
        /// <param name="encoding">Encoding format of the Px file</param>
        /// <param name="customContentValidationFunctions"><see cref="ContentValidationFunctions"/> object that contains any optional additional validation functions</param>
        /// <param name="cancellationToken">Cancellation token for cancelling the validation process</param>
        /// <returns>Array of <see cref="ValidationFeedbackItem"/> objects. Any issues found during validation will be listed here</returns>
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

            List<ContentValidationEntryFunctionDelegate> contentValidationEntryFunctions = contentValidationFunctions.DefaultContentValidationEntryFunctions;
            List<ContentValidationSearchFunctionDelegate> finalContentValidationSearchFunctions = contentValidationFunctions.FinalAsyncSearchFunctionGroup;

            if (customContentValidationFunctions is not null)
            {
                contentValidationEntryFunctions = [.. contentValidationEntryFunctions, .. customContentValidationFunctions.CustomContentValidationEntryFunctions];
                finalContentValidationSearchFunctions = [.. finalContentValidationSearchFunctions, .. customContentValidationFunctions.CustomContentValidationSearchFunctions];
            }

            // Some search type content validation functions are dependent on the results of other search type content validation functions. Thus they are run in groups.
            IEnumerable<Task<ValidationFeedbackItem[]?>> tasks = contentValidationFunctions.FirstAsyncSearchFunctionGroup
                .Select(func => Task.Run(() => func(entries, syntaxConf, ref contentValidationInfo)));
            await Task.WhenAll(tasks);
            tasks = contentValidationFunctions.SecondAsyncSearchFunctionGroup
                .Select(func => Task.Run(() => func(entries, syntaxConf, ref contentValidationInfo)));
            await Task.WhenAll(tasks);
            tasks = finalContentValidationSearchFunctions
                .Select(func => Task.Run(() => func(entries, syntaxConf, ref contentValidationInfo)));
            await Task.WhenAll(tasks);

            // Entry tasks can be run asynchronously without worrying about dependencies
            IEnumerable<Task<ValidationFeedbackItem[]?>> entryTasks = contentValidationEntryFunctions.SelectMany(entryFunction => entries.Select(entry => Task.Run(() => entryFunction(entry, syntaxConf, ref contentValidationInfo))));

            await Task.WhenAll(entryTasks);

            entryTasks.Select(task => task.Result).ToList().ForEach(feedback =>
                feedbackItems.AddRange(feedback is not null ? feedback : []));

            return [.. feedbackItems];
        }
    }
}
