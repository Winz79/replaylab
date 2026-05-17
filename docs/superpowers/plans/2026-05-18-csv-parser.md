# CSV Parser Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement issue #5 by loading a small synthetic CSV stream into generic `ReplayBatch` messages.

**Architecture:** Add `ReplayLab.Parsers.Csv` as a concrete parser adapter that depends only on `ReplayLab.Core`. Keep the parser generic: CSV columns become a whole-row JSON payload, headers stay empty, and metadata only records parser/tooling context.

**Tech Stack:** .NET `net10.0`, xUnit, `System.Text.Json`, current `ReplayLab.Core` contracts.

---

## Source Requirements

- GitHub issue: #5, `Slice: implement first CSV parser`.
- Planning issue: #4, `Task: write CSV parser implementation plan`.
- PRD: `docs/prd/0002-file-parsing.md`.
- ADR: `docs/adr/0003-start-with-csv-parser.md`.
- Vertical plan: `docs/vertical-slice-plan.md`.
- Decision issue: #2, whole-row CSV parser decision.

## File Structure

- Create `src/ReplayLab.Parsers.Csv/ReplayLab.Parsers.Csv.csproj`: parser project targeting `net10.0`, referencing only `ReplayLab.Core`.
- Create `src/ReplayLab.Parsers.Csv/CsvReplayMessageParser.cs`: `IMessageParser` implementation for CSV streams.
- Create `src/ReplayLab.Parsers.Csv/CsvParseException.cs`: clear parser exception type for invalid CSV input.
- Create `tests/ReplayLab.Parsers.Csv.Tests/ReplayLab.Parsers.Csv.Tests.csproj`: xUnit test project referencing `ReplayLab.Core` and `ReplayLab.Parsers.Csv`.
- Create `tests/ReplayLab.Parsers.Csv.Tests/CsvReplayMessageParserTests.cs`: focused parser behavior tests using synthetic CSV strings.
- Modify `ReplayLab.sln`: include the parser and parser test projects under existing `src` and `tests` solution folders.
- Do not modify `ReplayLab.Core` unless the implementation proves the current contract cannot satisfy issue #5.
- Do not modify `ReplayLab.Cli`; CLI integration belongs to Slice 3.

## Behavior Contract

- The first line that is neither empty nor a comment is the header row.
- A comment line is any line whose first non-whitespace character is `#`.
- Empty and comment lines are ignored before and after the header row.
- Every parsed data row becomes one `ReplayMessage`.
- Message IDs are generated as `record-1`, `record-2`, and so on from parsed data record numbers.
- `Payload` is a JSON object where property names are the CSV headers and property values are the row fields.
- `Headers` is an empty read-only dictionary.
- `Metadata` contains:
  - `sourceFormat`: `csv`
  - `sourceRowNumber`: the 1-based physical row number in the input stream
  - `dataRecordNumber`: the 1-based parsed data record number
- Invalid input throws `CsvParseException` with a message that names the problem and the row number when applicable.

## Task 1: Add Parser Project Skeleton

**Files:**
- Create: `src/ReplayLab.Parsers.Csv/ReplayLab.Parsers.Csv.csproj`
- Create: `src/ReplayLab.Parsers.Csv/CsvReplayMessageParser.cs`
- Create: `src/ReplayLab.Parsers.Csv/CsvParseException.cs`
- Modify: `ReplayLab.sln`

- [ ] **Step 1: Create the parser project**

Run:

```powershell
dotnet new classlib -n ReplayLab.Parsers.Csv -o src/ReplayLab.Parsers.Csv --framework net10.0
dotnet add src/ReplayLab.Parsers.Csv/ReplayLab.Parsers.Csv.csproj reference src/ReplayLab.Core/ReplayLab.Core.csproj
dotnet sln ReplayLab.sln add src/ReplayLab.Parsers.Csv/ReplayLab.Parsers.Csv.csproj --solution-folder src
Remove-Item src/ReplayLab.Parsers.Csv/Class1.cs
```

Expected: project is created and added to the solution under `src`.

- [ ] **Step 2: Add the initial exception type**

Create `src/ReplayLab.Parsers.Csv/CsvParseException.cs`:

```csharp
namespace ReplayLab.Parsers.Csv;

public sealed class CsvParseException : Exception
{
    public CsvParseException(string message)
        : base(message)
    {
    }
}
```

- [ ] **Step 3: Add the parser class stub**

Create `src/ReplayLab.Parsers.Csv/CsvReplayMessageParser.cs`:

