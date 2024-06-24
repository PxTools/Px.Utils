using Px.Utils.Models;
using Px.Utils.Models.Metadata;
using Px.Utils.Models.Metadata.Dimensions;
using System.Numerics;

namespace Px.Utils.Operations
{
    public static class MultiplicationMatrixFunctionExtensions
    {
        /// <summary>
        /// Multiplies the values defined in the multiplicationMap together and places the product in the new value which will be added to the dimension.
        /// </summary>
        /// <typeparam name="TData">Type of the data values in the matrix, must implement 
        /// <see cref="IMultiplyOperators{TSelf, TOther, TResult}"/> and <see cref="IMultiplicativeIdentity{TSelf, TResult}"/></typeparam>
        /// <param name="input">The source matrix for the operation</param>
        /// <param name="newValue">This value will be added to the dimension defined by the <paramref name="multiplicationMap"/></param>
        /// <param name="multiplicationMap">Defines the <see cref="Dimension"/> relative to which the products are calculated. 
        /// Also defines which <see cref="DimensionValue"/>s are included in the sum.</param>
        /// <returns>A new <see cref="Matrix{TData}"/> object that contais the results of the operation.</returns>
        public static Matrix<TData> MultiplyToNewValue<TData>(this Matrix<TData> input, DimensionValue newValue, IDimensionMap multiplicationMap, int insertIndex = -1) 
            where TData : IMultiplyOperators<TData, TData, TData>, IMultiplicativeIdentity<TData, TData>
        {
            return input.ApplyOverDimension(newValue, multiplicationMap, Multiply, TData.MultiplicativeIdentity, insertIndex);
        }

        
        /// <summary>
        /// Asynchronously multiplies the values defined in the <paramref name="multiplicationMap"/> together and places the product in the new value which will be added to the dimension.
        /// </summary>
        /// <typeparam name="TData">Type of the data values in the matrix, must implement 
        /// <see cref="IMultiplyOperators{TSelf, TOther, TResult}"/> and <see cref="IMultiplicativeIdentity{TSelf, TResult}"/></typeparam>
        /// <param name="input">The source matrix for the operation</param>
        /// <param name="newValue">This value will be added to the dimension defined by the <paramref name="multiplicationMap"/></param>
        /// <param name="multiplicationMap">Defines the <see cref="Dimension"/> relative to which the products are calculated. 
        /// Also defines which <see cref="DimensionValue"/>s are included in the sum.</param>
        /// <returns>A new <see cref="Matrix{TData}"/> object that contais the results of the operation.</returns>
        public async static Task<Matrix<TData>> MultiplyToNewValueAsync<TData>(this Matrix<TData> input, DimensionValue newValue, IDimensionMap multiplicationMap, int insertIndex = -1)
            where TData : IMultiplyOperators<TData, TData, TData>, IMultiplicativeIdentity<TData, TData>
        {
            return await Task.Factory.StartNew(() => MultiplyToNewValue(input, newValue, multiplicationMap, insertIndex));
        }

        /// <summary>
        /// Asynchronously multiplies the values defined in the <paramref name="multiplicationMap"/> together and places the product in the new value which will be added to the dimension.
        /// </summary>
        /// <typeparam name="TData">Type of the data values in the matrix, must implement 
        /// <see cref="IMultiplyOperators{TSelf, TOther, TResult}"/> and <see cref="IMultiplicativeIdentity{TSelf, TResult}"/></typeparam>
        /// <param name="input">The source matrix for the operation</param>
        /// <param name="newValue">This value will be added to the dimension defined by the <paramref name="multiplicationMap"/></param>
        /// <param name="multiplicationMap">Defines the <see cref="Dimension"/> relative to which the products are calculated. 
        /// Also defines which <see cref="DimensionValue"/>s are included in the sum.</param>
        /// <returns>A new <see cref="Matrix{TData}"/> object that contais the results of the operation.</returns>
        public async static Task<Matrix<TData>> MultiplyToNewValueAsync<TData>(this Task<Matrix<TData>> input, DimensionValue newValue, IDimensionMap multiplicationMap, int insertIndex = -1)
            where TData : IMultiplyOperators<TData, TData, TData>, IMultiplicativeIdentity<TData, TData>
        {
            return await MultiplyToNewValueAsync(await input, newValue, multiplicationMap, insertIndex);
        }

