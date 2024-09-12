using Px.Utils.Validation;
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
        public ValidationResult Validate(Stream stream, Encoding encoding, string filename, bool leaveStreamOpen = false)
        {
            return new ValidationResult([]);
        }
    }

    internal sealed class MockCustomStreamAsyncValidator : IPxFileStreamValidatorAsync
    {
        public async Task<ValidationResult> ValidateAsync(Stream stream, Encoding encoding, string filename, bool leaveStreamOpen = false, CancellationToken cancellationToken = default)
        {
            // Simulate async operation
            await Task.Delay(1, cancellationToken);

            return new ValidationResult([]);
        }
    }
}
