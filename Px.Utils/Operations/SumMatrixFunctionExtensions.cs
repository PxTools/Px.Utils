using Px.Utils.Models;
using Px.Utils.Models.Metadata;
using Px.Utils.Models.Metadata.Dimensions;
using System.Numerics;

namespace Px.Utils.Operations
{

    /// <summary>
    /// A collection of extension methods for computing different addition operations on matrices. 
    /// </summary>
    public static class SumMatrixFunctionExtensions
    {
        /// <summary>
        /// Adds the values defined in the sumMap together and places the sum in the new value which will be added to the dimension.
        /// </summary>
        /// <typeparam name="TData">Type of the data values in the matrix, must implement 
        /// <see cref="IAdditionOperators{TSelf, TOther, TResult}"/> and <see cref="IAdditiveIdentity{TSelf, TResult}"/></typeparam>
        /// <param name="input">The source matrix for the operation</param>
        /// <param name="newValue">This value will be added to the dimension defined by the <paramref name="sumMap"/></param>
        /// <param name="sumMap">Defines the <see cref="Dimension"/> relative to which the sums are calculated. 
        /// Also defines which <see cref="DimensionValue"/>s are included in the sum.</param>
        /// <returns>A new <see cref="Matrix{TData}"/> object that contais the results of the additions.</returns>
        public static Matrix<TData> SumToNewValue<TData>(this Matrix<TData> input, DimensionValue newValue, IDimensionMap sumMap, int insertIndex = -1) 
            where TData : IAdditionOperators<TData, TData, TData>, IAdditiveIdentity<TData, TData>
        {
            return input.ApplyOverDimension(newValue, sumMap, Sum, TData.AdditiveIdentity, insertIndex);
        }

        
        /// <summary>
        /// Asynchronously adds the values defined in the <paramref name="sumMap"/> together and places the sum in the new value which will be added to the dimension.
        /// </summary>
        /// <typeparam name="TData">Type of the data values in the matrix, must implement 
        /// <see cref="IAdditionOperators{TSelf, TOther, TResult}"/> and <see cref="IAdditiveIdentity{TSelf, TResult}"/></typeparam>
        /// <param name="input">The source matrix for the operation</param>
        /// <param name="newValue">This value will be added to the dimension defined by the <paramref name="sumMap"/></param>
        /// <param name="sumMap">Defines the <see cref="Dimension"/> relative to which the sums are calculated. 
        /// Also defines which <see cref="DimensionValue"/>s are included in the sum.</param>
        /// <returns>A new <see cref="Matrix{TData}"/> object that contais the results of the additions.</returns>
        public async static Task<Matrix<TData>> SumToNewValueAsync<TData>(this Matrix<TData> input, DimensionValue newValue, IDimensionMap sumMap, int insetIndex = -1)
            where TData : IAdditionOperators<TData, TData, TData>, IAdditiveIdentity<TData, TData>
        {
            return await Task.Factory.StartNew(() => SumToNewValue(input, newValue, sumMap, insetIndex));
        }

        /// <summary>
        /// Asynchronously adds the values defined in the <paramref name="sumMap"/> together and places the sum in the new value which will be added to the dimension.
        /// </summary>
        /// <typeparam name="TData">Type of the data values in the matrix, must implement 
        /// <see cref="IAdditionOperators{TSelf, TOther, TResult}"/> and <see cref="IAdditiveIdentity{TSelf, TResult}"/></typeparam>
        /// <param name="input">The source matrix for the operation</param>
        /// <param name="newValue">This value will be added to the dimension defined by the <paramref name="sumMap"/></param>
        /// <param name="sumMap">Defines the <see cref="Dimension"/> relative to which the sums are calculated. 
        /// Also defines which <see cref="DimensionValue"/>s are included in the sum.</param>
        /// <returns>A new <see cref="Matrix{TData}"/> object that contais the results of the additions.</returns>
        public async static Task<Matrix<TData>> SumToNewValueAsync<TData>(this Task<Matrix<TData>> input, DimensionValue newValue, IDimensionMap sumMap, int insetIndex = -1)
            where TData : IAdditionOperators<TData, TData, TData>, IAdditiveIdentity<TData, TData>
        {
            return await SumToNewValueAsync(await input, newValue, sumMap, insetIndex);
        }

        /// <summary>
        /// Adds a constant to each value defined by the <paramref name="targetMap"/>.
        /// </summary>
        /// <typeparam name="TData">Type of the data values in the matrix, must implement 
        /// <see cref="IAdditionOperators{TSelf, TOther, TResult}"/> and <see cref="IAdditiveIdentity{TSelf, TResult}"/></typeparam>
        /// <param name="input">The source matrix for the operation</param>
        /// <param name="targetMap">Defines the values to wich the constant will be added.</param>
        /// <param name="valueToAdd">The contant to be added.</param>
        /// <returns>A new <see cref="Matrix{TData}"/> object that contais the results of the additions.</returns>
        public static Matrix<TData> AddConstantToSubset<TData>(this Matrix<TData> input, IMatrixMap targetMap, TData valueToAdd)
            where TData : IAdditionOperators<TData, TData, TData>, IAdditiveIdentity<TData, TData>
        {
            return input.ApplyToSubMap(targetMap, (v) => v + valueToAdd);
        }

        /// <summary>
        /// Asyncronously adds a constant to each value defined by the <paramref name="targetMap"/>.
        /// </summary>
        /// <typeparam name="TData">Type of the data values in the matrix, must implement 
        /// <see cref="IAdditionOperators{TSelf, TOther, TResult}"/> and <see cref="IAdditiveIdentity{TSelf, TResult}"/></typeparam>
        /// <param name="input">The source matrix for the operation</param>
        /// <param name="targetMap">Defines the values to wich the constant will be added.</param>
        /// <param name="valueToAdd">The contant to be added.</param>
        /// <returns>A new <see cref="Matrix{TData}"/> object that contais the results of the additions.</returns>
        public async static Task<Matrix<TData>> AddConstantToSubsetAsync<TData>(this Matrix<TData> input, IMatrixMap targetMap, TData valueToAdd)
            where TData : IAdditionOperators<TData, TData, TData>, IAdditiveIdentity<TData, TData>
        {
            return await Task.Factory.StartNew(() => AddConstantToSubset(input, targetMap, valueToAdd));
        }

        /// <summary>
        /// Asyncronously adds a constant to each value defined by the <paramref name="targetMap"/>.
        /// </summary>
        /// <typeparam name="TData">Type of the data values in the matrix, must implement 
        /// <see cref="IAdditionOperators{TSelf, TOther, TResult}"/> and <see cref="IAdditiveIdentity{TSelf, TResult}"/></typeparam>
        /// <param name="input">The source matrix for the operation</param>
        /// <param name="targetMap">Defines the values to which the constant will be added.</param>
        /// <param name="valueToAdd">The constant to be added.</param>
        /// <returns>A new <see cref="Matrix{TData}"/> object that contains the results of the additions.</returns>
        public async static Task<Matrix<TData>> AddConstantToSubsetAsync<TData>(this Task<Matrix<TData>> input, IMatrixMap targetMap, TData valueToAdd)
            where TData : IAdditionOperators<TData, TData, TData>, IAdditiveIdentity<TData, TData>
        {
            return await AddConstantToSubsetAsync(await input, targetMap, valueToAdd);
        }

        private static TData Sum<TData>(TData a, TData b) 
            where TData : IAdditionOperators<TData, TData, TData>, IAdditiveIdentity<TData, TData> 
                => a + b; 
    }
}
