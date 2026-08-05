# Architecture — Readers & Binary Data

> Metadata reading, data reading, and binary data codecs.

## Metadata Reading

| Interface | Implementation | File |
|---|---|---|
| `IPxFileMetadataReader` | `PxFileMetadataReader` | `PxFile/Metadata/PxFileMetadataReader.cs` |

```
IEnumerable<KeyValuePair<string, string>> ReadMetadata(Stream, Encoding, int bufferSize = 4096)
Encoding GetEncoding(Stream)
Task<Encoding> GetEncodingAsync(Stream, CancellationToken)
IAsyncEnumerable<KeyValuePair<string, string>> ReadMetadataAsync(Stream, Encoding, int bufferSize = 4096)
```

Depends on: `PxFileConfiguration`.

## Data Reading

| Interface | Implementation | File |
|---|---|---|
| `IPxFileStreamDataReader` | `PxFileStreamDataReader` | `PxFile/Data/PxFileStreamDataReader.cs` |

```
void ReadUnsafeDoubles(double[] buffer, int offset, IMatrixMap target, IMatrixMap complete, double[] missingValueEncodings)
void ReadDoubleDataValues(DoubleDataValue[] buffer, int offset, IMatrixMap target, IMatrixMap complete)
void ReadDecimalDataValues(DecimalDataValue[] buffer, int offset, IMatrixMap target, IMatrixMap complete)
```

Async variants available. Implements `IDisposable`.  
Depends on: `PxFileConfiguration`.

When constructed at stream position `0`, `PxFileStreamDataReader` locates the first non-whitespace value following the top-level `DATA=` entry before reading. The overload that accepts `dataStart` requires that value's absolute raw byte offset.

### Helpers

| File | Purpose |
|---|---|
| `DataIndexer.cs` | Index mapping between source and target `IMatrixMap` |
| `DataValueParsers.cs` | Parse raw data values from byte spans |
| `StreamUtilities.cs` | `FindDataStartPosition` and async equivalent locate the first value after top-level `DATA=` as an absolute raw byte offset, preserving the original position of a seekable stream |

## Binary Data

| Type | File | Purpose |
|---|---|---|
| `BinaryDataReader<TCodec>` | `BinaryData/BinaryDataReader.cs` | Generic windowed reader for binary-encoded matrix data |
| `BinaryDataReaderStatic` | `BinaryData/BinaryDataReaderStatic.cs` | Static helper methods |
| `IBinaryValueCodec` | `BinaryData/ValueConverters/IBinaryValueCodec.cs` | Static interface for encode/decode |
| `BinaryValueCodecSelector` | `BinaryData/ValueConverters/BinaryValueCodecSelector.cs` | Codec factory by `BinaryValueCodecType` |

### Codecs

All implement `IBinaryValueCodec` (static abstract `ByteCount`):

`DoubleCodec`, `FloatCodec`, `Int16Codec`, `Int24Codec`, `Int32Codec`, `UInt16Codec`, `UInt24Codec`, `UInt32Codec`

Pattern: static interface members (`static abstract`) for zero-allocation encode/decode on spans.
