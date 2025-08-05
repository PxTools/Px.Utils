namespace Px.Utils.Models.Metadata.ExtensionMethods
{
    /// <summary>
    /// Contains extension methods for <see cref="IDimensionMap"/>.
    /// </summary>
    public static class DimensionMapExtensions
    {
        /// <summary>
        /// Checks if the other dimension map has the same code and contains all the values of this map in the same order. 
        /// The other map can contain additional values that are not found in this map.
        /// </summary>
        /// <returns>True if the other map contains all values of this map in the same order. Otherwise, false.</returns>
        public static bool IsSubmapOf(this IDimensionMap thisMap, IDimensionMap other)
        {
            if (thisMap.Code != other.Code) return false;
            int sourceIndex = 0;
            for (int j = 0; j < thisMap.ValueCodes.Count; j++)
            {
                bool found = false;
                for (int k = sourceIndex; k < other.ValueCodes.Count; k++)
                {
                    if (other.ValueCodes[k] == thisMap.ValueCodes[j])
                    {
                        sourceIndex = k + 1;
                        found = true;
                        break;
                    }
                }
                if (!found) return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if this dimension map has the same code and contains all the values of the other map in the same order.
        /// </summary>
        /// <returns>True if this map contains all values of the other map in the same order. Otherwise, false.</returns>
        public static bool IsSupermapOf(this IDimensionMap thisMap, IDimensionMap other)
        {
            return other.IsSubmapOf(thisMap);
        }

        /// <summary>
        /// Checks if this dimension map is identical to the other dimension map.
        /// </summary>
        /// <returns>True if the code and all values are the same in the same order. Otherwise, false.</returns>
        public static bool IsIdenticalMapTo(this IDimensionMap thisMap, IDimensionMap other)
        {
            if (thisMap.Code != other.Code) return false;
            if (thisMap.ValueCodes.Count != other.ValueCodes.Count) return false;
            for (int i = 0; i < thisMap.ValueCodes.Count; i++)
            {
                if (thisMap.ValueCodes[i] != other.ValueCodes[i]) return false;
            }
            return true;
        }
    }
}
