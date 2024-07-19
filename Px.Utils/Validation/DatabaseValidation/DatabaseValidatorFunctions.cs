using System.Runtime.InteropServices;
using System.Text;

namespace Px.Utils.Validation.DatabaseValidation
{
    /// <summary>
    /// TODO: Summary
    /// </summary>
    /// <param name="pxFiles"></param>
    public class DuplicatePxFileName(List<DatabaseFileInfo> pxFiles) : IDatabaseValidator
    {
        private readonly List<DatabaseFileInfo> _pxFiles = pxFiles;

        public ValidationFeedbackItem? Validate(DatabaseValidationItem item)
        {
            if (item is DatabaseFileInfo fileInfo &&
                _pxFiles.Exists(file => file.Name == fileInfo.Name && file != fileInfo))
            {
                return new (
                    new(fileInfo.Name, 0, []),
                    new(ValidationFeedbackLevel.Warning,
                        ValidationFeedbackRule.DuplicateFileNames,
                        0,
                        0,
                        $"Duplicate file name: {fileInfo.Name}"));

            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// TODO: Summary
    /// </summary>
    public class MissingPxFileLanguages(IEnumerable<string> allLanguages) : IDatabaseValidator
    {
        private readonly IEnumerable<string> _allLanguages = allLanguages;

        public ValidationFeedbackItem? Validate(DatabaseValidationItem item)
        {
            if (item is DatabaseFileInfo fileInfo &&
                !_allLanguages.All(lang => fileInfo.Languages.Contains(lang)))
            {
                return new (
                    new(fileInfo.Name, 0, []),
                    new(ValidationFeedbackLevel.Warning,
                    ValidationFeedbackRule.FileLanguageDiffersFromDatabase,
                    0,
                    0,
                    $"Missing languages in file {fileInfo.Name}: {string.Join(", ", _allLanguages.Except(fileInfo.Languages))}"));
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// TODO: SUmmary
    /// </summary>
    public class MismatchingEncoding(Encoding mostCommonEncoding) : IDatabaseValidator
    {
        private readonly Encoding _mostCommonEncoding = mostCommonEncoding;

        public ValidationFeedbackItem? Validate(DatabaseValidationItem item)
        {
            if (item is DatabaseFileInfo fileInfo &&
                fileInfo.Encoding != _mostCommonEncoding)
            {
                return new (
                    new(fileInfo.Name, 0, []),
                    new(ValidationFeedbackLevel.Warning,
                    ValidationFeedbackRule.FileEncodingDiffersFromDatabase,
                    0,
                    0,
                    $"Inconsistent encoding in file {fileInfo.Name}: {fileInfo.Encoding.EncodingName}. " +
                    $"Most commonly used encoding is {_mostCommonEncoding.EncodingName}"));
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// TODO: SUmmary
    /// </summary>
    public class MissingAliasFiles(List<DatabaseFileInfo> aliasFiles, IEnumerable<string> allLanguages) : IDatabaseValidator
    {
        private readonly List<DatabaseFileInfo> _aliasFiles = aliasFiles;
        private readonly IEnumerable<string> _allLanguages = allLanguages;

        public ValidationFeedbackItem? Validate(DatabaseValidationItem item)
        {
            foreach (string language in _allLanguages)
            {
                if (!_aliasFiles.Exists(file => file.Languages.Contains(language)))
                {
                    return new(
                        new(item.Path, 0, []),
                        new(ValidationFeedbackLevel.Warning,
                        ValidationFeedbackRule.AliasFileMissing,
                        0,
                        0,
                        $"Alias file for {language} in {item.Path} is missing"
                        ));
                }
            }

            return null;
        }
    }
}
