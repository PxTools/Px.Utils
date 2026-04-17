# Architecture — Operations

> Matrix aggregation and arithmetic extension methods.

## Extension Methods on `Matrix<T>`

| Class | File | Purpose |
|---|---|---|
| `MatrixFunctionExtensions` | `Operations/MatrixFunctionExtensions.cs` | `ApplyOverDimension` — abstract aggregation over a dimension |
| `SumMatrixFunctionExtensions` | `Operations/SumMatrixFunctionExtensions.cs` | Sum operations |
| `MultiplicationMatrixFunction` | `Operations/MultiplicationMatrixFunction.cs` | Multiplication operations |
| `DivisionMatrixFunctionExtensions` | `Operations/DivisionMatrixFunctionExtensions.cs` | Division operations |

## Pattern

All operations are extension methods on `Matrix<T>`. They use `ApplyOverDimension` which:

1. Takes a source dimension map, an aggregation function, and a function identity value
2. Adds a new `DimensionValue` to the target dimension holding the result
3. Returns a new `Matrix<T>` with the aggregated data
