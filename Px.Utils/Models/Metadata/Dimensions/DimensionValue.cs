using PxUtils.Language;

namespace PxUtils.Models.Metadata.Dimensions
{
    public class DimensionValue(string code, MultilanguageString name) : IReadOnlyDimensionValue
    {
        public string Code { get; } = code;

        public MultilanguageString Name { get; } = name;

        public Dictionary<string, Property> AdditionalProperties { get; } = [];

        #region Interface implementations

        MultilanguageString IReadOnlyDimensionValue.Name => Name;

        #endregion
    }
}
