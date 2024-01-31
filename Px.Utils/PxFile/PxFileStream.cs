using PxUtils.PxFile;
using System.Text;

namespace Px.Utils.PxFile
{
    public class PxFileStream : IPxFileStream
    {
        public Encoding Encoding { get; }

        private readonly FileStream _fileStream;

        public PxFileStream(FileStream fileStream, Encoding encoding)
        {
            Encoding = encoding;
            _fileStream = fileStream;
        }
    }
}
