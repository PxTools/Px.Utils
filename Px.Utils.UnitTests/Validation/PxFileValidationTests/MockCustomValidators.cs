using Px.Utils.Validation;

namespace Px.Utils.UnitTests.Validation.PxFileValidationTests
{
    internal class MockCustomValidators : IPxFileValidator, IPxFileValidatorAsync
    {
        public ValidationResult Validate()
        {
            return new ValidationResult([]);
        }

        public async Task<ValidationResult> ValidateAsync(CancellationToken cancellationToken = default)
        {
            // Simulate async operation
            await Task.Delay(1, cancellationToken);

            return new ValidationResult([]);
        }
    }
}
