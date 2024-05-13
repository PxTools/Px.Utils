using Px.Utils.Models.Metadata;
using Px.Utils.PxFile.Data;
using PxUtils.Models.Metadata;

namespace Px.Utils.Models
{
    /// <summary>
    /// Contains the data and the metadata that describes the data
    /// </summary>
    /// <typeparam name="T">Type of the data values</typeparam>
    /// <param name="metadata">Information that describes the data in order that matches the order of the data</param>
    /// <param name="data">The data values in order</param>
    public class Matrix<T>(IReadOnlyMatrixMetadata metadata, T[] data)
    {
        /// <summary>
        /// Describes the data in the matrix
        /// Consists of ordered collection the dimensions and the values of the dimensions
        /// Let n be the number of dimensions:
        /// Data[0] is described by the first value in each dimension (dims[0..n-1][0])
        /// Data[1] is described by the first value in each dimensions 0 - n-2 and the second value from dim n-1 (dims[0..n-2][0] dims[n-1][1])
        /// Data[2] is described by the first value in each dimensions 0 - n-2 and the third value from dim n-1 (dims[0..n-2][0] dims[n-1][2])
        /// The last data value is described by the last value in each dimension.
        /// </summary>
        public IReadOnlyMatrixMetadata Metadata { get; } = metadata;

        /// <summary>
        /// The data values of the matrix
        /// </summary>
        public T[] Data { get; } = data;

        /// <summary>
        /// Returns a new matrix where the data and metadata have the same order and structure as the map.
        /// </summary>
        /// <param name="map">Simplified representation of the target structure</param>
        /// <returns>A new <see cref="Matrix{T}"/> with the data and metadata order and structure matching the map</returns>"/>
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
