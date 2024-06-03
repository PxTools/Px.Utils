namespace Px.Utils.Validation
{
    /// <summary>
    /// Represents a validator for a Px file.
    /// </summary>
    public interface IPxFileValidator
    {
        /// <summary>
        /// Blocking method that validates some aspect of a Px file.
        /// </summary>
        /// <returns><see cref="ValidationResult"/> object that contains an array of feedback items gathered during the validation process.</returns>
        public ValidationResult Validate();
    }

    /// <summary>
    /// Represents a validator for a Px file with asynchronous validation capabilities.
    /// </summary>
    public interface IPxFileValidatorAsync
    {
        /// <summary>
        /// Asynchronous method that validates some aspect of a Px file.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for cancelling the validation process</param>
        /// <returns><see cref="ValidationResult"/> object that contains an array of feedback items gathered during the validation process.</returns>
        public Task<ValidationResult> ValidateAsync(CancellationToken cancellationToken = default);
    }
}
