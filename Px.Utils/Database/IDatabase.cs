using Px.Utils.Language;

namespace Px.Utils.Database
{
    public interface IDatabase
    {
        string Id { get; }

        MultilanguageString Name { get; }

        List<PxTableReference> GetTables();

        Task<List<PxTableReference>> GetTablesAsync();

        TableUpdateCheckResult CheckForUpdates(PxTableReference tableToCheck);

        Task<TableUpdateCheckResult> CheckForUpdatesAsync(PxTableReference tableToCheck);
        
        Stream OpenStream(PxTableReference targetTable);
         
    }
}
