using PxUtils.Language;

namespace PxUtils.Models.Metadata.Dimensions
{
    public interface IDimension : IReadOnlyDimension
    {
        new MultilanguageString Name { get; }

        new List<Property> AdditionalProperties { get; }

        new IReadOnlyList<DimensionValue> Values { get; }
    }
}
