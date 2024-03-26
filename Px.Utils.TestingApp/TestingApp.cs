using Px.Utils.TestingApp.Commands;

namespace Px.Utils.TestingApp
{
    internal class TestingApp
    {
        static void Main(string[] args)
        {
            if (args.Length == 0) StartInteractiveMode();
            else StartBatchMode(args);
        }

        static void StartInteractiveMode()
        {
            DataReadBenchmark benchmarks = new();
            benchmarks.PrintResults();
        }

        static void StartBatchMode(string[] _)
        {
            Console.WriteLine("Batch mode is not yet supported.");
        }
    }
}
