namespace Px.Utils.BinaryData.ValueConverters
{
    /// <summary>
    /// Enumeration of supported binary value codec types.
    /// Explicit integer values start from 1 to allow stable mapping.
    /// </summary>
    public enum BinaryValueCodecType
    {
        UInt16Codec = 1,
        Int16Codec = 2,
        UInt24Codec = 3,
        Int24Codec = 4,
        UInt32Codec = 5,
        Int32Codec = 6,
        FloatCodec = 7,
        DoubleCodec = 8
    }
}