        /// <summary>
        /// Multiplies a constant to each value defined by the <paramref name="targetMap"/>.
        /// </summary>
        /// <typeparam name="TData">Type of the data values in the matrix, must implement 
        /// <see cref="IMultiplyOperators{TSelf, TOther, TResult}"/> and <see cref="IMultiplicativeIdentity{TSelf, TResult}"/></typeparam>
        /// <param name="input">The source matrix for the operation</param>
        /// <param name="targetMap">Defines the values which will be multiplied by the constant.</param>
        /// <param name="valueToAdd">The multiplier constant.</param>
        /// <returns>A new <see cref="Matrix{TData}"/> object that contais the results of the operation.</returns>
        public static Matrix<TData> MultiplySubsetByConstant<TData>(this Matrix<TData> input, IMatrixMap targetMap, TData valueToAdd)
            where TData : IMultiplyOperators<TData, TData, TData>, IMultiplicativeIdentity<TData, TData>
        {
            return input.ApplyToSubMap(targetMap, (v) => v * valueToAdd);
        }

        /// <summary>
        /// Asyncronously multiplies each value defined by the <paramref name="targetMap"/> with a constant.
        /// </summary>
        /// <typeparam name="TData">Type of the data values in the matrix, must implement 
        /// <see cref="IMultiplyOperators{TSelf, TOther, TResult}"/> and <see cref="IMultiplicativeIdentity{TSelf, TResult}"/></typeparam>
        /// <param name="input">The source matrix for the operation</param>
        /// <param name="targetMap">Defines the values which will be multiplied by the constant.</param>
        /// <param name="valueToAdd">The multiplier constant.</param>
        /// <returns>A new <see cref="Matrix{TData}"/> object that contais the results of the operation.</returns>
        public async static Task<Matrix<TData>> MultiplySubsetByConstantAsync<TData>(this Matrix<TData> input, IMatrixMap targetMap, TData valueToAdd)
            where TData : IMultiplyOperators<TData, TData, TData>, IMultiplicativeIdentity<TData, TData>
        {
            return await Task.Factory.StartNew(() => MultiplySubsetByConstant(input, targetMap, valueToAdd));
        }

        /// <summary>
        /// Asyncronously multiplies each value defined by the <paramref name="targetMap"/> with a constant.
        /// </summary>
        /// <typeparam name="TData">Type of the data values in the matrix, must implement 
        /// <see cref="IMultiplyOperators{TSelf, TOther, TResult}"/> and <see cref="IMultiplicativeIdentity{TSelf, TResult}"/></typeparam>
        /// <param name="input">The source matrix for the operation.</param>
        /// <param name="targetMap">Defines the values which will be multiplied by the constant.</param>
        /// <param name="valueToAdd">The multiplier constant.</param>
        /// <returns>A new <see cref="Matrix{TData}"/> object that contais the results of the operation.</returns>
        public async static Task<Matrix<TData>> MultiplySubsetByConstantAsync<TData>(this Task<Matrix<TData>> input, IMatrixMap targetMap, TData valueToAdd)
            where TData : IMultiplyOperators<TData, TData, TData>, IMultiplicativeIdentity<TData, TData>
        {
            return await MultiplySubsetByConstantAsync(await input, targetMap, valueToAdd);
        }

        private static TData Multiply<TData>(TData a, TData b) 
            where TData : IMultiplyOperators<TData, TData, TData>, IMultiplicativeIdentity<TData, TData> 
                => a * b; 
    }
}
