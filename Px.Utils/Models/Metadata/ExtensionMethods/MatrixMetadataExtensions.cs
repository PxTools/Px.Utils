using Px.Utils.Models.Metadata.Dimensions;

namespace Px.Utils.Models.Metadata.ExtensionMethods

    /// <summary>
    /// Extension methods for <see cref="IReadOnlyMatrixMetadata"/>.
    /// </summary>
    public static class MatrixMetadataExtensions
    {
        /// <summary>
        /// Gets the content dimension from the metadata.
        /// </summary>
        /// <returns><see cref="ContentDimension"/> object containing information about the content dimension.</returns>
        /// <exception>Throws <see cref="InvalidOperationException"/> if the content dimension is not found in the metadata.</exception>
        public static ContentDimension GetContentDimension(this IReadOnlyMatrixMetadata metadata)
        {
            ContentDimension? contentDimension = metadata.Dimensions.FirstOrDefault(dimension => dimension.Type == Enums.DimensionType.Content) as ContentDimension;
            return contentDimension ?? throw new InvalidOperationException("Content dimension not found in metadata");
        }

        /// <summary>
        /// Gets the time dimension from the metadata.
        /// </summary>
        /// <returns><see cref="TimeDimension"/> object containing information about the time dimension.</returns>
        /// <exception>Throws <see cref="InvalidOperationException"/> if the time dimension is not found in the metadata.</exception>
        public static TimeDimension GetTimeDimension(this IReadOnlyMatrixMetadata metadata)
        {
            TimeDimension? timeDimension = metadata.Dimensions.FirstOrDefault(dimension => dimension.Type == Enums.DimensionType.Time) as TimeDimension;
            return timeDimension ?? throw new InvalidOperationException("Time dimension not found in metadata");
        }

        /// <summary>
        /// Tries to get the content dimension from the metadata.
        /// </summary>
        /// <param name="contentDimension">The content dimension if found.</param>
        /// <returns>True if the content dimension is found, false otherwise.</returns>
        /// <remarks>Use this method when missing the content dimension is not an exceptional case.</remarks>
        public static bool TryGetContentDimension(this IReadOnlyMatrixMetadata metadata, out ContentDimension contentDimension)
        {
            contentDimension = metadata.Dimensions.FirstOrDefault(dimension => dimension.Type == Enums.DimensionType.Content) as ContentDimension;
            return contentDimension is not null;
        }
    }
}
