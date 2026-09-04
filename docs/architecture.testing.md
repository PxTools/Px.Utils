# Architecture — Testing

> Unit tests, test mocks, and benchmarking CLI.

## Unit Tests (`Px.Utils.UnitTests`)

Framework: MSTest + Moq. Naming: `MethodNameStateUnderTestExpectedBehavior`.

### Validation Tests

| Test class | File | Covers |
|---|---|---|
| `StreamSyntaxValidationTests` | `Validation/SyntaxValidationTests/StreamSyntaxValidationTests.cs` | `SyntaxValidator` sync |
| `StreamSyntaxValidationAsyncTests` | `Validation/SyntaxValidationTests/StreamSyntaxValidationAsyncTests.cs` | `SyntaxValidator` async |
| `ContentValidationTests` | `Validation/ContentValidationTests/ContentValidationTests.cs` | `ContentValidator` |
| `DataValidationTest` | `Validation/DataValidationTests/DataValidationTest.cs` | `DataValidator` integration |
| `DataNumberValueValidatorTest` | `Validation/DataValidationTests/DataNumberValueValidatorTest.cs` | Numeric data validation |
| `DataStringValueValidatorTests` | `Validation/DataValidationTests/DataStringValueValidatorTests.cs` | String data validation |
| `DataSeparatorValidatorTest` | `Validation/DataValidationTests/DataSeparatorValidatorTest.cs` | Separator validation |
| `DataStructureValidationTests` | `Validation/DataValidationTests/DataStructureValidationTests.cs` | Data structure validation |
| `ValidationFeedbackSinkTests` | `Validation/ValidationFeedbackSinkTests.cs` | Per-signature feedback limits, truncation annotations, unlimited retention, and invalid limits |
| `DatabaseValidatorTests` | `Validation/DatabaseValidation/DatabaseValidatorTests.cs` | `DatabaseValidator` |
| `DatabaseValidatorFunctionTests` | `Validation/DatabaseValidation/DatabaseValidatorFunctionTests.cs` | Database validator functions |
| `PxFileValidationTests` | `Validation/PxFileValidationTests/PxFileValidationTests.cs` | `PxFileValidator` |

### Reader Tests

| Test class | File | Covers |
|---|---|---|
| `ReadMetadataTests` | `PxFileTests/PxFileMetadataReaderTests/ReadMetadataTests.cs` | `PxFileMetadataReader` |
| `ReadMetadataAsyncTests` | `PxFileTests/PxFileMetadataReaderTests/ReadMetadataAsyncTests.cs` | `PxFileMetadataReader` async |
| `GetEncodingTests` | `PxFileTests/PxFileMetadataReaderTests/GetEncodingTests.cs` | Encoding detection |
| `DataReaderTests` | `PxFileTests/DataTests/PxFileStreamDataReaderTests/DataReaderTests.cs` | `PxFileStreamDataReader` |
| `AsyncDataReaderTests` | `PxFileTests/DataTests/PxFileStreamDataReaderTests/AsyncDataReaderTests.cs` | Async data reading |
| `MultiPartReadingTests` | `PxFileTests/DataTests/PxFileStreamDataReaderTests/MultiPartReadingTests.cs` | Multi-part reads |
| `DataIndexerTests` | `PxFileTests/DataTests/DataIndexerTests.cs` | `DataIndexer` |
| `DataValueParserTests` | `PxFileTests/DataTests/DataValueParserTests.cs` | `DataValueParsers` |
| `StreamUtilitiesTests` | `PxFileTests/DataTests/StreamUtilitiesTests.cs` | Byte-accurate `DATA=` start offsets with BOM, multibyte metadata, whitespace, buffer splits, and missing data |

### Model & Builder Tests

| Test class | File | Covers |
|---|---|---|
| `MatrixMetadataBuilderTests` | `ModelBuilderTests/MatrixMetadataBuilderTests.cs` | `MatrixMetadataBuilder` |
| `MetadataEntryKeyBuilderTests` | `ModelBuilderTests/MetadataEntryKeyBuilderTests.cs` | `MetadataEntryKeyBuilder` |
| `MultilanguageStringTests` | `LanguageTests/MultilanguageStringTests.cs` | `MultilanguageString` |
| `MatrixTransformationsTests` | `ModelTests/MatrixTransformationsTests.cs` | `Matrix<T>.GetTransform` |
| `ValueListTests` | `ModelTests/ValueListTests.cs` | `ValueList` |
| `PropertyTests` | `ModelTests/PropertyTests.cs` | `MetaProperty` subtypes |
| `DataValueTests` | `ModelTests/DataValueTests.cs` | Data value types |

