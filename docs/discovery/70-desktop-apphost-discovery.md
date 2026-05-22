# Discovery Slice: Issue #70 Desktop AppHost with Native Web Views

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
| **A. Photino.NET + native web view + in-process Kestrel** | Cross-platform; uses OS-native web views; reuses M7 hostable Web hooks directly; simple project structure. | Requires runtime dependencies on Linux. |
| **B. WinUI 3 + WebView2 + in-process Kestrel** | Modern Windows stack; native WebView2 support; single-project deployment. | Windows-only. |
| **C. WPF + WebView2 + in-process Kestrel** | Mature; well-documented WebView2 embedding. | Legacy framework; no future roadmap alignment. |
| **D. Cross-platform (Avalonia/MAUI + native web control)** | Not Windows-only. | Web control quality varies; more framework surface than needed. |
| **E. Fixed localhost port** | Simple URL construction. | Collision risk; firewall prompts; hard to run multiple instances. |
| **F. Dynamic loopback port** | No collision; multiple instances possible. | Requires URL handoff from host to the native web view. |
| **G. In-memory/native bridge instead of HTTP** | No TCP/socket surface. | Requires new abstraction layer; breaks M7 hostable Web seam. |

## 5. Recommended Direction

**Option A + F**: Photino.NET desktop shell with native web views, self-hosting
ASP.NET Core in-process on a dynamic loopback port, navigating the embedded web
view to the discovered URL.

- Photino.NET keeps the shell thin while covering Windows, Linux, and macOS
  through the platform's native browser engine.
- WinUI 3 and WPF remain rejected because they lock the repo to Windows-only
  build and runtime assumptions.
- In-process Kestrel reuses the existing M7 `ReplayLab.Web.Hosting` seam directly.
- Dynamic port avoids collision and firewall friction.

## 6. MVP Boundary

- One Photino window with an embedded native web view.
- In-process ASP.NET Core host started on loopback with a free port.
- The native web view navigates to `http://localhost:<free-port>` after the host is ready.
- Graceful shutdown: Web host stops when the window closes.
- Public `ReplayLab.Desktop` project registered in the solution.
- Smoke test proving the desktop bootstrap starts and the hosted Web UI responds.

## 7. Explicit Out of Scope

- WebView2/runtime bundling beyond what the host OS already provides.
- Private adapter registration inside the desktop shell (M7 composition root
  ownership stays with the caller; the public shell uses safe defaults).
- Editable grid, parser quality, or HTTP sender UI work.
- Installer creation (MSIX, WiX, etc.).

## 8. Risks / Unknowns

| Risk | Mitigation |
| --- | --- |
| Linux machines may miss `libwebkit2gtk-4.0` | Document requirement and verify publish output. |
| Windows machines may miss Edge WebView2 runtime | Document requirement; rely on evergreen runtime. |
| Kestrel port discovery race during startup | Probe port availability before host start; retry once. |
| Desktop tests require UI automation to cover the full window | Keep public tests focused on bootstrap/address discovery and hosted Web response. |
| M7 hostable Web seam may need a small extension for clean shutdown | Accept as implementation risk; resolve in the first slice. |

## 9. Next Implementation-Ready Slice

**Issue draft: "Scaffold cross-platform desktop shell with Photino.NET and in-process Kestrel host"**

Scope:
1. Create `src/ReplayLab.Desktop` (Photino.NET desktop project).
2. Add Photino.NET and ASP.NET Core hosting package references.
3. Implement minimal startup code that:
   - starts an in-process `WebApplication` on a free loopback port via
      `ReplayLab.Web.Hosting` (`AddReplayLabWeb` / `MapReplayLabWeb`),
   - navigates the native web view to the local URL once the host is listening,
   - stops the host on window close.
4. Add a smoke/integration test that asserts the host starts and the hosted Web
   UI responds at the discovered local URL.
5. Update `ReplayLab.sln` with the new project.

Acceptance criteria:
- `dotnet build ReplayLab.sln` includes the desktop project and passes.
- Desktop app launches from `dotnet run` and shows the ReplayLab Web UI inside
  the native window.
- Close button shuts down the Kestrel host without process leaks.
- Smoke test passes.

## 10. Outcome

The implementation issue was promoted as `#90 feat: scaffold cross-platform
desktop AppHost with Photino.NET`, and the accepted direction is now captured by
ADR 0010 and the M8 milestone doc.

---

## Handoff to Conductor

```text
- Selected direction: Photino.NET + native web views + in-process Kestrel on
  dynamic loopback port.
- Why this now: The roadmap accepted M8 as Desktop AppHost, and Photino.NET kept
  the shell generic, thin, and cross-platform while still using WebView2 on Windows.
- MVP boundary:
  * One Photino window embedding the platform-native web view.
  * Self-hosted ASP.NET Core on loopback via existing M7 Web hosting seam.
  * Graceful startup/shutdown.
  * Smoke test.
- Out of scope:
  * Installer creation.
  * Runtime bundling beyond OS/browser prerequisites.
  * Private adapter registration in the public shell.
- Next slice: packaging and release workflow follow-ups, if the desktop shell is promoted further.
- Durable tracking needed: yes
- Suggested artifact: implementation issue #90 linked to #70
```
