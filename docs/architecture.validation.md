# Architecture — Validation

> Validation pipeline, interfaces, result types, and extensibility.

## Interfaces

| Interface | Implementations | Purpose |
|---|---|---|
| `IValidator` | `ContentValidator`, `DatabaseValidator` | Sync validation → `ValidationResult` |
| `IValidatorAsync` | `DatabaseValidator` | Async validation → `Task<ValidationResult>` |
| `IPxFileStreamValidator` | `SyntaxValidator`, `DataValidator`, `PxFileValidator` | Sync stream-based PX file validation |
| `IPxFileStreamValidatorAsync` | `SyntaxValidator`, `DataValidator`, `PxFileValidator` | Async stream-based PX file validation |
| `IDatabaseValidator` | — (consumer-provided) | Extension point for database-level custom validators |
| `IFileSystem` | `LocalFileSystem` | File system abstraction for database validation |

## Pipeline

`PxFileValidator` orchestrates: `SyntaxValidator` → `ContentValidator` → `DataValidator` → custom validators.

```
Validate(stream, filename, encoding?, fileSystem?)
ValidateAsync(stream, filename, encoding?, fileSystem?, cancellationToken)
```

### SyntaxValidator

Validates PX file metadata syntax (key-value structure, encoding, characters).  
File: `Validation/SyntaxValidation/SyntaxValidator.cs`  
Partial helpers: `SyntaxValidationFunctions.StringValidationFunctions.cs`, `KeyValueValidationFunctions.cs`, `StructuredValidationFunctions.cs`

### ContentValidator

Validates metadata content (required keys, language definitions, dimension consistency).  
File: `Validation/ContentValidation/ContentValidator.cs`  
Partial files: `ValidationEntryFunctions.cs`, `ValidationFindKeywordFunctions.cs`, `UtilityMethods.cs`

Dimension type validation uses `PxFileConfiguration.TokenDefinitions.VariableTypeTokens`. The first configured token for each dimension type is treated as the recommended primary value, while additional configured tokens are accepted as aliases and reported as warnings.

### DataValidator

Validates data section (row counts, row lengths, value types, separators).  
File: `Validation/DataValidation/DataValidator.cs`

### DatabaseValidator

Validates entire PX database directory (all `.px` files, alias files, directory structure).  
File: `Validation/DatabaseValidation/DatabaseValidator.cs`

## Extensibility

- `CustomSyntaxValidationFunctions` — inject custom string/key-value/structured validation functions.
- `CustomContentValidationFunctions` — inject custom content validation functions.
- `PxFileValidator.SetCustomValidators(...)` — inject `IPxFileStreamValidator[]`, `IPxFileStreamValidatorAsync[]`, `IValidator[]`, `IValidatorAsync[]`.
- `DatabaseValidator` accepts `IDatabaseValidator[]` arrays for px files, alias files, and directories.

## Result Types

- `ValidationResult` — wraps `ValidationFeedback`
- `ValidationFeedback` — `ConcurrentDictionary<ValidationFeedbackKey, List<ValidationFeedbackValue>>`
- `ValidationFeedbackKey` — (`ValidationFeedbackLevel`, `ValidationFeedbackRule`)
- `ValidationFeedbackLevel` — `Warning` | `Error`
- `ValidationFeedbackRule` — enum with 59 rule codes (see `Validation/Enums.cs`)

## Dependency Graph

```
PxFileValidator
├── SyntaxValidator : IPxFileStreamValidator
│   ├── PxFileConfiguration
│   ├── CustomSyntaxValidationFunctions (optional)
│   └── IFileSystem → LocalFileSystem
├── ContentValidator : IValidator
│   ├── PxFileConfiguration
│   ├── ValidationStructuredEntry[] (from SyntaxValidator)
│   └── CustomContentValidationFunctions (optional)
├── DataValidator : IPxFileStreamValidator
│   └── PxFileConfiguration
└── Custom validators (optional)

DatabaseValidator : IValidator, IValidatorAsync
├── IFileSystem → LocalFileSystem
├── PxFileConfiguration
├── PxFileMetadataReader
├── SyntaxValidator
└── IDatabaseValidator[] (optional custom)
```

## File Map

```
Validation/
├── IValidator.cs                            -- IValidator, IValidatorAsync
├── IPxFileStreamValidator.cs                -- IPxFileStreamValidator, IPxFileStreamValidatorAsync
├── IValidationResult.cs                     -- ValidationResult
├── ValidationFeedback.cs                    -- ValidationFeedbackKey, ValidationFeedbackValue, ValidationFeedback
├── ValidationObject.cs                      -- Validation context
├── Enums.cs                                 -- ValidationFeedbackLevel, ValidationFeedbackRule, ValueType
├── PxFileValidator.cs                       -- Orchestrator
├── ContentValidation/
│   ├── ContentValidator.cs                  -- + partial files
│   ├── ContentValidationResult.cs
│   └── CustomContentValidationFunctions.cs
├── DataValidation/
│   ├── DataValidator.cs
│   └── DataValidatorFunctions.cs
├── DatabaseValidation/
│   ├── DatabaseValidator.cs
│   ├── DatabaseValidatorFunctions.cs
│   ├── IFileSystem.cs
│   └── LocalFileSystem.cs
└── SyntaxValidation/
    ├── SyntaxValidator.cs
    ├── SyntaxValidationFunctions.*.cs
    ├── SyntaxValidationResult.cs
    ├── SyntaxValidationUtilityMethods.cs
    ├── CustomSyntaxValidationFunctions.cs
    ├── ValidationEntry.cs / ValidationStructuredEntry.cs / ValidationKeyValuePair.cs
    └── Bcp47Codes.cs
```
