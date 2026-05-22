param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$outputPath = Join-Path $repoRoot "artifacts/packages"

if (-not (Test-Path $outputPath)) {
    New-Item -ItemType Directory -Path $outputPath | Out-Null
}

$projects = @(
    "src/ReplayLab.Core/ReplayLab.Core.csproj"
    "src/ReplayLab.Parsers.Csv/ReplayLab.Parsers.Csv.csproj"
    "src/ReplayLab.Adapters.Mock/ReplayLab.Adapters.Mock.csproj"
    "src/ReplayLab.Adapters.Http/ReplayLab.Adapters.Http.csproj"
    "src/ReplayLab.Cli.Hosting/ReplayLab.Cli.Hosting.csproj"
    "src/ReplayLab.Web.Hosting/ReplayLab.Web.Hosting.csproj"
)

foreach ($project in $projects) {
    $projectPath = Join-Path $repoRoot $project
    Write-Host "Packing $project ..."
    dotnet pack $projectPath --configuration $Configuration --output $outputPath
}

Write-Host "Local pack complete. Packages written to $outputPath"
