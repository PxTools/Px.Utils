using PxUtils.Models.Metadata.Dimension;

namespace PxUtils.Models.Metadata
{
    public interface IReadOnlyMatrixMetadata
    {
        IReadOnlyList<IReadOnlyDimension> Dimensions { get; }
    }
}
