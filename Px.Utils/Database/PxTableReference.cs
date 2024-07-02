
using System.Diagnostics.CodeAnalysis;

namespace Px.Utils.Database
{
    /// <summary>
    /// Reference to a px-file that contains a unique identifier of the file and the last write time.
    /// </summary>
    [ExcludeFromCodeCoverage] // No implementation logic
    public readonly record struct PxTableReference
    {
        /// <summary>
        /// Identifies the file
        /// </summary>
        public readonly string Identifier { get; }

        /// <summary>
        /// When whas the file last written to.
        /// </summary>
        public readonly DateTime TableLastUpdated { get; }

        /// <summary>
        /// Creation time of the reference.
        /// </summary>
        public readonly DateTime ReferenceCreated { get; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="identifier">Unique identifier of the file.</param>
        /// <param name="tableLastUpdated">When was the table last written to.</param>
        public PxTableReference(string identifier, DateTime tableLastUpdated)
        {
            Identifier = identifier;
            TableLastUpdated = tableLastUpdated;
            ReferenceCreated = DateTime.Now;
        }
    }
}
