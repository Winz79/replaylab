# Discovery Slice: Issue #70 Desktop AppHost with WebView2

> Playbook: `.ai/playbooks/discovery-to-slice.md`
> Prompt: `.ai/prompts/product-strategist.md`
> Issue: `#70 [Discovery] Desktop AppHost with WebView2 and self-hosted Web UI`

---

## 1. Product Problem

ReplayLab today requires users to run CLI commands or manually start the Web app.
A desktop shell would make the tool accessible to non-developer operators and
provide a clearer launch/lifecycle model. The open question is not *whether* to
build it—the roadmap already accepted M8 as Desktop AppHost—but which hosting,
packaging, and framework decisions keep the scope minimal while proving value.

## 2. Target User / Operator

- Primary: developers and QA operators who want a local replay workbench without
  managing terminals or browser tabs.
- Secondary: private project teams who want to package a ReplayLab-powered
  workbench for internal distribution.

## 3. Value Proposition

- One-click launch experience instead of `dotnet run` or manual Kestrel startup.
- Window lifecycle managed by the OS (taskbar, close, restart).
- Reuses the existing M5/M7 Web UI without a frontend rewrite.
- Private hosts can later replace the public shell with their own composition root
  using the M7 hostable Web seam.

## 4. Options Considered

| Option | Pros | Cons |
| --- | --- | --- |
| **A. WinUI 3 + WebView2 + in-process Kestrel** | Modern Windows stack; native WebView2 support; single-project deployment; aligns with M7 hostable Web hooks. | Windows-only. |
| **B. WPF + WebView2 + in-process Kestrel** | Mature; well-documented WebView2 embedding. | Legacy framework; no future roadmap alignment. |
| **C. Cross-platform (Avalonia/MAUI + native web control)** | Not Windows-only. | Web control quality varies; no M7 seam reuse guarantee; expands scope beyond the current user base. |
| **D. Fixed localhost port** | Simple URL construction. | Collision risk; firewall prompts; hard to run multiple instances. |
| **E. Dynamic loopback port** | No collision; multiple instances possible. | Requires URL handoff from host to WebView2. |
| **F. In-memory/native bridge instead of HTTP** | No TCP/socket surface. | Requires new abstraction layer; breaks M7 hostable Web seam. |

## 5. Recommended Direction

**Option A + E**: WinUI 3 desktop shell with WebView2, self-hosting ASP.NET Core
in-process on a dynamic loopback port, navigating WebView2 to the discovered URL.

- WinUI 3 is the modern Windows desktop path; WPF is a dead-end for new work.
- Cross-platform expansion is a future candidate (Candidate M10+) only after the
  Windows shell proves the product model.
- In-process Kestrel reuses the existing M7 `ReplayLab.Web.Hosting` seam directly.
- Dynamic port avoids collision and firewall friction.

## 6. MVP Boundary

- One WinUI 3 window with an embedded WebView2.
- In-process ASP.NET Core host started on loopback with a free port.
- WebView2 navigates to `http://localhost:<free-port>` after the host is ready.
- Graceful shutdown: Web host stops when the window closes.
- Public `ReplayLab.Desktop` project registered in the solution.
- Smoke test proving the shell starts, navigates, and exits without errors.

## 7. Explicit Out of Scope

- Cross-platform desktop shell (deferred to future candidate).
- Single-file or self-contained publishing (packaging is a follow-up slice).
- WebView2 runtime bundling (assume preinstalled or evergreen installer).
- Private adapter registration inside the desktop shell (M7 composition root
  ownership stays with the caller; the public shell uses safe defaults).
- Editable grid, parser quality, or HTTP sender UI work.
- Installer creation (MSIX, WiX, etc.).

## 8. Risks / Unknowns

| Risk | Mitigation |
| --- | --- |
| WinUI 3 project system is heavier than WPF | Accept; it is the supported modern path. |
| WebView2 runtime missing on target machine | Document requirement; defer bundling. |
| Kestrel port discovery race during startup | Probe port availability before host start; retry once. |
| Desktop tests require Windows/UI automation | Keep smoke test minimal; use headless WebView2 where possible. |
| M7 hostable Web seam may need a small extension for clean shutdown | Accept as implementation risk; resolve in the first slice. |

## 9. Next Implementation-Ready Slice

**Issue draft: "Scaffold WinUI 3 desktop shell with WebView2 and in-process Kestrel host"**

Scope:
1. Create `src/ReplayLab.Desktop` (WinUI 3 packaged desktop project).
2. Add `Microsoft.Web.WebView2` and ASP.NET Core hosting package references.
3. Implement minimal `App.xaml.cs` + `MainWindow.xaml.cs` that:
   - starts an in-process `WebApplication` on a free loopback port via
     `ReplayLab.Web.Hosting` (`AddReplayLabWeb` / `MapReplayLabWeb`),
   - navigates WebView2 to the local URL once the host is listening,
   - stops the host on window close.
4. Add a smoke/integration test that asserts the host starts and the WebView
   navigates successfully.
5. Update `ReplayLab.sln` with the new project.

Acceptance criteria:
- `dotnet build ReplayLab.sln` includes the desktop project and passes.
- Desktop app launches from Visual Studio or `dotnet run` and shows the
  ReplayLab Web UI inside the window.
- Close button shuts down the Kestrel host without process leaks.
- Smoke test passes on Windows.

## 10. Suggested Artifact

- Create the issue draft above in `#70` discussion or as a new implementation issue
  linked to `#70`.
- Rename/supersede `docs/milestones/m8-web-external-composition.md` to avoid
  milestone numbering confusion. The roadmap already accepted M8 as Desktop
  AppHost; the Web external composition work should be treated as an M7.5
  completion item or folded into M8 prerequisite work, not a competing M8
  definition.

---

## Handoff to Conductor

```text
- Selected direction: WinUI 3 + WebView2 + in-process Kestrel on dynamic loopback port.
- Why this now: The roadmap already accepted M8 as Desktop AppHost; the only
  remaining open questions are framework and port strategy. WinUI 3 is the modern
  Windows path and dynamic port avoids runtime friction.
- MVP boundary:
  * One WinUI 3 window embedding WebView2.
  * Self-hosted ASP.NET Core on loopback via existing M7 Web hosting seam.
  * Graceful startup/shutdown.
  * Smoke test.
- Out of scope:
  * Cross-platform shell.
  * Packaging/installer.
  * WebView2 runtime bundling.
  * Private adapter registration in the public shell.
- Next slice: Scaffold WinUI 3 desktop shell with WebView2 and in-process Kestrel host.
- Durable tracking needed: yes
- Suggested artifact: issue draft linked to #70
```
