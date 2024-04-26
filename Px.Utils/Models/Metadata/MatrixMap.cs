namespace Px.Utils.Models.Metadata
{
    /// <summary>
    /// Minimal representation of a matrix.
    /// Can be used to map a matrix from one structure to another.
    /// </summary>
    /// <param name="dimensionMaps">Maps for each dimension in the matrix</param>
    public class MatrixMap(List<DimensionMap> dimensionMaps) : IMatrixMap
    {
        public List<DimensionMap> DimensionMaps { get; } = dimensionMaps;

        IReadOnlyList<IDimensionMap> IMatrixMap.DimensionMaps => DimensionMaps;
    }

    /// <summary>
    /// Minimal representation of a dimension.
    /// Can be used to map a dimension from one matrix to another.
    /// </summary>
    /// <param name="code">Unique code among the dimensions of the metadata matrix</param>
    /// <param name="valueCodes">Unique codes among the values of the dimension</param>
    public class DimensionMap(string code, List<string> valueCodes) : IDimensionMap
    {
        public string Code { get; } = code;
        public IReadOnlyList<string> ValueCodes { get; } = valueCodes;
    }
}
