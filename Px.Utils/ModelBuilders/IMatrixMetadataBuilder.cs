using Px.Utils.Models.Metadata;

namespace Px.Utils.ModelBuilders
{
    /// <summary>
    /// Interface for building <see cref="MatrixMetadata"/> objects from various input types.
    /// </summary>
    public interface IMatrixMetadataBuilder
    {
        /// <summary>
        /// Builds a <see cref="MatrixMetadata"/> object from a set of metadata entries.
        /// </summary>
        /// <param name="metadataInput">Entries in px-file format. Must contain all the entries required by the metadata specification.</param>
        /// <returns>A complete <see cref="MatrixMetadata"/> object containing all the information from the entries.</returns>
        MatrixMetadata Build(IEnumerable<KeyValuePair<string, string>> metadataInput);

        /// <summary>
        /// Builds a <see cref="MatrixMetadata"/> object from a set of metadata entries.
        /// </summary>
        /// <param name="metadataInput">Entries in px-file format. Must contain all the entries required by the metadata specification.</param>
        /// <returns>A complete <see cref="MatrixMetadata"/> object containing all the information from the entries.</returns>
        MatrixMetadata Build(IReadOnlyDictionary<string, string> metadataInput);
        
        /// <summary>
        /// Builds a <see cref="MatrixMetadata"/> object from an asynchronous input of metadata entries.
        /// </summary>
        /// <param name="metadataInput">Entries in px-file format. Must contain all the entries required by the metadata specification.</param>
        /// <returns>A complete <see cref="MatrixMetadata"/> object containing all the information from the entries.</returns>
        Task<MatrixMetadata> BuildAsync(IAsyncEnumerable<KeyValuePair<string, string>> metadataInput);
    }
}