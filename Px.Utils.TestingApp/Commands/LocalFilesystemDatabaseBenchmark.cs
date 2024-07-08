using Px.Utils.Database.FilesystemDatabase;
using System.Text;

namespace Px.Utils.TestingApp.Commands
{
    internal class LocalFilesystemDatabaseBenchmark : Benchmark
    {
        internal override string Help =>
        "Runs the px-file listing benchmarks for a database provided with the -f flag amount of times provided with the -i flag.";

        internal override string Description => "Benchmarks the speed for listing the files in a database.";

        internal LocalFilesystemDatabaseBenchmark()
        {
            BenchmarkFunctions = [GetAllTablesBenchmark, GetGroupContentsBenchmark];
            BenchmarkFunctionsAsync = [GetAllTablesBenchmarkAsync, GetGroupContentsAsyncBenchmark];
        }

        private void GetAllTablesBenchmark()
        {
            // Note that the encoding may be incorrect, but should not have impact when testing the read performance.
            LocalFilesystemDatabse database = new("test_db", new("foo", "test_db"), TestFilePath, Encoding.UTF8);            
            database.GetTables();
        }

        private async Task GetAllTablesBenchmarkAsync()
        {
            LocalFilesystemDatabse database = new("test_db", new("foo", "test_db"), TestFilePath, Encoding.UTF8);
            await database.GetTablesAsync();
        }

        private void GetGroupContentsBenchmark()
        {
            LocalFilesystemDatabse database = new("test_db", new("foo", "test_db"), TestFilePath, Encoding.UTF8);
            database.GetGroupContents([]);
        }

        private async Task GetGroupContentsAsyncBenchmark()
        {
            LocalFilesystemDatabse database = new("test_db", new("foo", "test_db"), TestFilePath, Encoding.UTF8);
            await database.GetGroupContentsAsync([]);
        }
    }
}
