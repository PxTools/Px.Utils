using PxUtils.Models.Metadata;
using PxUtils.PxFile;

namespace Px.Utils.ModelBuilders
{
    public class PropertyBuilder
    {
        private readonly char _langParamStart;
        private readonly char _langParamEnd;

        private readonly char _spesifierParamStart;
        private readonly char _spesifierParamEnd;

        private readonly char _stringDelimeter;
        private readonly char _listSeparator;

        private readonly char[] _illlegalKeyTokes;

        public PropertyBuilder(PxFileSyntaxConf? configuration = null)
        {
            PxFileSyntaxConf _conf = configuration ?? PxFileSyntaxConf.Default;
            _langParamStart = _conf.Symbols.Key.LangParamStart;
            _langParamEnd = _conf.Symbols.Key.LangParamEnd;

            _spesifierParamStart = _conf.Symbols.Key.SpecifierParamStart;
            _spesifierParamEnd = _conf.Symbols.Key.SpecifierParamEnd;

            _stringDelimeter = _conf.Symbols.Key.StringDelimeter;
            _listSeparator = _conf.Symbols.Key.ListSeparator;

            _illlegalKeyTokes = [ 
                _conf.Symbols.Key.LangParamEnd,
                _conf.Symbols.Key.SpecifierParamEnd,
                _conf.Symbols.Key.StringDelimeter,
                _conf.Symbols.Key.ListSeparator,
                ' ', '\r', '\n', '\t'
            ];
        }

        public MetadataEntryKey ParseMetadataEntryKey(string key)
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

            if (output.IndexOfAny(_illlegalKeyTokes) >= 0) throw new ArgumentException($"Illegal key name {output}");

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
                return remaining[1..^1]
                    .Split(_listSeparator)
                    .Select(s => ValidateAndTrimSpecifierValue(s))
                    .ToList();
            }
            else
            {
                throw new ArgumentException($"Unable to parse key specifier from input {remaining}");
            }
        }

        private string ValidateAndTrimSpecifierValue(string input)
        {
            if (input.Length > 2 && input[0] == _stringDelimeter && input[^1] == _stringDelimeter)
            {
                string trimmed = input[1..^1];
                if (!trimmed.Contains(_stringDelimeter)) return trimmed;
            }
            throw new ArgumentException($"Invalid symbol found when parsing specifer value from string {input}");
        }
    }
}
