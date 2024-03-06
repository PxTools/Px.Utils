namespace PxUtils.Models.Metadata.Enums
{
    /// <summary>
    /// Enum for all possible dimension types
    /// </summary>
    public enum DimensionType
    {
        /// <summary>
        /// Year, month, day, hour, minute, second, etc.
        /// </summary>
        Time,
        
        /// <summary>
        /// Defines the characteristics of the data
        /// </summary>
        Content,
        
        /// <summary>
        /// Defines the geographical location of the data
        /// </summary>
        Geographical,
        
        /// <summary>
        /// The values of the dimension have a natural order
        /// </summary>
        Ordinal,
        
        /// <summary>
        /// The values of the dimension do not have a natural order
        /// </summary>
        Nominal,
        
        /// <summary>
        /// When none of the other types apply
        /// </summary>
        Other,
        
        /// <summary>
        /// When the type of the dimension is unknown
        /// </summary>
        Unknown
    }
}
