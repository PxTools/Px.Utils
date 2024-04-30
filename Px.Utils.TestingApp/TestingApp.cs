using Px.Utils.TestingApp.Commands;
using PxUtils.TestingApp;

namespace Px.Utils.TestingApp
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    internal static class TestingApp
    {
        static void Main(string[] args)
        {
            if (args.Length == 0) new InteractiveFlow().Start();
            else StartBatchMode();
        }

        static void StartBatchMode()
        {
            var b = new DataValidationBenchmark();
            
            b.Run(true, ["-f", "D:\\UD\\reen1\\work\\code\\Px.Utils2\\statfin_tyonv_pxt_12ts.px",
            "-i", "2", "-r", "44712", "-c", "2821"] );

            b.PrintResults();
//            Console.WriteLine("Batch mode is not yet supported.");
        }
    }
}
