using Px.Utils.PxFile;
using Px.Utils.Validation.ContentValidation;
using Px.Utils.Validation.DataValidation;
using Px.Utils.Validation.SyntaxValidation;
using System.Text;

namespace Px.Utils.Validation
{
    public class PxFileValidator(
        Stream stream,
        string filename,
        Encoding? encoding,
        PxFileSyntaxConf? syntaxConf = null,
        CustomSyntaxValidationFunctions? customSyntaxValidationFunctions = null,
        CustomContentValidationFunctions? customContentValidationFunctions = null,
        IPxFileValidator[]? customValidators = null,
        IPxFileValidatorAsync[]? customAsyncValidators = null
        ) : IPxFileValidator, IPxFileValidatorAsync
    {
        public ValidationResult Validate()
        {
            encoding ??= Encoding.Default;
            syntaxConf ??= PxFileSyntaxConf.Default;

            List<ValidationFeedbackItem> feedbacks = [];
            SyntaxValidator syntaxValidator = new(stream, encoding, filename, syntaxConf, customSyntaxValidationFunctions, true);
            SyntaxValidationResult syntaxValidationResult = syntaxValidator.Validate();
            feedbacks.AddRange(syntaxValidationResult.FeedbackItems);

            ContentValidator contentValidator = new(filename, encoding, [..syntaxValidationResult.Result], customContentValidationFunctions, syntaxConf);
            ContentValidationResult contentValidationResult = contentValidator.Validate();
            feedbacks.AddRange(contentValidationResult.FeedbackItems);

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

            if (customValidators is not null)
            {
                foreach (IPxFileValidator customValidator in customValidators)
                {
                    ValidationResult customValidationResult = customValidator.Validate();
                    feedbacks.AddRange(customValidationResult.FeedbackItems);
                }
            }

            return new ValidationResult([..feedbacks]);
        }

        public async Task<ValidationResult> ValidateAsync(CancellationToken cancellationToken = default)
        {
            encoding ??= Encoding.Default;
            syntaxConf ??= PxFileSyntaxConf.Default;

            List<ValidationFeedbackItem> feedbacks = [];
            SyntaxValidator syntaxValidator = new(stream, encoding, filename, syntaxConf, customSyntaxValidationFunctions, true);
            SyntaxValidationResult syntaxValidationResult = await syntaxValidator.ValidateAsync(cancellationToken);
            feedbacks.AddRange(syntaxValidationResult.FeedbackItems);

            ContentValidator contentValidator = new(filename, encoding, [..syntaxValidationResult.Result], customContentValidationFunctions, syntaxConf);
            ContentValidationResult contentValidationResult = contentValidator.Validate();
            feedbacks.AddRange(contentValidationResult.FeedbackItems);

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

            if (customAsyncValidators is not null)
            {
                foreach (IPxFileValidatorAsync customValidator in customAsyncValidators)
                {
                    ValidationResult customValidationResult = await customValidator.ValidateAsync(cancellationToken);
                    feedbacks.AddRange(customValidationResult.FeedbackItems);
                }
            }

            return new ValidationResult([..feedbacks]);
        }
    }
}
