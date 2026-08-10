namespace Px.Utils.IntegrationTest;

internal static class Program
{
    private static async Task<int> Main()
    {
        return await IntegrationTestRunner.RunAsync();
    }
}
