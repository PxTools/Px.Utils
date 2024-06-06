using Px.Utils.Aggregations.SerializationModels.ValueFilters;
using Px.Utils.Language;
using Px.Utils.Models.Metadata;

namespace Px.Utils.Aggregations.SerializationModels
{
    public class ValueSum
    {
        public NewValueInfo NewValue { get; set; }
        public ValueFilter Filter { get; set; }

        public class NewValueInfo
        {
            public string Code { get; set; }
            public MultilanguageString Name { get; set; }
            public List<MetaProperty> AdditionalProperties { get; set; }
        }
    }
}
