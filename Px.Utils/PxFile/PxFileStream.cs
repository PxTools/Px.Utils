using System.Diagnostics.CodeAnalysis;

namespace PxUtils.PxFile
{
    /// <summary>
    /// A wrapper around <see cref="System.IO.FileStream"/> that implements <see cref="IPxFileStream"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PxFileStream(string path, FileMode mode, FileAccess access, FileShare share)
        : FileStream(path, mode, access, share), IPxFileStream { }
}