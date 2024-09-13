using Px.Utils.Exceptions;
using Px.Utils.PxFile;
using Px.Utils.PxFile.Metadata;
using Px.Utils.Validation.SyntaxValidation;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;

namespace Px.Utils.Validation.DatabaseValidation
{
    /// <summary>
    /// Validates a whole px file database including all px files it contains.
    /// </summary>
    /// <param name="directoryPath">Path to the database root directory</param>
    /// <param name="fileSystem"><see cref="IFileSystem"/> object that defines the file system used for the validation process.</param>
    /// <param name="syntaxConf"><see cref="PxFileSyntaxConf"/> object that defines the tokens and symbols for px file syntax.</param>
    /// <param name="customPxFileValidators">Optional custom <see cref="IDatabaseValidator"/> validator functions ran for each px file within the database</param>
    /// <param name="customAliasFileValidators">Optional custom <see cref="IDatabaseValidator"/> validator functions that are ran for each alias file within the database</param>
    /// <param name="customDirectoryValidators">Optional custom <see cref="IDatabaseValidator"/> validator functions for each subdirectory within the database.</param>
    public class DatabaseValidator(
        string directoryPath,
        IFileSystem fileSystem,
        PxFileSyntaxConf? syntaxConf = null,
        IDatabaseValidator[]? customPxFileValidators = null,
        IDatabaseValidator[]? customAliasFileValidators = null,
        IDatabaseValidator[]? customDirectoryValidators = null
        ) : IValidator, IValidatorAsync
    {
        private readonly string _directoryPath = directoryPath;
        private readonly PxFileSyntaxConf _syntaxConf = syntaxConf is not null ? syntaxConf : PxFileSyntaxConf.Default;
        private readonly IDatabaseValidator[]? _customPxFileValidators = customPxFileValidators;
        private readonly IDatabaseValidator[]? _customAliasFileValidators = customAliasFileValidators;
        private readonly IDatabaseValidator[]? _customDirectoryValidators = customDirectoryValidators;
        private readonly IFileSystem _fileSystem = fileSystem is not null ? fileSystem : new LocalFileSystem();

        /// <summary>
        /// Blocking px file database validation process.
        /// </summary>
        /// <returns><see cref="ValidationResult"/> object that contains feedback gathered during the validation process.</returns>
        public ValidationResult Validate()
        {
            ValidationFeedback feedbacks = [];
            ConcurrentBag<DatabaseFileInfo> pxFiles = [];
            ConcurrentBag<DatabaseFileInfo> aliasFiles = [];
            List<Task> fileTasks = [];

            IEnumerable<string> pxFilePaths = _fileSystem.EnumerateFiles(_directoryPath, "*.px");
            foreach (string fileName in pxFilePaths)
            {
                fileTasks.Add(Task.Run(() =>
                {
                    (DatabaseFileInfo? file, ValidationFeedback feedback) = ProcessPxFile(fileName);
                    if (file != null) pxFiles.Add(file);
                    feedbacks.AddRange(feedback);
                }));
            }
            
            IEnumerable<string> aliasFilePaths = _fileSystem.EnumerateFiles(_directoryPath, "Alias_*.txt");
            foreach (string fileName in aliasFilePaths)
            {
                fileTasks.Add(Task.Run(() =>
                {
                    DatabaseFileInfo file = ProcessAliasFile(fileName);
                    aliasFiles.Add(file);
                }));
            }

            Task.WaitAll([.. fileTasks]);
            feedbacks.AddRange(ValidateDatabaseContents(pxFiles, aliasFiles));
            return new (feedbacks);
        }

        /// <summary>
        /// Asynchronous process for validating a px file database.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns><see cref="ValidationResult"/> object that contains feedback gathered during the validation process.</returns>
        public async Task<ValidationResult> ValidateAsync(CancellationToken cancellationToken = default)
        {
            ValidationFeedback feedbacks = [];
            ConcurrentBag<DatabaseFileInfo> pxFiles = [];
            ConcurrentBag<DatabaseFileInfo> aliasFiles = [];
            List<Task> fileTasks = [];

            IEnumerable<string> pxFilePaths = _fileSystem.EnumerateFiles(_directoryPath, "*.px");
            foreach (string fileName in pxFilePaths)
            {
                fileTasks.Add(Task.Run(async () =>
                {
                    (DatabaseFileInfo? file, ValidationFeedback feedback) = await ProcessPxFileAsync(fileName, cancellationToken);
                    if (file != null) pxFiles.Add(file);
                    feedbacks.AddRange(feedback);
                }, cancellationToken));
            }

            IEnumerable<string> aliasFilePaths = _fileSystem.EnumerateFiles(_directoryPath, "Alias_*.txt");
            foreach (string fileName in aliasFilePaths)
            {
                fileTasks.Add(Task.Run(async () =>
                {
                    DatabaseFileInfo file = await ProcessAliasFileAsync(fileName, cancellationToken);
                    aliasFiles.Add(file);
                }, cancellationToken));
            }
            
            await Task.WhenAll(fileTasks);
            feedbacks.AddRange(ValidateDatabaseContents(pxFiles, aliasFiles));
            return new (feedbacks);
        }

        private (DatabaseFileInfo?, ValidationFeedback) ProcessPxFile(string fileName)
        {
            ValidationFeedback feedbacks = [];
            using Stream stream = _fileSystem.GetFileStream(fileName);
            (DatabaseFileInfo? fileInfo, KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? feedback) = GetPxFileInfo(fileName, stream);
            if (fileInfo == null)
            {
                if (feedback != null) feedbacks.Add((KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>)feedback);
                return (null, feedbacks);
            }
            stream.Position = 0;
            PxFileValidator validator = new(_syntaxConf);
            feedbacks.AddRange(validator.Validate(stream, fileName, fileInfo.Encoding).FeedbackItems);
            return (fileInfo, feedbacks);
        }

        private DatabaseFileInfo ProcessAliasFile(string fileName)
        {
            using Stream stream = _fileSystem.GetFileStream(fileName);
            return GetAliasFileInfo(fileName, stream);
        }

        private async Task<(DatabaseFileInfo?, ValidationFeedback)> ProcessPxFileAsync(string fileName, CancellationToken cancellationToken)
        {
            ValidationFeedback feedbacks = [];
            using Stream stream = _fileSystem.GetFileStream(fileName);
            (DatabaseFileInfo? fileInfo, KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? feedback) = await GetPxFileInfoAsync(fileName, stream, cancellationToken);
            if (fileInfo == null)
            {
                if (feedback != null) feedbacks.Add((KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>)feedback);
                return (null, feedbacks);
            }
            stream.Position = 0;
            PxFileValidator validator = new(_syntaxConf);
            ValidationResult result = await validator.ValidateAsync(stream, fileName, fileInfo.Encoding, cancellationToken: cancellationToken);
            feedbacks.AddRange(result.FeedbackItems);
            cancellationToken.ThrowIfCancellationRequested();
            return (fileInfo, feedbacks);
        }

        private async Task<DatabaseFileInfo> ProcessAliasFileAsync(string fileName, CancellationToken cancellationToken)
        {
            using Stream stream = _fileSystem.GetFileStream(fileName);
            cancellationToken.ThrowIfCancellationRequested();
            return await GetAliasFileInfoAsync(fileName, stream, cancellationToken);
        }

        private ValidationFeedback ValidateDatabaseContents(ConcurrentBag<DatabaseFileInfo> pxFiles, ConcurrentBag<DatabaseFileInfo> aliasFiles)
        {
            ValidationFeedback feedbacks = [];
            IEnumerable<DatabaseFileInfo> allFiles = pxFiles.Concat(aliasFiles);
            IEnumerable<string> databaseLanguages = pxFiles.SelectMany(file => file.Languages).Distinct();
            Encoding mostCommonEncoding = allFiles.Select(file => file.Encoding)
                .GroupBy(enc => enc)
                .OrderByDescending(group => group.Count())
                .First()
                .Key;

            feedbacks.AddRange(ValidatePxFiles(databaseLanguages, mostCommonEncoding, pxFiles));
            feedbacks.AddRange(ValidateAliasFiles(mostCommonEncoding, aliasFiles));
            feedbacks.AddRange(ValidateDirectories(databaseLanguages, aliasFiles));
            return feedbacks;
        }

        private ValidationFeedback ValidatePxFiles(
            IEnumerable<string> databaseLanguages, 
            Encoding mostCommonEncoding, 
            ConcurrentBag<DatabaseFileInfo> pxFiles)
        {
            ValidationFeedback feedbacks = [];
            IDatabaseValidator[] pxFileValidators =
            [
                new DuplicatePxFileName([.. pxFiles]),
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
                    KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? feedback = validator.Validate(fileInfo);
                    if (feedback is not null)
                    {
                        feedbacks.Add((KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>)feedback);
                    }
                }
            }
            return feedbacks;
        }

