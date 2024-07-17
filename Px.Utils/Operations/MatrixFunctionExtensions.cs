using Px.Utils.Models;
using Px.Utils.Models.Metadata;
using Px.Utils.Models.Metadata.Dimensions;
using Px.Utils.Models.Metadata.ExtensionMethods;
using Px.Utils.PxFile.Data;

namespace Px.Utils.Operations
{
    /// <summary>
    /// Base class for matrix functions, defines some operation that can be applied to a matrix, such as a transformation or a sum
    /// </summary>
    /// <typeparam name="TData">Type of the data values in the matrix</typeparam>
    public static class MatrixFunctionExtensions
    {
        /// <summary>
        /// Applies the function to the input matrix, returning a new matrix with the result
        /// </summary>
        /// <param name="input">inoput matrix</param>
        /// <returns>a new matrix with the result of the operation</returns>
        public static Matrix<TData> ApplyOverDimension<TData>(
            this Matrix<TData> input, DimensionValue newValue, IDimensionMap sourceMap, Func<TData, TData, TData> func, TData functionIdentity, int valueIndex = -1){
            MatrixMetadata newMeta = valueIndex == -1
                ? CopyMetaAndAddValue(input.Metadata, sourceMap.Code, newValue)
                : CopyMetaAndInsertValue(input.Metadata, sourceMap.Code, newValue, valueIndex);

            // Compute sum values to the output matrix

            IMatrixMap sumOnlyMap = newMeta.CollapseDimension(sourceMap.Code, newValue.Code);
            TData[] outData = new TData[input.Metadata.GetSize() + sumOnlyMap.GetSize()];
            
            // Initialize the output matrix with the identity value
            for (int i = 0; i < outData.Length; i++) outData[i] = functionIdentity; 

            for (int i = 0; i < sourceMap.ValueCodes.Count; i++)
            {
                IMatrixMap sumSubMap = input.Metadata.CollapseDimension(sourceMap.Code, sourceMap.ValueCodes[i]);
                DataIndexer source = new(input.Metadata, sumSubMap);
                DataIndexer target = new(newMeta, sumOnlyMap);

                do outData[target.CurrentIndex] = func(outData[target.CurrentIndex], input.Data[source.CurrentIndex]);
                while (source.Next() && target.Next());
            }

            // Copy the original data to the output matrix

            DataIndexer original = new(newMeta, input.Metadata);
            int ogIndex = 0;
            do outData[original.CurrentIndex] = input.Data[ogIndex++];
            while (original.Next());

            return new Matrix<TData>(newMeta, outData);
        }

        public static Matrix<TData> ApplyToSubMap<TData>(this Matrix<TData> input, IMatrixMap subMap, Func<TData, TData> func)
        {
            MatrixMetadata metaCopy = input.Metadata.GetTransform(input.Metadata);
            TData[] dataCopy = [.. input.Data];

            DataIndexer indexer = new(input.Metadata, subMap);

            do dataCopy[indexer.CurrentIndex] = func(input.Data[indexer.CurrentIndex]);
            while (indexer.Next());

            return new Matrix<TData>(metaCopy, dataCopy);
        }

        public static Matrix<TData> ApplyRelative<TData>(this Matrix<TData> input, Func<TData, TData, TData> func, IDimensionMap sourceMap, string baseValueCode)
        {
            IMatrixMap baseValueMap = input.Metadata.CollapseDimension(sourceMap.Code, baseValueCode);
            TData[] resultData = [.. input.Data];
            foreach(string valueCode in sourceMap.ValueCodes)
            {
                DataIndexer baseValueIndexer = new(input.Metadata, baseValueMap);
                DataIndexer targetIndexer = new(input.Metadata, input.Metadata.CollapseDimension(sourceMap.Code, valueCode));

                do
                {
                    TData sourceValue = input.Data[targetIndexer.CurrentIndex];
                    TData baseValue = input.Data[baseValueIndexer.CurrentIndex];
                    resultData[targetIndexer.CurrentIndex] = func(sourceValue, baseValue);
                }
                while (targetIndexer.Next() && baseValueIndexer.Next());     
            }
            return new Matrix<TData>(input.Metadata.GetTransform(input.Metadata), resultData);
        }

        private static MatrixMetadata CopyMetaAndAddValue(IReadOnlyMatrixMetadata meta, string dimCode, DimensionValue valueToAdd)
        {
            IReadOnlyDimension dimension = meta.Dimensions.First(d => d.Code == dimCode);
            Dimension dimCopy = dimension.GetTransform(dimension);
            dimCopy.Values.Add(valueToAdd);

            MatrixMetadata metaCopy = meta.GetTransform(meta);
            int dimIndex = metaCopy.Dimensions.FindIndex(d => d.Code == dimCode);
            metaCopy.Dimensions[dimIndex] = dimCopy;
            return metaCopy;
        }

        private static MatrixMetadata CopyMetaAndInsertValue(IReadOnlyMatrixMetadata meta, string dimCode, DimensionValue valueToAdd, int index)
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
}
