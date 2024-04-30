using System.Numerics;

namespace Px.Utils.Models.Data
{
    public interface IDataValue<TSelf> :
          IAdditionOperators<TSelf, TSelf, TSelf>,
          IAdditiveIdentity<TSelf, TSelf>,
          IDecrementOperators<TSelf>,
          IDivisionOperators<TSelf, TSelf, TSelf>,
          IEquatable<TSelf>,
          IEqualityOperators<TSelf, TSelf, bool>,
          IIncrementOperators<TSelf>,
          IMultiplicativeIdentity<TSelf, TSelf>,
          IMultiplyOperators<TSelf, TSelf, TSelf>,
          ISubtractionOperators<TSelf, TSelf, TSelf>,
          IUnaryPlusOperators<TSelf, TSelf>,
          IUnaryNegationOperators<TSelf, TSelf>
        where TSelf : IDataValue<TSelf>
    { }
}
