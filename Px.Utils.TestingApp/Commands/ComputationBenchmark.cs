using Px.Utils.Models;
using Px.Utils.Models.Data.DataValue;
using Px.Utils.Models.Metadata;
using Px.Utils.Models.Metadata.Dimensions;
using Px.Utils.Models.Metadata.Enums;
using Px.Utils.Operations;
using Px.Utils.TestingApp.TestDataGenerator;

namespace Px.Utils.TestingApp.Commands
{
    internal class ComputationBenchmark : Benchmark
    {
        internal override string Help => "Test the performance of the computations";

        internal override string Description => Help;

        private Matrix<DecimalDataValue> DecimalDataValueMatrix { get; set; }

        private Matrix<double> DoubleMatrix { get; set; }

        internal ComputationBenchmark()
        {
            BenchmarkFunctions = [ComputeSumBenchmarkDecimalDataValue, ComputeSumBenchmarkDouble];

            List<MetaGenerator.DimensionDefinition> definitions = [
                new(DimensionType.Content, 1),
                new(DimensionType.Time, 100),
                new(DimensionType.Nominal, 100),
                new(DimensionType.Ordinal, 100),
                ];

            MatrixMetadata meta = MetaGenerator.Generate(definitions);

            DecimalDataValueMatrix = new(meta, DataGenerator.GenerateDecimnalDataValues(1000000));
            DoubleMatrix = new(meta, DataGenerator.GenerateDoubles(1000000));
        }

        private void ComputeSumBenchmarkDecimalDataValue()
        {
            DimensionValue valueToAdd = new("new", new("foo", "new"));
            SumMatrixFunction<DecimalDataValue> func = new(valueToAdd, DecimalDataValueMatrix.Metadata.Dimensions[2]);
            func.Apply(DecimalDataValueMatrix);
        }

        private void ComputeSumBenchmarkDouble()
        {
            DimensionValue valueToAdd = new("new", new("foo", "new"));
            SumMatrixFunction<double> func = new(valueToAdd, DoubleMatrix.Metadata.Dimensions[2]);
            func.Apply(DoubleMatrix);
        }
    }
}
