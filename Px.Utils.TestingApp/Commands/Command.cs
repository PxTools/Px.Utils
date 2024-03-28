using System.Diagnostics;

namespace Px.Utils.TestingApp.Commands
{
    internal abstract class Command
    {
        internal abstract string Help { get; }

        internal abstract string Description { get; }

        internal abstract void Run(bool batchMode, List<string>? inputs = null);

        protected static Dictionary<string, List<string>> GroupParameters(List<string> inputs, List<string> flags)
        {
            Dictionary<string, List<string>> parameters = [];
            string? latestFlag = null;
            foreach (string input in inputs)
            {
                if(flags.Contains(input))
                {
                    if(parameters.ContainsKey(input))
                    {
                        throw new ArgumentException($"Flag {input} was provided multiple times.");
                    }
                    latestFlag = input;
                    parameters.Add(latestFlag, []);
                }
                else if (latestFlag is not null && parameters.TryGetValue(latestFlag, out List<string>? value))
                {
                    value.Add(input);
                }
                else
                {
                    throw new ArgumentException($"Parameter {input} was provided without a flag.");
                }
            }

            return parameters;
        }
    }
}
