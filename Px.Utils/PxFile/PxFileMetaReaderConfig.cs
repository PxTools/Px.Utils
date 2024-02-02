using System.Text;

namespace PxUtils.PxFile.Meta
{
    public class PxFileMetaReaderConfig(Encoding encoding)
    {
        public Encoding Encoding { get; set; } = encoding;

        public int ReadBufferSize { get; set; } = 2048;
    }
}
