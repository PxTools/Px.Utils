using Px.Utils.PxFile;
using Px.Utils.PxFile.Metadata;
using Px.Utils.Validation.SyntaxValidation;
using System.Text;

namespace Px.Utils.Validation.DatabaseValidation
{
    /// <summary>
    /// TODO: Add summary
    /// </summary>
    public class DatabaseValidator(
        string folderPath, 
        PxFileSyntaxConf? syntaxConf = null,
        IDatabaseValidator[]? customPxFileValidators = null,
        IDatabaseValidator[]? customAliasFileValidators = null,
        IDatabaseValidator[]? customFolderValidators = null
        ) : IPxFileValidator, IPxFileValidatorAsync
    {
        private readonly string _folderPath = folderPath;
        private readonly PxFileSyntaxConf _syntaxConf = syntaxConf is not null ? syntaxConf : PxFileSyntaxConf.Default;
        private readonly IDatabaseValidator[]? _customPxFileValidators = customPxFileValidators;
        private readonly IDatabaseValidator[]? _customAliasFileValidators = customAliasFileValidators;
        private readonly IDatabaseValidator[]? _customFolderValidators = customFolderValidators;

        /// <summary>
        /// TODO: Add summary
        /// </summary>
        public ValidationResult Validate()
        {
            List<ValidationFeedbackItem> feedbacks = [];
            List<DatabaseFileInfo> pxFiles = [];
            List<DatabaseFileInfo> aliasFiles = [];

            IEnumerable<string> pxFilePaths = Directory.EnumerateFiles(_folderPath, "*.px", SearchOption.AllDirectories);

            foreach (string fileName in pxFilePaths)
            {
                using FileStream stream = new(fileName, FileMode.Open, FileAccess.Read);
                DatabaseFileInfo fileInfo = GetPxFileInfo(fileName, stream);
                pxFiles.Add(fileInfo);

                PxFileValidator validator = new (stream, fileName, fileInfo.Encoding, _syntaxConf);
                ValidationResult result = validator.Validate();
                feedbacks.AddRange(result.FeedbackItems);
            }

            IEnumerable<string> aliasFilePaths = Directory.EnumerateFiles(_folderPath, "Alias_*.txt", SearchOption.AllDirectories);
            foreach (string filenName in aliasFilePaths)
            {
                using FileStream stream = new(filenName, FileMode.Open, FileAccess.Read);
                DatabaseFileInfo fileInfo = GetAliasFileInfo(filenName, stream);
                aliasFiles.Add(fileInfo);
            }

            ValidateDatabaseContents(pxFiles, aliasFiles, ref feedbacks);

            return new ValidationResult([..feedbacks]);
        }

        /// <summary>
        /// TODO: Add summary
        /// </summary>
        public async Task<ValidationResult> ValidateAsync(CancellationToken cancellationToken = default)
        {
            List<ValidationFeedbackItem> feedbacks = [];
            List<DatabaseFileInfo> pxFiles = [];
            List<DatabaseFileInfo> aliasFiles = [];

            IEnumerable<string> pxFilePaths = Directory.EnumerateFiles(_folderPath, "*.px", SearchOption.AllDirectories);
            foreach (string fileName in pxFilePaths)
            {
                using FileStream stream = new(fileName, FileMode.Open, FileAccess.Read);
                DatabaseFileInfo fileInfo = await GetPxFileInfoAsync(fileName, stream);
                pxFiles.Add(fileInfo);

                PxFileValidator validator = new(stream, fileName, fileInfo.Encoding, _syntaxConf);
                ValidationResult result = await validator.ValidateAsync(cancellationToken);
                feedbacks.AddRange(result.FeedbackItems);

                if (cancellationToken.IsCancellationRequested)
                {
                    return new ValidationResult([..feedbacks]);
                }
            }

            IEnumerable<string> aliasFilePaths = Directory.EnumerateFiles(_folderPath, "Alias_*.txt", SearchOption.AllDirectories);
            foreach (string filenName in aliasFilePaths)
            {
                using FileStream stream = new(filenName, FileMode.Open, FileAccess.Read);
                DatabaseFileInfo fileInfo = GetAliasFileInfo(filenName, stream);
                aliasFiles.Add(fileInfo);

                if (cancellationToken.IsCancellationRequested)
                {
                    return new ValidationResult([..feedbacks]);
                }
            }

            ValidateDatabaseContents(pxFiles, aliasFiles, ref feedbacks);

            return new ValidationResult([..feedbacks]);
        }

        private void ValidateDatabaseContents(List<DatabaseFileInfo> pxFiles, List<DatabaseFileInfo> aliasFiles, ref List<ValidationFeedbackItem> feedbacks)
        {
            IEnumerable<DatabaseFileInfo> allFiles = pxFiles.Concat(aliasFiles);
            IEnumerable<string> databaseLanguages = pxFiles.SelectMany(file => file.Languages).Distinct();
            Encoding mostCommonEncoding = allFiles.Select(file => file.Encoding)
                .GroupBy(enc => enc)
                .OrderByDescending(group => group.Count())
                .First()
                .Key;

            ValidatePxFiles(pxFiles, databaseLanguages, mostCommonEncoding, ref feedbacks);
            ValidateAliasFiles(aliasFiles, mostCommonEncoding, ref feedbacks);
            ValidateFolders(aliasFiles, databaseLanguages, ref feedbacks);
        }

        private void ValidatePxFiles(List<DatabaseFileInfo> pxFiles, IEnumerable<string> databaseLanguages, Encoding mostCommonEncoding, ref List<ValidationFeedbackItem> feedbacks)
        {
            IDatabaseValidator[] pxFileValidators =
            [
                new DuplicatePxFileName(pxFiles),
                new MissingPxFileLanguages(databaseLanguages),
                new MismatchingEncoding(mostCommonEncoding),
            ];
            if (_customPxFileValidators is not null)
            {
                pxFileValidators = [.. pxFileValidators, .. _customPxFileValidators];
            }

            foreach (DatabaseFileInfo fileInfo in pxFiles)
            {
                foreach (IDatabaseValidator validator in pxFileValidators)
                {
                    ValidationFeedbackItem? feedback = validator.Validate(fileInfo);
                    if (feedback is not null)
                    {
                        feedbacks.Add((ValidationFeedbackItem)feedback);
                    }
                }
            }
        }

        private void ValidateAliasFiles(List<DatabaseFileInfo> aliasFiles, Encoding mostCommonEncoding, ref List<ValidationFeedbackItem> feedbacks)
        {
            IDatabaseValidator[] aliasFileValidators =
            [
                new MismatchingEncoding(mostCommonEncoding),
            ];
            if (_customAliasFileValidators is not null)
            {
                aliasFileValidators = [.. aliasFileValidators, .. _customAliasFileValidators];
            }

            foreach (DatabaseFileInfo fileInfo in aliasFiles)
            {
                foreach (IDatabaseValidator validator in aliasFileValidators)
                {
                    ValidationFeedbackItem? feedback = validator.Validate(fileInfo);
                    if (feedback is not null)
                    {
                        feedbacks.Add((ValidationFeedbackItem)feedback);
                    }
                }
            }
        }

        private void ValidateFolders(List<DatabaseFileInfo> aliasFiles, IEnumerable<string> databaseLanguages, ref List<ValidationFeedbackItem> feedbacks)
        {
            IDatabaseValidator[] folderValidators =
            [
                new MissingAliasFiles(aliasFiles, databaseLanguages),
            ];
            if (_customFolderValidators is not null)
            {
                folderValidators = [.. folderValidators, .. _customFolderValidators];
            }

            IEnumerable<string> allFolders = Directory.EnumerateDirectories(_folderPath, "*", SearchOption.AllDirectories);
            foreach (string folder in allFolders)
            {
                foreach (IDatabaseValidator validator in folderValidators)
                {
                    ValidationFeedbackItem? feedback = validator.Validate(new DatabaseValidationItem(folder));
                    if (feedback is not null)
                    {
                        feedbacks.Add((ValidationFeedbackItem)feedback);
                    }
                }
            }
        }

        private DatabaseFileInfo GetPxFileInfo(string filename, Stream stream)
        {
            string name = Path.GetFileName(filename);
            string? path = Path.GetDirectoryName(filename);
            string location =  path is not null ? path : string.Empty;
            string[] languages = [];
            PxFileMetadataReader metadataReader = new ();
            Encoding encoding = metadataReader.GetEncoding(stream, _syntaxConf);

            const int bufferSize = 1024;
            bool isProcessingString = false;
            using StreamReader streamReader = new(stream, encoding);
            StringBuilder entryBuilder = new();
            char[] buffer = new char[bufferSize];
            string defaultLanguage = string.Empty;

            while (languages.Length == 0 && (streamReader.Read(buffer, 0, bufferSize) > 0))
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    if (SyntaxValidator.IsEndOfMetadataSection(buffer[i], _syntaxConf, entryBuilder, isProcessingString) && defaultLanguage != string.Empty)
                    {
                        languages = [defaultLanguage.Trim(_syntaxConf.Symbols.Key.StringDelimeter)];
                    }
                    else if (buffer[i] == _syntaxConf.Symbols.Key.StringDelimeter)
                    {
                        isProcessingString = !isProcessingString;
                    }
                    else if (buffer[i] == _syntaxConf.Symbols.EntrySeparator && !isProcessingString)
                    {
                        ProcessEntry(entryBuilder, ref defaultLanguage, ref languages);
                    }
                    else
                    {
                        entryBuilder.Append(buffer[i]);
                    }
                }
            }

