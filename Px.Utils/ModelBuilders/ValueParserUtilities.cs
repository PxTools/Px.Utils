using PxUtils.Language;
using PxUtils.Models.Metadata;
using PxUtils.Models.Metadata.Enums;
using PxUtils.PxFile;

namespace PxUtils.ModelBuilders
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
        /// <param name="conf">An optional <see cref="PxFileSyntaxConf"/> object. If not provided, the default configuration is used.</param>
        /// <returns>A <see cref="TimeDimensionInterval"/> enumeration value parsed from the input string.</returns>
        public static TimeDimensionInterval ParseTimeIntervalFromTimeVal(string input, PxFileSyntaxConf? conf = null)
        {
            conf ??= PxFileSyntaxConf.Default;
            string intervalPart = new(input.TakeWhile(c => c != conf.Symbols.Value.ListSeparator).ToArray());
            string inputStart = conf.Tokens.Time.TimeIntervalIndicator + conf.Symbols.Value.TimeSeriesIntervalStart;
            char inputEnd = conf.Symbols.Value.TimeSeriesIntervalEnd;
            if(!string.IsNullOrEmpty(intervalPart) && intervalPart.StartsWith(inputStart) && intervalPart.EndsWith(inputEnd))
            {
                Dictionary<string, TimeDimensionInterval> map = new()
                {
                    {conf.Tokens.Time.YearInterval, TimeDimensionInterval.Year},
                    {conf.Tokens.Time.HalfYearInterval, TimeDimensionInterval.HalfYear},
                    {conf.Tokens.Time.QuarterYearInterval, TimeDimensionInterval.Quarter},
                    {conf.Tokens.Time.MonthInterval, TimeDimensionInterval.Month},
                    {conf.Tokens.Time.WeekInterval, TimeDimensionInterval.Week}
                };

                if(map.TryGetValue(intervalPart[6..^1], out TimeDimensionInterval interval)) return interval;
                else return TimeDimensionInterval.Other;
            }
            else
            {
                throw new ArgumentException($"Invalid time interval string {input}");
            }
        }

        /// <summary>
        /// Parses a string into a <see cref="DimensionType"/> enumeration value.
        /// This method maps the input string to a <see cref="DimensionType"/> enumeration value based on the provided or default PxFileSyntaxConf configuration.
        /// If the input string does not map to a known <see cref="DimensionType"/>, the method returns <see cref="DimensionType.Unknown"/>.
        /// </summary>
        /// <param name="input">The string to parse into a <see cref="DimensionType"/> enumeration value.</param>
        /// <param name="conf">An optional <see cref="PxFileSyntaxConf"/> object. If not provided, the default configuration is used.</param>
        /// <returns>A <see cref="DimensionType"/> enumeration value parsed from the input string.</returns>
        public static DimensionType StringToDimensionType(string input, PxFileSyntaxConf? conf = null)
        {
            conf ??= PxFileSyntaxConf.Default;
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

            if (map.TryGetValue(input, out DimensionType value)) return value;
            else return DimensionType.Unknown;
        }
    }
}
