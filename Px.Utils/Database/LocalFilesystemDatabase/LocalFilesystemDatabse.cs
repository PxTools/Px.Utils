using Px.Utils.Language;
using System.Diagnostics.CodeAnalysis;

namespace Px.Utils.Database.FilesystemDatabase
{
    /// <summary>
    /// Class for using a px-file database on local filesystem.
    /// </summary>
    /// <param name="id">Unique identifier for the database.</param>
    /// <param name="name">Translated names of the database.</param>
    /// <param name="databaseFilePath">Path to the root of the database.</param>
    [ExcludeFromCodeCoverage] // All of the methods in this class call IO and contain very litle logic
    public class LocalFilesystemDatabse(string id, MultilanguageString name, string databaseFilePath) : IDatabase
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

                foreach (string file in Directory.EnumerateFiles(currentDir))
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
    }
}
