namespace Px.Utils.TestingApp
{
    internal static class TestAppConsole
    {
        private const string EXIT = "exit";

        internal static List<string> ReadLine()
        {
            string input = Console.ReadLine() ?? "";
            if(input == EXIT) Environment.Exit(0);
            return new(input.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        internal static List<string> AskQuestion(string question, bool required, string? followUp = null)
        {
            Console.WriteLine(question);
            List<string> answer = ReadLine() ?? [];
            while (answer.Count == 0 && required)
            {
                Console.WriteLine(followUp ?? question);
                answer = ReadLine() ?? [];
            }

            return answer;
        }
    }
}
