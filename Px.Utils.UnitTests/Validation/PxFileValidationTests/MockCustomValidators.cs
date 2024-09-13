using Px.Utils.Validation;
using Px.Utils.Validation.DatabaseValidation;
using System.Text;

namespace Px.Utils.UnitTests.Validation.PxFileValidationTests
{
    internal sealed class MockCustomValidator : IValidator
    {
        public ValidationResult Validate()
        {
            return new ValidationResult([]);
        }
    }

    internal sealed class MockCustomAsyncValidator : IValidatorAsync
    {
        public async Task<ValidationResult> ValidateAsync(CancellationToken cancellationToken = default)
        {
            // Simulate async operation
            await Task.Delay(1, cancellationToken);

            return new ValidationResult([]);
        }
    }

    internal sealed class MockCustomStreamValidator : IPxFileStreamValidator
    {
        public ValidationResult Validate(Stream stream, string filename, Encoding? encoding = null, IFileSystem? fileSystem = null)
        {
            return new ValidationResult([]);
        }
    }

    internal sealed class MockCustomStreamAsyncValidator : IPxFileStreamValidatorAsync
    {
        public async Task<ValidationResult> ValidateAsync(
            Stream stream, 
            string filename, 
            Encoding? encoding = null,
            IFileSystem? fileSystem = null,
            CancellationToken cancellationToken = default)
        {
            // Simulate async operation
            await Task.Delay(1, cancellationToken);

            return new ValidationResult([]);
        }
    }
}
