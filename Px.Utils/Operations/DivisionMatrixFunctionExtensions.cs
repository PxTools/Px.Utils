using Px.Utils.Models;
using Px.Utils.Models.Metadata;
using System.Numerics;

namespace Px.Utils.Operations
{
    /// <summary>
    /// Collection of extension methods for the <see cref="Matrix{TData}"/> for dividing the datapoints in various ways.
    /// </summary>
    public static class DivisionMatrixFunctionExtensions
    {
        /// <summary>
        /// Divides all datapoints defined by the <paramref name="targetMap"/> with a given constant.
        /// </summary>
        /// <typeparam name="TData">Type of the data values in the matrix, must implement 
        /// <see cref="IDivisionOperators{TSelf, TOther, TResult}{TSelf, TOther, TResult}"/> and <see cref="IMultiplicativeIdentity{TSelf, TResult}"/></typeparam>
        /// <param name="input">The source matrix for the operation.</param>
        /// <param name="targetMap">Defines the datapoints to be divided.</param>
        /// <param name="divider">The constant used to divide the datapoints.</param>
        /// <returns>A new <see cref="Matrix{TData}"/> object that contais the results of the operation.</returns>
        public static Matrix<TData> DivideSubsetByConstant<TData>(this Matrix<TData> input, IMatrixMap targetMap, TData divider)
            where TData : IDivisionOperators<TData, TData, TData>, IMultiplicativeIdentity<TData, TData>
        {
            return input.ApplyToSubMap(targetMap, (v) => v / divider);
        }

        /// <summary>
        /// Asyncronously divides all datapoints defined by the <paramref name="targetMap"/> with a given constant.
        /// </summary>
        /// <typeparam name="TData">Type of the data values in the matrix, must implement 
        /// <see cref="IDivisionOperators{TSelf, TOther, TResult}{TSelf, TOther, TResult}"/> and <see cref="IMultiplicativeIdentity{TSelf, TResult}"/></typeparam>
        /// <param name="input">The source matrix for the operation.</param>
        /// <param name="targetMap">Defines the datapoints to be divided.</param>
        /// <param name="divider">The constant used to divide the datapoints.</param>
        /// <returns>A new <see cref="Matrix{TData}"/> object that contais the results of the operation.</returns>
        public static async Task<Matrix<TData>> DivideSubsetByConstantAsync<TData>(this Matrix<TData> input, IMatrixMap targetMap, TData divider)
            where TData : IDivisionOperators<TData, TData, TData>, IMultiplicativeIdentity<TData, TData>
        {
            return await Task.Factory.StartNew(() => DivideSubsetByConstant(input, targetMap, divider));
        }

        /// <summary>
        /// Asyncronously divides all datapoints defined by the <paramref name="targetMap"/> with a given constant.
        /// </summary>
        /// <typeparam name="TData">Type of the data values in the matrix, must implement 
        /// <see cref="IDivisionOperators{TSelf, TOther, TResult}{TSelf, TOther, TResult}"/> and <see cref="IMultiplicativeIdentity{TSelf, TResult}"/></typeparam>
        /// <param name="input">A tasks that produces the source matrix for the operation.</param>
        /// <param name="targetMap">Defines the datapoints to be divided.</param>
        /// <param name="divider">The constant used to divide the datapoints.</param>
        /// <returns>A new <see cref="Matrix{TData}"/> object that contais the results of the operation.</returns>
        public static async Task<Matrix<TData>> DivideSubsetByConstantAsync<TData>(this Task<Matrix<TData>> input, IMatrixMap targetMap, TData divider)
            where TData : IDivisionOperators<TData, TData, TData>, IMultiplicativeIdentity<TData, TData>
        {
            return await DivideSubsetByConstantAsync(await input, targetMap, divider);
        }