            DatabaseFileInfo fileInfo = new (name, location, languages, encoding);
            return fileInfo;
        }

        private async Task<DatabaseFileInfo> GetPxFileInfoAsync(string filename, Stream stream)
        {
            string name = Path.GetFileName(filename);
            string? path = Path.GetDirectoryName(filename);
            string location =  path is not null ? path : string.Empty;
            string[] languages = [];
            PxFileMetadataReader metadataReader = new ();
            Encoding encoding = await metadataReader.GetEncodingAsync(stream, _syntaxConf);

            const int bufferSize = 1024;
            bool isProcessingString = false;
            using StreamReader streamReader = new(stream, encoding);
            StringBuilder entryBuilder = new();
            char[] buffer = new char[bufferSize];
            string defaultLanguage = string.Empty;

            while (languages.Length == 0 && (await streamReader.ReadAsync(buffer, 0, bufferSize) > 0))
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    if (SyntaxValidator.IsEndOfMetadataSection(buffer[i], _syntaxConf, entryBuilder, isProcessingString) && defaultLanguage != string.Empty)
                    {
                        languages = [defaultLanguage.Trim(_syntaxConf.Symbols.Key.StringDelimeter)];
                    }
                    else if (buffer[i] == _syntaxConf.Symbols.Key.StringDelimeter)
                    {
                        isProcessingString = !isProcessingString;
                    }
                    else if (buffer[i] == _syntaxConf.Symbols.EntrySeparator && !isProcessingString)
                    {
                        ProcessEntry(entryBuilder, ref defaultLanguage, ref languages);
                    }
                    else
                    {
                        entryBuilder.Append(buffer[i]);
                    }
                }
            }

            DatabaseFileInfo fileInfo = new (name, location, languages, encoding);
            return fileInfo;
        }

        private void ProcessEntry(StringBuilder entryBuilder, ref string defaultLanguage, ref string[] languages)
        {
            string[] entry = entryBuilder.ToString().Split(_syntaxConf.Symbols.KeywordSeparator);
            if (entry[0] == _syntaxConf.Tokens.KeyWords.DefaultLanguage)
            {
                defaultLanguage = entry[1];
            }
            else if (entry[0] == _syntaxConf.Tokens.KeyWords.AvailableLanguages)
            {
                languages = entry[1].Split(_syntaxConf.Symbols.Value.ListSeparator);
                for (int j = 0; j < languages.Length; j++)
                {
                    languages[j] = languages[j].Trim(_syntaxConf.Symbols.Key.StringDelimeter);
                }
            }
        }

        private static DatabaseFileInfo GetAliasFileInfo(string filename, Stream stream)
        {
            string name = Path.GetFileName(filename);
            string? path = Path.GetDirectoryName(filename);
            string location =  path is not null ? path : string.Empty;
            string[] languages = [
                name.Split('_')[1].Split('.')[0]
            ];

            Encoding encoding = GetEncoding(stream);
            DatabaseFileInfo fileInfo = new (name, location, languages, encoding);
            return fileInfo;
        }

        private static Encoding GetEncoding(Stream stream)
        {
            const int bomLength = 3;
            long position = stream.Position;

            byte[] bom = new byte[bomLength];
            stream.Read(bom);
            stream.Position = position;

            if (PxFileMetadataReader.GetEncodingFromBOM(bom) is Encoding utf) return utf;

            return Encoding.ASCII;
        }
    }

    public class DatabaseValidationItem(string path)
    {
        public readonly string Path = path;
    }

    public class DatabaseFileInfo(string name, string location, string[] languages, Encoding encoding) : DatabaseValidationItem(name)
    {
        public readonly string Name = name;
        public readonly string Location = location;
        public readonly string[] Languages = languages;
        public readonly Encoding Encoding = encoding;
    }


    public interface IDatabaseValidator
    {
        public ValidationFeedbackItem? Validate(DatabaseValidationItem item);
    }
}
