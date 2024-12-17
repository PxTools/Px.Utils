using Px.Utils.Models.Metadata.Enums;
using Px.Utils.Models.Metadata.ExtensionMethods;
using Px.Utils.PxFile;

namespace Px.Utils.ModelBuilders
{
    /// <summary>
    /// Class containing utility methods for parsing various types of values from Px-style metadata value string.
    /// </summary>
    public static class ValueParserUtilities
    {
        /// <summary>
        /// Parses a string into a <see cref="TimeDimensionInterval"/> enumeration value.
        /// This method extracts the time interval part from the input string and maps it to a <see cref="TimeDimensionInterval"/> enumeration value.
        /// It throws an exception if the input string is not in the correct format or does not represent a valid time interval.
        /// </summary>
        /// <param name="input">The string to parse into a <see cref="TimeDimensionInterval"/> enumeration value.</param>
        /// <param name="conf">An optional <see cref="PxFileConfiguration"/> object. If not provided, the default configuration is used.</param>
        /// <returns>A <see cref="TimeDimensionInterval"/> enumeration value parsed from the input string.</returns>
        public static TimeDimensionInterval ParseTimeIntervalFromTimeVal(string input, PxFileConfiguration? conf = null)
        {
            conf ??= PxFileConfiguration.Default;
            int tlistTokenEndIndex = input.IndexOf(conf.Symbols.Value.TimeSeriesIntervalEnd, StringComparison.InvariantCulture); 
            if(tlistTokenEndIndex < 0) throw new ArgumentException($"Invalid time value string {input}");
            string timevalString = input[..(tlistTokenEndIndex + 1)];
            string inputStart = conf.Tokens.Time.TimeIntervalIndicator + conf.Symbols.Value.TimeSeriesIntervalStart;
            char inputEnd = conf.Symbols.Value.TimeSeriesIntervalEnd;
            if(!string.IsNullOrEmpty(timevalString) && timevalString.StartsWith(inputStart, StringComparison.InvariantCulture) && timevalString.EndsWith(inputEnd))
            {
                int endIndex = timevalString.IndexOfAny([conf.Symbols.Value.TimeSeriesIntervalEnd, conf.Symbols.Value.ListSeparator]);
                Dictionary<string, TimeDimensionInterval> map = new()
                {
                    {conf.Tokens.Time.YearInterval, TimeDimensionInterval.Year},
                    {conf.Tokens.Time.HalfYearInterval, TimeDimensionInterval.HalfYear},
                    {conf.Tokens.Time.QuarterYearInterval, TimeDimensionInterval.Quarter},
                    {conf.Tokens.Time.MonthInterval, TimeDimensionInterval.Month},
                    {conf.Tokens.Time.WeekInterval, TimeDimensionInterval.Week}
                };

                if(map.TryGetValue(timevalString[inputStart.Length..endIndex], out TimeDimensionInterval interval)) return interval;
                else return TimeDimensionInterval.Other;
            }
            else
            {
                throw new ArgumentException($"Invalid time interval string {input}");
            }
        }

        /// <summary>
        /// Removes the interval entry from the beginning of a time value string.
        /// and returns the rest split into a list of strings.
        /// </summary>
        /// <param name="input">The complete timeval value in one language.</param>
        /// <param name="conf">Configuration used for parsing the value strings and the interval part.</param>
        /// <returns>
        /// List of value strings excluding the interval part.
        /// If the input string is in the range format, empty list is returned.
        /// </returns>
        public static List<string> GetTimeValValueList(string input, PxFileConfiguration? conf = null)
        {
            conf ??= PxFileConfiguration.Default;
            int endOftoken = input.IndexOf(conf.Symbols.Value.TimeSeriesIntervalEnd);
            int firstStringDelimeter = input.IndexOf(conf.Symbols.Value.StringDelimeter, endOftoken);
            if (firstStringDelimeter >= 0)
            {
                return input[firstStringDelimeter..]
                    .SplitToListOfStrings(conf.Symbols.Value.ListSeparator, conf.Symbols.Value.StringDelimeter);
            }
            else return [];
        }

        /// <summary>
        /// Removes the interval entry from the beginning of a time value string
        /// and returns the rest as a string if the range format is used.
        /// </summary>
        /// <param name="input">The complete timeval value in one language.</param>
        /// <param name="conf">Configuration used for parsing the value strings and the interval part.</param>
        /// <returns> Value string excluding the interval part. If the right format is not used an exception is thrown.</returns>
        /// <exception cref="ArgumentException">Thrown when the input string is not in the correct format.</exception>
        public static string GetTimeValValueRangeString(string input, PxFileConfiguration? conf = null)
        {
            conf ??= PxFileConfiguration.Default;
            int startOfRange = input.IndexOf(conf.Symbols.Value.ListSeparator);
            int endOfToken = input.IndexOf(conf.Symbols.Value.TimeSeriesIntervalEnd);
            if (startOfRange == -1 || endOfToken < startOfRange)
            {
                throw new ArgumentException($"Invalid time value range string. {input} range is not defined inside the time interval token");
            }
            string range = input[(startOfRange + 1)..endOfToken].Trim();
            if (range.Count(c => c == conf.Symbols.Value.StringDelimeter) != 2 ||
                (range[0] != conf.Symbols.Value.StringDelimeter || range[^1] != conf.Symbols.Value.StringDelimeter) ||
                range.Count(c => c == conf.Symbols.Value.TimeSeriesLimitsSeparator) != 1)
            {
                throw new ArgumentException($"Invalid time value range string. {input} is not in valid range format.");
            }
            else
            {
                return range[1..^1];
            }
        }

        /// <summary>
        /// Parses a string into a <see cref="DimensionType"/> enumeration value.
        /// This method maps the input string to a <see cref="DimensionType"/> enumeration value based on the provided or default PxFileConfiguration configuration.
        /// If the input string does not map to a known <see cref="DimensionType"/>, the method returns <see cref="DimensionType.Unknown"/>.
        /// </summary>
        /// <param name="input">The string to parse into a <see cref="DimensionType"/> enumeration value.</param>
        /// <param name="conf">An optional <see cref="PxFileConfiguration"/> object. If not provided, the default configuration is used.</param>
        /// <returns>A <see cref="DimensionType"/> enumeration value parsed from the input string.</returns>
        public static DimensionType StringToDimensionType(string input, PxFileConfiguration? conf = null)
        {
            conf ??= PxFileConfiguration.Default;
            Dictionary<string, DimensionType> map = new()
            {
                {conf.Tokens.VariableTypes.Content, DimensionType.Content},
                {conf.Tokens.VariableTypes.Time, DimensionType.Time},
                {conf.Tokens.VariableTypes.Ordinal, DimensionType.Ordinal},
                {conf.Tokens.VariableTypes.Nominal, DimensionType.Nominal},
                {conf.Tokens.VariableTypes.Geographical, DimensionType.Geographical},
                {conf.Tokens.VariableTypes.Other, DimensionType.Other},
                {conf.Tokens.VariableTypes.Unknown, DimensionType.Unknown}
            };

            string cleanString = input.CleanStringDelimeters(conf.Symbols.Value.StringDelimeter);
            if (map.TryGetValue(cleanString, out DimensionType value)) return value;
            else return DimensionType.Unknown;
        }
    }
}
