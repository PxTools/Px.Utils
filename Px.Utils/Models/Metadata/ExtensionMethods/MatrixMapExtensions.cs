namespace Px.Utils.Models.Metadata.ExtensionMethods
{
    public static class MatrixMapExtensions
    {
        /// <summary>
        /// Returns the total number of cells in the matrix described by the matrix map.
        /// </summary>
        /// <param name="matrixMap">The matrix map.</param>
        /// <returns>Total number of cells in the matrix described by the matrix map.</returns>
        public static int GetSize(this IMatrixMap matrixMap)
        {
            int numberOfCells = 1;
            foreach (IDimensionMap dimensionMap in matrixMap.DimensionMaps)
            {
                numberOfCells *= dimensionMap.ValueCodes.Count;
            }
            return numberOfCells;
        }

        /// <summary>
        /// Returns a copy of the map where the dimension with the given code is collapsed to a single value.
        /// </summary>
        /// <param name="matrixMap">The source matrix map.</param>
        /// <param name="dimensionCode">Code of the dimension to collapse.</param>
        /// <param name="valueCode">The value code to collapse the dimension to.</param>
        /// <returns>New matrix map with the specified dimension collapsed to a single value.</returns>
        public static IMatrixMap CollapseDimension(this IMatrixMap matrixMap, string dimensionCode, string valueCode)
        {
            List<IDimensionMap> newDimensionMaps = [];
            foreach (IDimensionMap dimensionMap in matrixMap.DimensionMaps)
            {
                if (dimensionMap.Code != dimensionCode) newDimensionMaps.Add(dimensionMap);
                else newDimensionMaps.Add(new DimensionMap(dimensionCode, [valueCode]));
            }
            return new MatrixMap(newDimensionMaps);
        }

        /// <summary>
        /// Checks if the other map contains the same dimensions and values as this map in the same order.
        /// The other map can contain values that are not found in this map.
        /// </summary>
        /// <returns>True if all of the values are found in the correct order.</returns>
        public static bool IsSubmapOf(this IMatrixMap thisMap, IMatrixMap other)
        {
            if(thisMap.DimensionMaps.Count != other.DimensionMaps.Count) return false;
            for (int i = 0; i < thisMap.DimensionMaps.Count; i++)
            {
                if (other.DimensionMaps[i].Code != thisMap.DimensionMaps[i].Code) return false;
                int sourceIndex = 0;
                for (int j = 0; j < thisMap.DimensionMaps[i].ValueCodes.Count; j++)
                {
                    bool found = false;
                    for(int k = sourceIndex; k < other.DimensionMaps[i].ValueCodes.Count; k++)
                    {
                        if (other.DimensionMaps[i].ValueCodes[k] == thisMap.DimensionMaps[i].ValueCodes[j])
                        {
                            sourceIndex = k + 1;
                            found = true;
                            break;
                        }
                    }
                    if(!found) return false;
                }
            }
            return true;
        }
    }
}
