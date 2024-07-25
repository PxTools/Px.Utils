using Px.Utils.Models.Metadata.Dimensions;

namespace Px.Utils.Models.Metadata
{
    public static class MatrixMetadataExtensions
    {
        public static ContentDimension GetContentDimension(this MatrixMetadata metadata)
        {
            ContentDimension? ct = metadata.Dimensions.OfType<ContentDimension>().FirstOrDefault();
            return ct ?? throw new InvalidOperationException("Content dimension not found in metadata");
        }

        public static TimeDimension GetTimeDimension(this MatrixMetadata metadata)
        {             
            TimeDimension? time = metadata.Dimensions.OfType<TimeDimension>().FirstOrDefault();
            return time ?? throw new InvalidOperationException("Time dimension not found in metadata");
        }
    }
}
