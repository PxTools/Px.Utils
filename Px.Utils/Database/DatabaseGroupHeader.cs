using Px.Utils.Language;
using System.Diagnostics.CodeAnalysis;

namespace Px.Utils.Database
{
    /// <summary>
    /// Identity information of a grouping of items in the database.
    /// </summary>
    /// <param name="code">Unique identifier of the group.</param>
    /// <param name="name">Translated names of the group.</param>
    [ExcludeFromCodeCoverage] // No functionality
    public class DatabaseGroupHeader(string code, MultilanguageString name)
    {
        /// <summary>
        /// Unique identifier of the group.
        /// </summary>
        public string Code { get; } = code;

        /// <summary>
        /// Translated names of the group.
        /// </summary>
        public MultilanguageString Name { get; } = name;

        /// <summary>
        /// Additional properties of the grouping, can be used for various database implementations.
        /// </summary>
        public object? AditionalProperties { get; set; }
    }
}
