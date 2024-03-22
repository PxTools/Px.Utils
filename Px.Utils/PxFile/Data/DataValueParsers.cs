﻿using PxUtils.Models.Data;
using PxUtils.Models.Data.DataValue;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace PxUtils.PxFile.Data
{
    public static class DataValueParsers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DoubleDataValue FastParseDoubleDataValueDangerous(char[] buffer, int len)
        {
            if (buffer[0] == '"')
            {
                if (buffer[1] == '-') return new DoubleDataValue(0, DataValueType.Nill);
                return new DoubleDataValue(0, (DataValueType)(len - 2));
            }
            else
            {
                double value = FastParseDoubleDangerous(buffer, len);
                return new DoubleDataValue(value, DataValueType.Exists);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DecimalDataValue FastParseDecimalDataValueDangerous(char[] buffer, int len)
        {
            if (buffer[0] == '"')
            {
                if (buffer[1] == '-') return new DecimalDataValue(0, DataValueType.Nill);
                return new DecimalDataValue(0, (DataValueType)(len - 2));
            }
            else
            {
                decimal value = FastParseDecimalDangerous(buffer, len);
                return new DecimalDataValue(value, DataValueType.Exists);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double FastParseUnsafeDoubleDangerous(char[] buffer, int len, double[] missingValueEncodings)
        {
            if (buffer[0] == '"')
            {
                if (buffer[1] == '-') return missingValueEncodings[0];
                return missingValueEncodings[len - 2];
            }
            else
            {
                return FastParseDoubleDangerous(buffer, len);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DoubleDataValue ParseDoubleDataValue(char[] buffer, int len)
        {
            if (double.TryParse(new ReadOnlySpan<char>(buffer, 0, len), CultureInfo.InvariantCulture, out double value))
            {
                return new DoubleDataValue(value, DataValueType.Exists);
            }
            else
            {
                if (buffer[0] != '"' || buffer[len - 1] != '"' || len < 3 || len > 8)
                {
                    throw new ArgumentException($"Invalid symbol found when parsing data values {new string(buffer, 0, len)}");
                }

                if (buffer[1] == '-') return new DoubleDataValue(0.0, DataValueType.Nill);

                int dots = 0;
                while (dots < len - 2)
                {
                    if (buffer[dots + 1] == '.') dots++;
                    else throw new ArgumentException($"Invalid symbol found when parsing data values {new string(buffer, 0, len)}");
                }

                return new DoubleDataValue(double.NaN, (DataValueType)dots);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DecimalDataValue ParseDecimalDataValue(char[] buffer, int len)
        {
            if (decimal.TryParse(new ReadOnlySpan<char>(buffer, 0, len), CultureInfo.InvariantCulture, out decimal value))
            {
                return new DecimalDataValue(value, DataValueType.Exists);
            }
            else
            {
                if (buffer[0] != '"' || buffer[len - 1] != '"' || len < 3 || len > 8)
                {
                    throw new ArgumentException($"Invalid symbol found when parsing data values {new string(buffer, 0, len)}");
                }

                if (buffer[1] == '-') return new DecimalDataValue(decimal.Zero, DataValueType.Nill);

                int dots = 0;
                while (dots < len - 2)
                {
                    if (buffer[dots + 1] == '.') dots++;
                    else throw new ArgumentException($"Invalid symbol found when parsing data values {new string(buffer, 0, len)}");
                }

                return new DecimalDataValue(decimal.Zero, (DataValueType)dots);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ParseUnsafeDouble(char[] buffer, int len, double[] missingValueEncodings)
        {
            if (double.TryParse(new ReadOnlySpan<char>(buffer, 0, len), CultureInfo.InvariantCulture, out double value))
            {
                return value;
            }
            else
            {
                if (buffer[0] != '"' || buffer[len - 1] != '"' || len < 3 || len > 8)
                {
                    throw new ArgumentException($"Invalid symbol found when parsing data values {new string(buffer, 0, len)}");
                }

                if (buffer[1] == '-') return missingValueEncodings[0];

                int dots = 0;
                while (dots < len - 2)
                {
                    if (buffer[dots + 1] == '.') dots++;
                    else throw new ArgumentException($"Invalid symbol found when parsing data values {new string(buffer, 0, len)}");
                }

                return missingValueEncodings[dots];
            }
        }

        private static readonly double[] doublePowersOf10 =
        [
            1d, 1d, 10d, 100d, 1000d,
            10000d, 100000d, 1000000d, 10000000d,
            100000000d, 1000000000d, 1000000000d, 1000000000d,
            10000000000d, 100000000000d, 1000000000000d, 10000000000000d,
            100000000000000d, 1000000000000000d
        ];

        /// <summary>
        /// IMPORTANT: This function does not perform any validation on the input.
        /// Using this method with invalid input will result in undefined behavior.
        /// Parses a double value from a char array. The array can contain a decimal separator (.) and a negative sign.
        /// </summary>
        /// <param name="buffer">Must contain a number with optional negative sign and decimal separator</param>
        /// <param name="len">Length of the number in the buffer</param>
        /// <returns>Double value parsed from the characters in the input buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double FastParseDoubleDangerous(char[] buffer, int len)
        {
            long n = 0;
            int decimalPosition = len;

            for (int k = 0; k < len; k++)
            {
                char c = buffer[k];
                if (c >= '0')
                {
                    n = (n * 10) + (c - '0');
                }
                else if (c == '.')
                {
                    decimalPosition = k;
                }
            }
            if (buffer[0] == '-') return -n / doublePowersOf10[len - decimalPosition];
            else return n / doublePowersOf10[len - decimalPosition];
        }

        private static readonly decimal[] decimalPowersOf10 =
        [
            1m, 1m, 10m, 100m, 1000m,
            10000m, 100000m, 1000000m, 10000000m,
            100000000m, 1000000000m, 1000000000m, 1000000000m,
            10000000000m, 100000000000m, 1000000000000m, 10000000000000m,
            100000000000000m, 1000000000000000m
        ];

        /// <summary>
        /// IMPORTANT: This function does not perform any validation on the input.
        /// Using this method with invalid input will result in undefined behavior.
        /// Parses a decimal value from a char array. The array can contain a decimal separator (.) and a negative sign.
        /// </summary>
        /// <param name="buffer">Must contain a number with optional negative sign and decimal separator</param>
        /// <param name="len">Length of the number in the buffer</param>
        /// <returns>Decimal value parsed from the characters in the input buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static decimal FastParseDecimalDangerous(char[] buffer, int len)
        {
            long n = 0;
            int decimalPosition = len;

            for (int k = 0; k < len; k++)
            {
                char c = buffer[k];
                if (c >= '0')
                {
                    n = (n * 10) + (c - '0');
                }
                else if (c == '.')
                {
                    decimalPosition = k;
                }
            }
            if (buffer[0] == '-') return -n / decimalPowersOf10[len - decimalPosition];
            else return n / decimalPowersOf10[len - decimalPosition];
        }
    }
}
