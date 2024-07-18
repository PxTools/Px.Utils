using Px.Utils.Models;
using Px.Utils.Models.Metadata;
using Px.Utils.Models.Metadata.Dimensions;
using Px.Utils.Models.Metadata.ExtensionMethods;
using Px.Utils.PxFile.Data;

namespace Px.Utils.Operations
{
    /// <summary>
    /// Contains extension methods for the <see cref="Matrix{T}"/> class that can be used to perform varius abstract operations on the data.
    /// Mainly intended as a base for more practical iperations such as sums or multiplications.
    /// </summary>
    public static class MatrixFunctionExtensions
    {
        /// <summary>
        /// Applies a function to the input matrix over one spesified dimension.
        /// Adds a new value to that dimension which is used to store the resulting values.
        /// </summary>
        /// <param name="input">The input matrix</param>
        /// <param name="newValue">The resulting values will be defined by this new dimension value that will be added to the source dimension.</param>
        /// <param name="sourceMap">Values defined by this map will be used as inputs for the function. The result value will be added to the dimension defined by this map.</param>
        /// <param name="func">Aggregation function that takes one value from the input matrix and a result of the previous operation as parameters.</param>
        /// <param name="functionIdentity">Value that is used as a parameter when the operation is performed on the first value.</param>
        /// <param name="valueIndex">Index in which the new value holding the results will be inserted in the dimension.</param>
        /// <returns>A new matrix with the result of the operation</returns>
        public static Matrix<TData> ApplyOverDimension<TData>(
            this Matrix<TData> input, DimensionValue newValue, IDimensionMap sourceMap, Func<TData, TData, TData> func, TData functionIdentity, int valueIndex = -1){
            MatrixMetadata newMeta = valueIndex == -1
                ? CopyMetaAndAddValue(input.Metadata, sourceMap.Code, newValue)
                : CopyMetaAndInsertValue(input.Metadata, sourceMap.Code, newValue, valueIndex);

            IMatrixMap resultOnlyMap = newMeta.CollapseDimension(sourceMap.Code, newValue.Code);
            TData[] outData = new TData[input.Metadata.GetSize() + resultOnlyMap.GetSize()];
            
            // Initialize the output matrix with the identity value
            for (int i = 0; i < outData.Length; i++) outData[i] = functionIdentity; 

            for (int i = 0; i < sourceMap.ValueCodes.Count; i++)
            {
                IMatrixMap resultSubMap = input.Metadata.CollapseDimension(sourceMap.Code, sourceMap.ValueCodes[i]);
                DataIndexer source = new(input.Metadata, resultSubMap);
                DataIndexer target = new(newMeta, resultOnlyMap);

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

        /// <summary>
        /// Applies a function to the values of the input matrix which a defined by the submap.
        /// </summary>
        /// <param name="input">The input matrix</param>
        /// <param name="subMap">Defines the values that will be changed using the input function.</param>
        /// <param name="func">A function that takes one value from the input matrix and returns a new value.</param>
        /// <returns>A new matrix with the resulting values.</returns>
        public static Matrix<TData> ApplyToSubMap<TData>(this Matrix<TData> input, IMatrixMap subMap, Func<TData, TData> func)
        {
            MatrixMetadata metaCopy = input.Metadata.GetTransform(input.Metadata);
            TData[] dataCopy = [.. input.Data];

            DataIndexer indexer = new(input.Metadata, subMap);

            do dataCopy[indexer.CurrentIndex] = func(input.Data[indexer.CurrentIndex]);
            while (indexer.Next());

            return new Matrix<TData>(metaCopy, dataCopy);
        }

        /// <summary>
        /// Applies a function to a matrix over one specified dimension and uses one specified value of that dimension as a additional input to the function.
        /// </summary>
        /// <typeparam name="TData">Type of the data values in the matrix.</typeparam>
        /// <param name="input">The input matrix.</param>
        /// <param name="func">The first parameter is the changing value of the dimension and the second parameter is the provided base value of the dimension.</param>
        /// <param name="sourceMap">Values of the source dimension that will be changed using the parameter function.</param>
        /// <param name="baseValueCode">One value of the source value dimension that will be used as a parameter to the function.</param>
        /// <returns>A new matrix where the values defined by the <paramref name="sourceMap"/> have been changed.</returns>
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
