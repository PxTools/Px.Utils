using Px.Utils.PxFile;
using Px.Utils.Validation.ContentValidation;
using Px.Utils.Validation.DatabaseValidation;
using Px.Utils.Validation.DataValidation;
using Px.Utils.Validation.SyntaxValidation;
using System.Text;

namespace Px.Utils.Validation
{
    /// <summary>
    /// Validates a Px file as a whole.
    /// </summary>
    /// <param name="conf"><see cref="PxFileConfiguration"/> object that contains symbols and tokens required for the px file syntax.</param>
    public class PxFileValidator(
        PxFileConfiguration? conf = null
        ) : IPxFileStreamValidator, IPxFileStreamValidatorAsync
    {
        private CustomSyntaxValidationFunctions? _customSyntaxValidationFunctions;
        private CustomContentValidationFunctions? _customContentValidationFunctions;
        private IPxFileStreamValidator[]? _customStreamValidators;
        private IPxFileStreamValidatorAsync[]? _customStreamAsyncValidators;
        private IValidator[]? _customValidators;
        private IValidatorAsync[]? _customAsyncValidators;

        /// <summary>
        /// Set custom validation functions to be used during validation.
        /// </summary>
        /// <param name="customSyntaxValidationFunctions"><see cref="CustomSyntaxValidationFunctions"/> object that contains functions for validating the px file metadata syntax.</param>
        /// <param name="customContentValidationFunctions"><see cref="CustomContentValidationFunctions"/> object that contains functions for validating the px file metadata contents.</param>
        public void SetCustomValidatorFunctions(CustomSyntaxValidationFunctions? customSyntaxValidationFunctions = null, CustomContentValidationFunctions? customContentValidationFunctions = null)
        {
            _customSyntaxValidationFunctions = customSyntaxValidationFunctions;
            _customContentValidationFunctions = customContentValidationFunctions;
        }

        /// <summary>
        /// Set custom validators to be used during validation.
        /// </summary>
        /// <param name="customStreamValidators">Array of objects that implement <see cref="IPxFileStreamValidator"/> interface, used for px file stream blocking validation.</param>
        /// <param name="customStreamAsyncValidators">Array of objects that implement <see cref="IPxFileStreamValidatorAsync"/> interface, used for px file stream asynchronous validation.</param>
        /// <param name="customValidators">Array of objects that implement <see cref="IValidator"/> interface, used for custom blocking validation.</param>
        /// <param name="customAsyncValidators">Array of objects that implement <see cref="IValidatorAsync"/> interface, used for custom asynchronous validation.</param>
        public void SetCustomValidators(
            IPxFileStreamValidator[]? customStreamValidators = null,
            IPxFileStreamValidatorAsync[]? customStreamAsyncValidators = null,
            IValidator[]? customValidators = null,
            IValidatorAsync[]? customAsyncValidators = null)
        {
            _customStreamValidators = customStreamValidators;
            _customStreamAsyncValidators = customStreamAsyncValidators;
            _customValidators = customValidators;
            _customAsyncValidators = customAsyncValidators;
        }

        /// <summary>
        /// Validates the Px file. Starts with metadata syntax validation, then metadata content validation, and finally data validation.
        /// If any custom validation functions are set, they are executed after the default validation steps.
        /// <param name="stream">Px file stream to be validated</param>
        /// <param name="filename">Name of the file subject to validation</param>
        /// <param name="encoding">Encoding format of the px file</param>
        /// <param name="fileSystem">File system to use for file operations. If not provided, default file system is used.</param>
        /// </summary>
        /// <returns><see cref="ValidationResult"/> object that contains the feedback gathered during the validation process.</returns>
        public ValidationResult Validate(
            Stream stream,
            string filename,
            Encoding? encoding = null,
            IFileSystem? fileSystem = null)
            => Validate(stream, filename, encoding, fileSystem, new ValidationFeedbackSink());

        /// <summary>
        /// Validates the PX file using the specified feedback retention options.
        /// </summary>
        /// <param name="stream">The PX file stream to validate.</param>
        /// <param name="filename">The name used in reported feedback.</param>
        /// <param name="encoding">The PX file encoding, or <see langword="null"/> to detect it.</param>
        /// <param name="fileSystem">The file system used for encoding detection, or <see langword="null"/> for the default.</param>
        /// <param name="options">Feedback retention options. A positive limit applies per filename, level, and rule; <see langword="null"/> limit retains all feedback.</param>
        /// <returns>The complete PX file validation result with retained feedback.</returns>
        public ValidationResult Validate(
            Stream stream,
            string filename,
            Encoding? encoding,
            IFileSystem? fileSystem,
            ValidationOptions options)
            => Validate(stream, filename, encoding, fileSystem, new ValidationFeedbackSink(options));

        internal ValidationResult Validate(
            Stream stream,
            string filename,
            Encoding? encoding,
            IFileSystem? fileSystem,
            ValidationFeedbackSink sink)
        {
            encoding ??= new LocalFileSystem().GetEncoding(stream);
            conf ??= PxFileConfiguration.Default;

            SyntaxValidator syntaxValidator = new(conf, _customSyntaxValidationFunctions);
            SyntaxValidationResult syntaxValidationResult = syntaxValidator.Validate(stream, filename, encoding, fileSystem, sink);

            ContentValidator contentValidator = new(filename, encoding, [.. syntaxValidationResult.Result], _customContentValidationFunctions, conf);
            ContentValidationResult contentValidationResult = contentValidator.Validate(sink);

            if (syntaxValidationResult.DataStartStreamPosition == -1)
            {
                sink.Report(new KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>(
                    new(ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.StartOfDataSectionNotFound),
                    new(filename, 0, 0)
                    ));

                return new(sink.ToFeedback());
            }

            stream.Position = syntaxValidationResult.DataStartStreamPosition;
            DataValidator dataValidator = new(
                contentValidationResult.DataRowLength,
                contentValidationResult.DataRowAmount,
                syntaxValidationResult.DataStartRow, 
                conf);
            dataValidator.Validate(stream, filename, encoding, fileSystem, sink);

            if (_customStreamValidators is not null)
            {
                foreach (IPxFileStreamValidator customValidator in _customStreamValidators)
                {
                    ValidationResult customValidationResult = customValidator.Validate(stream, filename, encoding, fileSystem);
                    sink.ReportRange(customValidationResult.FeedbackItems);
                }
            }
            if (_customValidators is not null)
            {
                foreach (IValidator customValidator in _customValidators)
                {
                    ValidationResult customValidationResult = customValidator.Validate();
                    sink.ReportRange(customValidationResult.FeedbackItems);
                }
            }
            stream.Close();
            return new ValidationResult(sink.ToFeedback());
        }

        /// <summary>
        /// Validates the Px file asynchronously. Starts with metadata syntax validation, then metadata content validation, and finally data validation.
        /// If any custom validation functions are set, they are executed after the default validation steps.
        /// <param name="stream">Px file stream to be validated</param>
        /// <param name="filename">Name of the file subject to validation</param>
        /// <param name="encoding">Encoding format of the px file</param>
        /// <param name="fileSystem">File system to use for file operations. If not provided, default file system is used.</param>
        /// <param name="cancellationToken">Cancellation token for cancelling the validation process</param>
        /// </summary>
        /// <returns><see cref="ValidationResult"/> object that contains the feedback gathered during the validation process.</returns>
        public async Task<ValidationResult> ValidateAsync(
            Stream stream,
            string filename,
            Encoding? encoding = null,
            IFileSystem? fileSystem = null,
            CancellationToken cancellationToken = default)
            => await ValidateAsync(stream, filename, encoding, fileSystem, new ValidationFeedbackSink(), cancellationToken);

        /// <summary>
        /// Asynchronously validates the PX file using the specified feedback retention options.
        /// </summary>
        /// <param name="stream">The PX file stream to validate.</param>
        /// <param name="filename">The name used in reported feedback.</param>
        /// <param name="encoding">The PX file encoding, or <see langword="null"/> to detect it.</param>
        /// <param name="fileSystem">The file system used for encoding detection, or <see langword="null"/> for the default.</param>
        /// <param name="options">Feedback retention options. A positive limit applies per filename, level, and rule; <see langword="null"/> limit retains all feedback.</param>
        /// <param name="cancellationToken">A token that cancels the operation.</param>
        /// <returns>A task that produces the complete PX file validation result with retained feedback.</returns>
        public async Task<ValidationResult> ValidateAsync(
            Stream stream,
            string filename,
            Encoding? encoding,
            IFileSystem? fileSystem,
            ValidationOptions options,
            CancellationToken cancellationToken = default)
            => await ValidateAsync(stream, filename, encoding, fileSystem, new ValidationFeedbackSink(options), cancellationToken);

        internal async Task<ValidationResult> ValidateAsync(
            Stream stream,
            string filename,
            Encoding? encoding,
            IFileSystem? fileSystem,
            ValidationFeedbackSink sink,
            CancellationToken cancellationToken = default)
        {
            encoding ??= await new LocalFileSystem().GetEncodingAsync(stream, cancellationToken);
            conf ??= PxFileConfiguration.Default;

            SyntaxValidator syntaxValidator = new(conf, _customSyntaxValidationFunctions);
            SyntaxValidationResult syntaxValidationResult = await syntaxValidator.ValidateAsync(stream, filename, encoding, fileSystem, sink, cancellationToken);

            ContentValidator contentValidator = new(filename, encoding, [..syntaxValidationResult.Result], _customContentValidationFunctions, conf);
            ContentValidationResult contentValidationResult = contentValidator.Validate(sink);

            if (syntaxValidationResult.DataStartStreamPosition == -1)
            {
                sink.Report(new KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>(
                    new(ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.StartOfDataSectionNotFound),
                    new(filename, 0, 0)
                ));

                return new(sink.ToFeedback());
            }

            stream.Position = syntaxValidationResult.DataStartStreamPosition;
            DataValidator dataValidator = new(
                contentValidationResult.DataRowLength,
                contentValidationResult.DataRowAmount,
                syntaxValidationResult.DataStartRow, 
                conf);

            await dataValidator.ValidateAsync(stream, filename, encoding, fileSystem, sink, cancellationToken);

            if (_customStreamAsyncValidators is not null)
            {
                foreach (IPxFileStreamValidatorAsync customValidator in _customStreamAsyncValidators)
                {
                    ValidationResult customValidationResult = await customValidator.ValidateAsync(stream, filename, encoding, fileSystem, cancellationToken);
                    sink.ReportRange(customValidationResult.FeedbackItems);
                }
            }
            if (_customAsyncValidators is not null)
            {
                foreach (IValidatorAsync customValidator in _customAsyncValidators)
                {
                    ValidationResult customValidationResult = await customValidator.ValidateAsync(cancellationToken);
                    sink.ReportRange(customValidationResult.FeedbackItems);
                }
            }
            stream.Close();
            return new ValidationResult(sink.ToFeedback());
        }
    }
}
