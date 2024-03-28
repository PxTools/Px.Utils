using Px.Utils.TestingApp.Commands;
using PxUtils.TestingApp;

namespace Px.Utils.TestingApp
{
    internal class TestingApp
    {
        static void Main(string[] args)
        {
            if (args.Length == 0) new InteractiveFlow().Start();
            else StartBatchMode(args);
        }

        static void StartBatchMode(string[] _)
        {
            Console.WriteLine("Batch mode is not yet supported.");
        }
    }
}