        private ValidationFeedback ValidateAliasFiles(Encoding mostCommonEncoding, ConcurrentBag<DatabaseFileInfo> aliasFiles)
        {
            ValidationFeedback feedbacks = [];
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
                    KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? feedback = validator.Validate(fileInfo);
                    if (feedback is not null)
                    {
                        feedbacks.Add((KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>)feedback);
                    }
                }
            }
            return feedbacks;
        }

        private ValidationFeedback ValidateDirectories(IEnumerable<string> databaseLanguages, ConcurrentBag<DatabaseFileInfo> aliasFiles)
        {
            ValidationFeedback feedbacks = [];
            IDatabaseValidator[] directoryValidators =
            [
                new MissingAliasFiles([..aliasFiles], databaseLanguages),
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
                    KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? feedback = validator.Validate(new DatabaseValidationItem(directory));
                    if (feedback is not null)
                    {
                        feedbacks.Add((KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>)feedback);
                    }
                }
            }
            return feedbacks;
        }

        private (DatabaseFileInfo?, KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>?) GetPxFileInfo(string filename, Stream stream)
        {
            string name = _fileSystem.GetFileName(filename);
            string? path = _fileSystem.GetDirectoryName(filename);
            string location =  path is not null ? path : string.Empty;
            string[] languages = [];
            PxFileMetadataReader metadataReader = new ();
            Encoding encoding;
            try
            {
                encoding = metadataReader.GetEncoding(stream, _syntaxConf);
            }
            catch (InvalidPxFileMetadataException e)
            {
                return (null, new(
                    new(ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.NoEncoding),
                    new(filename, additionalInfo: $"Error while reading the encoding of the file {filename}: {e.Message}"))
                );
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
            return (fileInfo, null);
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
                ProcessEntry(entryBuilder.ToString(), ref defaultLanguage, ref languages);
                entryBuilder.Clear();
            }
            else
            {
                entryBuilder.Append(character);
            }
        }

        private async Task<(DatabaseFileInfo?, KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>?)> GetPxFileInfoAsync(string filename, Stream stream, CancellationToken cancellationToken)
        {
            string name = _fileSystem.GetFileName(filename);
            string? path = _fileSystem.GetDirectoryName(filename);
            string location =  path is not null ? path : string.Empty;
            string[] languages = [];
            PxFileMetadataReader metadataReader = new ();
            Encoding encoding;
            try
            {
                encoding = await metadataReader.GetEncodingAsync(stream, _syntaxConf, cancellationToken);
            }
            catch (InvalidPxFileMetadataException e)
            {
                return (null, new(
                    new(ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.NoEncoding),
                    new(filename, additionalInfo: $"Error while reading the encoding of the file {filename}: {e.Message}"))
                );
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
            return (fileInfo, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessEntry(string entry, ref string defaultLanguage, ref string[] languages)
        {
            string[] splitEntry = entry.Trim().Split(_syntaxConf.Symbols.KeywordSeparator);
            if (splitEntry[0] == _syntaxConf.Tokens.KeyWords.DefaultLanguage)
            {
                defaultLanguage = splitEntry[1];
            }
            else if (splitEntry[0] == _syntaxConf.Tokens.KeyWords.AvailableLanguages)
            {
                languages = splitEntry[1].Split(_syntaxConf.Symbols.Value.ListSeparator);
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
                name.Split(_syntaxConf.Tokens.Database.LanguageSeparator)[1].Split('.')[0]
            ];

            Encoding encoding = _fileSystem.GetEncoding(stream);
            DatabaseFileInfo fileInfo = new (name, location, languages, encoding);
            return fileInfo;
        }

        private async Task<DatabaseFileInfo> GetAliasFileInfoAsync(string filename, Stream stream, CancellationToken cancellationToken)
        {
            string name = _fileSystem.GetFileName(filename);
            string? path = _fileSystem.GetDirectoryName(filename);
            string location =  path is not null ? path : string.Empty;
            string[] languages = [
                name.Split(_syntaxConf.Tokens.Database.LanguageSeparator)[1].Split('.')[0]
            ];

            Encoding encoding = await _fileSystem.GetEncodingAsync(stream, cancellationToken);
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
        public KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? Validate(DatabaseValidationItem item);
    }
}
