$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$packagesPath = Join-Path $repoRoot "artifacts/packages"
$verifyPath = Join-Path $repoRoot "artifacts/package-verify"

if (-not (Test-Path $packagesPath)) {
    throw "Packages directory not found at $packagesPath. Run ./eng/pack-local.ps1 first."
}

$packageFiles = Get-ChildItem -Path $packagesPath -Filter "*.nupkg"
if ($packageFiles.Count -eq 0) {
    throw "No .nupkg files found in $packagesPath. Run ./eng/pack-local.ps1 first."
}

if (Test-Path $verifyPath) {
    Remove-Item -Recurse -Force $verifyPath
}

New-Item -ItemType Directory -Path $verifyPath | Out-Null

$nugetConfig = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="LocalReplayLab" value="$packagesPath" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
"@

$nugetConfigPath = Join-Path $verifyPath "NuGet.config"
Set-Content -Path $nugetConfigPath -Value $nugetConfig

$csprojContent = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <OutputType>Exe</OutputType>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="ReplayLab.Core" Version="0.6.0" />
    <PackageReference Include="ReplayLab.Parsers.Csv" Version="0.6.0" />
    <PackageReference Include="ReplayLab.Adapters.Mock" Version="0.6.0" />
    <PackageReference Include="ReplayLab.Adapters.Http" Version="0.6.0" />
    <PackageReference Include="ReplayLab.Cli.Hosting" Version="0.6.0" />
    <PackageReference Include="ReplayLab.Web.Hosting" Version="0.6.0" />
  </ItemGroup>
</Project>
"@

$csprojPath = Join-Path $verifyPath "VerifyLocalPackages.csproj"
Set-Content -Path $csprojPath -Value $csprojContent

$programCs = @"
Console.WriteLine("Local package restore verification succeeded.");
"@

$programPath = Join-Path $verifyPath "Program.cs"
Set-Content -Path $programPath -Value $programCs

Write-Host "Restoring verification project..."
dotnet restore $csprojPath --configfile $nugetConfigPath

Write-Host "Building verification project..."
dotnet build $csprojPath --no-restore

Write-Host "Local package verification passed."
Write-Host "Packages path: $packagesPath"
Write-Host "Verification project: $verifyPath"
