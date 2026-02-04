namespace Px.Utils.UnitTests.BinaryData
{
    internal sealed class CountingSeekableStream(byte[] data) : Stream
    {
        private readonly MemoryStream _inner = new(data, writable: false);
        private int _currentWindowBytes;

        public List<long> SeekOffsets { get; } = [];

        public long SeekForSetup(long offset, SeekOrigin origin)
        {
            return _inner.Seek(offset, origin);
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => _inner.Length;
        public override long Position { get => _inner.Position; set => _inner.Position = value; }

        public override void Flush() { }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = _inner.Read(buffer, offset, count);
            _currentWindowBytes += read;
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (_currentWindowBytes > 0)
            {
                _currentWindowBytes = 0;
            }
            SeekOffsets.Add(offset);
            return _inner.Seek(offset, origin);
        }

        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            int read = _inner.Read(buffer.Span);
            _currentWindowBytes += read;
            return ValueTask.FromResult(read);
        }
    }

    internal sealed class NonSeekableReadOnlyStream(byte[] data) : Stream
    {
        private readonly MemoryStream _inner = new(data, writable: false);

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => _inner.Length;
        public override long Position { get => _inner.Position; set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(_inner.Read(buffer.Span));
        }
    }
}
