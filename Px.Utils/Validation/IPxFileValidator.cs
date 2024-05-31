namespace Px.Utils.Validation
{
    /// <summary>
    /// Represents a validator for a Px file.
    /// </summary>
    public interface IPxFileValidator
    {
        public IValidationResult Validate();

        public Task<IValidationResult> ValidateAsync(CancellationToken cancellationToken = default);
    }
}
