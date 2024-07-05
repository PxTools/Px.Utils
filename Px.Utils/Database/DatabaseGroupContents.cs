
namespace Px.Utils.Database
{
    /// <summary>
    /// Container for grouped items in a database.
    /// </summary>
    /// <param name="groups">Subgroups in the group.</param>
    /// <param name="tables">Tables directly in the group.</param>
    public class DatabaseGroupContents(IReadOnlyList<DatabaseGroupHeader> groups, IReadOnlyList<PxTableReference> tables)
    {
        /// <summary>
        /// Groups directrly contained in this group.
        /// </summary>
        public IReadOnlyList<DatabaseGroupHeader> SubGroups { get; } = groups;

        /// <summary>
        /// Tables directly contained in this group.
        /// </summary>
        public IReadOnlyList<PxTableReference> Tables { get; } = tables;
    }
}
