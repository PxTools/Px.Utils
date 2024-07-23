using Px.Utils.PxFile;
using Px.Utils.PxFile.Metadata;
using Px.Utils.Validation.SyntaxValidation;
using System.Runtime.CompilerServices;
using System.Text;

namespace Px.Utils.Validation.DatabaseValidation
{
    /// <summary>
    /// Validates a whole px file database including all px files it contains.
    /// </summary>
    /// <param name="directoryPath">Path to the database root directory</param>
    /// <param name="syntaxConf"><see cref="PxFileSyntaxConf"/> object that defines the tokens and symbols for px file syntax.</param>
    /// <param name="customPxFileValidators">Optional custom <see cref="IDatabaseValidator"/> validator functions ran for each px file within the database</param>
    /// <param name="customAliasFileValidators">Optional custom <see cref="IDatabaseValidator"/> validator functions that are ran for each alias file within the database</param>
    /// <param name="customDirectoryValidators">Optional custom <see cref="IDatabaseValidator"/> validator functions for each subdirectory within the database.</param>
    /// <param name="fileSystem">Optional <see cref="IFileSystem"/> that defines the file system used for the validation process. Default file system is used if none provided.</param>
    public class DatabaseValidator(
        string directoryPath, 
        PxFileSyntaxConf? syntaxConf = null,
        IDatabaseValidator[]? customPxFileValidators = null,
        IDatabaseValidator[]? customAliasFileValidators = null,
        IDatabaseValidator[]? customDirectoryValidators = null,
        IFileSystem? fileSystem = null
        ) : IPxFileValidator, IPxFileValidatorAsync
    {
        private readonly string _directoryPath = directoryPath;
        private readonly PxFileSyntaxConf _syntaxConf = syntaxConf is not null ? syntaxConf : PxFileSyntaxConf.Default;
        private readonly IDatabaseValidator[]? _customPxFileValidators = customPxFileValidators;
        private readonly IDatabaseValidator[]? _customAliasFileValidators = customAliasFileValidators;
        private readonly IDatabaseValidator[]? _customDirectoryValidators = customDirectoryValidators;
        private readonly IFileSystem _fileSystem = fileSystem is not null ? fileSystem : new DefaultFileSystem();

        /// <summary>
        /// Blocking px file database validation process.
        /// </summary>
        /// <returns><see cref="ValidationResult"/> object that contains feedback gathered during the validation process.</returns>
        public ValidationResult Validate()
        {
            List<ValidationFeedbackItem> feedbacks = [];
            List<DatabaseFileInfo> pxFiles = [];
            List<DatabaseFileInfo> aliasFiles = [];

            IEnumerable<string> pxFilePaths = _fileSystem.EnumerateFiles(_directoryPath, "*.px");

            foreach (string fileName in pxFilePaths)
            {
                Stream stream = _fileSystem.GetFileStream(fileName);
                DatabaseFileInfo fileInfo = GetPxFileInfo(fileName, stream);
                pxFiles.Add(fileInfo);
                stream.Position = 0;
                PxFileValidator validator = new(stream, fileName, fileInfo.Encoding, _syntaxConf);
                ValidationResult result = validator.Validate();
                feedbacks.AddRange(result.FeedbackItems);
            }

            IEnumerable<string> aliasFilePaths = _fileSystem.EnumerateFiles(_directoryPath, "Alias_*.txt");
            foreach (string fileName in aliasFilePaths)
            {
                Stream stream = _fileSystem.GetFileStream(fileName);
                DatabaseFileInfo fileInfo = GetAliasFileInfo(fileName, stream);
                aliasFiles.Add(fileInfo);
            }

            ValidateDatabaseContents(pxFiles, aliasFiles, ref feedbacks);
            return new ValidationResult([..feedbacks]);
        }

        /// <summary>
        /// Asynchronous process for validating a px file database.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns><see cref="ValidationResult"/> object that contains feedback gathered during the validation process.</returns>
        public async Task<ValidationResult> ValidateAsync(CancellationToken cancellationToken = default)
        {
            List<ValidationFeedbackItem> feedbacks = [];
            List<DatabaseFileInfo> pxFiles = [];
            List<DatabaseFileInfo> aliasFiles = [];

            IEnumerable<string> pxFilePaths = _fileSystem.EnumerateFiles(_directoryPath, "*.px");
            foreach (string fileName in pxFilePaths)
            {
                Stream stream = _fileSystem.GetFileStream(fileName);
                DatabaseFileInfo fileInfo = await GetPxFileInfoAsync(fileName, stream, cancellationToken);
                pxFiles.Add(fileInfo);
                stream.Position = 0;
                PxFileValidator validator = new(stream, fileName, fileInfo.Encoding, _syntaxConf);
                ValidationResult result = await validator.ValidateAsync(cancellationToken);
                feedbacks.AddRange(result.FeedbackItems);

                if (cancellationToken.IsCancellationRequested)
                {
                    return new ValidationResult([..feedbacks]);
                }
            }

            IEnumerable<string> aliasFilePaths = _fileSystem.EnumerateFiles(_directoryPath, "Alias_*.txt");
            foreach (string filenName in aliasFilePaths)
            {
                Stream stream = _fileSystem.GetFileStream(filenName);
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
            ValidateDirectories(aliasFiles, databaseLanguages, ref feedbacks);
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

        private void ValidateDirectories(List<DatabaseFileInfo> aliasFiles, IEnumerable<string> databaseLanguages, ref List<ValidationFeedbackItem> feedbacks)
        {
            IDatabaseValidator[] directoryValidators =
            [
                new MissingAliasFiles(aliasFiles, databaseLanguages),
            ];
            if (_customDirectoryValidators is not null)
            {
                directoryValidators = [.. directoryValidators, .. _customDirectoryValidators];
            }

            IEnumerable<string> allDirectories = _fileSystem.EnumerateDirectories(_directoryPath);
            foreach (string directory in allDirectories)
            {
                string directoryName = new DirectoryInfo(directory).Name;
                if (directoryName == _syntaxConf.Tokens.Database.Index) continue;

                foreach (IDatabaseValidator validator in directoryValidators)
                {
                    ValidationFeedbackItem? feedback = validator.Validate(new DatabaseValidationItem(directory));
                    if (feedback is not null)
                    {
                        feedbacks.Add((ValidationFeedbackItem)feedback);
                    }
                }
            }
        }

        private DatabaseFileInfo GetPxFileInfo(string filename, Stream stream)
        {
            string name = _fileSystem.GetFileName(filename);
            string? path = _fileSystem.GetDirectoryName(filename);
            string location =  path is not null ? path : string.Empty;
            string[] languages = [];
            PxFileMetadataReader metadataReader = new ();
            Encoding encoding = Encoding.Default;
            try
            {
                encoding = metadataReader.GetEncoding(stream, _syntaxConf);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while reading the encoding of the file {filename}: {e.Message}");
            }
            stream.Position = 0;
            const int bufferSize = 1024;
            bool isProcessingString = false;
            using StreamReader streamReader = new(stream, encoding, leaveOpen: true);
            StringBuilder entryBuilder = new();
            char[] buffer = new char[bufferSize];
            string defaultLanguage = string.Empty;

            while (languages.Length == 0 && (streamReader.Read(buffer, 0, bufferSize) > 0))
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    ProcessBuffer(buffer[i], ref entryBuilder, ref isProcessingString, ref defaultLanguage, ref languages);
                }
            }

            DatabaseFileInfo fileInfo = new (name, location, languages, encoding);
            return fileInfo;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBuffer(char character, ref StringBuilder entryBuilder, ref bool isProcessingString, ref string defaultLanguage, ref string[] languages)
        {
            if (SyntaxValidator.IsEndOfMetadataSection(character, _syntaxConf, entryBuilder, isProcessingString) && defaultLanguage != string.Empty)
            {
                languages = [defaultLanguage.Trim(_syntaxConf.Symbols.Key.StringDelimeter)];
            }
            else if (character == _syntaxConf.Symbols.Key.StringDelimeter)
            {
                isProcessingString = !isProcessingString;
            }
            else if (character == _syntaxConf.Symbols.EntrySeparator && !isProcessingString)
            {
                ProcessEntry(entryBuilder, ref defaultLanguage, ref languages);
                entryBuilder.Clear();
            }
            else
            {
                entryBuilder.Append(character);
            }
        }

        private async Task<DatabaseFileInfo> GetPxFileInfoAsync(string filename, Stream stream, CancellationToken cancellationToken)
        {
            string name = _fileSystem.GetFileName(filename);
            string? path = _fileSystem.GetDirectoryName(filename);
            string location =  path is not null ? path : string.Empty;
            string[] languages = [];
            PxFileMetadataReader metadataReader = new ();
            Encoding encoding = Encoding.Default;
            try
            {
                encoding = await metadataReader.GetEncodingAsync(stream, _syntaxConf, cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while reading the encoding of the file {filename}: {e.Message}");
            }
            stream.Position = 0;
            const int bufferSize = 1024;
            bool isProcessingString = false;
            using StreamReader streamReader = new(stream, encoding, leaveOpen: true);
            StringBuilder entryBuilder = new();
            char[] buffer = new char[bufferSize];
            string defaultLanguage = string.Empty;
            int read = 0;
            do
            {
                cancellationToken.ThrowIfCancellationRequested();
                read = await streamReader.ReadAsync(buffer.AsMemory(), cancellationToken);
                for (int i = 0; i < buffer.Length; i++)
                {
                    ProcessBuffer(buffer[i], ref entryBuilder, ref isProcessingString, ref defaultLanguage, ref languages);
                }
            } while (languages.Length == 0 && read > 0);

            DatabaseFileInfo fileInfo = new (name, location, languages, encoding);
            return fileInfo;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessEntry(StringBuilder entryBuilder, ref string defaultLanguage, ref string[] languages)
        {
            string[] entry = entryBuilder.ToString().Trim().Split(_syntaxConf.Symbols.KeywordSeparator);
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

        private DatabaseFileInfo GetAliasFileInfo(string filename, Stream stream)
        {
            string name = _fileSystem.GetFileName(filename);
            string? path = _fileSystem.GetDirectoryName(filename);
            string location =  path is not null ? path : string.Empty;
            string[] languages = [
                name.Split('_')[1].Split('.')[0]
            ];

            Encoding encoding = _fileSystem.GetEncoding(stream);
            DatabaseFileInfo fileInfo = new (name, location, languages, encoding);
            return fileInfo;
        }
    }

    public class DatabaseValidationItem(string path)
    {
        public string Path { get; } = path;
    }

    public class DatabaseFileInfo(string name, string location, string[] languages, Encoding encoding) : DatabaseValidationItem(name)
    {
        public string Name { get; } = name;
        public string Location { get; } = location;
        public string[] Languages { get; } = languages;
        public Encoding Encoding { get; } = encoding;
    }


    public interface IDatabaseValidator
    {
        public ValidationFeedbackItem? Validate(DatabaseValidationItem item);
    }
}
