using Px.Utils.Models.Metadata.Dimensions;

namespace Px.Utils.Models.Metadata.ExtensionMethods
{
    /// <summary>
    /// Extension methods for <see cref="IReadOnlyMatrixMetadata"/>.
    /// </summary>
    public static class MatrixMetadataExtensions
    {
        /// <summary>
        /// Gets the content dimension from the metadata.
        /// </summary>
        /// <returns><see cref="ContentDimension"/> object representing the first dimension with type <see cref="Enums.DimensionType.Content"/>.</returns>
        /// <exception>Throws <see cref="InvalidOperationException"/> if the content dimension is not found in the metadata.</exception>
        public static ContentDimension GetContentDimension(this IReadOnlyMatrixMetadata metadata)
        {
            ContentDimension? contentDimension = metadata.Dimensions.FirstOrDefault(dimension => dimension.Type == Enums.DimensionType.Content) as ContentDimension;
            return contentDimension ?? throw new InvalidOperationException("Content dimension not found in metadata");
        }

        /// <summary>
        /// Try to get the content dimension from the metadata.
        /// </summary>
        /// <param name="contentDimension"><see cref="ContentDimension"/> object representing the first dimension with type <see cref="Enums.DimensionType.Content"/>.</param>
        /// <returns>True if the content dimension is found in the metadata, false otherwise.</returns>
        public static bool TryGetContentDimension(this IReadOnlyMatrixMetadata metadata, out ContentDimension? contentDimension)
        {
            contentDimension = metadata.Dimensions.FirstOrDefault(dimension => dimension.Type == Enums.DimensionType.Content) as ContentDimension;
            return contentDimension != null;
        }

        /// <summary>
        /// Gets the time dimension from the metadata.
        /// </summary>
        /// <returns><see cref="TimeDimension"/> object representing the first dimension with type <see cref="Enums.DimensionType.Time"/>.</returns>
        /// <exception>Throws <see cref="InvalidOperationException"/> if the time dimension is not found in the metadata.</exception>
        public static TimeDimension GetTimeDimension(this IReadOnlyMatrixMetadata metadata)
        {
            TimeDimension? timeDimension = metadata.Dimensions.FirstOrDefault(dimension => dimension.Type == Enums.DimensionType.Time) as TimeDimension;
            return timeDimension ?? throw new InvalidOperationException("Time dimension not found in metadata");
        }

        /// <summary>
        /// Try to get the time dimension from the metadata.
        /// </summary>
        /// <param name="timeDimension"><see cref="TimeDimension"/> object representing the first dimension with type <see cref="Enums.DimensionType.Time"/>.</param>
        /// <returns>True if the time dimension is found in the metadata, false otherwise.</returns>
        public static bool TryGetTimeDimension(this IReadOnlyMatrixMetadata metadata, out TimeDimension? timeDimension)
        {
            timeDimension = metadata.Dimensions.FirstOrDefault(dimension => dimension.Type == Enums.DimensionType.Time) as TimeDimension;
            return timeDimension != null;
        }
    }
}
