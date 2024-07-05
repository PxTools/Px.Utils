using Px.Utils.Language;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Px.Utils.Database.FilesystemDatabase
{
    /// <summary>
    /// Class for using a px-file database on local filesystem.
    /// </summary>
    /// <param name="id">Unique identifier for the database.</param>
    /// <param name="name">Translated names of the database.</param>
    /// <param name="databaseFilePath">Path to the root of the database.</param>
    /// <param name="defaultEncoding">Default encoding for all files in the database.</param>
    [ExcludeFromCodeCoverage] // All of the methods in this class call IO and contain very litle logic
    public class LocalFilesystemDatabse(string id, MultilanguageString name, string databaseFilePath, Encoding defaultEncoding) : IDatabase
    {
        /// <summary>
        /// Unique identifier of the database.
        /// </summary>
        public string Id { get; } = id;

        /// <summary>
        /// Translated names of the database.
        /// </summary>
        public MultilanguageString Name { get; } = name;

        private readonly string _databaseFilePath = databaseFilePath;

        private readonly Encoding _defaultEncoding = defaultEncoding;

        private const string PX_FILE_EXTENSION = "px";
        private const string ALIAS_FILE_EXTENSION = "txt";

        /// <summary>
        /// Checks if the last writetime of the referenced file has been changed.
        /// </summary>
        /// <param name="tableToCheck">Reference to the table to be checked.</param>
        /// <returns>
        /// The returned struct contains a boolean indicating if the table has been updated and
        /// if the table has been updated also a new <see cref="PxTableReference"/>.
        /// </returns>
        public TableUpdateCheckResult CheckForUpdates(PxTableReference tableToCheck)
        {
            DateTime updateTime = Directory.GetLastWriteTime(tableToCheck.Identifier);
            if (updateTime > tableToCheck.TableLastUpdated)
            {
                PxTableReference newReference = new(tableToCheck.Identifier, updateTime);
                return new TableUpdateCheckResult(true, newReference); 
            }
            else
            {
                return new TableUpdateCheckResult(false, null);
            }
        }

        /// <summary>
        /// Asynchronously cheks if the last writetime of the referenced file has been changed.
        /// </summary>
        /// <param name="tableToCheck">Reference to the table to be checked.</param>
        /// <returns>
        /// The returned struct contains a boolean indicating if the table has been updated and
        /// if the table has been updated also a new <see cref="PxTableReference"/>.
        /// </returns>
        public async Task<TableUpdateCheckResult> CheckForUpdatesAsync(PxTableReference tableToCheck) =>
            await Task.Factory.StartNew(() => CheckForUpdates(tableToCheck));

        /// <summary>
        /// Gets all tables and sub groups directly containded in the target group.
        /// </summary>
        /// <param name="groupHierarcy">Hierarchical path from the base level using group codes.</param>
        /// <returns>Contains all tables and subgroups in the group.</returns>
        public DatabaseGroupContents GetGroupContents(IReadOnlyList<string> groupHierarcy)
        {
            List<DatabaseGroupHeader> headers = [];
            List<PxTableReference> tables = [];

            string path = Path.Combine(_databaseFilePath, string.Join(Path.PathSeparator, groupHierarcy));
            string[] directories = Directory.GetDirectories(path);
            foreach (string directory in directories)
            {
                string code = new DirectoryInfo(directory).Name; 
                MultilanguageString alias = GetGroupName(directory);
                headers.Add(new DatabaseGroupHeader(code, alias));
            }

            string[] pxFiles = Directory.GetFiles(path, $"*.{PX_FILE_EXTENSION}");
            foreach (string pxFile in pxFiles)
            {
                DateTime lastUpdated = Directory.GetLastWriteTime(pxFile);
                tables.Add(new PxTableReference(pxFile, lastUpdated));
            }

            return new DatabaseGroupContents(headers, tables);
        }

        /// <summary>
        /// Asynchronously gets all tables and sub groups directly containded in the target group.
        /// </summary>
        /// <param name="groupHierarcy">Hierarchical path from the base level using group codes.</param>
        /// <returns>Contains all tables and subgroups in the group.</returns>// <returns></returns>
        public async Task<DatabaseGroupContents> GetGroupContentsAsync(IReadOnlyList<string> groupHierarcy) =>
            await Task.Factory.StartNew(() =>  GetGroupContents(groupHierarcy));
        /// <summary>
        /// Lists all of the files in the database.
        /// </summary>
        /// <returns>
        /// A list of references to the tables, containing the file information
        /// and the writetime of the table.
        /// </returns>
        public List<PxTableReference> GetTables()
        {
            List<PxTableReference> files = [];
            Queue<string> directories = new();
            directories.Enqueue(_databaseFilePath);
            while (directories.Count > 0)
            {
                string currentDir = directories.Dequeue();
                foreach (string subDirectory in Directory.GetDirectories(currentDir))
                {
                    directories.Enqueue(subDirectory);
                }

                foreach (string file in Directory.EnumerateFiles(currentDir, $"*.{PX_FILE_EXTENSION}"))
                {
                    DateTime lastUpdated = Directory.GetLastWriteTime(file);
                    files.Add(new PxTableReference(file, lastUpdated));
                }
            }
            return files;
    }

        /// <summary>
        /// Asynchronously lists all of the files in the database.
        /// </summary>
        /// <returns>
        /// A list of references to the tables, containing the file information
        /// and the writetime of the table.
        /// </returns>
        public async Task<List<PxTableReference>> GetTablesAsync() =>
            await Task.Factory.StartNew(GetTables);

        /// <summary>
        /// Opens a stream for reading a px-file.
        /// </summary>
        /// <param name="targetTable">Reference to the table to be opened.</param>
        /// <returns><see cref="Stream"/> for reading the file.</returns>
        public Stream OpenStream(PxTableReference targetTable)
        {
            return File.OpenRead(targetTable.Identifier);
        }
        
        private MultilanguageString GetGroupName(string path)
        {
            Dictionary<string, string> translatedNames = [];
            IEnumerable<string> aliasFiles = Directory.GetFiles(path, $"*.{ALIAS_FILE_EXTENSION}")
                .Where(p => Path.GetFileName(p).StartsWith("alias", StringComparison.OrdinalIgnoreCase));
            foreach (string aliasFile in aliasFiles)
            {
                string lang = new([.. Path.GetFileName(aliasFile).Skip(6).TakeWhile(c => c != '.')]);
                string alias = File.ReadAllText(aliasFile, _defaultEncoding);
                translatedNames.Add(lang, alias.Trim());
            }
            return new MultilanguageString(translatedNames);
        }
    }
}
