using Px.Utils.Models;
using Px.Utils.Models.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Px.Utils.Operations
{
    public class TransformationMatrixFunction<TData>(IMatrixMap targetMap) : IMatrixFunction<TData>
    {
        private readonly IMatrixMap _targetMap = targetMap;

        public Matrix<TData> Apply(Matrix<TData> input)
        {
            return input.GetTransform(_targetMap);
        }
    }
}
