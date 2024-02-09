namespace Px.Utils.PxFile.Exceptions
{
    public class InvalidPxFileMetadataException : Exception
    {
        public InvalidPxFileMetadataException() { }

        public InvalidPxFileMetadataException(string message) : base(message) { }

        public InvalidPxFileMetadataException(string message, Exception innerException) : base(message, innerException) { }
    }
}
