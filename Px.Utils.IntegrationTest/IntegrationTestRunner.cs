using Px.Utils.IntegrationTest.Scenarios;
using System.Diagnostics;
using System.Text;

namespace Px.Utils.IntegrationTest;

internal static class IntegrationTestRunner
{
    private const int PassedExitCode = 0;
    private const int FailedExitCode = 1;
    private const int SetupFailureExitCode = 2;

    public static async Task<int> RunAsync()
    {
        try
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string databaseRoot = Path.Combine(AppContext.BaseDirectory, "test-database");
            string expectationsRoot = Path.Combine(AppContext.BaseDirectory, "ExpectedResults");
            EnsureDirectoryExists(databaseRoot);
            EnsureDirectoryExists(expectationsRoot);

            DataScenarios dataScenarios = new(databaseRoot, expectationsRoot);
            CalculationScenario calculationScenario = new(databaseRoot, expectationsRoot);
            List<TestResult> results =
            [
                await dataScenarios.ValidateDatabaseAsync(),
                dataScenarios.VerifyQueries("full-query-results.json", "Full data query"),
                dataScenarios.VerifyQueries("limited-query-results.json", "Limited data query"),
                calculationScenario.Run()
            ];

            PrintResults(results);
            return results.All(result => result.Passed) ? PassedExitCode : FailedExitCode;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"ERROR Integration runner could not execute: {exception}");
            return SetupFailureExitCode;
        }
    }

    private static void EnsureDirectoryExists(string directory)
    {
        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"Required integration-test asset directory was not found: {directory}");
        }
    }

    private static void PrintResults(IEnumerable<TestResult> results)
    {
        List<TestResult> orderedResults = results.OrderBy(result => result.Name, StringComparer.Ordinal).ToList();
        foreach (TestResult result in orderedResults)
        {
            string status = result.Passed ? "PASS" : "FAIL";
            Console.WriteLine($"{status} {result.Name} ({result.Elapsed.TotalMilliseconds:F0} ms)");
            foreach (string failure in result.Failures.OrderBy(value => value, StringComparer.Ordinal))
            {
                Console.Error.WriteLine($"  {failure}");
            }
        }

        int passed = orderedResults.Count(result => result.Passed);
        Console.WriteLine($"Total: {passed}/{orderedResults.Count} scenarios passed.");
    }
}
