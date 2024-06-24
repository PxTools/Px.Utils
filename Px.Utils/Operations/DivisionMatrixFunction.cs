using Px.Utils.Models;
using Px.Utils.Models.Metadata;
using System.Numerics;

namespace Px.Utils.Operations
{
    public static class DivisionMatrixFunctionExtensions
    {
        public static Matrix<TData> DivideSubsetByConstant<TData>(this Matrix<TData> input, IMatrixMap targetMap, TData divider)
            where TData : IDivisionOperators<TData, TData, TData>, IMultiplicativeIdentity<TData, TData>
        {
            return input.ApplyToSubMap(targetMap, (v) => v / divider);
        }

        public static async Task<Matrix<TData>> DivideSubsetByConstantAsync<TData>(this Matrix<TData> input, IMatrixMap targetMap, TData divider)
            where TData : IDivisionOperators<TData, TData, TData>, IMultiplicativeIdentity<TData, TData>
        {
            return await Task.Factory.StartNew(() => DivideSubsetByConstant(input, targetMap, divider));
        }

        public static async Task<Matrix<TData>> DivideSubsetByConstantAsync<TData>(this Task<Matrix<TData>> input, IMatrixMap targetMap, TData divider)
            where TData : IDivisionOperators<TData, TData, TData>, IMultiplicativeIdentity<TData, TData>
        {
            return await DivideSubsetByConstantAsync(await input, targetMap, divider);
        }

        public static Matrix<TData> DivideSubsetBySelectedValue<TData>(this Matrix<TData> input, IDimensionMap targetMap, string baseValueCode)
            where TData : IDivisionOperators<TData, TData, TData>, IMultiplicativeIdentity<TData, TData>
        { 
            return input.ApplyRelative(Divide, targetMap, baseValueCode);
        }

        public static async Task<Matrix<TData>> DivideSubsetBySelectedValueAsync<TData>(this Matrix<TData> input, IDimensionMap targetMap, string baseValueCode)
            where TData : IDivisionOperators<TData, TData, TData>, IMultiplicativeIdentity<TData, TData>
        { 
            return await Task.Factory.StartNew(() => DivideSubsetBySelectedValue(input, targetMap, baseValueCode));
        }
        public static async Task<Matrix<TData>> DivideSubsetBySelectedValueAsync<TData>(this Task<Matrix<TData>> input, IDimensionMap targetMap, string baseValueCode)
            where TData : IDivisionOperators<TData, TData, TData>, IMultiplicativeIdentity<TData, TData>
        { 
            return await DivideSubsetBySelectedValueAsync(await input, targetMap, baseValueCode);
        }

        private static TData Divide<TData>(TData a, TData b) where TData : IDivisionOperators<TData, TData, TData>, IMultiplicativeIdentity<TData, TData>
            => a / b;
    }
}
