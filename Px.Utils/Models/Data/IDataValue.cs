using System.Numerics;

namespace Px.Utils.Models.Data
{
    /// <summary>
    /// Interface for wrapping the basic arithmetic operations for a data value.
    /// </summary>
    /// <typeparam name="TSelf"></typeparam>
    public interface IDataValue<TSelf> :
          IAdditionOperators<TSelf, TSelf, TSelf>,
          IAdditiveIdentity<TSelf, TSelf>,
          IDivisionOperators<TSelf, TSelf, TSelf>,
          IEquatable<TSelf>,
          IEqualityOperators<TSelf, TSelf, bool>,
          IMultiplicativeIdentity<TSelf, TSelf>,
          IMultiplyOperators<TSelf, TSelf, TSelf>,
          ISubtractionOperators<TSelf, TSelf, TSelf>,
          IUnaryPlusOperators<TSelf, TSelf>,
          IUnaryNegationOperators<TSelf, TSelf>
        where TSelf : IDataValue<TSelf>
    { }
}
