
namespace Px.Utils.Database
{
    /// <summary>
    /// Contains information about table update status.
    /// </summary>
    /// <param name="hasBeenUpdated">True if the last write time of the table has been changed.</param>
    /// <param name="newReference">If the table has been updated the new reference needs to be provided.</param>
    public readonly struct TableUpdateCheckResult(bool hasBeenUpdated, PxTableReference? newReference)
    {
        /// <summary>
        /// True if the last write time of the table has been changed. False otherwice.
        /// </summary>
        public readonly bool HasBeenUpdated { get; } = hasBeenUpdated;

        /// <summary>
        /// A new reference to the table if the write time has baan changed.
        /// </summary>
        public readonly PxTableReference? NewReference { get; } = newReference;
    }
}
