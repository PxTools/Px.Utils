using Px.Utils.Models.Metadata.Dimensions;

namespace Px.Utils.Models.Metadata
{
    public static class MatrixMetadataExtensions
    {
        public static ContentDimension GetContentDimension(this MatrixMetadata metadata)
        {
            ContentDimension? contentDimension = metadata.Dimensions.Find(dimension => dimension.Type == Enums.DimensionType.Content) as ContentDimension;
            return contentDimension ?? throw new InvalidOperationException("Content dimension not found in metadata");
        }

        public static TimeDimension GetTimeDimension(this MatrixMetadata metadata)
        {
            TimeDimension? timeDimension = metadata.Dimensions.Find(dimension => dimension.Type == Enums.DimensionType.Time) as TimeDimension;
            return timeDimension ?? throw new InvalidOperationException("Time dimension not found in metadata");
        }
    }
}
