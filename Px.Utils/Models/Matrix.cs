using PxUtils.Models.Metadata;

namespace Px.Utils.Models
{
    public class Matrix<T>(IReadOnlyMatrixMetadata metadata, T[] data)
    {
        public IReadOnlyMatrixMetadata Metadata { get; } = metadata;

        public T[] Data { get; } = data;
    }
}
