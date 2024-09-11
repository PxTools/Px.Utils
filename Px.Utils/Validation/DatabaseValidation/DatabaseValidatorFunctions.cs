using System.Runtime.InteropServices;
using System.Text;

namespace Px.Utils.Validation.DatabaseValidation
{
    /// <summary>
    /// Validates that each px file within the database has a unique filename
    /// </summary>
    /// <param name="pxFiles"></param>
    public class DuplicatePxFileName(List<DatabaseFileInfo> pxFiles) : IDatabaseValidator
    {
        private readonly List<DatabaseFileInfo> _pxFiles = pxFiles;

        /// <summary>
        /// Validation function that checks if the filename of the given file is unique within the database
        /// </summary>
        /// <param name="item"><see cref="DatabaseValidationItem"/> object - in this case <see cref="DatabaseFileInfo"/> subject to validation</param>
        /// <returns>Null if no issues are found, a key value pair containing information about rule violation in case there are multiple files with the same filename.</returns>
        public KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? Validate(DatabaseValidationItem item)
        {
            if (item is DatabaseFileInfo fileInfo &&
                _pxFiles.Exists(file => file.Name == fileInfo.Name && file != fileInfo))
            {
                return new(
                    new(ValidationFeedbackLevel.Warning,
                        ValidationFeedbackRule.DuplicateFileNames),
                    new(fileInfo.Name, 0, 0, $"Duplicate file name: {fileInfo.Name}")
                );
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Validates that each px file within the database contains the same languages
    /// </summary>
    /// <param name="allLanguages">Codes of all languages that should be present in each px file</param>
    public class MissingPxFileLanguages(IEnumerable<string> allLanguages) : IDatabaseValidator
    {
        private readonly IEnumerable<string> _allLanguages = allLanguages;

        /// <summary>
        /// Validation function that checks if the given file contains all the languages that should be present in each px file
        /// </summary>
        /// <param name="item"><see cref="DatabaseValidationItem"/> object - in this case <see cref="DatabaseFileInfo"/> subject to validation</param>
        /// <returns>Null if no issues are found, a key value pair containing information about rule violation> in case some languages are missing</returns>
        public KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? Validate(DatabaseValidationItem item)
        {
            if (item is DatabaseFileInfo fileInfo &&
                !_allLanguages.All(lang => fileInfo.Languages.Contains(lang)))
            {
                return new(
                    new(ValidationFeedbackLevel.Warning,
                    ValidationFeedbackRule.FileLanguageDiffersFromDatabase),
                    new(fileInfo.Name, 0, 0, $"Missing languages in file {fileInfo.Name}: {string.Join(", ", _allLanguages.Except(fileInfo.Languages))}")
                );
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Validates that the encoding of each px and alias file within the database is consistent
    /// </summary>
    /// <param name="mostCommonEncoding">The most commonly used encoding within the database</param>
    public class MismatchingEncoding(Encoding mostCommonEncoding) : IDatabaseValidator
    {
        private readonly Encoding _mostCommonEncoding = mostCommonEncoding;

        /// <summary>
        /// Validation function that checks if the encoding of the given file is consistent with the most commonly used encoding
        /// </summary>
        /// <param name="item"><see cref="DatabaseValidationItem"/> object - in this case <see cref="DatabaseFileInfo"/> subject to validation</param>
        /// <returns>Null if no issues are found, a key value pair containing information about rule violation> in case the encoding is inconsistent</returns>
        public KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? Validate(DatabaseValidationItem item)
        {
            if (item is DatabaseFileInfo fileInfo &&
                fileInfo.Encoding != _mostCommonEncoding)
            {
                return new (
                    new(ValidationFeedbackLevel.Warning,
                    ValidationFeedbackRule.FileEncodingDiffersFromDatabase),
                    new(fileInfo.Name, 0, 0, $"Inconsistent encoding in file {fileInfo.Name}: {fileInfo.Encoding.EncodingName}. " +
                    $"Most commonly used encoding is {_mostCommonEncoding.EncodingName}"));
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Validates that each subdirectory within the database has an alias file for each language
    /// </summary>
    /// <param name="aliasFiles">List of alias files within the database</param>
    /// <param name="allLanguages">Codes of all languages that should be present in each alias file</param>
    public class MissingAliasFiles(List<DatabaseFileInfo> aliasFiles, IEnumerable<string> allLanguages) : IDatabaseValidator
    {
        private readonly List<DatabaseFileInfo> _aliasFiles = aliasFiles;
        private readonly IEnumerable<string> _allLanguages = allLanguages;

        /// <summary>
        /// Validation function that checks if the given subdirectory contains an alias file for each language
        /// </summary>
        /// <param name="item"><see cref="DatabaseValidationItem"/> object subject to validation</param>
        /// <returns>Null if no issues are found, a key value pair containing information about rule violation> in case some alias files are missing</returns>
        public KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? Validate(DatabaseValidationItem item)
        {
            foreach (string language in _allLanguages)
            {
                if (!_aliasFiles.Exists(file => file.Languages.Contains(language)))
                {
                    return new(
                        new(ValidationFeedbackLevel.Warning,
                        ValidationFeedbackRule.AliasFileMissing),
                        new(item.Path, 0, 0, $"Alias file for {language} in {item.Path} is missing")
                    );
                }
            }

            return null;
        }
    }
}
