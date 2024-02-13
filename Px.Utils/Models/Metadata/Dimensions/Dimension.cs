using PxUtils.Language;
using PxUtils.Models.Metadata.Enums;

namespace PxUtils.Models.Metadata.Dimensions
{
    public class Dimension(string code, MultilanguageString name, List<Property> additionalProperties, List<DimensionValue> values, DimensionType type) : IDimension
    {
        public string Code { get; } = code;

        public DimensionType Type { get; } = type;

        public MultilanguageString Name { get; } = name;

        public List<Property> AdditionalProperties { get; } = additionalProperties;

        public IReadOnlyList<DimensionValue> Values { get; } = values;

        public string? DefaultValueCode { get; }

        #region Interface implementations

        IReadOnlyMultilanguageString IReadOnlyDimension.Name => Name;

        IReadOnlyList<Property> IReadOnlyDimension.AdditionalProperties => AdditionalProperties;

        IReadOnlyList<IReadOnlyDimensionValue> IReadOnlyDimension.Values => Values;

        #endregion

    }
}
