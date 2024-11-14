using System.Text.Json.Serialization;

namespace Px.Utils.Models.Metadata
{
    /// <summary>
    /// Describes the structure of a matrix.
    /// </summary>
    public interface IMatrixMap
    {
        [JsonIgnore]
        IReadOnlyList<IDimensionMap> DimensionMaps { get; }
    }

    /// <summary>
    /// Describes the structure of a dimension.
    /// </summary>
    public interface IDimensionMap
    {
        /// <summary>
        /// Unique code among the dimensions of the matrix.
        /// </summary>
        public string Code { get; }


        /// <summary>
        /// Codes of the values of the dimension.
        /// </summary>
        public IReadOnlyList<string> ValueCodes { get; }
    }
}