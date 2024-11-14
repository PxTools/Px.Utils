using Px.Utils.Models.Metadata;
using Px.Utils.PxFile;
using System.Text;

namespace Px.Utils.ModelBuilders
{
    /// <summary>
    /// Class for building <see cref="MetadataEntryKey"/> records from strings.
    /// </summary>
    public class MetadataEntryKeyBuilder
    {
        private readonly char _langParamStart;
        private readonly char _langParamEnd;

        private readonly char _spesifierParamStart;
        private readonly char _spesifierParamEnd;

        private readonly char _stringDelimeter;
        private readonly char _listSeparator;

        private readonly char[] _illegalKeyTokens;

        /// <summary>
        /// Initializes a new instance of the MetadataEntryKeyBuilder class.
        /// This constructor takes an optional PxFileConfiguration object. If none is provided, it uses the default configuration.
        /// </summary>
        /// <param name="configuration">An optional configuration object for the Px file. If not provided, the default configuration is used.</param>
        public MetadataEntryKeyBuilder(PxFileConfiguration? configuration = null)
        {
            PxFileConfiguration _conf = configuration ?? PxFileConfiguration.Default;
            _langParamStart = _conf.Symbols.Key.LangParamStart;
            _langParamEnd = _conf.Symbols.Key.LangParamEnd;

            _spesifierParamStart = _conf.Symbols.Key.SpecifierParamStart;
            _spesifierParamEnd = _conf.Symbols.Key.SpecifierParamEnd;

            _stringDelimeter = _conf.Symbols.Key.StringDelimeter;
            _listSeparator = _conf.Symbols.Key.ListSeparator;

            _illegalKeyTokens = [ 
                _conf.Symbols.Key.LangParamEnd,
                _conf.Symbols.Key.SpecifierParamEnd,
                _conf.Symbols.Key.StringDelimeter,
                _conf.Symbols.Key.ListSeparator,
                ' ', '\r', '\n', '\t'
            ];
        }

        /// <summary>
        /// Parses a string into a <see cref="MetadataEntryKey"/> record.
        /// This method extracts the key name, language, and specifiers from the input string and constructs the record.
        /// It throws an exception if the input string is not in the correct format or contains illegal characters.
        /// </summary>
        /// <param name="key">The string to parse into a <see cref="MetadataEntryKey"/> record.</param>
        /// <returns>A <see cref="MetadataEntryKey"/> record constructed from the input string.</returns>
        public MetadataEntryKey Parse(string key)
        {
            string name = ParseKeyName(ref key);
            string? lang = ParseLang(ref key);

            List<string> specifiers = ParseSpecifier(key);
            string? firstIdentifier = specifiers.Count > 0 ? specifiers[0] : null;
            string? secondIdentifier = specifiers.Count > 1 ? specifiers[1] : null;

            if(specifiers.Count > 2) throw new ArgumentException($"Too many specifiers found in key {key}");

            return new MetadataEntryKey(name, lang, firstIdentifier, secondIdentifier);
        }

        private string ParseKeyName(ref string remaining)
        {
            char[] sectionStartTokens = [_langParamStart, _spesifierParamStart];
            string output = new(remaining);
            int index = remaining.IndexOfAny(sectionStartTokens);
            if (index < 0)
            {
                remaining = string.Empty;
            }
            else if (index > 0)
            {
                output = remaining[..index];
                remaining = remaining[index..];
            }
            else // 0 -> There is no legal key at the start of the string
            {
                throw new ArgumentException($"Can not parse key name from string {remaining}");
            }

            if (output.IndexOfAny(_illegalKeyTokens) >= 0) throw new ArgumentException($"Illegal key name {output}");

            return output.Trim();
        }

        private string? ParseLang(ref string remaining)
        {
            remaining = remaining.TrimStart();
            if (string.IsNullOrEmpty(remaining) || !remaining.StartsWith(_langParamStart))
            {
                return null;
            }
            else
            {
                int index = remaining.IndexOf(_langParamEnd);
                if (index > 1) // 1 because empty lang string is not valid
                {
                    string output = remaining[1..index].Trim();
                    remaining = remaining[(index + 1)..];
                    if (output.Contains(_langParamStart))
                    {
                        throw new ArgumentException($"Language identifier {output} contains invalid symbols");
                    }
                    return output;
                }
                else // <= 0, end symbol not found
                {
                    throw new ArgumentException($"Can not parse language identifier from string {remaining}");
                }
            }
        }

        private List<string> ParseSpecifier(string remaining)
        {
            remaining = remaining.Trim();
            if (remaining.Length == 0) return [];
            else if (remaining.Length > 4 && 
                remaining[0] == _spesifierParamStart &&
                remaining[^1] == _spesifierParamEnd)
            {
                return SplitSpecifierParts(remaining[1..^1], _listSeparator)
                    .Select(ValidateAndTrimSpecifierValue)
                    .ToList();
            }
            else
            {
                throw new ArgumentException($"Unable to parse key specifier from input {remaining}");
            }
        }

        private string ValidateAndTrimSpecifierValue(string input)
        {
            input = input.Trim();
            if (input.Length > 2 && input[0] == _stringDelimeter && input[^1] == _stringDelimeter)
            {
                string trimmed = input[1..^1];
                if (!trimmed.Contains(_stringDelimeter)) return trimmed;
            }
            throw new ArgumentException($"Invalid symbol found when parsing specifer value from string {input}");
        }

        private string[] SplitSpecifierParts(string input, char listSeparator)
        {
            bool inString = false;
            List<string> output = [];
            StringBuilder current = new();
            foreach (char c in input)
            {
                if (c == _stringDelimeter) inString = !inString;
                if (c == listSeparator && !inString)
                {
                    output.Add(current.ToString());
                    current.Clear();
                }
                else current.Append(c);
            }
            output.Add(current.ToString());
            return [.. output];
        }
    }
}
