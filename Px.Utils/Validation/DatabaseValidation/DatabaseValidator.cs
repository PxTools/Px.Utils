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
    /// <param name="conf">Configuration for the px files.</param>
    /// <param name="customPxFileValidators">Optional custom <see cref="IDatabaseValidator"/> validator functions ran for each px file within the database</param>
    /// <param name="customAliasFileValidators">Optional custom <see cref="IDatabaseValidator"/> validator functions that are ran for each alias file within the database</param>
    /// <param name="customDirectoryValidators">Optional custom <see cref="IDatabaseValidator"/> validator functions for each subdirectory within the database.</param>
    public class DatabaseValidator(
        string directoryPath,
        IFileSystem fileSystem,
        PxFileConfiguration? conf = null,
        IDatabaseValidator[]? customPxFileValidators = null,
        IDatabaseValidator[]? customAliasFileValidators = null,
        IDatabaseValidator[]? customDirectoryValidators = null
        ) : IValidator, IValidatorAsync
    {
        private readonly string _directoryPath = directoryPath;
        private readonly PxFileConfiguration _conf = conf is not null ? conf : PxFileConfiguration.Default;
        private readonly IDatabaseValidator[]? _customPxFileValidators = customPxFileValidators;
        private readonly IDatabaseValidator[]? _customAliasFileValidators = customAliasFileValidators;
        private readonly IDatabaseValidator[]? _customDirectoryValidators = customDirectoryValidators;
        private readonly IFileSystem _fileSystem = fileSystem is not null ? fileSystem : new LocalFileSystem();

        /// <summary>
        /// Blocking px file database validation process.
        /// </summary>
        /// <returns><see cref="ValidationResult"/> object that contains feedback gathered during the validation process.</returns>
        public ValidationResult Validate()
            => Validate(new ValidationOptions());

        /// <summary>
        /// Runs database validation using the specified feedback retention options.
        /// </summary>
        /// <param name="options">Feedback retention options. A positive limit applies per filename, level, and rule; <see langword="null"/> limit retains all feedback.</param>
        /// <returns>The database validation result with retained feedback.</returns>
        public ValidationResult Validate(ValidationOptions options)
        {
            ValidationFeedbackSink sink = new(options);
            ConcurrentBag<DatabaseFileInfo> pxFiles = [];
            ConcurrentBag<DatabaseFileInfo> aliasFiles = [];
            List<Task> fileTasks = [];

            IEnumerable<string> pxFilePaths = _fileSystem.EnumerateFiles(_directoryPath, "*.px");
            foreach (string fileName in pxFilePaths)
            {
                fileTasks.Add(Task.Run(() =>
                {
                    (DatabaseFileInfo? file, ValidationFeedback feedback) = ProcessPxFile(fileName, sink);
                    if (file != null) pxFiles.Add(file);
                    sink.ReportRange(feedback);
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
            sink.ReportRange(ValidateDatabaseContents(pxFiles, aliasFiles));
            return new(sink.ToFeedback());
        }

        /// <summary>
        /// Asynchronous process for validating a px file database.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns><see cref="ValidationResult"/> object that contains feedback gathered during the validation process.</returns>
        public async Task<ValidationResult> ValidateAsync(CancellationToken cancellationToken = default)
            => await ValidateAsync(new ValidationOptions(), cancellationToken);

        /// <summary>
        /// Runs database validation asynchronously using the specified feedback retention options.
        /// </summary>
        /// <param name="options">Feedback retention options. A positive limit applies per filename, level, and rule; <see langword="null"/> limit retains all feedback.</param>
        /// <param name="cancellationToken">A token that cancels the operation.</param>
        /// <returns>A task that produces the database validation result with retained feedback.</returns>
        public async Task<ValidationResult> ValidateAsync(ValidationOptions options, CancellationToken cancellationToken = default)
        {
            ValidationFeedbackSink sink = new(options);
            ConcurrentBag<DatabaseFileInfo> pxFiles = [];
            ConcurrentBag<DatabaseFileInfo> aliasFiles = [];
            List<Task> fileTasks = [];

            IEnumerable<string> pxFilePaths = _fileSystem.EnumerateFiles(_directoryPath, "*.px");
            foreach (string fileName in pxFilePaths)
            {
                fileTasks.Add(Task.Run(async () =>
                {
                    (DatabaseFileInfo? file, ValidationFeedback feedback) = await ProcessPxFileAsync(fileName, sink, cancellationToken);
                    if (file != null) pxFiles.Add(file);
                    sink.ReportRange(feedback);
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
            sink.ReportRange(ValidateDatabaseContents(pxFiles, aliasFiles));
            return new(sink.ToFeedback());
        }

        private (DatabaseFileInfo?, ValidationFeedback) ProcessPxFile(string fileName, ValidationFeedbackSink sink)
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
            PxFileValidator validator = new(_conf);
            validator.ValidateIntoSink(stream, fileName, fileInfo.Encoding, null, sink);
            return (fileInfo, feedbacks);
        }

        private DatabaseFileInfo ProcessAliasFile(string fileName)
        {
            using Stream stream = _fileSystem.GetFileStream(fileName);
            return GetAliasFileInfo(fileName, stream);
        }

        private async Task<(DatabaseFileInfo?, ValidationFeedback)> ProcessPxFileAsync(string fileName, ValidationFeedbackSink sink, CancellationToken cancellationToken)
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
            PxFileValidator validator = new(_conf);
            await validator.ValidateIntoSinkAsync(stream, fileName, fileInfo.Encoding, null, sink, cancellationToken);
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
                if (directoryName == _conf.Tokens.Database.Index) continue;

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
            PxFileMetadataReader metadataReader = new (_conf);
            Encoding encoding;
            try
            {
                encoding = metadataReader.GetEncoding(stream);
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
            if (SyntaxValidator.IsEndOfMetadataSection(character, _conf, entryBuilder, isProcessingString) && defaultLanguage != string.Empty)
            {
                languages = [defaultLanguage.Trim(_conf.Symbols.Key.StringDelimeter)];
            }
            else if (character == _conf.Symbols.Key.StringDelimeter)
            {
                isProcessingString = !isProcessingString;
            }
            else if (character == _conf.Symbols.EntrySeparator && !isProcessingString)
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
            PxFileMetadataReader metadataReader = new (_conf);
            Encoding encoding;
            try
            {
                encoding = await metadataReader.GetEncodingAsync(stream, cancellationToken);
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
            string[] splitEntry = entry.Trim().Split(_conf.Symbols.KeywordSeparator);
            if (splitEntry[0] == _conf.Tokens.KeyWords.DefaultLanguage)
            {
                defaultLanguage = splitEntry[1];
            }
            else if (splitEntry[0] == _conf.Tokens.KeyWords.AvailableLanguages)
            {
                languages = [.. SyntaxValidationUtilityMethods.GetListItemsFromString(
                    splitEntry[1], 
                    _conf.Symbols.Value.ListSeparator, 
                    _conf.Symbols.Value.StringDelimeter)];
                for (int j = 0; j < languages.Length; j++)
                {
                    languages[j] = languages[j].Trim(_conf.Symbols.Key.StringDelimeter);
                }
            }
        }

        private DatabaseFileInfo GetAliasFileInfo(string filename, Stream stream)
        {
            string name = _fileSystem.GetFileName(filename);
            string? path = _fileSystem.GetDirectoryName(filename);
            string location =  path is not null ? path : string.Empty;
            string[] languages = [
                name.Split(_conf.Tokens.Database.LanguageSeparator)[1].Split('.')[0]
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
                name.Split(_conf.Tokens.Database.LanguageSeparator)[1].Split('.')[0]
            ];

            Encoding encoding = await _fileSystem.GetEncodingAsync(stream, cancellationToken);
            DatabaseFileInfo fileInfo = new (name, location, languages, encoding);
            return fileInfo;
        }
    }

    /// <summary>
    /// Represents a database validation item.
    /// </summary>
    /// <param name="path">The path of the database validation item.</param>
    public class DatabaseValidationItem(string path)
    {
        /// <summary>
        /// Gets the path of the file or directory.
        /// </summary>
        public string Path { get; } = path;
    }

    /// <summary>
    /// Represents a px file or alias file within a px file database for validation purposes.
    /// </summary>
    /// <param name="name">Name of the file.</param>
    /// <param name="location">Path of the file's directory.</param>
    /// <param name="languages">Languages associated with the file.</param>
    /// <param name="encoding">Encoding of the file.</param>
    public class DatabaseFileInfo(string name, string location, string[] languages, Encoding encoding) : DatabaseValidationItem(name)
    {
        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        public string Name { get; } = name;
        /// <summary>
        /// Gets the path of the file's directory.
        /// </summary>
        public string Location { get; } = location;
        /// <summary>
        /// Gets the languages associated with the file.
        /// </summary>
        public string[] Languages { get; } = languages;
        /// <summary>
        /// Gets the encoding of the file.
        /// </summary>
        public Encoding Encoding { get; } = encoding;
    }

    /// <summary>
    /// Validator interface for validating a px file database including px files, alias files, and directory structures.
    /// </summary>
    public interface IDatabaseValidator
    {
        /// <summary>
        /// Validates a given <see cref="DatabaseValidationItem"/> and returns a feedback entry if validation fails, or <see langword="null"/> if validation passes.
        /// </summary>
        /// <param name="item">The database validation item to validate.</param>
        /// <returns>A key-value pair representing the validation feedback if validation fails, or <see langword="null"/> if validation passes.</returns>
        public KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? Validate(DatabaseValidationItem item);
    }
}
