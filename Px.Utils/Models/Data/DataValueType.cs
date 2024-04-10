namespace PxUtils.Models.Data
{
    /// <summary>
    /// Type of a singular data value in a px file.
    /// </summary>
    public enum DataValueType : byte
    {
        Exists = 0,
        Missing = 1,
        CanNotRepresent = 2,
        Confidential = 3,
        NotAcquired = 4,
        NotAsked = 5,
        Empty = 6,
        Nill = 7,
    }
}
