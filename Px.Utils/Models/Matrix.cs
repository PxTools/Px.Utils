using Px.Utils.Models.Metadata;
using Px.Utils.PxFile.Data;
using PxUtils.Models.Metadata;

namespace Px.Utils.Models
{
    public class Matrix<T>(IReadOnlyMatrixMetadata metadata, T[] data)
    {
        public IReadOnlyMatrixMetadata Metadata { get; } = metadata;

        public T[] Data { get; } = data;

        public Matrix<T> GetTransform(IMatrixMap map)
        {
            DataIndexer indexer = new(Metadata, map);
            T[] newData = new T[indexer.DataLength];
            int index = 0;
            do newData[index++] = Data[indexer.CurrentIndex];
            while (indexer.Next());

            return new Matrix<T>(Metadata.GetTransform(map), newData);
        }
    }
}
