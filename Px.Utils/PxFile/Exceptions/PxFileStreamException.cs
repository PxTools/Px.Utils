namespace Px.Utils.PxFile.Exceptions
{
    public class PxFileStreamException : Exception
    {
        public PxFileStreamException() { }

        public PxFileStreamException(string message) : base(message) { }

        public PxFileStreamException(string message, Exception innerException) : base(message, innerException) { }
    }
}