        /// <summary>
        /// Divides datapoints defined by <paramref name="targetMap"/> by the values of datapoints defined by the <paramref name="dividerValueCode"/>.
        /// </summary>
        /// <typeparam name="TData">Type of the data values in the matrix, must implement 
        /// <see cref="IDivisionOperators{TSelf, TOther, TResult}{TSelf, TOther, TResult}"/> and <see cref="IMultiplicativeIdentity{TSelf, TResult}"/></typeparam>
        /// <param name="input">The source matrix for the operation.</param>
        /// <param name="targetMap">The datapoints defined by these values will be divided.</param>
        /// <param name="dividerValueCode">The set of datapoints defined by this dimension value 
        /// will be used to divide the corresponding datapoints defined by <paramref name="targetMap"/></param>
        /// <returns>A new <see cref="Matrix{TData}"/> object that contais the results of the operation.</returns>
        public static Matrix<TData> DivideSubsetBySelectedValue<TData>(this Matrix<TData> input, IDimensionMap targetMap, string dividerValueCode)
            where TData : IDivisionOperators<TData, TData, TData>, IMultiplicativeIdentity<TData, TData>
        { 
            return input.ApplyRelative(Divide, targetMap, dividerValueCode);
        }

        /// <summary>
        /// Asyncronously divides datapoints defined by <paramref name="targetMap"/> by the values of datapoints defined by the <paramref name="dividerValueCode"/>.
        /// </summary>
        /// <typeparam name="TData">Type of the data values in the matrix, must implement 
        /// <see cref="IDivisionOperators{TSelf, TOther, TResult}{TSelf, TOther, TResult}"/> and <see cref="IMultiplicativeIdentity{TSelf, TResult}"/></typeparam>
        /// <param name="input">The source matrix for the operation.</param>
        /// <param name="targetMap">The datapoints defined by these values will be divided.</param>
        /// <param name="dividerValueCode">The set of datapoints defined by this dimension value 
        /// will be used to divide the corresponding datapoints defined by <paramref name="targetMap"/></param>
        /// <returns>A new <see cref="Matrix{TData}"/> object that contais the results of the operation.</returns>
        public static async Task<Matrix<TData>> DivideSubsetBySelectedValueAsync<TData>(this Matrix<TData> input, IDimensionMap targetMap, string dividerValueCode)
            where TData : IDivisionOperators<TData, TData, TData>, IMultiplicativeIdentity<TData, TData>
        { 
            return await Task.Factory.StartNew(() => DivideSubsetBySelectedValue(input, targetMap, dividerValueCode));
        }

        /// <summary>
        /// Asyncronously divides datapoints defined by <paramref name="targetMap"/> by the values of datapoints defined by the <paramref name="dividerValueCode"/>.
        /// </summary>
        /// <typeparam name="TData">Type of the data values in the matrix, must implement 
        /// <see cref="IDivisionOperators{TSelf, TOther, TResult}{TSelf, TOther, TResult}"/> and <see cref="IMultiplicativeIdentity{TSelf, TResult}"/></typeparam>
        /// <param name="input">A tasks that produces the source matrix for the operation.</param>
        /// <param name="targetMap">The datapoints defined by these values will be divided.</param>
        /// <param name="dividerValueCode">The set of datapoints defined by this dimension value 
        /// will be used to divide the corresponding datapoints defined by <paramref name="targetMap"/></param>
        /// <returns>A new <see cref="Matrix{TData}"/> object that contais the results of the operation.</returns>
        public static async Task<Matrix<TData>> DivideSubsetBySelectedValueAsync<TData>(this Task<Matrix<TData>> input, IDimensionMap targetMap, string dividerValueCode)
            where TData : IDivisionOperators<TData, TData, TData>, IMultiplicativeIdentity<TData, TData>
        { 
            return await DivideSubsetBySelectedValueAsync(await input, targetMap, dividerValueCode);
        }

        private static TData Divide<TData>(TData a, TData b) where TData : IDivisionOperators<TData, TData, TData>, IMultiplicativeIdentity<TData, TData>
            => a / b;
    }
}
