# Architecture — Overview

> Solution structure, configuration, patterns, dependencies, and file map.

## Solution

Library for reading, processing, and validating PX (PC-Axis) statistical file data and metadata.

| Project | Type | Target | Description |
|---|---|---|---|
| `Px.Utils` | Class library (NuGet) | `net10.0` | Core library — validation, models, readers, serializers, operations |
| `Px.Utils.TestingApp` | Console app | `net10.0` | Benchmarking CLI for performance testing |
| `Px.Utils.UnitTests` | Test project (MSTest) | `net10.0` | Unit tests for `Px.Utils` |

No DI container — all types instantiated directly via constructors. Configuration via `PxFileConfiguration`.

## Configuration

`PxFileConfiguration` — configurable symbols and tokens for PX file format. Access default via `PxFileConfiguration.Default`.

| Property path | Purpose | Default |
|---|---|---|
| `Symbols.Key.ListSeparator` | Separator in keys | `,` |
| `Symbols.Key.StringDelimeter` | String delimiter in keys | `"` |
| `Symbols.Value.StringDelimeter` | String delimiter in values | `"` |
| `Symbols.Value.ListSeparator` | Separator in values | `,` |
| `Symbols.KeywordSeparator` | Keyword = value separator | `=` |
| `Symbols.EntrySeparator` | Entry separator | `;` |
| `Tokens.KeyWords.Data` | Data section keyword | `DATA` |

## Key Patterns

- **No DI container** — manual wiring via constructors; primary constructors used extensively.
- **Configuration object** — `PxFileConfiguration` passed through constructors; `.Default` as fallback.
- **Sync + async pairs** — most validators and readers expose both sync and async APIs.
- **Partial classes** — `ContentValidator` and `SyntaxValidationFunctions` split across multiple files by concern.
- **Primary constructors** — used for validators, readers, builders, and model types.
- **Stream-oriented** — PX file processing operates on `Stream` inputs throughout.
- **`IFileSystem` abstraction** — enables testing without real file I/O.
- **InternalsVisibleTo** — `Px.Utils` exposes internals to `Px.Utils.UnitTests`.

## External Dependencies (Px.Utils only)

| Dependency | Purpose |
|---|---|
| `System.Text.Json` | JSON serialization of metadata models |

No third-party NuGet dependencies in the main library.

## File Map

```
Px.Utils/
├── BinaryData/                    -- Binary data reading and codec implementations
├── Exceptions/                    -- Custom exceptions
├── Language/                      -- MultilanguageString and extensions
├── ModelBuilders/                 -- IMatrixMetadataBuilder, MetadataEntryKeyBuilder, ValueParserUtilities
├── Models/
│   ├── Matrix.cs                  -- Matrix<T> container
│   ├── Data/                      -- IDataValue, DataValueType, DoubleDataValue, DecimalDataValue
│   └── Metadata/                  -- Metadata interfaces, implementations, dimensions, enums, properties, extensions
├── Operations/                    -- Matrix aggregation/sum/multiply/divide extensions
├── PxFile/
│   ├── PxFileConfiguration.cs     -- Format configuration
│   ├── CharacterConstants.cs      -- Byte constants
│   ├── Data/                      -- IPxFileStreamDataReader, PxFileStreamDataReader, DataIndexer, parsers
│   └── Metadata/                  -- IPxFileMetadataReader, PxFileMetadataReader
├── Serializers/Json/              -- JSON converters for models
└── Validation/                    -- Validation pipeline (syntax → content → data → database)
```
