using System.Collections.Immutable;

namespace PxUtils.Validation.SyntaxValidation
{
    public static class Bcp47Codes
    {
        public static readonly ImmutableHashSet<string> Codes =
        [
            "ar-SA", "cs-CZ", "da-DK", "de-DE", "el-GR", "en-AU",
            "en-GB", "en-IE", "en-US", "en-ZA", "es-ES", "es-MX",
            "fi-FI", "fr-CA", "fr-FR", "he-IL", "hi-IN", "hu-HU",
            "id-ID", "it-IT", "ja-JP", "ko-KR", "nl-BE", "nl-NL",
            "no-NO", "pl-PL", "pt-BR", "pt-PT", "ro-RO", "ru-RU",
            "sk-SK", "sv-SE", "th-TH", "tr-TR", "zh-CN", "zh-HK",
            "zh-TW"
        ];
    }
}
