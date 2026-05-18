# ReplayLab Samples

Samples in this folder are tiny, synthetic, and generic. They are intended for
documentation, manual parser checks, and future smoke tests.

`basic.csv` follows the current first-slice CSV parser limitations:

- no quoted fields
- no embedded commas
- no embedded newlines
- generic column names and values only

`basic.csv` is also sufficient for the current M3 explicit input format flow,
so no second sample is required yet.

Run the CLI from source with the explicit CSV format:

```powershell
dotnet run --project src/ReplayLab.Cli/ReplayLab.Cli.csproj -- --format csv samples/basic.csv
```

After publishing `ReplayLab.Cli`, use the same explicit CSV format with the
local executable output:

```powershell
dotnet publish src/ReplayLab.Cli/ReplayLab.Cli.csproj --configuration Release --output ./artifacts/publish/replaylab
./artifacts/publish/replaylab/ReplayLab.Cli.exe --format csv samples/basic.csv
```

On non-Windows systems, run the apphost without the `.exe` extension:

```bash
./artifacts/publish/replaylab/ReplayLab.Cli --format csv samples/basic.csv
```
