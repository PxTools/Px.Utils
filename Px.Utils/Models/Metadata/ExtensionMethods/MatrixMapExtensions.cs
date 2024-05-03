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
    }
}
