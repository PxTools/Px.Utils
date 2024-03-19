using PxUtils.Models.Data;
using PxUtils.Models.Data.DataValue;
using System.Collections;

namespace Px.Utils.Models.Data.Collections
{
    public class DoubleCollection(double[] values, DataValueType[] types) : IDataCollection<DoubleDataValue>
    {
        private readonly DataValueType[] _types = types;
        private readonly double[] _values = values;

        public DoubleDataValue this[int index] => new(_values[index], _types[index]);

        public int Count => _values.Length;

        public IEnumerator<DoubleDataValue> GetEnumerator()
        {
            int len = _values.Length;
            for (int i = 0; i < len; i++)
            {
                yield return new DoubleDataValue(_values[i], _types[i]);
            }
        }

        public DoubleDataValue[] ToArray()
        {
            int len = _values.Length;
            DoubleDataValue[] array = new DoubleDataValue[len];
            for (int i = 0; i < len; i++)
            {
                array[i] = new DoubleDataValue(_values[i], _types[i]);
            }
            return array;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
