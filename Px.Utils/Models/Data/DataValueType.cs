namespace PxUtils.Models.Data
{
    public enum DataValueType : byte
    {
        Exist = 1,
        Missing = 2,
        CannotRepresent = 4,
        Confidential = 8,
        NotAcquired = 16,
        NotAsked = 32,
        Empty = 64,
        Nill = 128,
    }
}
