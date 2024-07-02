using Px.Utils.Language;

namespace Px.Utils.Database
{
    /// <summary>
    /// Interface for modeling a database that contains multiple px-files.
    /// </summary>
    public interface IDatabase
    {
        /// <summary>
        /// Unique identifier of the database.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Translated names for the database.
        /// </summary>
        MultilanguageString Name { get; }

        /// <summary>
        /// Returns a list that contains áll of the files in the database.
        /// </summary>
        /// <returns></returns>
        List<PxTableReference> GetTables();

        /// <summary>
        /// Asynchronously returns a list that contains all of the files in the database.
        /// </summary>
        /// <returns></returns>
        Task<List<PxTableReference>> GetTablesAsync();

        /// <summary>
        /// Checks if the last write time of the file matches the timestamp stored in the reference.
        /// </summary>
        /// <param name="tableToCheck">Checks the last write time against this reference.</param>
        /// <returns>Result stuct contains a boolean indicating if the last write time has changed. If it has also contains a new reference to the table.</returns>
        TableUpdateCheckResult CheckForUpdates(PxTableReference tableToCheck);

        /// <summary>
        /// Asynchronously checks if the last write time of the file matches the timestamp stored in the reference.
        /// </summary>
        /// <param name="tableToCheck">Checks the last write time against this reference.</param>
        /// <returns>Result stuct contains a boolean indicating if the last write time has changed. If it has also contains a new reference to the table.</returns>
        Task<TableUpdateCheckResult> CheckForUpdatesAsync(PxTableReference tableToCheck);
        
        /// <summary>
        /// Opens a stream for reading the file.
        /// </summary>
        /// <param name="targetTable">Reference to file to read.</param>
        /// <returns>A read stream for the file.</returns>
        Stream OpenStream(PxTableReference targetTable);
         
    }
}
