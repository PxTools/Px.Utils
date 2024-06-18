using Px.Utils.Models.Metadata;
using Px.Utils.Models.Metadata.Dimensions;
using System.Numerics;

namespace Px.Utils.Operations
{
    internal class DivisionMatrixFunction<TData> : MatrixFunction<TData> where TData : IDivisionOperators<TData, TData, TData>, IMultiplicativeIdentity<TData, TData>
    {
        /// <summary>
        /// Constructor for multiplying all values defined by the target map with a given value.
        /// </summary>
        /// <param name="targetMap">Defines which values will be multiplied</param>
        /// <param name="multiplier">Multiply by this constant</param>
        public DivisionMatrixFunction(IMatrixMap targetMap, TData multiplier) : base(targetMap, (input) => input / multiplier) { }

        public DivisionMatrixFunction(IDimensionMap sourceMap, string baseValueCode) : base(Divide, sourceMap, baseValueCode) { }

        private static TData Divide(TData a, TData b) => a / b;
    }
}
