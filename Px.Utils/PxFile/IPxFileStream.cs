namespace PxUtils.PxFile
{
    public interface IPxFileStream : IDisposable
    {
        string Name { get; }

        long Length { get; }

        long Position { get; set; }

        Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellation = default);

        ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellation = default);
    }
}
