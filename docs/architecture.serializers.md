# Architecture — JSON Serializers

> Custom `System.Text.Json` converters for model types.

## Converters

| Type | File | Serializes |
|---|---|---|
| `MultilanguageStringConverter` | `Serializers/Json/MultilanguageStringConverter.cs` | `MultilanguageString` |
| `DimensionConverter` | `Serializers/Json/DimensionConverter.cs` | `Dimension` and subtypes |
| `MetaPropertyConverter` | `Serializers/Json/MetaPropertyConverter.cs` | `MetaProperty` subtypes |
| `ContentValueListConverter` | `Serializers/Json/ContentValueListConverter.cs` | Content dimension value lists |
| `ValueListConverter` | `Serializers/Json/ValueListConverter.cs` | Generic value lists |

### Helpers

`ExtensionMethods.cs` — `JsonElement.GetProperty` with case-insensitive support.
