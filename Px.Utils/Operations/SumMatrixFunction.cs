using Px.Utils.Models.Metadata;
using Px.Utils.Models.Metadata.Dimensions;
using System.Numerics;

namespace Px.Utils.Operations
{

    /// <summary>
    /// A function adds a new value to a dimension which is a sum of values defined by a map
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public class SumMatrixFunction<TData> : MatrixFunction<TData> where TData : IAdditionOperators<TData, TData, TData>, IAdditiveIdentity<TData, TData>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="newValue">Dimension value that will represent the sum of the values</param>
        /// <param name="sumMap">Defines which values will be summed</param>
        public SumMatrixFunction(DimensionValue newValue, IDimensionMap sumMap) : base(newValue, sumMap, Sum, TData.AdditiveIdentity) {}

        /// <summary>
        /// Constructor that allows to specify the index where the new value will be inserted in the dimension
        /// </summary>
        /// <param name="newValue">Dimension value that will represent the sum of the values</param>
        /// <param name="sumMap">Defines which values will be summed</param>
        /// <param name="insertIndex">Zero based index where the new value will be inserted in the dimension</param>
        public SumMatrixFunction(DimensionValue newValue, IDimensionMap sumMap, int insertIndex) : base(newValue, sumMap, Sum, TData.AdditiveIdentity, insertIndex) { }

        private static TData Sum(TData a, TData b) => a + b; 
    }
}
