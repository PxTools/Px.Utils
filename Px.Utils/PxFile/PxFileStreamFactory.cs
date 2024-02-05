namespace PxUtils.PxFile
{
    public class PxFileStreamFactory(string filePath) : IPxFileStreamFactory
    {
        public string FilePath { get; } = filePath;

        public IPxFileStream OpenPxFileStream()
        {
            return new PxFileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public Stream OpenStream()
        {
            return new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}
