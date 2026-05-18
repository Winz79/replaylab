using System.Diagnostics;

namespace ReplayLab.Cli.Tests;

public sealed class CliEndToEndSmokeTests
{
    [Fact]
    public async Task Cli_process_replays_valid_synthetic_csv_successfully()
    {
        var repoRoot = FindRepositoryRoot();
        var samplePath = Path.Combine(repoRoot, "samples", "basic.csv");

        var result = await RunCliProcessAsync(repoRoot, [samplePath]);

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Loaded 2 message(s).", result.StandardOutput);
        Assert.Contains("Inspected 2 message(s).", result.StandardOutput);
        Assert.Contains("Sent 2 message(s): 2 succeeded, 0 failed.", result.StandardOutput);
        Assert.Contains("record-1: succeeded", result.StandardOutput);
        Assert.Contains("record-2: succeeded", result.StandardOutput);
    }

    [Fact]
    public async Task Cli_process_replays_valid_synthetic_csv_successfully_when_format_csv_is_explicit()
    {
        var repoRoot = FindRepositoryRoot();
        var samplePath = Path.Combine(repoRoot, "samples", "basic.csv");

        var result = await RunCliProcessAsync(repoRoot, ["--format", "csv", samplePath]);

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Loaded 2 message(s).", result.StandardOutput);
        Assert.Contains("Inspected 2 message(s).", result.StandardOutput);
        Assert.Contains("Sent 2 message(s): 2 succeeded, 0 failed.", result.StandardOutput);
        Assert.Contains("record-1: succeeded", result.StandardOutput);
        Assert.Contains("record-2: succeeded", result.StandardOutput);
    }

    [Fact]
    public async Task Cli_process_accepts_single_file_argument_that_starts_with_dash()
    {
        var repoRoot = FindRepositoryRoot();
        var csvPath = Path.Combine(Path.GetTempPath(), $"-{Guid.NewGuid():N}.csv");
        await File.WriteAllTextAsync(csvPath, """
            kind,name
            Created,alpha
            """);

        try
        {
            var result = await RunCliProcessAsync(repoRoot, [csvPath]);

            Assert.Equal(0, result.ExitCode);
            Assert.Contains("Loaded 1 message(s).", result.StandardOutput);
            Assert.Contains("Sent 1 message(s): 1 succeeded, 0 failed.", result.StandardOutput);
            Assert.Equal(string.Empty, result.StandardError);
        }
        finally
        {
            if (File.Exists(csvPath))
            {
                File.Delete(csvPath);
            }
        }
    }

    [Fact]
    public async Task Cli_process_returns_non_zero_for_missing_input_file()
    {
        var repoRoot = FindRepositoryRoot();
        var missingPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.csv");

        var result = await RunCliProcessAsync(repoRoot, [missingPath]);

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
            var result = await RunCliProcessAsync(repoRoot, [csvPath]);

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

    [Fact]
    public async Task Cli_process_returns_non_zero_for_unsupported_format_before_parsing()
    {
        var repoRoot = FindRepositoryRoot();
        var samplePath = Path.Combine(repoRoot, "samples", "basic.csv");

        var result = await RunCliProcessAsync(repoRoot, ["--format", "json", samplePath]);

        Assert.NotEqual(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.Contains("Unsupported input format: json", result.StandardError);
        Assert.Contains("Supported formats: csv", result.StandardError);
    }

    [Fact]
    public async Task Cli_process_returns_non_zero_when_format_option_is_missing_a_value()
    {
        var repoRoot = FindRepositoryRoot();

        var result = await RunCliProcessAsync(repoRoot, ["--format"]);

        Assert.NotEqual(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.Contains("Missing value for --format.", result.StandardError);
        Assert.Contains("Supported formats: csv", result.StandardError);
        Assert.Contains("Usage: replaylab <file>", result.StandardError);
        Assert.Contains("Usage: replaylab --format csv <file>", result.StandardError);
    }

    [Fact]
    public async Task Cli_process_returns_non_zero_for_unsupported_sender()
    {
        var repoRoot = FindRepositoryRoot();
        var samplePath = Path.Combine(repoRoot, "samples", "basic.csv");

        var result = await RunCliProcessAsync(repoRoot, ["--sender", "ftp", samplePath]);

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.Contains("Unsupported sender: ftp", result.StandardError);
    }

    [Fact]
    public async Task Cli_process_returns_non_zero_when_http_sender_is_missing_endpoint_url()
    {
        var repoRoot = FindRepositoryRoot();
        var samplePath = Path.Combine(repoRoot, "samples", "basic.csv");

        var result = await RunCliProcessAsync(repoRoot, ["--sender", "http", samplePath]);

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.Contains("The --endpoint-url option is required when --sender http is selected.", result.StandardError);
    }

    private static async Task<CliProcessResult> RunCliProcessAsync(
        string repoRoot,
        IReadOnlyList<string> arguments)
    {
        var cliAssemblyPath = GetCliAssemblyPath(repoRoot);
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = BuildArguments(cliAssemblyPath, arguments),
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

    private static string BuildArguments(string cliAssemblyPath, IReadOnlyList<string> arguments)
    {
        var escapedArguments = arguments
            .Select(argument => $"\"{argument}\"");
        return string.Join(" ", [$"\"{cliAssemblyPath}\"", .. escapedArguments]);
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