```csharp
using ReplayLab.Core;

namespace ReplayLab.Parsers.Csv;

public sealed class CsvReplayMessageParser : IMessageParser
{
    public Task<ReplayBatch> ParseAsync(Stream input, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
```

- [ ] **Step 4: Build the project skeleton**

Run:

```powershell
dotnet build src/ReplayLab.Parsers.Csv/ReplayLab.Parsers.Csv.csproj
```

Expected: build succeeds.

## Task 2: Add Parser Test Project

**Files:**
- Create: `tests/ReplayLab.Parsers.Csv.Tests/ReplayLab.Parsers.Csv.Tests.csproj`
- Create: `tests/ReplayLab.Parsers.Csv.Tests/CsvReplayMessageParserTests.cs`
- Modify: `ReplayLab.sln`

- [ ] **Step 1: Create the test project**

Run:

```powershell
dotnet new xunit -n ReplayLab.Parsers.Csv.Tests -o tests/ReplayLab.Parsers.Csv.Tests --framework net10.0
dotnet add tests/ReplayLab.Parsers.Csv.Tests/ReplayLab.Parsers.Csv.Tests.csproj reference src/ReplayLab.Core/ReplayLab.Core.csproj
dotnet add tests/ReplayLab.Parsers.Csv.Tests/ReplayLab.Parsers.Csv.Tests.csproj reference src/ReplayLab.Parsers.Csv/ReplayLab.Parsers.Csv.csproj
dotnet sln ReplayLab.sln add tests/ReplayLab.Parsers.Csv.Tests/ReplayLab.Parsers.Csv.Tests.csproj --solution-folder tests
Remove-Item tests/ReplayLab.Parsers.Csv.Tests/UnitTest1.cs
```

Expected: test project is created and added to the solution under `tests`.

- [ ] **Step 2: Align test package versions with existing test projects**

Set `tests/ReplayLab.Parsers.Csv.Tests/ReplayLab.Parsers.Csv.Tests.csproj` to:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="6.0.4" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.4" />
  </ItemGroup>

  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\ReplayLab.Core\ReplayLab.Core.csproj" />
    <ProjectReference Include="..\..\src\ReplayLab.Parsers.Csv\ReplayLab.Parsers.Csv.csproj" />
  </ItemGroup>

</Project>
```

## Task 3: Test Valid Whole-Row CSV Parsing

**Files:**
- Modify: `tests/ReplayLab.Parsers.Csv.Tests/CsvReplayMessageParserTests.cs`
- Modify: `src/ReplayLab.Parsers.Csv/CsvReplayMessageParser.cs`

- [ ] **Step 1: Write the failing valid CSV test**

Create `tests/ReplayLab.Parsers.Csv.Tests/CsvReplayMessageParserTests.cs`:

```csharp
using System.Text;
using System.Text.Json;

namespace ReplayLab.Parsers.Csv.Tests;

public class CsvReplayMessageParserTests
{
    [Fact]
    public async Task ParseAsync_converts_valid_csv_rows_into_replay_messages()
    {
        const string csv = """
            messageType,name,quantity
            Created,alpha,2
            Updated,beta,5
            """;

        var batch = await Parse(csv);

        Assert.Equal(2, batch.Messages.Count);
        Assert.Equal("record-1", batch.Messages[0].Id);
        Assert.Equal("record-2", batch.Messages[1].Id);
        Assert.Empty(batch.Messages[0].Headers!);

        using var payload = JsonDocument.Parse(batch.Messages[0].Payload);
        Assert.Equal("Created", payload.RootElement.GetProperty("messageType").GetString());
        Assert.Equal("alpha", payload.RootElement.GetProperty("name").GetString());
        Assert.Equal("2", payload.RootElement.GetProperty("quantity").GetString());
    }

