namespace Px.Utils.Models.Metadata.ExtensionMethods
{
    public static class MatrixMapExtensions
    {
        /// <summary>
        /// Returns the total number of cells in the matrix described by the matrix map.
        /// </summary>
        /// <param name="matrixMap">The matrix map.</param>
        /// <returns>Total number of cells in the matrix described by the matrix map.</returns>
        public static long GetSize(this IMatrixMap matrixMap)
        {
            long numberOfCells = 1;
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
        /// Checks if the other map contains the dimensions and values of this map in the same order.
        /// The dimensions of the other map can contain values that are not found in this map.
        /// </summary>
        /// <returns>True if all of the values are found in the correct order.</returns>
        public static bool IsSubmapOf(this IMatrixMap thisMap, IMatrixMap other)
        {
            if(thisMap.DimensionMaps.Count != other.DimensionMaps.Count) return false;
            for (int i = 0; i < thisMap.DimensionMaps.Count; i++)
            {
                if (!thisMap.DimensionMaps[i].IsSubmapOf(other.DimensionMaps[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Checks if this map contains the dimensions and values of the other map in the same order.
        /// The dimension of this map can contain values that are not found in the other map.
        /// </summary>
        /// <returns>True if all values are found in the correct order.</returns>
        public static bool IsSupermapOf(this IMatrixMap thisMap, IMatrixMap other)
        {
            return other.IsSubmapOf(thisMap);
        }

        /// <summary>
        /// Determines whether the current <see cref="IMatrixMap"/> is identical to the specified <paramref
        /// name="other"/> map.
        /// </summary>
        /// <remarks>Two <see cref="IMatrixMap"/> instances are considered identical if they have the same
        /// number of dimension mappings and each corresponding dimension mapping is also identical.</remarks>
        /// <returns>
        /// <see langword="true"/> if the current map is identical to the specified map, including all dimension
        /// mappings; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsIdenticalMapTo(this IMatrixMap thisMap, IMatrixMap other)
        {
            if (thisMap.DimensionMaps.Count != other.DimensionMaps.Count) return false;
            for (int i = 0; i < thisMap.DimensionMaps.Count; i++)
            {
                if (!thisMap.DimensionMaps[i].IsIdenticalMapTo(other.DimensionMaps[i]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
