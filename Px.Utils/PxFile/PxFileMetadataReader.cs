
using Px.Utils.PxFile.Exceptions;
using PxUtils.PxFile.MetadataUtility;
using System.Text;

namespace PxUtils.PxFile.Meta
{
    public class PxFileMetadataReader(PxFileStreamFactory pxFileStreamFactory, PxFileMetaReaderConfig config) : IPxFileMetadataReader
    {
        private readonly PxFileStreamFactory _pxFileStreamFactory = pxFileStreamFactory;
        private Encoding? _encoding;

        public IEnumerable<KeyValuePair<string, string>> ReadMetadata()
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<KeyValuePair<string, string>> ReadMetadataAsync()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyDictionary<string, string> ReadMetadataToDictionary()
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyDictionary<string, string>> ReadMetadataToDictionaryAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously determines the encoding of the file managed by the PxFileStreamFactory.
        /// The method first checks for the presence of a Byte Order Mark (BOM) to identify UTF-8, UTF-16, and UTF-32 encodings.
        /// If no BOM is found, it reads the file's metadata to find the 'CODEPAGE' keyword and uses its value to determine the encoding.
        /// If the 'CODEPAGE' keyword is not found or the encoding it specifies is not available, an exception is thrown.
        /// </summary>
        /// <returns>The detected Encoding of the file.</returns>
        /// <exception cref="PxFileStreamException">Thrown when the 'CODEPAGE' keyword is not found in the file's metadata or the specified encoding is not available.</exception>
        private async Task<Encoding> GetEncodingAsync()
        {
            if (_encoding is not null) return _encoding;

            using Stream stream = _pxFileStreamFactory.OpenStream();

            stream.Position = 0;

            byte[] bom = new byte[3];
            await stream.ReadAsync(bom.AsMemory(0, 3));

            // UTF8 BOM
            if (bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
            {
                _encoding = Encoding.UTF8;
                return _encoding;
            }

            // UTF16 BOM
            else if (bom[0] == 0xFE && bom[1] == 0xFF)
            {
                _encoding = Encoding.Unicode;
                return _encoding;
            }

            // UTF32 BOM
            else if (bom[0] == 0x00 && bom[1] == 0x00 && bom[2] == 0xFE)
            {
                _encoding = Encoding.UTF32;
                return _encoding;
            }

            PxFileSymbolsConf symbolsConf = PxFileSymbolsConf.Default;
            KeyValuePair<string, string> encoding = await MetadataParsing
                .GetMetadataEntriesAsync(new StreamReader(stream, Encoding.ASCII), symbolsConf)
                .FirstOrDefaultAsync(kvp => kvp.Key == symbolsConf.Symbols.KeyWords.CodePage);

            if (encoding.Value is null) throw new PxFileStreamException($"Could not find CODEPAGE keyword in file {_pxFileStreamFactory.FilePath}.");

            string encodingName = encoding.Value.Trim(symbolsConf.Tokens.Value.StringDelimeter);

            bool isAvailable = Array.Exists(Encoding.GetEncodings(), e => e.Name == encodingName);
            if (!isAvailable) Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            try
            {
                return Encoding.GetEncoding(encodingName);
            }
            catch (ArgumentException aExp)
            {
                throw new PxFileStreamException($"The encoding {encodingName} provided with the CODEPAGE keyword is not available", aExp);
            }
        }
    }
}
