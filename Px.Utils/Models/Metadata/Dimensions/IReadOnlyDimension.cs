using PxUtils.Language;
using PxUtils.Models.Metadata.Enums;

namespace PxUtils.Models.Metadata.Dimensions
{
    public interface IReadOnlyDimension
    {
        string Code { get; }

        DimensionType Type { get; }

        IReadOnlyMultilanguageString Name { get; }

        IReadOnlyList<Property> AdditionalProperties { get; }

        IReadOnlyList<IReadOnlyDimensionValue> Values { get; }

        string? DefaultValueCode { get; }
    }
}
