using PxUtils.Models.Metadata.Dimensions;

namespace PxUtils.Models.Metadata
{
    public interface IReadOnlyMatrixMetadata
    {
        IReadOnlyList<IReadOnlyDimension> Dimensions { get; }
    }
}
