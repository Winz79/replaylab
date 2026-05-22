#!/usr/bin/env pwsh
#Requires -Version 7.0
param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$repoRoot = $PSScriptRoot | Split-Path -Parent
$sampleDir = Join-Path $repoRoot "samples/CustomReplayTool"
$solutionFile = Join-Path $sampleDir "CustomReplayTool.slnx"

Write-Host "Step 1: Produce local NuGet packages..."
& "$repoRoot/eng/pack-local.ps1" -Configuration $Configuration

Write-Host "Step 2: Restore sample solution from local feed..."
dotnet restore $solutionFile

Write-Host "Step 3: Build sample solution..."
dotnet build $solutionFile -c $Configuration --no-restore

Write-Host "Step 4: Verify no ProjectReference to ReplayLab source projects..."
$domainProject = Join-Path $sampleDir "src/CustomReplayTool.Domain/CustomReplayTool.Domain.csproj"
$webProject = Join-Path $sampleDir "src/CustomReplayTool.Web/CustomReplayTool.Web.csproj"

foreach ($proj in @($domainProject, $webProject)) {
    $content = Get-Content $proj -Raw
    if ($content -match '<ProjectReference.*ReplayLab') {
        Write-Error "Found ProjectReference to ReplayLab in $proj. Sample must use PackageReference only."
        exit 1
    }
}

Write-Host "Verification complete. Sample builds correctly using PackageReference."
