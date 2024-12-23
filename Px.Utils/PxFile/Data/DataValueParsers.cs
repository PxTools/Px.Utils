﻿using Px.Utils.Models.Data;
using Px.Utils.Models.Data.DataValue;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Px.Utils.PxFile.Data
{
    public static class DataValueParsers
    {
        const int decimalPlaceShift = 10;
        const int stringDelimiterOffset = 2;
        const int missingDataEntryMinLength = 3;
        const int missingDataEntryMaxLength = 8;

        /// <summary>
        /// This method uses a fast, but potentially unsafe, parser to convert the characters into a <see cref="DoubleDataValue"/>.
        /// It is important that the input has been validated before using this method since the method does not perform any validation.
        /// Invalid input will result in undefined behavior.
        /// </summary>
        /// <param name="buffer">The characters to parse.</param>
        /// <param name="len">The number of characters to parse.</param>
        /// <returns>A <see cref="DoubleDataValue"/> instance representing the parsed value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DoubleDataValue FastParseDoubleDataValueDangerous(char[] buffer, int len)
        {
            // All special/missing values are encoded either as strings in the format "..." and "-" or ... and -
            // The length of the string (number of dots) is used to determine the type of missing value.
            if (buffer[0] == '"')
            {
                if (buffer[1] == '-') return new DoubleDataValue(0, DataValueType.Nill);
                return new DoubleDataValue(0, (DataValueType)(len - stringDelimiterOffset));
            }
            else if (buffer[0] < '0')
            {
                if (buffer[0] == '-')
                    if (len == 1) return new DoubleDataValue(0, DataValueType.Nill);
                    else return new(FastParseDoubleDangerous(buffer, len), DataValueType.Exists);
                return new DoubleDataValue(0, (DataValueType)len);
            }
            else
            {
                double value = FastParseDoubleDangerous(buffer, len);
                return new DoubleDataValue(value, DataValueType.Exists);
            }
        }

        /// <summary>
        /// This method uses a fast, but potentially unsafe, parser to convert the characters into a <see cref="DecimalDataValue"/>.
        /// It is important that the input has been validated before using this method since the method does not perform any validation.
        /// Invalid input will result in undefined behavior.
        /// </summary>
        /// <param name="buffer">The characters to parse.</param>
        /// <param name="len">The number of characters to parse.</param>
        /// <returns>A <see cref="DecimalDataValue"/> instance representing the parsed value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DecimalDataValue FastParseDecimalDataValueDangerous(char[] buffer, int len)
        {
            // All special/missing values are encoded as strings in the format "..." or "-".
            // The length of the string (number of dots) is used to determine the type of missing value.
            if (buffer[0] == '"')
            {
                if (buffer[1] == '-') return new DecimalDataValue(0, DataValueType.Nill);
                return new DecimalDataValue(0, (DataValueType)(len - stringDelimiterOffset));
            }
            else if (buffer[0] < '0')
            {
                if (buffer[0] == '-')
                    if (len == 1) return new DecimalDataValue(0, DataValueType.Nill);
                    else return new(FastParseDecimalDangerous(buffer, len), DataValueType.Exists);
                return new DecimalDataValue(0, (DataValueType)len);
            }
            else
            {
                decimal value = FastParseDecimalDangerous(buffer, len);
                return new (value, DataValueType.Exists);
            }
        }

        /// <summary>
        /// This method uses a fast, but potentially unsafe, parser to convert the characters into a <see cref="double"/>.
        /// If the value is missing, the method returns a value from the missingValueEncodings array.
        /// Otherwise, the method interprets the characters as a number and returns that number.
        /// This method does not perform any validation, so ensure the input is valid before use.
        /// </summary>
        /// <param name="buffer">The characters to parse.</param>
        /// <param name="len">The number of characters to parse.</param>
        /// <param name="missingValueEncodings">
        /// Array of values to use for missing encodings:
        /// [0] "-"
        /// [1] "."
        /// [2] ".."
        /// [3] "..."
        /// [4] "...."
        /// [5] "....."
        /// [6] "......"
        /// </param>
        /// <returns>A double value representing the parsed value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double FastParseUnsafeDoubleDangerous(char[] buffer, int len, double[] missingValueEncodings)
        {
            if (buffer[0] == '"')
            {
                if (buffer[1] == '-') return missingValueEncodings[0];
                return missingValueEncodings[len - stringDelimiterOffset];
            }
            else if (buffer[0] < '0')
            {
                if (buffer[0] == '-')
                    if (len == 1) return missingValueEncodings[0];
                    else return FastParseDoubleDangerous(buffer, len);
                return missingValueEncodings[len];
            }
            else
            {
                return FastParseDoubleDangerous(buffer, len);
            }
        }

        /// <summary>
        /// Parses a set of characters into a <see cref="DoubleDataValue"/> instance.
        /// </summary>
        /// <param name="buffer">The characters to parse.</param>
        /// <param name="len">The number of characters to parse.</param>
        /// <returns>A <see cref="DoubleDataValue"/> instance representing the parsed value.</returns>
        /// <exception cref="ArgumentException">Thrown if the input is not a valid number or a missing value code.</exception>"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DoubleDataValue ParseDoubleDataValue(char[] buffer, int len)
        {
            if (double.TryParse(new ReadOnlySpan<char>(buffer, 0, len), CultureInfo.InvariantCulture, out double value))
            {
                return new DoubleDataValue(value, DataValueType.Exists);
            }
            else
            {
                if (buffer[0] != '"' || buffer[len - 1] != '"' || len > missingDataEntryMaxLength)
                {
                    throw new ArgumentException($"Invalid symbol found when parsing data values {new string(buffer, 0, len)}");
                }

                if (buffer[1] == '-' || buffer[0] == '-') return new DoubleDataValue(0.0, DataValueType.Nill);

                int dots = 0;
                int offset = buffer[0] == '"' ? stringDelimiterOffset : 0;
                while (dots < len - offset)
                {
                    if (buffer[dots + 1] == '.') dots++;
                    else throw new ArgumentException($"Invalid symbol found when parsing data values {new string(buffer, 0, len)}");
                }

                return new DoubleDataValue(double.NaN, (DataValueType)dots);
            }
        }

        /// <summary>
        /// Parses a set of characters into a <see cref="DecimalDataValue"/> instance.
        /// </summary>
        /// <param name="buffer">The characters to parse.</param>
        /// <param name="len">The number of characters to parse.</param>
        /// <returns>A <see cref="DecimalDataValue"/> instance representing the parsed value.</returns>
        /// <exception cref="ArgumentException">Thrown if the input is not a valid number or a missing value code.</exception>"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DecimalDataValue ParseDecimalDataValue(char[] buffer, int len)
        {
            if (decimal.TryParse(new ReadOnlySpan<char>(buffer, 0, len), CultureInfo.InvariantCulture, out decimal value))
            {
                return new DecimalDataValue(value, DataValueType.Exists);
            }
            else
            {
                if (buffer[0] == '"')
                {
                    return ParseEnclosedDecimalDataValue(buffer, len);
                }

                return ParseUnenclosedDecimalDataValue(buffer, len);
            }
        }

        /// <summary>
        /// Parses a set of characters into a <see cref="double"/> value.
        /// If the value is missing, the method returns a value from the missingValueEncodings array.
        /// Otherwise, the method interprets the characters as a number and returns that number.
        /// </summary>
        /// <param name="buffer">The characters to parse.</param>
        /// <param name="len">The number of characters to parse.</param>
        /// <param name="missingValueEncodings">
        /// Array of values to use for missing encodings:
        /// [0] "-"
        /// [1] "."
        /// [2] ".."
        /// [3] "..."
        /// [4] "...."
        /// [5] "....."
        /// [6] "......"
        /// </param>
        /// <returns>A <see cref="double"/> value representing the parsed value.</returns>
        /// <exception cref="ArgumentException">Thrown if the input is not a valid number or a missing value code.</exception>"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ParseUnsafeDouble(char[] buffer, int len, double[] missingValueEncodings)
        {
            if (double.TryParse(new ReadOnlySpan<char>(buffer, 0, len), CultureInfo.InvariantCulture, out double value))
            {
                return value;
            }
            else
            {
                if (buffer[0] == '"')
                {
                    return ParseEnclosedUnsafeDouble(buffer, len, missingValueEncodings);
                }

                return ParseUnenclosedUnsafeDouble(buffer, len, missingValueEncodings);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static DecimalDataValue ParseEnclosedDecimalDataValue(char[] buffer, int len)
        {
            if (buffer[len - 1] != '"' || len < missingDataEntryMinLength || len > missingDataEntryMaxLength)
            {
                throw new ArgumentException($"Invalid symbol found when parsing data values {new string(buffer, 0, len)}");
            }

            if (buffer[1] == '-')
            {
                return new DecimalDataValue(decimal.Zero, DataValueType.Nill);
            }

            int dots = CountDots(buffer, 1, len - stringDelimiterOffset);
            return new DecimalDataValue(decimal.Zero, (DataValueType)dots);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static DecimalDataValue ParseUnenclosedDecimalDataValue(char[] buffer, int len)
        {
            if (buffer[0] == '-' && len == 1)
            {
                return new DecimalDataValue(decimal.Zero, DataValueType.Nill);
            }

            int dots = CountDots(buffer, 0, len);
            return new DecimalDataValue(decimal.Zero, (DataValueType)dots);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double ParseEnclosedUnsafeDouble(char[] buffer, int len, double[] missingValueEncodings)
        {
            if (buffer[len - 1] != '"' || len < missingDataEntryMinLength || len > missingDataEntryMaxLength)
            {
                throw new ArgumentException($"Invalid symbol found when parsing data values {new string(buffer, 0, len)}");
            }

            if (buffer[1] == '-')
            {
                return missingValueEncodings[0];
            }

            int dots = CountDots(buffer, 1, len - stringDelimiterOffset);
            return missingValueEncodings[dots];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double ParseUnenclosedUnsafeDouble(char[] buffer, int len, double[] missingValueEncodings)
        {
            if (buffer[0] == '-' && len == 1)
            {
                return missingValueEncodings[0];
            }

            int dots = CountDots(buffer, 0, len);
            return missingValueEncodings[dots];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int CountDots(char[] buffer, int offset, int end)
        {
            int dots = 0;
            for (int i = 0; i < end; i++)
            {
                if (buffer[i + offset] == '.')
                {
                    dots++;
                }
                else
                {
                    throw new ArgumentException($"Invalid symbol found when parsing data values {new string(buffer, 0, end)}");
                }
            }
            return dots;
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
                    n = (n * decimalPlaceShift) + (c - '0');
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
                    n = (n * decimalPlaceShift) + (c - '0');
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
