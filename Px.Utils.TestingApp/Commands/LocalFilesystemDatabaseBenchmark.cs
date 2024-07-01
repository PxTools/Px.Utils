using Px.Utils.Database;
using Px.Utils.Database.FilesystemDatabase;

namespace Px.Utils.TestingApp.Commands
{
    internal class LocalFilesystemDatabaseBenchmark : Benchmark
    {
        internal override string Help =>
        "Runs the px-file listing benchmarks for a database provided with the -f flag amount of times provided with the -i flag.";

        internal override string Description => "Benchmarks the speed for listing the files in a database.";

        internal LocalFilesystemDatabaseBenchmark()
        {
            BenchmarkFunctions = [GetAllTablesBenchmark];
            BenchmarkFunctionsAsync = [GetAllTablesBenchmarkAsync];
        }

        private void GetAllTablesBenchmark()
        {
            LocalFilesystemDatabse database = new("test_db", new("foo", "test_db"), TestFilePath);            
            List<PxTableReference> pxTableReferences = database.GetTables();
        }

        private async Task GetAllTablesBenchmarkAsync()
        {
            LocalFilesystemDatabse database = new("test_db", new("foo", "test_db"), TestFilePath);
            List<PxTableReference> pxTableReferences = await database.GetTablesAsync();
        }
    }
}
