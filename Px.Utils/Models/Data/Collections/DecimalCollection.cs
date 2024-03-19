using PxUtils.Models.Data;
using PxUtils.Models.Data.DataValue;
using System.Collections;

namespace Px.Utils.Models.Data.Collections
{
    public class DecimalCollection(decimal[] values, DataValueType[] types) : IDataCollection<DecimalDataValue>
    {
        private readonly DataValueType[] _types = types;
        private readonly decimal[] _values = values;

        public DecimalDataValue this[int index] => new(_values[index], _types[index]);

        public int Count => _values.Length;

        public IEnumerator<DecimalDataValue> GetEnumerator()
        {
            int len = _values.Length;
            for (int i = 0; i < len; i++)
            {
                yield return new DecimalDataValue(_values[i], _types[i]);
            }
        }

        public DecimalDataValue[] ToArray()
        {
            int len = _values.Length;
            DecimalDataValue[] array = new DecimalDataValue[len];
            for (int i = 0; i < len; i++)
            {
                array[i] = new DecimalDataValue(_values[i], _types[i]);
            }
            return array;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}