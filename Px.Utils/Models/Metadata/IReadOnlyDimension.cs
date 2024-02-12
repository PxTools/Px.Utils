using PxUtils.Language;
using PxUtils.Models.Metadata.Enums;

namespace PxUtils.Models.Metadata
{
    public interface IReadOnlyDimension
    {
        string Code { get; }

        DimensionType Type { get; }

        IReadOnlyMultilanguageString Name { get; }
    }
}
