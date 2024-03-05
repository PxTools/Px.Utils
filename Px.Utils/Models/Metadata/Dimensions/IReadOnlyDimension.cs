using PxUtils.Language;
using PxUtils.Models.Metadata.Enums;

namespace PxUtils.Models.Metadata.Dimensions
{
    public interface IReadOnlyDimension
    {
        string Code { get; }

        DimensionType Type { get; }

        MultilanguageString Name { get; }

        IReadOnlyDictionary<string, Property> AdditionalProperties { get; }

        IReadOnlyList<IReadOnlyDimensionValue> Values { get; }

        IReadOnlyDimensionValue? DefaultValue { get; }
    }
}
