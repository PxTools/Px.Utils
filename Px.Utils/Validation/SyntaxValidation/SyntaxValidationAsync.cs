using PxUtils.Exceptions;
using PxUtils.PxFile;
using PxUtils.PxFile.Meta;
using System.Text;
using static PxUtils.Validation.SyntaxValidation.SyntaxValidation;

namespace PxUtils.Validation.SyntaxValidation
{
    public static class SyntaxValidationAsync
    {
        private const int DEFAULT_BUFFER_SIZE = 4096;

        /// <summary>
        /// Asynchronously validates the syntax of a PX file's metadata.
        /// </summary>
        /// <param name="stream">The stream of the PX file to be validated.</param>
        /// <param name="filename">The name of the file to be validated.</param>
        /// <param name="syntaxConf">An optional <see cref="PxFileSyntaxConf"/> parameter that specifies the syntax configuration for the PX file. If not provided, the default syntax configuration is used.</param>
        /// <param name="bufferSize">An optional parameter that specifies the buffer size for reading the file. If not provided, a default buffer size of 4096 is used.</param>
        /// <param name="customValidationFunctions">An optional <see cref="CustomValidationFunctions"/> parameter that specifies custom validation functions to be used during validation. If not provided, the default validation functions are used.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> parameter that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The task result contains a <see cref="SyntaxValidationResult"/> object which includes a <see cref="ValidationReport"/> and a list of <see cref="StructuredValidationEntry"/> objects. The ValidationReport contains feedback items that provide information about any syntax errors or warnings found during validation. The list of StructuredValidationEntry objects represents the structured entries in the PX file that were validated.</returns>
        public static async Task<SyntaxValidationResult> ValidatePxFileMetadataSyntaxAsync(
            Stream stream,
            string filename,
            PxFileSyntaxConf? syntaxConf = null,
            int bufferSize = DEFAULT_BUFFER_SIZE,
            CustomValidationFunctions? customValidationFunctions = null,
            CancellationToken cancellationToken = default)
        {
            SyntaxValidationFunctions validationFunctions = new();
            IEnumerable<ValidationFunctionDelegate> stringValidationFunctions = validationFunctions.DefaultStringValidationFunctions;
            IEnumerable<ValidationFunctionDelegate> keyValueValidationFunctions = validationFunctions.DefaultKeyValueValidationFunctions;
            IEnumerable<ValidationFunctionDelegate> structuredValidationFunctions = validationFunctions.DefaultStructuredValidationFunctions;

            if (customValidationFunctions is not null)
            {
                stringValidationFunctions = stringValidationFunctions.Concat(customValidationFunctions.CustomStringValidationFunctions);
                keyValueValidationFunctions = keyValueValidationFunctions.Concat(customValidationFunctions.CustomKeyValueValidationFunctions);
                structuredValidationFunctions = structuredValidationFunctions.Concat(customValidationFunctions.CustomStructuredValidationFunctions);
            }

            syntaxConf ??= PxFileSyntaxConf.Default;
            ValidationReport report = new();

            try
            {
                Encoding encoding = await PxFileMetadataReader.GetEncodingAsync(stream, syntaxConf, cancellationToken);

                List<StringValidationEntry> stringEntries = await BuildStringEntriesAsync(stream, encoding, syntaxConf, filename, bufferSize);
                ValidateEntries(stringEntries, stringValidationFunctions, report, syntaxConf);
                List<KeyValuePairValidationEntry> keyValuePairs = SyntaxValidation.BuildKeyValuePairs(stringEntries, syntaxConf);
                ValidateEntries(keyValuePairs, keyValueValidationFunctions, report, syntaxConf);
                List<StructuredValidationEntry> structuredEntries = SyntaxValidation.BuildStructuredEntries(keyValuePairs, syntaxConf);
                ValidateEntries(structuredEntries, structuredValidationFunctions, report, syntaxConf);

                return new(report, structuredEntries);
            }
            catch (InvalidPxFileMetadataException)
            {
                report.FeedbackItems.Add(new ValidationFeedbackItem(new StringValidationEntry(0, 0, filename, string.Empty, 0), new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.NoEncoding)));
                return new(report, []);
            }
        }

        private static async Task<List<StringValidationEntry>> BuildStringEntriesAsync(Stream stream, Encoding encoding, PxFileSyntaxConf syntaxConf, string filename, int bufferSize)
        {
            LineCharacterState state = new() { Line = 0, Character = 0 };
            stream.Seek(0, SeekOrigin.Begin);
            using StreamReader reader = new(stream, encoding);
            List<StringValidationEntry> stringEntries = [];
            StringBuilder entryBuilder = new();
            char[] buffer = new char[bufferSize];

            while (await reader.ReadAsync(buffer, 0, bufferSize) > 0)
            {
                ProcessBuffer(buffer, syntaxConf, state, filename, stringEntries, entryBuilder);
                Array.Clear(buffer, 0, buffer.Length);
            }
            return stringEntries;
        }
    }
}
    