using System.Linq;

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

        /// <summary>
        /// Returns a union <see cref="IMatrixMap"/> of this map and other map that contains all values of each of them
        /// </summary>
        /// <remarks>Intended use of this extension method assumes that both maps are of the same base group.</remarks>
        /// <exception cref="InvalidOperationException">Thrown if either map contains dimensions not included in the other map.</exception>
        /// <returns>Combined union map with all values from both maps.</returns>
        public static IMatrixMap UnionMap(this IMatrixMap thisMap, IMatrixMap other)
        {
            if (thisMap.DimensionMaps.Count != other.DimensionMaps.Count)
            {
                throw new InvalidOperationException("Maps must have the same number of dimensions.");
            }

            List<IDimensionMap> unionDimensions = [];

            for (int i = 0; i < thisMap.DimensionMaps.Count; i++)
            {
                IDimensionMap thisDim = thisMap.DimensionMaps[i];
                IDimensionMap otherDim = other.DimensionMaps[i];

                if (thisDim.Code != otherDim.Code)
                {
                    throw new InvalidOperationException($"Dimension codes at position {i} do not match: '{thisDim.Code}' vs '{otherDim.Code}'.");
                }

                // Combine all values preserving order: first from thisMap, then new ones from other
                List<string> allValues = [.. thisDim.ValueCodes];
                allValues.AddRange(otherDim.ValueCodes.Where(valueCode => !allValues.Contains(valueCode)));
                unionDimensions.Add(new DimensionMap(thisDim.Code, allValues));
            }

            return new MatrixMap(unionDimensions);
        }

        ///<summary>
        /// Returns an intersection <see cref="IMatrixMap"/> of this map and other map that contains only the common values of each of them
        /// </summary>
        /// <remarks>Intended use of this extension method assumes that both maps are of the same base group.</remarks>
        /// <exception cref="InvalidOperationException">Thrown if either map contains dimensions not included in the other map.</exception>
        /// <returns>Combined intersection map with only the common values from both maps.</returns>
        public static IMatrixMap IntersectionMap(this IMatrixMap thisMap, IMatrixMap other)
        {
            if (thisMap.DimensionMaps.Count != other.DimensionMaps.Count)
            {
                throw new InvalidOperationException("Maps must have the same number of dimensions.");
            }

            List<IDimensionMap> intersectedDimensions = [];

            for (int i = 0; i < thisMap.DimensionMaps.Count; i++)
            {
                IDimensionMap thisDim = thisMap.DimensionMaps[i];
                IDimensionMap otherDim = other.DimensionMaps[i];

                if (thisDim.Code != otherDim.Code)
                {
                    throw new InvalidOperationException($"Dimension codes at position {i} do not match: '{thisDim.Code}' vs '{otherDim.Code}'.");
                }

                // Find common values preserving order from thisMap
                List<string> commonValues = [];
                commonValues.AddRange(thisDim.ValueCodes.Where(valueCode => otherDim.ValueCodes.Contains(valueCode)));
                intersectedDimensions.Add(new DimensionMap(thisDim.Code, commonValues));
            }

            return new MatrixMap(intersectedDimensions);
        }

        /// <summary>
        /// Computes index mappings from the <paramref name="subMap"/> to the current (super) map for each dimension.
        /// </summary>
        /// <param name="thisMap">The super map that contains all dimensions and values.</param>
        /// <param name="subMap">The sub map whose value codes exist in the same order within the super map.</param>
        /// <returns>
        /// A jagged array where each element corresponds to a dimension; for each dimension it contains an array of
        /// indices mapping the position of each value in <paramref name="subMap"/> to its index in the corresponding
        /// dimension of <paramref name="thisMap"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the number of dimensions differs between <paramref name="subMap"/> and <paramref name="thisMap"/>.
        /// </exception>
        /// <remarks>
        /// This method assumes that <paramref name="subMap"/> is a valid submap of <paramref name="thisMap"/>,
        /// meaning that for every dimension, the sequence of value codes in <paramref name="subMap"/> appears in the
        /// same order within the corresponding dimension of <paramref name="thisMap"/>. If a value from
        /// <paramref name="subMap"/> does not exist in <paramref name="thisMap"/>, the behavior is undefined.
        /// Consider validating with <see cref="IsSubmapOf(IMatrixMap, IMatrixMap)"/> before calling.
        /// </remarks>
        public static int[][] GetIndicesOfSubmap(this IMatrixMap thisMap, IMatrixMap subMap)
        {
            if (subMap.DimensionMaps.Count != thisMap.DimensionMaps.Count)
            {
                throw new ArgumentException("Number of dimensions differ between source and target maps, index mapping can not be computed.");
            }

            int[][] result = new int[subMap.DimensionMaps.Count][];

            for (int dimIndx = 0; dimIndx < subMap.DimensionMaps.Count; dimIndx++)
            {
                IReadOnlyList<string> subDimCodes = subMap.DimensionMaps[dimIndx].ValueCodes;
                IReadOnlyList<string> superDimCodes = thisMap.DimensionMaps[dimIndx].ValueCodes;

                int subValIndex = 0;
                int[] valIndices = new int[subDimCodes.Count];
                for (int superValIndex = 0; superValIndex < superDimCodes.Count; superValIndex++)
                {
                    if (subDimCodes[subValIndex] == superDimCodes[superValIndex])
                    {
                        valIndices[subValIndex] = superValIndex;
                        subValIndex++;
                        if (subValIndex >= subDimCodes.Count) break;
                    }
                }
                result[dimIndx] = valIndices;
            }
            return result;
        }
    }
}
