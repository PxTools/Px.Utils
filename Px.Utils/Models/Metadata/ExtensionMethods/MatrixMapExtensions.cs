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
    }
}
