# ReplayLab Samples

Samples in this folder are tiny, synthetic, and generic. They are intended for
documentation, manual parser checks, and future smoke tests.

`basic.csv` follows the current first-slice CSV parser limitations:

- no quoted fields
- no embedded commas
- no embedded newlines
- generic column names and values only

`basic.csv` is also sufficient for the current M3 command shapes, so no second
sample is required yet.

Run the CLI from source with the compatibility command:

```powershell
dotnet run --project src/ReplayLab.Cli/ReplayLab.Cli.csproj -- samples/basic.csv
```

Run the CLI from source with the explicit CSV format:

```powershell
dotnet run --project src/ReplayLab.Cli/ReplayLab.Cli.csproj -- --format csv samples/basic.csv
```

For the M4 HTTP preview, start a local-only receiver in a separate PowerShell
window:

```powershell
$listener = [System.Net.HttpListener]::new()
$listener.Prefixes.Add("http://localhost:5087/")
$listener.Start()
Write-Host "Listening on http://localhost:5087/ . Press Ctrl+C to stop."

while ($listener.IsListening) {
    $context = $listener.GetContext()
    $reader = [System.IO.StreamReader]::new(
        $context.Request.InputStream,
        $context.Request.ContentEncoding)
    $body = $reader.ReadToEnd()
    Write-Host "$($context.Request.HttpMethod) $($context.Request.Url.AbsoluteUri)"
    Write-Host "Content-Type: $($context.Request.ContentType)"
    Write-Host $body
    $context.Response.StatusCode = 202
    $context.Response.Close()
}
```

Then run the HTTP preview command from the repository root:

```powershell
dotnet run --project src/ReplayLab.Cli/ReplayLab.Cli.csproj -- --sender http --endpoint-url http://localhost:5087/ samples/basic.csv
```

Expected CLI success output shape:

```text
Loaded 2 message(s).
Inspected 2 message(s).
- record-1: payload 70 character(s)
- record-2: payload 70 character(s)
Sent 2 message(s): 2 succeeded, 0 failed.
- record-1: succeeded
- record-2: succeeded
```

Expected receiver output shape:

```text
POST http://localhost:5087/
Content-Type: application/json
{"messageType":"Created","name":"alpha","quantity":"2","status":"new"}
POST http://localhost:5087/
Content-Type: application/json
{"messageType":"Updated","name":"beta","quantity":"5","status":"done"}
```

To see the CLI failure shape, stop the listener and run the same command again:

```text
Loaded 2 message(s).
Inspected 2 message(s).
- record-1: payload 70 character(s)
- record-2: payload 70 character(s)
Sent 2 message(s): 0 succeeded, 2 failed.
- record-1: failed - [platform-specific connection error]
- record-2: failed - [platform-specific connection error]
```

The HTTP preview is intentionally limited to local synthetic testing. M4 does
not add method selection, header mapping, body mapping, response body capture,
authentication, certificates, retries, config files, config DSL, Docker, WCF,
or private adapters.

After publishing `ReplayLab.Cli`, use the same explicit CSV format with the
local executable output:

```powershell
dotnet publish src/ReplayLab.Cli/ReplayLab.Cli.csproj --configuration Release --output ./artifacts/publish/replaylab
./artifacts/publish/replaylab/ReplayLab.Cli.exe samples/basic.csv
./artifacts/publish/replaylab/ReplayLab.Cli.exe --format csv samples/basic.csv
./artifacts/publish/replaylab/ReplayLab.Cli.exe --sender http --endpoint-url http://localhost:5087/ samples/basic.csv
```

On non-Windows systems, run the apphost without the `.exe` extension:

```bash
./artifacts/publish/replaylab/ReplayLab.Cli samples/basic.csv
./artifacts/publish/replaylab/ReplayLab.Cli --format csv samples/basic.csv
./artifacts/publish/replaylab/ReplayLab.Cli --sender http --endpoint-url http://localhost:5087/ samples/basic.csv
```

## Hostable Entry Point Sample

`samples/ReplayLab.HostSample/` is a tiny synthetic composition proof for M7.
It is not a new product shell. It owns DI registration and host startup, then
invokes ReplayLab's public hostable entry points.

Run the synthetic sample CLI host:

```powershell
dotnet run --project samples/ReplayLab.HostSample/ReplayLab.HostSample.csproj -- cli samples/basic.csv
```

Run the synthetic sample Web host:

```powershell
dotnet run --project samples/ReplayLab.HostSample/ReplayLab.HostSample.csproj -- web
```

The sample proves Web host ownership through `AddReplayLabWeb()` and `MapReplayLabWeb()`, and it can override parser and sender services from its own composition root.

## NuGet-based Custom Replay Tool Sample

`samples/CustomReplayTool/` demonstrates the M10B developer adoption story:
consume ReplayLab through local NuGet packages (not project references),
provide a custom parser and sender, and compose the ReplayLab Web host.

See [samples/CustomReplayTool/README.md](CustomReplayTool/README.md) for
build, run, and composition instructions.
