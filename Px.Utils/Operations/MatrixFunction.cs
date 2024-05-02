using Px.Utils.Models;
using Px.Utils.Models.Metadata;
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
        public abstract Matrix<TData> Apply(Matrix<TData> input);
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

    public class SumMatrixFunction<TData>(DimensionValue newValue, DimensionMap sumMap)
        : MatrixFunction<TData> where TData : IAdditionOperators<TData, TData, TData>
    {

        public override Matrix<TData> Apply(Matrix<TData> input)
        {
            SumMatrixFunction<double> sumMatrixFunction = new(newValue, sumMap);
            throw new NotImplementedException();
        }
    }

    public class GroupSumMatrixFunction<TData>(IEnumerable<(DimensionValue newValue, DimensionMap sumMap)> parameters)
        : MatrixFunction<TData> where TData : IAdditionOperators<TData, TData, TData>
    {
        public override Matrix<TData> Apply(Matrix<TData> input)
        {
            throw new NotImplementedException();
        }
    }
}