### Operations Tests

| Test class | File | Covers |
|---|---|---|
| `SumMatrixFunctionExtensionTests` | `OperationsTests/SumMatrixFunctionExtensionTests.cs` | Sum |
| `MultiplicationMatrixFunctionExtensionTests` | `OperationsTests/MultiplicationMatrixFunctionExtensionTests.cs` | Multiplication |
| `DivisionMatrixFunctionExtensionTests` | `OperationsTests/DivisionMatrixFunctionExtensionTests.cs` | Division |

### Binary Data Tests

| Test class | File | Covers |
|---|---|---|
| `BinaryDataReaderCreateTests` | `BinaryData/BinaryDataReaderCreateTests.cs` | Reader creation |
| `BinaryDataReaderStreamTests` | `BinaryData/BinaryDataReaderStreamTests.cs` | Stream reading |
| `BinaryDataReaderChunkTests` | `BinaryData/BinaryDataReaderChunkTests.cs` | Chunk reading |
| `*CodecTests` | `BinaryData/ValueConverters/*CodecTests.cs` | Individual codecs |

### Serializer Tests

| Test class | File | Covers |
|---|---|---|
| `MatrixMetadataConverterSerializeTests` | `SerializerTests/MatrixMetadataConverterSerializeTests.cs` | JSON round-trip |
| `DimensionConverterTests` | `SerializerTests/DimensionConverterTests.cs` | Dimension JSON |
| `MetaPropertyConverterTests` | `SerializerTests/MetaPropertyConverterTests.cs` | MetaProperty JSON |
| `MultilanguageStringConverterTests` | `SerializerTests/MultilanguageStringConverterTests.cs` | MultilanguageString JSON |
| `ValueListConverterTests` | `SerializerTests/ValueListConverterTests.cs` | ValueList JSON |

### Mocks

`MockFileSystem`, `MockCustomValidatorFunctions`, `MockCustomSyntaxValidationFunctions`, `MockCustomContentValidationFunctions`, `MockDatabaseFileStreams`.

---

## Benchmarking CLI (`Px.Utils.TestingApp`)

Entry point: `TestingApp.Main(args)` → no args starts `InteractiveFlow`; batch mode not yet implemented.

All commands inherit `Command`. Benchmarks inherit `Benchmark`.

| Command | Purpose |
|---|---|
| `PxFileValidationBenchmark` | Full PX file validation |
| `DatabaseValidationBenchmark` | Database validation |
| `MetadataSyntaxValidationBenchmark` | Syntax validation |
| `MetadataContentValidationBenchmark` | Content validation |
| `DataValidationBenchmark` | Data validation |
| `MetadataReaderBenchmark` | Metadata reading |
| `MetadataBuilderBenchmark` | Metadata building |
| `DataReadBenchmark` | Data reading |
| `FileBenchmark` | File I/O |
| `ComputationBenchmark` | Matrix operations |
| `BinaryReadBenchmark` | Binary read |
| `BinaryWriteBenchmark` | Binary write |

Supporting: `BenchmarkRunner.cs`, `TestAppConsole.cs`, `InteractiveFlow.cs`, `TestDataGenerator/`.

---

## Integration Runner (`Px.Utils.IntegrationTest`)

The .NET 10 console runner validates checked-in database fixtures through the public `Px.Utils` APIs. It returns `0` when every scenario passes, `1` for expectation mismatches, and `2` when setup or execution fails.

It compares structured validation feedback for all checked-in PX files, complete reads for every valid file, representative mapped reads, and public sum, multiplication, division, and relative-subtraction operations. Expected results are committed JSON fixtures under `Px.Utils.IntegrationTest/ExpectedResults`; comparisons require exact value types and use a documented tolerance only for division values.

The runner locates copied `test-database` and expectation assets from `AppContext.BaseDirectory`, so it can run independently of the repository working directory. CI runs it after build only for the `10.x` SDK matrix entries on Windows and Linux.
