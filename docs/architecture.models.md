# Architecture — Models

> Matrix, metadata, dimensions, properties, data values, and model builders.

## Core Types

| Type | Purpose |
|---|---|
| `Matrix<T>` | Container for metadata + data array; supports `GetTransform(IMatrixMap)` |
| `IReadOnlyMatrixMetadata` | Read-only metadata: languages, dimensions, additional properties |
| `IMatrixMap` / `IDimensionMap` | Structural map of matrix dimensions and value codes |
| `MatrixMetadata` | Mutable metadata implementation of `IReadOnlyMatrixMetadata` |
| `MatrixMap` | Implementation of `IMatrixMap` |
| `MultilanguageString` | Dictionary-backed multilingual string with JSON serialization support |

## Dimensions

| Type | Purpose |
|---|---|
| `Dimension` | Base dimension with code, name, and `ValueList` |
| `ContentDimension` | Dimension with `ContentDimensionValue` entries (units, precision) |
| `TimeDimension` | Dimension with time interval information |
| `DimensionValue` | Single value in a dimension |
| `ContentDimensionValue` | Value with unit and decimal precision |
| `ValueList` | Ordered list of dimension values |

Enums: `DimensionType`, `TimeDimensionInterval`

## Meta Properties

Abstract base: `MetaProperty` (`MetaPropertyType` enum).

| Subtype | Value type |
|---|---|
| `StringProperty` | `string` |
| `NumericProperty` | `decimal` |
| `BooleanProperty` | `bool` |
| `StringListProperty` | `IReadOnlyList<string>` |
| `MultilanguageStringProperty` | `MultilanguageString` |
| `MultilanguageStringListProperty` | `IReadOnlyList<MultilanguageString>` |

## Data Values

| Type | Purpose |
|---|---|
| `IDataValue` | Marker interface |
| `DoubleDataValue` | `double` value + `DataValueType` |
| `DecimalDataValue` | `decimal` value + `DataValueType` |
| `DataValueType` | Enum classifying value type |

## Model Building

| Interface | Implementation |
|---|---|
| `IMatrixMetadataBuilder` | `MatrixMetadataBuilder` |

```
MatrixMetadata Build(IEnumerable<KeyValuePair<string, string>> metadataInput)
MatrixMetadata Build(IReadOnlyDictionary<string, string> metadataInput)
Task<MatrixMetadata> BuildAsync(IAsyncEnumerable<KeyValuePair<string, string>> metadataInput)
```

Helpers: `MetadataEntryKeyBuilder`, `ValueParserUtilities`, `MetadataEntryKey`.

`PxFileConfiguration.TokenDefinitions.VariableTypeTokens` exposes configurable string arrays for each dimension type. The first value is the primary token used by builders, and any additional values are treated as aliases during parsing.

## Extension Methods

| File | Purpose |
|---|---|
| `MatrixMapExtensions` | Size calculations, dimension collapsing |
| `MatrixMetadataExtensions` | Metadata transforms and queries |
| `DimensionMapExtensions` | Dimension map utilities |
| `PropertyUtilities` | Property parsing and cleaning |

## Dependency Graph

```
Matrix<T>
├── IReadOnlyMatrixMetadata → MatrixMetadata
│   ├── Dimension / ContentDimension / TimeDimension
│   │   └── ValueList → DimensionValue / ContentDimensionValue
│   └── MetaProperty subtypes
└── DataIndexer (for GetTransform)

MatrixMetadataBuilder : IMatrixMetadataBuilder
└── PxFileConfiguration
```
