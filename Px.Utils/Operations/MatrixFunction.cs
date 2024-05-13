using Px.Utils.Models;
using Px.Utils.Models.Metadata;
using Px.Utils.Models.Metadata.ExtensionMethods;
using Px.Utils.PxFile.Data;
using PxUtils.Models.Metadata;
using PxUtils.Models.Metadata.Dimensions;
using System.Numerics;

namespace Px.Utils.Operations
{
    /// <summary>
    /// Base class for matrix functions, defines some operation that can be applied to a matrix, such as a transformation or a sum
    /// </summary>
    /// <typeparam name="TData">Type of the data values in the matrix</typeparam>
    public abstract class MatrixFunction<TData>
    {
        /// <summary>
        /// Applies the function to the input matrix, returning a new matrix with the result
        /// </summary>
        /// <param name="input">inoput matrix</param>
        /// <returns>a new matrix with the result of the operation</returns>
        public abstract Matrix<TData> Apply(Matrix<TData> input);

        protected static MatrixMetadata CopyMetaAndAddValue(IReadOnlyMatrixMetadata meta, string dimCode, DimensionValue valueToAdd)
        {
            IReadOnlyDimension dimension = meta.Dimensions.First(d => d.Code == dimCode);
            Dimension dimCopy = dimension.GetTransform(dimension);
            dimCopy.Values.Add(valueToAdd);

            MatrixMetadata metaCopy = meta.GetTransform(meta);
            int dimIndex = metaCopy.Dimensions.FindIndex(d => d.Code == dimCode);
            metaCopy.Dimensions[dimIndex] = dimCopy;
            return metaCopy;
        }

        protected static MatrixMetadata CopyMetaAndInsertValue(IReadOnlyMatrixMetadata meta, string dimCode, DimensionValue valueToAdd, int index)
        {
            IReadOnlyDimension dimension = meta.Dimensions.First(d => d.Code == dimCode);
            Dimension dimCopy = dimension.GetTransform(dimension);
            dimCopy.Values.Insert(index, valueToAdd);

            MatrixMetadata metaCopy = meta.GetTransform(meta);
            int dimIndex = metaCopy.Dimensions.FindIndex(d => d.Code == dimCode);
            metaCopy.Dimensions[dimIndex] = dimCopy;
            return metaCopy;
        }
    }

    /// <summary>
    /// Function that applies a transformation map to a matrix
    /// </summary>
    /// <typeparam name="TData">Type of the data values in the matrix</typeparam>
    /// <param name="map">The resulting matrix will have this structure</param>
    public class TransformationMatrixFunction<TData>(IMatrixMap map) : MatrixFunction<TData>
    {
        public override Matrix<TData> Apply(Matrix<TData> input)
        {
            return input.GetTransform(map);
        }
    }

    /// <summary>
    /// A function adds a new value to a dimension which is a sum of values defined by a map
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public class SumMatrixFunction<TData> : MatrixFunction<TData> where TData : IAdditionOperators<TData, TData, TData>
    {
        private readonly DimensionValue _newValue;
        private readonly DimensionMap _sumMap;
        readonly int _valueIndex;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="newValue">Dimension value that will represent the sum of the values</param>
        /// <param name="sumMap">Defines which values will be summed</param>
        public SumMatrixFunction(DimensionValue newValue, DimensionMap sumMap)
        {
            _newValue = newValue;
            _sumMap = sumMap;
            _valueIndex = -1;
        }

        /// <summary>
        /// Constructor that allows to specify the index where the new value will be inserted in the dimension
        /// </summary>
        /// <param name="newValue">Dimension value that will represent the sum of the values</param>
        /// <param name="sumMap">Defines which values will be summed</param>
        /// <param name="insertIndex">Zero based index where the new value will be inserted in the dimension</param>
        public SumMatrixFunction(DimensionValue newValue, DimensionMap sumMap, int insertIndex)
        {
            _newValue = newValue;
            _sumMap = sumMap;
            _valueIndex = insertIndex;
        }

        /// <summary>
        /// Applies the sum operation to the input matrix, returning a new matrix with the result
        /// </summary>
        /// <param name="input">The input matrix</param>
        /// <returns>New matrix with the result of the sum operation</returns>
        public override Matrix<TData> Apply(Matrix<TData> input)
        {
            MatrixMetadata newMeta = _valueIndex == -1 
                ? CopyMetaAndAddValue(input.Metadata, _sumMap.Code, _newValue)
                : CopyMetaAndInsertValue(input.Metadata, _sumMap.Code, _newValue, _valueIndex);

            // Compute sum values to the output matrix

            IMatrixMap sumOnlyMap = newMeta.CollapseDimension(_sumMap.Code, _newValue.Code);
            TData[] outData = new TData[input.Metadata.GetSize() + sumOnlyMap.GetSize()];

            for(int i = 0; i < _sumMap.ValueCodes.Count; i++)
            {
                IMatrixMap sumSubMap = input.Metadata.CollapseDimension(_sumMap.Code, _sumMap.ValueCodes[i]);
                DataIndexer source = new(input.Metadata, sumSubMap);
                DataIndexer target = new(newMeta, sumOnlyMap);

                do outData[target.CurrentIndex] = outData[target.CurrentIndex] + input.Data[source.CurrentIndex];
                while (source.Next() && target.Next());
            }

            // Copy the original data to the output matrix

            DataIndexer original = new(newMeta, input.Metadata);
            int ogIndex = 0;
            do outData[original.CurrentIndex] = input.Data[ogIndex++];
            while (original.Next());

            return new Matrix<TData>(newMeta, outData);
        }
    }
}
