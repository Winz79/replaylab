using System.Diagnostics;

namespace ReplayLab.Cli.Tests;

public sealed class CliEndToEndSmokeTests
{
    [Fact]
    public async Task Cli_process_replays_valid_synthetic_csv_successfully()
    {
        var repoRoot = FindRepositoryRoot();
        var samplePath = Path.Combine(repoRoot, "samples", "basic.csv");

        var result = await RunCliProcessAsync(repoRoot, samplePath);

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Loaded 2 message(s).", result.StandardOutput);
        Assert.Contains("Inspected 2 message(s).", result.StandardOutput);
        Assert.Contains("Sent 2 message(s): 2 succeeded, 0 failed.", result.StandardOutput);
        Assert.Contains("record-1: succeeded", result.StandardOutput);
        Assert.Contains("record-2: succeeded", result.StandardOutput);
    }

    [Fact]
    public async Task Cli_process_returns_non_zero_for_missing_input_file()
    {
        var repoRoot = FindRepositoryRoot();
        var missingPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.csv");

        var result = await RunCliProcessAsync(repoRoot, missingPath);

        Assert.NotEqual(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.Contains("Input file was not found", result.StandardError);
        Assert.Contains(missingPath, result.StandardError);
    }

    [Fact]
    public async Task Cli_process_returns_non_zero_for_invalid_csv()
    {
        var repoRoot = FindRepositoryRoot();
        var csvPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.csv");
        await File.WriteAllTextAsync(csvPath, """
            kind,name
            Created,alpha,extra
            """);

        try
        {
            var result = await RunCliProcessAsync(repoRoot, csvPath);

            Assert.NotEqual(0, result.ExitCode);
            Assert.Equal(string.Empty, result.StandardOutput);
            Assert.Contains("CSV parse failed:", result.StandardError);
            Assert.Contains("header row has 2 fields", result.StandardError);
        }
        finally
        {
            if (File.Exists(csvPath))
            {
                File.Delete(csvPath);
            }
        }
    }

    private static async Task<CliProcessResult> RunCliProcessAsync(
        string repoRoot,
        string inputPath)
    {
        var cliAssemblyPath = GetCliAssemblyPath(repoRoot);
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"\"{cliAssemblyPath}\" \"{inputPath}\"",
            WorkingDirectory = repoRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        }) ?? throw new InvalidOperationException("Failed to start dotnet CLI process.");

        var standardOutput = await process.StandardOutput.ReadToEndAsync();
        var standardError = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return new CliProcessResult(process.ExitCode, standardOutput, standardError);
    }

    private static string GetCliAssemblyPath(string repoRoot)
    {
        var testOutputDirectory = new DirectoryInfo(AppContext.BaseDirectory);
        var targetFramework = testOutputDirectory.Name;
        var configuration = testOutputDirectory.Parent?.Name
            ?? throw new InvalidOperationException("Could not determine test build configuration.");
        var cliAssemblyPath = Path.Combine(
            repoRoot,
            "src",
            "ReplayLab.Cli",
            "bin",
            configuration,
            targetFramework,
            "ReplayLab.Cli.dll");

        if (!File.Exists(cliAssemblyPath))
        {
            throw new FileNotFoundException("Could not locate built ReplayLab.Cli assembly.", cliAssemblyPath);
        }

        return cliAssemblyPath;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "ReplayLab.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root.");
    }

    private sealed record CliProcessResult(
        int ExitCode,
        string StandardOutput,
        string StandardError);
}
