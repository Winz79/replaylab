# ReplayLab Samples

Samples in this folder are tiny, synthetic, and generic. They are intended for
documentation, manual parser checks, and future smoke tests.

`basic.csv` follows the current first-slice CSV parser limitations:

- no quoted fields
- no embedded commas
- no embedded newlines
- generic column names and values only

After publishing `ReplayLab.Cli`, use `basic.csv` to verify the local
executable output:

```powershell
dotnet publish src/ReplayLab.Cli/ReplayLab.Cli.csproj --configuration Release --output ./artifacts/publish/replaylab
./artifacts/publish/replaylab/ReplayLab.Cli.exe samples/basic.csv
```

On non-Windows systems, run `./artifacts/publish/replaylab/ReplayLab.Cli`
instead.
