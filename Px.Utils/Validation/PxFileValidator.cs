using Px.Utils.Exceptions;
using Px.Utils.PxFile;
using Px.Utils.Validation.ContentValidation;
using Px.Utils.Validation.DataValidation;
using Px.Utils.Validation.SyntaxValidation;
using System.Text;

namespace Px.Utils.Validation
{
    /// <summary>
    /// Validates a Px file as a whole.
    /// </summary>
    /// <param name="syntaxConf"><see cref="PxFileSyntaxConf"/> object that contains symbols and tokens required for the px file syntax.</param>
    public class PxFileValidator(
        PxFileSyntaxConf? syntaxConf = null
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
        /// <param name="leaveStreamOpen">Boolean value that determines whether the stream should be left open after validation.
        /// </summary>
        /// <returns><see cref="ValidationResult"/> object that contains the feedback gathered during the validation process.</returns>
        public ValidationResult Validate(
            Stream stream,
            string filename,
            Encoding? encoding = null,
            bool leaveStreamOpen = false)
        {
            syntaxConf ??= PxFileSyntaxConf.Default;

            ValidationFeedback feedbacks = [];
            SyntaxValidator syntaxValidator = new(syntaxConf, _customSyntaxValidationFunctions);
            SyntaxValidationResult syntaxValidationResult = syntaxValidator.Validate(stream, filename, encoding, true);
            feedbacks.AddRange(syntaxValidationResult.FeedbackItems);

            ContentValidator contentValidator = new(filename, encoding, [.. syntaxValidationResult.Result], _customContentValidationFunctions, syntaxConf);
            ContentValidationResult contentValidationResult = contentValidator.Validate();
            feedbacks.AddRange(contentValidationResult.FeedbackItems);

            if (syntaxValidationResult.DataStartStreamPosition == -1)
            {
                feedbacks.Add(new(
                    new(ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.StartOfDataSectionNotFound),
                    new(filename, 0, 0)
                    ));

                return new (feedbacks);
            }

            stream.Position = syntaxValidationResult.DataStartStreamPosition;
            DataValidator dataValidator = new(
                contentValidationResult.DataRowLength,
                contentValidationResult.DataRowAmount,
                syntaxValidationResult.DataStartRow, 
                syntaxConf);
            ValidationResult dataValidationResult = dataValidator.Validate(stream, filename, encoding, leaveStreamOpen);
            feedbacks.AddRange(dataValidationResult.FeedbackItems);

            if (_customStreamValidators is not null)
            {
                foreach (IPxFileStreamValidator customValidator in _customStreamValidators)
                {
                    ValidationResult customValidationResult = customValidator.Validate(stream, filename, encoding, leaveStreamOpen);
                    feedbacks.AddRange(customValidationResult.FeedbackItems);
                }
            }
            if (_customValidators is not null)
            {
                foreach (IValidator customValidator in _customValidators)
                {
                    ValidationResult customValidationResult = customValidator.Validate();
                    feedbacks.AddRange(customValidationResult.FeedbackItems);
                }
            }

            return new ValidationResult(feedbacks);
        }

        /// <summary>
        /// Validates the Px file asynchronously. Starts with metadata syntax validation, then metadata content validation, and finally data validation.
        /// If any custom validation functions are set, they are executed after the default validation steps.
        /// <param name="stream">Px file stream to be validated</param>
        /// <param name="filename">Name of the file subject to validation</param>
        /// <param name="encoding">Encoding format of the px file</param>
        /// <param name="leaveStreamOpen">Boolean value that determines whether the stream should be left open after validation.
        /// <param name="cancellationToken">Cancellation token for cancelling the validation process</param>
        /// </summary>
        /// <returns><see cref="ValidationResult"/> object that contains the feedback gathered during the validation process.</returns>
        public async Task<ValidationResult> ValidateAsync(
            Stream stream,
            string filename,
            Encoding? encoding = null,
            bool leaveStreamOpen = false,
            CancellationToken cancellationToken = default)
        {
            syntaxConf ??= PxFileSyntaxConf.Default;

            ValidationFeedback feedbacks = [];
            SyntaxValidator syntaxValidator = new(syntaxConf, _customSyntaxValidationFunctions);
            SyntaxValidationResult syntaxValidationResult = await syntaxValidator.ValidateAsync(stream, filename, encoding, true, cancellationToken);
            feedbacks.AddRange(syntaxValidationResult.FeedbackItems);

            ContentValidator contentValidator = new(filename, encoding, [..syntaxValidationResult.Result], _customContentValidationFunctions, syntaxConf);
            ContentValidationResult contentValidationResult = contentValidator.Validate();
            feedbacks.AddRange(contentValidationResult.FeedbackItems);

            if (syntaxValidationResult.DataStartStreamPosition == -1)
            {
                feedbacks.Add(new(
                    new(ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.StartOfDataSectionNotFound),
                    new(filename, 0, 0)
                ));

                return new (feedbacks);
            }

            stream.Position = syntaxValidationResult.DataStartStreamPosition;
            DataValidator dataValidator = new(
                contentValidationResult.DataRowLength,
                contentValidationResult.DataRowAmount,
                syntaxValidationResult.DataStartRow, 
                syntaxConf);

            ValidationResult dataValidationResult = await dataValidator.ValidateAsync(stream, filename, encoding, leaveStreamOpen, cancellationToken);
            feedbacks.AddRange(dataValidationResult.FeedbackItems);

            if (_customStreamAsyncValidators is not null)
            {
                foreach (IPxFileStreamValidatorAsync customValidator in _customStreamAsyncValidators)
                {
                    ValidationResult customValidationResult = await customValidator.ValidateAsync(stream, filename, encoding, leaveStreamOpen, cancellationToken);
                    feedbacks.AddRange(customValidationResult.FeedbackItems);
                }
            }
            if (_customAsyncValidators is not null)
            {
                foreach (IValidatorAsync customValidator in _customAsyncValidators)
                {
                    ValidationResult customValidationResult = await customValidator.ValidateAsync(cancellationToken);
                    feedbacks.AddRange(customValidationResult.FeedbackItems);
                }
            }

            return new ValidationResult(feedbacks);
        }
    }
}
