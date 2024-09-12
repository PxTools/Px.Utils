using System.Text;

namespace Px.Utils.Validation
{
    /// <summary>
    /// Represents a validator for a Px file.
    /// </summary>
    public interface IPxFileStreamValidator
    {
        /// <summary>
        /// Blocking method that validates some aspect of a Px file.
        /// </summary>
        /// <param name="stream">Stream of the PX file to be validated</param>
        /// <param name="encoding">Encoding of the PX file</param>
        /// <param name="filename">Name of the PX file</param>
        /// <param name="leaveStreamOpen">Boolean value that determines whether the stream should be left open after validation.
        /// <returns><see cref="ValidationResult"/> object that contains an array of feedback items gathered during the validation process.</returns>
        public ValidationResult Validate(Stream stream, Encoding encoding, string filename, bool leaveStreamOpen = false);
    }

    /// <summary>
    /// Represents a validator for a Px file with asynchronous validation capabilities.
    /// </summary>
    public interface IPxFileStreamValidatorAsync
    {
        /// <summary>
        /// Asynchronous method that validates some aspect of a Px file.
        /// </summary>
        /// <param name="stream">Stream of the PX file to be validated</param>
        /// <param name="encoding">Encoding of the PX file</param>
        /// <param name="filename">Name of the PX file</param>
        /// <param name="leaveStreamOpen">Boolean value that determines whether the stream should be left open after validation.
        /// <param name="cancellationToken">Cancellation token for cancelling the validation process</param>
        /// <returns><see cref="ValidationResult"/> object that contains an array of feedback items gathered during the validation process.</returns>
        public Task<ValidationResult> ValidateAsync(Stream stream, Encoding encoding, string filename, bool leaveStreamOpen = false, CancellationToken cancellationToken = default);
    }
}
