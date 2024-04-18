namespace Px.Utils.Models.Metadata
{
    public class MatrixMap(List<MatrixMap.DimensionMap> dimensionMaps)
    {
        public class DimensionMap(string code, List<string> valueCodes)
        {
            public string Code { get; } = code;
            public List<string> ValueCodes { get; } = valueCodes;
        }

        public List<DimensionMap> DimensionMaps { get; } = dimensionMaps;
    }
}
