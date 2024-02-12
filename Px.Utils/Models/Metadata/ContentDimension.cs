using PxUtils.Language;
using PxUtils.Models.Metadata.Enums;

namespace PxUtils.Models.Metadata
{
    public class ContentDimension(string code, MultilanguageString name, List<Property> additionalProperties)
        : Dimension(code, name, additionalProperties, DimensionType.Content)
    {
    }
}
