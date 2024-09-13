using Px.Utils.Validation.DatabaseValidation;
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
        /// <param name="filename">Name of the PX file</param>
        /// <param name="encoding">Encoding of the PX file. If not provided, validator tries to find the encoding.</param>
        /// <param name="fileSystem">File system to use for file operations. If not provided, default file system is used.</param>
        /// <returns><see cref="ValidationResult"/> object that contains an array of feedback items gathered during the validation process.</returns>
        public ValidationResult Validate(Stream stream, string filename, Encoding? encoding = null, IFileSystem? fileSystem = null);
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
        /// <param name="filename">Name of the PX file</param>
        /// <param name="encoding">Encoding of the PX file. If not provided, validator tries to find the encoding.</param>
        /// <param name="fileSystem">File system to use for file operations. If not provided, default file system is used.</param>
        /// <param name="cancellationToken">Cancellation token for cancelling the validation process</param>
        /// <returns><see cref="ValidationResult"/> object that contains an array of feedback items gathered during the validation process.</returns>
        public Task<ValidationResult> ValidateAsync(Stream stream, string filename, Encoding? encoding = null, IFileSystem? fileSystem = null, CancellationToken cancellationToken = default);
    }
}
