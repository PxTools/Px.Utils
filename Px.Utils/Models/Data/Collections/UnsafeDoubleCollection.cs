using System.Collections;

namespace Px.Utils.Models.Data.Collections
{
    public class UnsafeDoubleCollection(double[] values) : IDataCollection<double>
    {
        private readonly double[] _values = values;

        public double this[int index] => _values[index];

        public int Count => _values.Length;

        public IEnumerator<double> GetEnumerator() => ((IEnumerable<double>)_values).GetEnumerator();

        public double[] ToArray() => [.. _values];

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
