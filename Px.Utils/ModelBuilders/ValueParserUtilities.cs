using PxUtils.Language;
using PxUtils.Models.Metadata;
using PxUtils.Models.Metadata.Enums;
using PxUtils.PxFile;

namespace PxUtils.ModelBuilders
{
    public static class ValueParserUtilities
    {
        public static List<string> ParseStringList(string input, char listSeparator, char stringDelimeter)
        {
            List<string> list = [];

            if(string.IsNullOrEmpty(input)) return list;

            bool inString = false;
            bool newSection = true;
            int strStartIndex = 0;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c == stringDelimeter)
                {
                    if(!newSection && !inString) throw new ArgumentException($"Invalid symbol {c} found in input string {input}");
                    inString = !inString;
                    newSection = false;
                }
                else if (!inString && !char.IsWhiteSpace(c))
                {
                    if (c == listSeparator)
                    {
                        list.Add(input[strStartIndex..i].Trim().Trim(stringDelimeter));
                        strStartIndex = i + 1;
                        newSection = true;
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid symbol {c} found in input string {input}");
                    }
                }
            }
            if (inString) throw new ArgumentException($"String not closed in input string {input}");
            if (newSection) throw new ArgumentException($"Invalid input string {input}");
            list.Add(input[strStartIndex..].Trim().Trim(stringDelimeter));

            return list;
        }

        public static List<MultilanguageString> ParseListOfMultilanguageStrings(Property input, char listSeparator, char stringDelimeter)
        {
            if (!input.CanGetMultilanguageValue) throw new ArgumentException($"Property {input.Entries.First().Key.KeyWord} does not have multilanguage value");
            MultilanguageString values = input.GetMultiLanguageString();
            Dictionary<string, List<string>> langToListDict = [];
            
            // Build dictionary of language to list of strings
            foreach (string lang in values.Languages) langToListDict[lang] = ParseStringList(values[lang], listSeparator, stringDelimeter);
            
            // Create list of multilanguage strings
            return langToListDict.First().Value.Select((_, index) =>
                {
                    IEnumerable<KeyValuePair<string, string>> translations = langToListDict.Keys
                        .Select(lang => new KeyValuePair<string, string>(lang, langToListDict[lang][index])); 
                    return new MultilanguageString(translations);
                }).ToList();
        }

        public static TimeDimensionInterval ParseTimeIntervalFromTimeVal(string input, PxFileSyntaxConf? conf = null)
        {
            conf ??= PxFileSyntaxConf.Default;
            string inputStart = conf.Tokens.Time.TimeIntervalIndicator + conf.Symbols.Value.TimeSeriesIntervalStart;
            char inputEnd = conf.Symbols.Value.TimeSeriesIntervalEnd;
            if(input.StartsWith(inputStart) && input.EndsWith(inputEnd))
            {
                Dictionary<string, TimeDimensionInterval> map = new()
                {
                    {conf.Tokens.Time.YearInterval, TimeDimensionInterval.Year},
                    {conf.Tokens.Time.HalfYearInterval, TimeDimensionInterval.HalfYear},
                    {conf.Tokens.Time.QuarterYearInterval, TimeDimensionInterval.Quarter},
                    {conf.Tokens.Time.MonthInterval, TimeDimensionInterval.Month},
                    {conf.Tokens.Time.WeekInterval, TimeDimensionInterval.Week}
                };

                if(map.TryGetValue(input[6..^1], out TimeDimensionInterval interval)) return interval;
                else return TimeDimensionInterval.Other;
            }
            else
            {
                throw new ArgumentException($"Invalid time interval string {input}");
            }
        }
        
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
            else throw new ArgumentException($"Invalid dimension type string {input}");
        }
    }
}
