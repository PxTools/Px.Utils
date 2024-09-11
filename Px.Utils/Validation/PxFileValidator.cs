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
    /// <param name="stream">Px file stream to be validated</param>
    /// <param name="filename">Name of the file subject to validation</param>
    /// <param name="encoding">Encoding format of the px file</param>
    /// <param name="syntaxConf"><see cref="PxFileSyntaxConf"/> object that contains symbols and tokens required for the px file syntax.</param>
    public class PxFileValidator(
        Stream stream,
        string filename,
        Encoding? encoding,
        PxFileSyntaxConf? syntaxConf = null
        ) : IPxFileValidator, IPxFileValidatorAsync
    {
        private CustomSyntaxValidationFunctions? _customSyntaxValidationFunctions;
        private CustomContentValidationFunctions? _customContentValidationFunctions;
        private IPxFileValidator[]? _customValidators;
        private IPxFileValidatorAsync[]? _customAsyncValidators;

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
        /// <param name="customValidators">Array of objects that implement <see cref="IPxFileValidator"/> interface, used for blocking validation.</param>
        /// <param name="customAsyncValidators">Array of objects that implement <see cref="IPxFileValidatorAsync"/> interface, used for asynchronous validation.</param>
        public void SetCustomValidators(IPxFileValidator[]? customValidators = null, IPxFileValidatorAsync[]? customAsyncValidators = null)
        {
            _customValidators = customValidators;
            _customAsyncValidators = customAsyncValidators;
        }

        /// <summary>
        /// Validates the Px file. Starts with metadata syntax validation, then metadata content validation, and finally data validation.
        /// If any custom validation functions are set, they are executed after the default validation steps.
        /// </summary>
        /// <returns><see cref="ValidationResult"/> object that contains the feedback gathered during the validation process.</returns>
        public ValidationResult Validate()
        {
            encoding ??= Encoding.Default;
            syntaxConf ??= PxFileSyntaxConf.Default;

            ValidationFeedback feedbacks = [];
            SyntaxValidator syntaxValidator = new(stream, encoding, filename, syntaxConf, _customSyntaxValidationFunctions, true);
            SyntaxValidationResult syntaxValidationResult = syntaxValidator.Validate();
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
                stream,
                contentValidationResult.DataRowLength,
                contentValidationResult.DataRowAmount,
                filename,
                syntaxValidationResult.DataStartRow, 
                encoding,
                syntaxConf);
            ValidationResult dataValidationResult = dataValidator.Validate();
            feedbacks.AddRange(dataValidationResult.FeedbackItems);

            if (_customValidators is not null)
            {
                foreach (IPxFileValidator customValidator in _customValidators)
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
        /// </summary>
        /// <returns><see cref="ValidationResult"/> object that contains the feedback gathered during the validation process.</returns>
        public async Task<ValidationResult> ValidateAsync(CancellationToken cancellationToken = default)
        {
            encoding ??= Encoding.Default;
            syntaxConf ??= PxFileSyntaxConf.Default;

            ValidationFeedback feedbacks = [];
            SyntaxValidator syntaxValidator = new(stream, encoding, filename, syntaxConf, _customSyntaxValidationFunctions, true);
            SyntaxValidationResult syntaxValidationResult = await syntaxValidator.ValidateAsync(cancellationToken);
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
                stream,
                contentValidationResult.DataRowLength,
                contentValidationResult.DataRowAmount,
                filename,
                syntaxValidationResult.DataStartRow, 
                encoding,
                syntaxConf);

            ValidationResult dataValidationResult = await dataValidator.ValidateAsync(cancellationToken);
            feedbacks.AddRange(dataValidationResult.FeedbackItems);

            if (_customAsyncValidators is not null)
            {
                foreach (IPxFileValidatorAsync customValidator in _customAsyncValidators)
                {
                    ValidationResult customValidationResult = await customValidator.ValidateAsync(cancellationToken);
                    feedbacks.AddRange(customValidationResult.FeedbackItems);
                }
            }

            return new ValidationResult(feedbacks);
        }
    }
}
