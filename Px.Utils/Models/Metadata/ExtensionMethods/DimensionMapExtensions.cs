namespace Px.Utils.Models.Metadata.ExtensionMethods
{
    public static class DimensionMapExtensions
    {
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

        public static bool IsSupermapOf(this IDimensionMap thisMap, IDimensionMap other)
        {
            return other.IsSubmapOf(thisMap);
        }

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
