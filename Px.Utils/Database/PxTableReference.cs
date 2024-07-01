
namespace Px.Utils.Database
{
    public readonly record struct PxTableReference
    {
        public readonly string Identifier { get; }

        public readonly DateTime TableLastUpdated { get; }

        public readonly DateTime ReferenceCreated { get; }

        public PxTableReference(string identifier, DateTime tableLastUpdated, DateTime referenceCreated)
        {
            Identifier = identifier;
            TableLastUpdated = tableLastUpdated;
            ReferenceCreated = referenceCreated;
        }
    }
}
