using Px.Utils.Models.Data;
using Px.Utils.Models.Data.DataValue;

namespace Px.Utils.BinaryData.ValueConverters
{
    /// <summary>
    /// Selects an efficient <see cref="IBinaryValueCodec"/> implementation for a sequence of
    /// <see cref="DoubleDataValue"/> samples. Values are analyzed for range, sign, integer-ness,
    /// and exact single-precision representability. Feed values via <see cref="Process(ReadOnlySpan{DoubleDataValue})"/>
    /// and create the codec with <see cref="CreateCodec()"/>.
    /// </summary>
    public sealed class BinaryValueCodecSelector
    {
        /// <summary>
        /// Absolute tolerance for deciding whether a double value is treated as an integer
        /// when selecting integer codecs.
        /// </summary>
        public const double IntegerThreshold = 1e-9;

        private bool _any;
        private bool _allIntegers = true;
        private double _minValue = double.PositiveInfinity;
        private double _maxValue = double.NegativeInfinity;
        private bool _requireDouble;

        /// <summary>
        /// Processes a span of values and updates internal statistics used for codec selection.
        /// Values with <see cref="DataValueType.Exists"/> are considered; others are ignored.
        /// </summary>
        /// <param name="input">The values to analyze.</param>
        public void Process(ReadOnlySpan<DoubleDataValue> input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                DoubleDataValue dv = input[i];
                if (dv.Type != DataValueType.Exists)
                {
                    continue;
                }

                _any = true;
                double value = dv.UnsafeValue;
                if (value < _minValue) _minValue = value;
                if (value > _maxValue) _maxValue = value;

                if (!IsEffectivelyInteger(value))
                {
                    _allIntegers = false;

                    if (!ExactlyRepresentableAsFloat(dv))
                    {
                        _requireDouble = true;
                        break; // no need to continue analyzing
                    }
                }
            }
        }

        /// <summary>
        /// Determines the <see cref="BinaryValueCodecType"/> required to losslessly represent all
        /// values processed so far via <see cref="Process(ReadOnlySpan{DoubleDataValue})"/>.
        /// </summary>
        /// <returns>
        /// The selected <see cref="BinaryValueCodecType"/>. If no values were processed, returns
        /// <see cref="BinaryValueCodecType.UInt16Codec"/>.
        /// </returns>
        public BinaryValueCodecType GetCodecType()
        {
            if (!_any)
            {
                return BinaryValueCodecType.UInt16Codec;
            }

            if (_requireDouble)
            {
                return BinaryValueCodecType.DoubleCodec;
            }

            if (_allIntegers)
            {
                if (_minValue >= 0 && _maxValue < UInt16Codec.SentinelStart)
                {
                    return BinaryValueCodecType.UInt16Codec;
                }

                if (_minValue >= short.MinValue && _maxValue < Int16Codec.SentinelStart)
                {
                    return BinaryValueCodecType.Int16Codec;
                }

                if (_minValue >= 0 && _maxValue < UInt24Codec.SentinelStart)
                {
                    return BinaryValueCodecType.UInt24Codec;
                }

                if (_minValue >= -8388608.0 && _maxValue < Int24Codec.SentinelStart)
                {
                    return BinaryValueCodecType.Int24Codec;
                }

                if (_minValue >= 0)
                {
                    return BinaryValueCodecType.UInt32Codec;
                }

                return BinaryValueCodecType.Int32Codec;
            }
            else
            {
                return BinaryValueCodecType.FloatCodec;
            }
        }

        /// <summary>
        /// Creates a codec that can losslessly represent all previously processed values.
        /// If no values were processed, a <see cref="UInt16Codec"/> is returned by default.
        /// </summary>
        /// <returns>The selected <see cref="IBinaryValueCodec"/> instance.</returns>
        public IBinaryValueCodec CreateCodec()
        {
            return GetCodecType() switch
            {
                BinaryValueCodecType.UInt16Codec => new UInt16Codec(),
                BinaryValueCodecType.Int16Codec => new Int16Codec(),
                BinaryValueCodecType.UInt24Codec => new UInt24Codec(),
                BinaryValueCodecType.Int24Codec => new Int24Codec(),
                BinaryValueCodecType.UInt32Codec => new UInt32Codec(),
                BinaryValueCodecType.Int32Codec => new Int32Codec(),
                BinaryValueCodecType.FloatCodec => new FloatCodec(),
                BinaryValueCodecType.DoubleCodec => new DoubleCodec(),
                _ => throw new InvalidOperationException("Unsupported codec type."),
            };
        }

        private static bool IsEffectivelyInteger(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return false;
            }

            double rounded = Math.Round(value);
            return Math.Abs(value - rounded) < IntegerThreshold;
        }

        private static bool ExactlyRepresentableAsFloat(DoubleDataValue input)
        {
            if (input.Type != DataValueType.Exists)
            {
                return true;
            }

            double v = input.UnsafeValue;
            float f = (float)v;
            if (double.IsNaN(v) || double.IsInfinity(v))
            {
                return false;
            }

            if ((double)f != v)
            {
                return false;
            }

            return true;
        }
    }
}
