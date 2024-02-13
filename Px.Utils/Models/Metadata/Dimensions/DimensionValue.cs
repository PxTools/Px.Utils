using PxUtils.Language;

namespace PxUtils.Models.Metadata.Dimensions
{
    public class DimensionValue(string code, MultilanguageString name) : IReadOnlyDimensionValue
    {
        public string Code { get; } = code;

        public MultilanguageString Name { get; } = name;

        #region Interface implementations

        IReadOnlyMultilanguageString IReadOnlyDimensionValue.Name => Name;

        #endregion
    }
}
