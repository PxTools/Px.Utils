using PxUtils.Language;
using PxUtils.Models.Metadata.Enums;

namespace PxUtils.Models.Metadata
{
    public class Dimension(string code, MultilanguageString name, DimensionType type) : IReadOnlyDimension
    {
        public string Code { get; } = code;

        public DimensionType Type { get; } = type;

        public MultilanguageString Name { get; } = name;

        #region Interface implementations

        IReadOnlyMultilanguageString IReadOnlyDimension.Name => Name;

        #endregion

    }
}
