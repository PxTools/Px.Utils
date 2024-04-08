using Px.Utils.TestingApp;
using Px.Utils.TestingApp.Commands;

namespace PxUtils.TestingApp
{
    internal class InteractiveFlow
    {
        private readonly Dictionary<string, Command> _commands = [];

        internal InteractiveFlow()
        {
            _commands.Add("bench", new BenchmarkRunner());
        }

        public void Start()
        {
            string q = "Enter the command and optional parameters:";
            string followUp = "Valid command is required, please input the command:";
            List<string> inputs = TestAppConsole.AskQuestion(q, true, followUp);
            while (!_commands.ContainsKey(inputs[0]))
            {
                inputs = TestAppConsole.AskQuestion(followUp, true);
            }
            _commands[inputs[0]].Run(false, inputs.Skip(1).ToList());
        }

        private void Help()
        {
            Console.WriteLine("Available commands:");
            Console.WriteLine(string.Join(Environment.NewLine, _commands.Select(c => $"{c.Key}:{Environment.NewLine}{c.Value.Description}{Environment.NewLine}")));
        }
    }
}
