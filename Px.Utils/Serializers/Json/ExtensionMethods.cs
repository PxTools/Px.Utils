using System.Text.Json;

namespace Px.Utils.Serializers.Json
{
    internal static class ExtensionMethods
    {
        /// <summary>
        /// Extension method to enable case insensitive property name lookup.
        /// </summary>
        internal static JsonElement GetProperty(this JsonElement root, string propertyName, JsonSerializerOptions options)
        {
            if (options.PropertyNameCaseInsensitive)
            {
                return root.EnumerateObject()
                    .FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                    .Value;
            }
            else
            {
                return root.GetProperty(propertyName);
            }
        }
    }
}
