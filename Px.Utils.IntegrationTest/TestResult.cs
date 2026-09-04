namespace Px.Utils.IntegrationTest;

internal sealed record TestResult(string Name, bool Passed, TimeSpan Elapsed, List<string> Failures);
