param(
    [string]$Configuration = "Release",
    [string]$OutputPath,
    [switch]$SkipSmokeTest
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$projectPath = Join-Path $repoRoot "src/ReplayLab.Desktop/ReplayLab.Desktop.csproj"

$architecture = [System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture

$isWindows = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)
$isLinux = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Linux)
$isMacOs = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::OSX)

$rid = if ($isWindows) {
    "win-x64"
}
elseif ($isLinux) {
    "linux-x64"
}
elseif ($isMacOs) {
    "osx-x64"
}
else {
    throw "Unsupported OS platform for desktop publish verification."
}

if ($architecture -ne [System.Runtime.InteropServices.Architecture]::X64) {
    Write-Warning "Desktop publish verification is configured for x64. Current architecture is $architecture."
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $repoRoot "artifacts/publish/replaylab-desktop-$rid"
}
else {
    $OutputPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputPath)
}

dotnet publish $projectPath --configuration $Configuration --runtime $rid --output $OutputPath

$executableName = if ($isWindows) { "ReplayLab.Desktop.exe" } else { "ReplayLab.Desktop" }
$executablePath = Join-Path $OutputPath $executableName

if (-not (Test-Path $executablePath)) {
    throw "Published executable was not found at $executablePath"
}

Write-Host "Published executable verified: $executablePath"

if ($SkipSmokeTest) {
    Write-Host "Smoke test skipped by request."
    return
}

Write-Host "Running smoke test (launching process for up to 5 seconds)..."

$processStartInfo = [System.Diagnostics.ProcessStartInfo]::new()
$processStartInfo.FileName = $executablePath
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

$exited = $process.WaitForExit(5000)

if ($exited) {
    $standardError = $process.StandardError.ReadToEnd()
    if ($process.ExitCode -ne 0) {
        Write-Error "Published Desktop app exited immediately with code $($process.ExitCode). Standard error: $standardError"
    }
    else {
        Write-Host "Smoke test passed (process exited cleanly)."
    }
}
else {
    $process.Kill()
    $process.WaitForExit()
    Write-Host "Smoke test passed (process stayed alive for 5 seconds)."
}

Write-Host "Published Desktop verification passed."
Write-Host "Executable: $executablePath"
Write-Host "Runtime Identifier: $rid"
