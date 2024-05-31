﻿using Px.Utils.PxFile;
using Px.Utils.Validation.ContentValidation;
using Px.Utils.Validation.DataValidation;
using Px.Utils.Validation.SyntaxValidation;
using System.ComponentModel.DataAnnotations;
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
        IPxFileValidator[]? customValidators = null
        ) : IPxFileValidator
    {
        public IValidationResult Validate()
        {
            encoding ??= Encoding.Default;
            syntaxConf ??= PxFileSyntaxConf.Default;

            List<ValidationFeedbackItem> feedbacks = [];
            SyntaxValidator syntaxValidator = new(stream, encoding, filename, syntaxConf, customSyntaxValidationFunctions, true);
            SyntaxValidationResult syntaxValidationResult = (SyntaxValidationResult)syntaxValidator.Validate();
            feedbacks.AddRange(syntaxValidationResult.FeedbackItems);

            ContentValidator contentValidator = new(filename, encoding, [..syntaxValidationResult.Result], customContentValidationFunctions, syntaxConf);
            ContentValidationResult contentValidationResult = (ContentValidationResult)contentValidator.Validate();
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
            DataValidationResult dataValidationResult = (DataValidationResult)dataValidator.Validate();
            feedbacks.AddRange(dataValidationResult.FeedbackItems);

            if (customValidators is not null)
            {
                foreach (IPxFileValidator customValidator in customValidators)
                {
                    IValidationResult customValidationResult = customValidator.Validate();
                    feedbacks.AddRange(customValidationResult.FeedbackItems);
                }
            }

            return new PxFileValidationResult([..feedbacks]);
        }

        public async Task<IValidationResult> ValidateAsync(CancellationToken cancellationToken = default)
        {
            encoding ??= Encoding.Default;
            syntaxConf ??= PxFileSyntaxConf.Default;

            List<ValidationFeedbackItem> feedbacks = [];
            SyntaxValidator syntaxValidator = new(stream, encoding, filename, syntaxConf, customSyntaxValidationFunctions, true);
            SyntaxValidationResult syntaxValidationResult = (SyntaxValidationResult)await syntaxValidator.ValidateAsync(cancellationToken);
            feedbacks.AddRange(syntaxValidationResult.FeedbackItems);

            ContentValidator contentValidator = new(filename, encoding, [..syntaxValidationResult.Result], customContentValidationFunctions, syntaxConf);
            ContentValidationResult contentValidationResult = (ContentValidationResult)await contentValidator.ValidateAsync(cancellationToken);
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

            DataValidationResult dataValidationResult = (DataValidationResult)await dataValidator.ValidateAsync(cancellationToken);
            feedbacks.AddRange(dataValidationResult.FeedbackItems);

            if (customValidators is not null)
            {
                foreach (IPxFileValidator customValidator in customValidators)
                {
                    IValidationResult customValidationResult = await customValidator.ValidateAsync(cancellationToken);
                    feedbacks.AddRange(customValidationResult.FeedbackItems);
                }
            }

            return new PxFileValidationResult([..feedbacks]);
        }
    }
}
