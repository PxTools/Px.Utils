
namespace PxUtils.PxFile
{
    public interface IPxFileStreamFactory
    {
        IPxFileStream OpenPxFileStream();
        Stream OpenStream();
    }
}