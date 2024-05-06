using Px.Utils.TestingApp;
using Px.Utils.TestingApp.Commands;

namespace PxUtils.TestingApp
{
    /// <summary>
    /// Runs the interactive flow of the application.
    /// Prompts the user for a commands and parameters and then runs the commands.
    /// </summary>
    internal class InteractiveFlow
    {
        /// <summary>
        /// Collection of commands that can be run.
        /// </summary>
        private readonly Dictionary<string, Command> _commands = [];

        internal InteractiveFlow()
        {
            _commands.Add("bench", new BenchmarkRunner());
            _commands.Add("help", new HelpCommand(_commands));
        }

#pragma warning disable S2190 // The Ask Questions breaks the loop if exit is entered
        public void Start()
        {
            while(true)
            {
                const string q = "Enter the command and optional parameters:";
                const string followUp = "Valid command is required, please input the command:";
                List<string> inputs = TestAppConsole.AskQuestion(q, true, followUp);
                while (!_commands.ContainsKey(inputs[0]))
                {
                    inputs = TestAppConsole.AskQuestion(followUp, true);
                }
                _commands[inputs[0]].Run(false, inputs.Skip(1).ToList());
            }
        }
#pragma warning restore S2190

        private sealed class HelpCommand : Command
        {
            internal override string Help => "Displays the list of available commands and instructions for running them";
            internal override string Description => "Displays the list of available commands.";

            private readonly Dictionary<string, Command> _commands;

            internal HelpCommand(Dictionary<string, Command> commands)
            {
                _commands = commands;
            }

            internal override void Run(bool batchMode, List<string>? inputs = null)
            {
                Console.Clear();
                Console.WriteLine("Available commands:");
                Console.WriteLine(string.Join(Environment.NewLine, _commands.Select(c => $"\t{c.Key}: {c.Value.Description}")));
                Console.WriteLine();
            }
        }
    }
}
