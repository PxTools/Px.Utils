using Px.Utils.Models;
using Px.Utils.Models.Metadata;
using PxUtils.Models.Metadata.Dimensions;
using System.Numerics;

namespace Px.Utils.Operations
{
    public abstract class MatrixFunction<T>
    {
        public abstract Matrix<T> Apply(Matrix<T> input);
    }

    public class SumMatrixFunction<T>(DimensionValue newValue, DimensionMap sumMap) : MatrixFunction<T> where T : IAdditionOperators<T, T, T>
    {
        public override Matrix<T> Apply(Matrix<T> input)
        {
            SumMatrixFunction<double> sumMatrixFunction = new(newValue, sumMap);
            throw new NotImplementedException();
        }
    }

    public class GroupSumMatrixFunction<T>(IEnumerable<(DimensionValue newValue, DimensionMap sumMap)> parameters) : MatrixFunction<T> where T : IAdditionOperators<T, T, T>
    {
        public override Matrix<T> Apply(Matrix<T> input)
        {
            throw new NotImplementedException();
        }
    }
}
