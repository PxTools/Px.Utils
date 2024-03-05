using PxUtils.Language;

namespace PxUtils.Models.Metadata.Dimensions
{
    public interface IDimension : IReadOnlyDimension
    {
        new MultilanguageString Name { get; }

        new Dictionary<string, Property> AdditionalProperties { get; }

        new IReadOnlyList<DimensionValue> Values { get; }

        new DimensionValue? DefaultValue { get; }
    }
}
