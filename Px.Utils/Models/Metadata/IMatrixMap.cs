using System.Text.Json.Serialization;

namespace Px.Utils.Models.Metadata
{
    public interface IMatrixMap
    {
        [JsonIgnore]
        IReadOnlyList<IDimensionMap> DimensionMaps { get; }
    }

    public interface IDimensionMap
    {
        /// <summary>
        /// Unique code among the dimensions of the matrix.
        /// </summary>
        public string Code { get; }


        public IReadOnlyList<string> ValueCodes { get; }
    }
}