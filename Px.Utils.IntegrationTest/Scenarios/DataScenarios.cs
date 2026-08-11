using Px.Utils.IntegrationTest.Assertions;
using Px.Utils.IntegrationTest.ExpectedResults;
using Px.Utils.Models.Metadata;
using Px.Utils.Validation;
using Px.Utils.Validation.DatabaseValidation;
using System.Diagnostics;
using System.Text.Json;

namespace Px.Utils.IntegrationTest.Scenarios;

internal sealed class DataScenarios(string databaseRoot, string expectationsRoot)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<TestResult> ValidateDatabaseAsync()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        List<string> failures = [];
        ValidationExpectationDocument expected = Load<ValidationExpectationDocument>("validation-results.json");
        DatabaseValidator validator = new(databaseRoot, new LocalFileSystem());
        ValidationResult result = await validator.ValidateAsync();
        Dictionary<string, List<(ValidationFeedbackKey Key, ValidationFeedbackValue Value)>> actualByFile = result.FeedbackItems
            .SelectMany(pair => pair.Value.Select(value => (Pair: pair.Key, Value: value)))
            .GroupBy(item => GetDatabaseRelativePath(item.Value.Filename))
            .ToDictionary(group => group.Key, group => group.Select(item => (item.Pair, item.Value)).ToList(), StringComparer.OrdinalIgnoreCase);

        foreach (ValidationExpectationFile file in expected.Files.Concat(expected.DatabaseEntries))
        {
            string expectedPath = NormalizePath(file.FileName);
            if (!actualByFile.Remove(expectedPath, out List<(ValidationFeedbackKey Key, ValidationFeedbackValue Value)>? values))
            {
                failures.Add($"Database validation / {file.FileName}: no feedback was produced.");
                continue;
            }

            int warningCount = values.Count(item => item.Key.Level == ValidationFeedbackLevel.Warning);
            int errorCount = values.Count(item => item.Key.Level == ValidationFeedbackLevel.Error);
            if (warningCount != file.WarningCount || errorCount != file.ErrorCount)
            {
                failures.Add($"Database validation / {file.FileName}: expected warnings/errors {file.WarningCount}/{file.ErrorCount}, actual {warningCount}/{errorCount}.");
            }

            foreach (ValidationExpectationEntry entry in file.Entries)
            {
                int count = values.Count(item =>
                    item.Key.Level.ToString() == entry.Level &&
                    item.Key.Rule.ToString() == entry.Rule &&
                    (!entry.Row.HasValue || item.Value.Line == entry.Row) &&
                    (!entry.Character.HasValue || item.Value.Character == entry.Character) &&
                    (entry.AdditionalInformation is null || item.Value.AdditionalInfo == entry.AdditionalInformation));
                if (count != entry.Count)
                {
                    failures.Add($"Database validation / " +
                        $"{file.FileName}: {entry.Level} {entry.Rule} ({entry.Row}, {entry.Character}, " +
                        $"{entry.AdditionalInformation}): expected {entry.Count}, actual {count}.");
                }
            }
        }

        foreach (string unexpectedFile in actualByFile.Keys.OrderBy(value => value, StringComparer.Ordinal))
        {
            string details = string.Join(", ", actualByFile[unexpectedFile]
                .OrderBy(item => item.Key.Level)
                .ThenBy(item => item.Key.Rule)
                .Select(item => $"{item.Key.Level} {item.Key.Rule} ({item.Value.AdditionalInfo})"));
            failures.Add($"Database validation: unexpected feedback for {unexpectedFile}: {details}.");
        }

        return new TestResult("Database validation", failures.Count == 0, stopwatch.Elapsed, failures);
    }

    public TestResult VerifyQueries(string fixtureName, string scenarioName)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        List<string> failures = [];
        QueryExpectationDocument document = Load<QueryExpectationDocument>(fixtureName);
        foreach (QueryExpectation query in document.Queries)
        {
            MatrixMap map = PxLoader.CreateMap(query.Dimensions);
            string filePath = Path.Combine(databaseRoot, "valid-files", query.FileName);
            LoadedMatrix loaded = PxLoader.Load(filePath, fixtureName.StartsWith("limited", StringComparison.Ordinal) ? map : null);
            failures.AddRange(IntegrationAssert.CompareDimensions($"{scenarioName} / {query.FileName}", query.Dimensions, loaded.Metadata));
            failures.AddRange(IntegrationAssert.CompareMatrix($"{scenarioName} / {query.Name}", map, query.Values, loaded.Matrix.Data));
        }
        return new TestResult(scenarioName, failures.Count == 0, stopwatch.Elapsed, failures);
    }

    private T Load<T>(string fileName)
    {
        string path = Path.Combine(expectationsRoot, fileName);
        string json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<T>(json, JsonOptions) ?? throw new InvalidOperationException($"Could not read expectation fixture {fileName}.");
    }

    private string GetDatabaseRelativePath(string path)
    {
        string fullDatabasePath = Path.GetFullPath(databaseRoot);
        string fullFeedbackPath = Path.GetFullPath(path);
        return NormalizePath(Path.GetRelativePath(fullDatabasePath, fullFeedbackPath));
    }

    private static string NormalizePath(string path)
    {
        return path.Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');
    }

}