    private static async Task<Core.ReplayBatch> Parse(string csv)
    {
        var parser = new CsvReplayMessageParser();
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        return await parser.ParseAsync(stream);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests/ReplayLab.Parsers.Csv.Tests/ReplayLab.Parsers.Csv.Tests.csproj --filter ParseAsync_converts_valid_csv_rows_into_replay_messages
```

Expected: FAIL because `CsvReplayMessageParser.ParseAsync` throws `NotImplementedException`.

- [ ] **Step 3: Implement minimal valid parsing**

Replace `src/ReplayLab.Parsers.Csv/CsvReplayMessageParser.cs` with:

```csharp
using System.Text.Json;
using ReplayLab.Core;

namespace ReplayLab.Parsers.Csv;

public sealed class CsvReplayMessageParser : IMessageParser
{
    public async Task<ReplayBatch> ParseAsync(Stream input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        using var reader = new StreamReader(input, leaveOpen: true);
        string[]? headers = null;
        var messages = new List<ReplayMessage>();
        var rowNumber = 0;

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var line = await reader.ReadLineAsync(cancellationToken);
            rowNumber++;

            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#'))
            {
                continue;
            }

            var fields = ParseLine(line, rowNumber);
            if (headers is null)
            {
                headers = fields;
                continue;
            }

            if (fields.Length != headers.Length)
            {
                throw new CsvParseException($"CSV row {rowNumber} has {fields.Length} fields but header row has {headers.Length} fields.");
            }

            var recordNumber = messages.Count + 1;
            var payloadValues = new Dictionary<string, string>(StringComparer.Ordinal);
            for (var i = 0; i < headers.Length; i++)
            {
                payloadValues[headers[i]] = fields[i];
            }

            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["sourceFormat"] = "csv",
                ["sourceRowNumber"] = rowNumber.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["dataRecordNumber"] = recordNumber.ToString(System.Globalization.CultureInfo.InvariantCulture),
            };

            messages.Add(new ReplayMessage(
                $"record-{recordNumber}",
                JsonSerializer.Serialize(payloadValues),
                new Dictionary<string, string>(StringComparer.Ordinal),
                metadata));
        }

        if (headers is null)
        {
            throw new CsvParseException("CSV input does not contain a header row.");
        }

        return new ReplayBatch(messages);
    }

