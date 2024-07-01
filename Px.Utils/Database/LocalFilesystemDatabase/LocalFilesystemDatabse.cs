using Px.Utils.Language;

namespace Px.Utils.Database.FilesystemDatabase
{
    public class LocalFilesystemDatabse(string id, MultilanguageString name, string databaseFilePath) : IDatabase
    {
        public string Id { get; } = id;

        public MultilanguageString Name { get; } = name;

        private readonly string _databaseFilePath = databaseFilePath; 

        public TableUpdateCheckResult CheckForUpdates(PxTableReference tableToCheck)
        {
            DateTime updateTime = Directory.GetLastWriteTime(tableToCheck.Identifier);
            if (updateTime > tableToCheck.TableLastUpdated)
            {
                PxTableReference newReference = new(tableToCheck.Identifier, updateTime, DateTime.Now);
                return new TableUpdateCheckResult(true, newReference); 
            }
            else
            {
                return new TableUpdateCheckResult(false, null);
            }
        }

        public async Task<TableUpdateCheckResult> CheckForUpdatesAsync(PxTableReference tableToCheck) =>
            await Task.Factory.StartNew(() => CheckForUpdates(tableToCheck));

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
                    files.Add(new PxTableReference(file, lastUpdated, DateTime.Now));
                }
            }
            return files;
        }

        public async Task<List<PxTableReference>> GetTablesAsync() =>
            await Task.Factory.StartNew(GetTables);

        public Stream OpenStream(PxTableReference targetTable)
        {
            return File.OpenRead(targetTable.Identifier);
        }
    }
}
