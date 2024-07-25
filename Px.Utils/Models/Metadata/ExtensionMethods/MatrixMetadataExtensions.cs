using Px.Utils.Models.Metadata.Dimensions;

namespace Px.Utils.Models.Metadata.ExtensionMethods
{
    public static class MatrixMetadataExtensions
    {
        public static ContentDimension GetContentDimension(this IReadOnlyMatrixMetadata metadata)
        {
            ContentDimension? contentDimension = metadata.Dimensions.FirstOrDefault(dimension => dimension.Type == Enums.DimensionType.Content) as ContentDimension;
            return contentDimension ?? throw new InvalidOperationException("Content dimension not found in metadata");
        }

        public static TimeDimension GetTimeDimension(this IReadOnlyMatrixMetadata metadata)
        {
            TimeDimension? timeDimension = metadata.Dimensions.FirstOrDefault(dimension => dimension.Type == Enums.DimensionType.Time) as TimeDimension;
            return timeDimension ?? throw new InvalidOperationException("Time dimension not found in metadata");
        }
    }
}