    private static string[] ParseLine(string line, int rowNumber)
    {
        if (line.Contains('"'))
        {
            throw new CsvParseException($"CSV row {rowNumber} contains quoted fields, which are not supported in the first parser slice.");
        }

        return line.Split(',');
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run:

```powershell
dotnet test tests/ReplayLab.Parsers.Csv.Tests/ReplayLab.Parsers.Csv.Tests.csproj --filter ParseAsync_converts_valid_csv_rows_into_replay_messages
```

Expected: PASS.

## Task 4: Test Ignored Empty And Comment Lines

**Files:**
- Modify: `tests/ReplayLab.Parsers.Csv.Tests/CsvReplayMessageParserTests.cs`
- Modify: `src/ReplayLab.Parsers.Csv/CsvReplayMessageParser.cs` only if the test fails

- [ ] **Step 1: Add the ignored line test**

Add this test to `CsvReplayMessageParserTests`:

```csharp
[Fact]
public async Task ParseAsync_ignores_empty_lines_and_comment_lines()
{
    const string csv = """

          # synthetic comment before header
        name,status

        alpha,new
          # synthetic comment between records
        beta,done

        """;

    var batch = await Parse(csv);

    Assert.Equal(2, batch.Messages.Count);
    Assert.Equal("record-1", batch.Messages[0].Id);
    Assert.Equal("record-2", batch.Messages[1].Id);
}
```

- [ ] **Step 2: Run test to verify behavior**

Run:

```powershell
dotnet test tests/ReplayLab.Parsers.Csv.Tests/ReplayLab.Parsers.Csv.Tests.csproj --filter ParseAsync_ignores_empty_lines_and_comment_lines
```

Expected: PASS with the implementation from Task 3.

## Task 5: Test Metadata Contract

**Files:**
- Modify: `tests/ReplayLab.Parsers.Csv.Tests/CsvReplayMessageParserTests.cs`
- Modify: `src/ReplayLab.Parsers.Csv/CsvReplayMessageParser.cs` only if the test fails

- [ ] **Step 1: Add the metadata test**

Add this test to `CsvReplayMessageParserTests`:

```csharp
[Fact]
public async Task ParseAsync_records_csv_metadata_for_each_message()
{
    const string csv = """
        # comment row
        name,status
        alpha,new

        beta,done
        """;

    var batch = await Parse(csv);

    Assert.Equal("csv", batch.Messages[0].Metadata!["sourceFormat"]);
    Assert.Equal("3", batch.Messages[0].Metadata!["sourceRowNumber"]);
    Assert.Equal("1", batch.Messages[0].Metadata!["dataRecordNumber"]);
    Assert.Equal("5", batch.Messages[1].Metadata!["sourceRowNumber"]);
    Assert.Equal("2", batch.Messages[1].Metadata!["dataRecordNumber"]);
}
```

- [ ] **Step 2: Run test to verify behavior**

Run:

```powershell
dotnet test tests/ReplayLab.Parsers.Csv.Tests/ReplayLab.Parsers.Csv.Tests.csproj --filter ParseAsync_records_csv_metadata_for_each_message
```

Expected: PASS with the implementation from Task 3.

## Task 6: Test Invalid Input

**Files:**
- Modify: `tests/ReplayLab.Parsers.Csv.Tests/CsvReplayMessageParserTests.cs`
- Modify: `src/ReplayLab.Parsers.Csv/CsvReplayMessageParser.cs`

- [ ] **Step 1: Add invalid input tests**

Add these tests to `CsvReplayMessageParserTests`:

```csharp
[Fact]
public async Task ParseAsync_throws_clear_exception_when_header_is_missing()
{
    var exception = await Assert.ThrowsAsync<CsvParseException>(() => Parse("""

        # only comments

        """));

    Assert.Contains("header row", exception.Message);
}

[Fact]
public async Task ParseAsync_throws_clear_exception_when_field_count_does_not_match_header()
{
    var exception = await Assert.ThrowsAsync<CsvParseException>(() => Parse("""
        name,status
        alpha,new,extra
        """));

    Assert.Contains("row 2", exception.Message);
    Assert.Contains("header row has 2 fields", exception.Message);
}

[Fact]
public async Task ParseAsync_throws_clear_exception_for_quoted_fields_in_first_slice()
{
    var exception = await Assert.ThrowsAsync<CsvParseException>(() => Parse("""
        name,status
        "alpha",new
        """));

    Assert.Contains("row 2", exception.Message);
    Assert.Contains("quoted fields", exception.Message);
}
```

- [ ] **Step 2: Run invalid input tests**

Run:

```powershell
dotnet test tests/ReplayLab.Parsers.Csv.Tests/ReplayLab.Parsers.Csv.Tests.csproj --filter "ParseAsync_throws_clear_exception"
```

Expected: PASS.

## Task 7: Run Full Verification

**Files:**
- No new files.

- [ ] **Step 1: Run parser tests**

Run:

```powershell
dotnet test tests/ReplayLab.Parsers.Csv.Tests/ReplayLab.Parsers.Csv.Tests.csproj
```

Expected: all parser tests pass.

- [ ] **Step 2: Run full solution tests**

Run:

```powershell
dotnet test ReplayLab.sln
```

Expected: all solution tests pass.

- [ ] **Step 3: Inspect dependency direction**

Run:

```powershell
dotnet list src/ReplayLab.Parsers.Csv/ReplayLab.Parsers.Csv.csproj reference
```

Expected: only `src\ReplayLab.Core\ReplayLab.Core.csproj` is listed.

## Task 8: Completion Notes

**Files:**
- Modify docs or ADRs only if implementation behavior diverges from this plan, PRD 0002, or ADR 0003.

- [ ] **Step 1: Confirm scope boundaries**

Verify no code was added for CLI integration, replay orchestration, filtering, dynamic header mapping, WCF, proprietary formats, customer data, or business-specific adapters.

- [ ] **Step 2: Prepare final implementation summary**

Include:

```markdown
Files changed:
- src/ReplayLab.Parsers.Csv/ReplayLab.Parsers.Csv.csproj
- src/ReplayLab.Parsers.Csv/CsvReplayMessageParser.cs
- src/ReplayLab.Parsers.Csv/CsvParseException.cs
- tests/ReplayLab.Parsers.Csv.Tests/ReplayLab.Parsers.Csv.Tests.csproj
- tests/ReplayLab.Parsers.Csv.Tests/CsvReplayMessageParserTests.cs
- ReplayLab.sln

Tests run:
- dotnet test tests/ReplayLab.Parsers.Csv.Tests/ReplayLab.Parsers.Csv.Tests.csproj
- dotnet test ReplayLab.sln

Assumptions:
- Quoted CSV fields are intentionally rejected in the first parser slice rather than partially supported.
- Message IDs use the stable `record-N` format from parsed data record numbers.

Risks:
- The first parser is deliberately small and does not implement full RFC 4180 CSV support.
- Later header mapping must remain outside this parser behavior unless a future issue changes the design.

Follow-up issues:
- Implement #5 using this plan.
- Close or update #2 because PR #6 merged the docs decision checkbox.
```

## Self-Review

- Spec coverage: PRD 0002 and ADR 0003 requirements are covered by Tasks 1 through 8.
- Placeholder scan: no task relies on unspecified behavior or business-specific mappings.
- Type consistency: the plan uses existing `IMessageParser.ParseAsync(Stream, CancellationToken)`, `ReplayBatch`, and `ReplayMessage` signatures.
