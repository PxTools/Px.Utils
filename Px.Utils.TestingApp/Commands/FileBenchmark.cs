namespace Px.Utils.TestingApp.Commands
{
    internal abstract class FileBenchmark : Benchmark
    {
        private static readonly string[] fileFlags = ["-f", "-file"];

        private protected FileBenchmark()
        {
            ParameterFlags.Add(fileFlags);
        }

        protected override void SetRunParameters()
        {
            foreach (string key in fileFlags)
            {
                if (Parameters.TryGetValue(key, out List<string>? value) && value.Count == 1)
                {
                    TestFilePath = value[0];
                    base.SetRunParameters();
                    return;
                }
            }

            StartInteractiveMode();
        }

        protected override void StartInteractiveMode()
        {
            SetFilePaths();
            base.StartInteractiveMode();
        }

        private void SetFilePaths()
        {
            Console.WriteLine("Enter the path to the PX file to benchmark");
            string file = Console.ReadLine() ?? "";

            while (!Path.Exists(file) || !Path.GetFileName(file).EndsWith(".px", StringComparison.Ordinal))
            {
                Console.WriteLine("File provided is not valid, please enter a path to a valid px file");
                file = Console.ReadLine() ?? "";
            }

            TestFilePath = file;
        }
    }
}
