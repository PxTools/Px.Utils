using Px.Utils.PxFile.Exceptions;
using PxUtils.PxFile.MetadataUtility;
using System.Text;

namespace PxUtils.PxFile
{
    public sealed class PxFileStream : IDisposable
    {
        private Encoding? _encoding;
        private readonly FileStream _stream;

        public PxFileStream(string pathToPxFile)
        {
            _stream = new FileStream(pathToPxFile, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _stream.Dispose();
        }

        private async Task<Encoding> GetEncodingAsync()
        {
            if(_encoding is not null) return _encoding;

            long oldPostiion = _stream.Position;
            byte[] bom = new byte[3];
            _stream.Position = 0;
            await _stream.ReadAsync(bom, 0, 3);
            _stream.Position = oldPostiion;

            // UTF8 BOM
            if (bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF) return Encoding.UTF8;

            // UTF16 BOM
            else if (bom[0] == 0xFE && bom[1] == 0xFF) return Encoding.Unicode;

            // UTF32 BOM
            else if (bom[0] == 0x00 && bom[1] == 0x00 && bom[2] == 0xFE) return Encoding.UTF32;

            PxFileSymbolsConf symbolsConf = PxFileSymbolsConf.Default;

            KeyValuePair<string, string> encoding = await MetadataParsing
                .GetMetadataEntriesAsync(new StreamReader(_stream, Encoding.ASCII), symbolsConf)
                .FirstOrDefaultAsync(kvp => kvp.Key == symbolsConf.Symbols.KeyWords.CodePage);

            if(encoding.Value is null) throw new PxFileStreamException($"Could not find CODEPAGE keyword in file {_stream.Name}.");

            string encodingName = encoding.Value.Trim(symbolsConf.Tokens.Value.StringDelimeter);

            bool isAvailable = Array.Exists(Encoding.GetEncodings(), e => e.Name == encodingName);
            if(!isAvailable) Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

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