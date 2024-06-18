using Px.Utils.Models;

namespace Px.Utils.Operations
{
    public interface IMatrixFunction<TData>
    {
        Matrix<TData> Apply(Matrix<TData> input);
    }
}
