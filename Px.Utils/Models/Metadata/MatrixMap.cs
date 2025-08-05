namespace Px.Utils.Models.Metadata
{
    /// <summary>
    /// Minimal representation of a matrix.
    /// Can be used to map a matrix from one structure to another.
    /// </summary>
    /// <param name="dimensionMaps">Maps for each dimension in the matrix</param>
    public class MatrixMap(List<IDimensionMap> dimensionMaps) : IMatrixMap
    {
        public List<DimensionMap> DimensionMaps { get; } = [.. dimensionMaps.Select(dm => new DimensionMap(dm))];

        IReadOnlyList<IDimensionMap> IMatrixMap.DimensionMaps => DimensionMaps;
    }

    /// <summary>
    /// Minimal representation of a dimension.
    /// Can be used to map a dimension from one matrix to another.
    /// </summary>
    public class DimensionMap : IDimensionMap
    {
        public string Code { get; }
        public IReadOnlyList<string> ValueCodes { get; }

        public DimensionMap(string code, List<string> valueCodes)
        {
            Code = code;
            ValueCodes = valueCodes;
        }

        public DimensionMap(IDimensionMap dimensionMap)
        {
            Code = dimensionMap.Code;
            ValueCodes = [.. dimensionMap.ValueCodes];
        }
    }
}
