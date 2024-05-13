using Px.Utils.Models;
using Px.Utils.Models.Data;
using System.Diagnostics.CodeAnalysis;

namespace Px.Utils.Operations
{
    [ExcludeFromCodeCoverage] // Tests for this class will be added once more operations are implemented and this feature is integrated.
    public class MatrixFunctionProcessor<T> where T : IDataValue<T>
    {
        private readonly Queue<MatrixFunction<T>> _queue = new();

        public void Add(MatrixFunction<T> function)
        {
            _queue.Enqueue(function);
        }

        public void Add(IEnumerable<MatrixFunction<T>> functions)
        {
            foreach (var function in functions) _queue.Enqueue(function);
        }

        public Matrix<T> Compute(Matrix<T> input)
        {
            while (_queue.Count > 0)
            {
                input = _queue.Dequeue().Apply(input);
            }
            return input;
        }

        public async Task<Matrix<T>> ComputeAsync(Matrix<T> input)
        {
            while (_queue.Count > 0)
            {
                input = await Task.Factory.StartNew(
                    () => _queue.Dequeue().Apply(input)
                );
            }
            return input;
        }
    }
}
