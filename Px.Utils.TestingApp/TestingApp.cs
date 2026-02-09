using Px.Utils.TestingApp.Commands;
using Px.Utils.TestingApp;

namespace Px.Utils.TestingApp
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    internal static class TestingApp
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0) await new InteractiveFlow().StartAsync();
            else StartBatchMode();
        }

        static void StartBatchMode()
        {
            Console.WriteLine("Batch mode is not yet supported.");
        }
    }
}
