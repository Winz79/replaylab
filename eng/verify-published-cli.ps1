param(
    [string]$Configuration = "Release",
    [string]$OutputPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$projectPath = Join-Path $repoRoot "src/ReplayLab.Cli/ReplayLab.Cli.csproj"
$samplePath = Join-Path $repoRoot "samples/basic.csv"

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $repoRoot "artifacts/publish/replaylab"
}
else {
    $OutputPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputPath)
}

dotnet publish $projectPath --configuration $Configuration --output $OutputPath

$isWindows = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform(
    [System.Runtime.InteropServices.OSPlatform]::Windows)
$executableName = if ($isWindows) { "ReplayLab.Cli.exe" } else { "ReplayLab.Cli" }
$executablePath = Join-Path $OutputPath $executableName

if (-not (Test-Path $executablePath)) {
    throw "Published executable was not found at $executablePath"
}

$processStartInfo = [System.Diagnostics.ProcessStartInfo]::new()
$processStartInfo.FileName = $executablePath
$processStartInfo.Arguments = "`"$samplePath`""
$processStartInfo.WorkingDirectory = $repoRoot
$processStartInfo.RedirectStandardOutput = $true
$processStartInfo.RedirectStandardError = $true
$processStartInfo.UseShellExecute = $false
$processStartInfo.CreateNoWindow = $true

$process = [System.Diagnostics.Process]::new()
$process.StartInfo = $processStartInfo

if (-not $process.Start()) {
    throw "Failed to start published executable at $executablePath"
}

$standardOutput = $process.StandardOutput.ReadToEnd()
$standardError = $process.StandardError.ReadToEnd()
$process.WaitForExit()

if ($process.ExitCode -ne 0) {
    Write-Error "Published CLI exited with code $($process.ExitCode). Standard error: $standardError"
}

$expectedOutput = @(
    "Loaded 2 message(s).",
    "Inspected 2 message(s).",
    "Sent 2 message(s): 2 succeeded, 0 failed.",
    "- record-1: succeeded",
    "- record-2: succeeded"
)

foreach ($line in $expectedOutput) {
    if (-not $standardOutput.Contains($line)) {
        Write-Error "Published CLI output did not contain expected line '$line'. Standard output: $standardOutput"
    }
}

if (-not [string]::IsNullOrWhiteSpace($standardError)) {
    Write-Error "Published CLI wrote unexpected standard error: $standardError"
}

Write-Host "Published CLI verification passed."
Write-Host "Executable: $executablePath"
Write-Host "Sample: $samplePath"
