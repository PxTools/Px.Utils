﻿using Px.Utils.Models;
using Px.Utils.Models.Data.DataValue;
using Px.Utils.Models.Metadata;
using Px.Utils.Models.Metadata.Dimensions;
using Px.Utils.Models.Metadata.Enums;
using Px.Utils.Operations;
using Px.Utils.TestingApp.TestDataGenerator;

namespace Px.Utils.TestingApp.Commands
{
    internal sealed class ComputationBenchmark : Benchmark
    {
        internal override string Help => "Test the performance of the computations";

        internal override string Description => Help;

        private Matrix<DecimalDataValue> DecimalDataValueMatrix { get; set; }

        private Matrix<double> DoubleMatrix { get; set; }

        internal ComputationBenchmark()
        {
            BenchmarkFunctions = [ComputeSumDecimalDataValue, ComputeSumDouble,
                ComputeDivisionDecimalDataValue, ComputeDivisionDouble];

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

        private void ComputeSumDecimalDataValue()
        {
            DimensionValue valueToAdd = new("new", new("foo", "new"));
            DecimalDataValueMatrix.SumToNewValue(valueToAdd, DecimalDataValueMatrix.Metadata.Dimensions[2]);
        }

        private void ComputeSumDouble()
        {
            DimensionValue valueToAdd = new("new", new("foo", "new"));
            DoubleMatrix.SumToNewValue(valueToAdd, DoubleMatrix.Metadata.Dimensions[2]);
        }
        private void ComputeDivisionDecimalDataValue()
        {
            IReadOnlyDimension map = DecimalDataValueMatrix.Metadata.Dimensions[2];
            string code = map.Values[0].Code;
            DecimalDataValueMatrix.DivideSubsetBySelectedValue(map, code);
        }

        private void ComputeDivisionDouble()
        {
            IReadOnlyDimension map = DoubleMatrix.Metadata.Dimensions[2];
            string code = map.Values[0].Code;
            DecimalDataValueMatrix.DivideSubsetBySelectedValue(map, code);
        }
    }
}
