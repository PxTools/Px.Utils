using PxUtils.Language;
using PxUtils.Models.Metadata.Enums;

namespace PxUtils.Models.Metadata
{
    public class Dimension(string code, MultilanguageString name, List<Property> additionalProperties, DimensionType type) : IReadOnlyDimension
    {
        public string Code { get; } = code;

        public DimensionType Type { get; } = type;

        public MultilanguageString Name { get; } = name;

        public List<Property> AdditionalProperties { get; } = additionalProperties;

        #region Interface implementations

        IReadOnlyMultilanguageString IReadOnlyDimension.Name => Name;

        #endregion

    }
}
