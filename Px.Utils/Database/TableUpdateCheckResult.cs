
namespace Px.Utils.Database
{
    public readonly struct TableUpdateCheckResult(bool hasBeenUpdated, PxTableReference? newReference)
    {
        public readonly bool HasBeenUpdated { get; } = hasBeenUpdated;

        public readonly PxTableReference? NewReference { get; } = newReference;
    }
}
