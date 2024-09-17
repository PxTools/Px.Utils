namespace Px.Utils.Validation
{
    /// <summary>
    /// Represents a validator for a Px file or database
    /// </summary>
    public interface IValidator
    {
        /// <summary>
        /// Blocking method that validates some aspect of a Px file or a database
        /// </summary>
        /// <returns><see cref="ValidationResult"/> object that contains an array of feedback items gathered during the validation process.</returns>
        public ValidationResult Validate();
    }

    /// <summary>
    /// Represents a validator for a Px file or database with asynchronous validation capabilities.
    /// </summary>
    public interface IValidatorAsync
    {
        /// <summary>
        /// Asynchronous method that validates some aspect of a Px file.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for cancelling the validation process</param>
        /// <returns><see cref="ValidationResult"/> object that contains an array of feedback items gathered during the validation process.</returns>
        public Task<ValidationResult> ValidateAsync(CancellationToken cancellationToken = default);
    }
}