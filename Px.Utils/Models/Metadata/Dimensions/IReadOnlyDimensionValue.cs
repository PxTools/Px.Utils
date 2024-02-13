using PxUtils.Language;

namespace PxUtils.Models.Metadata.Dimensions
{
    public interface IReadOnlyDimensionValue
    {
        string Code { get; }

        IReadOnlyMultilanguageString Name { get; }
    }
}
