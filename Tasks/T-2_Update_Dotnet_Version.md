# T-2: Update .NET version

## Summary
Upgrade the entire solution from .NET 7 to a current supported .NET version (LTS), bumping matching Microsoft.* packages and verifying that build, EF migrations, and integration tests all still succeed.

## Context / Background
`TECHNICAL_SPECIFICATION.md` §1.3 and §7.1.1 pin the project to ASP.NET Core 7.0 / .NET 7 Runtime, and `README.md` §Development instructs installing .NET 7 for after-checkout setup. .NET 7 is out of Microsoft support. The project's stated purpose (practicing tech) makes staying on a supported runtime a natural upgrade — the latest LTS is the sensible target (.NET 8 as of this writing; if a newer LTS is current, pick that).

The solution contains an ASP.NET Core web API (`App/`), a WPF Launcher (Windows-only), and integration tests (`IntegrationTests/Tests.Integration`). The WPF project needs a `net<N>.0-windows` TFM, unlike the cross-platform projects. This task is independent of [[T-1]] and [[T-3]].

## Acceptance Criteria
- [ ] Every `.csproj` `TargetFramework` (or `TargetFrameworks`) is updated to the chosen version — cross-platform projects to `netX.0`, WPF Launcher to `netX.0-windows`.
- [ ] `global.json` (if present at the repo root or under a project) is updated to the new SDK version, or removed intentionally with a note on why.
- [ ] EF Core, ASP.NET Core, and any other `Microsoft.*` packages are bumped to the matching major version (e.g. `Microsoft.EntityFrameworkCore.*` 8.x for .NET 8).
- [ ] `dotnet build` succeeds for the whole solution with no new warnings-as-errors.
- [ ] `dotnet ef database update` succeeds on a fresh dev database: run `.\LocalDevScripts\Drop_Create.ps1` from `App`, then `dotnet ef database update`.
- [ ] All `Tests.Integration` tests pass with the web app running.
- [ ] `README.md` is updated to state the new required SDK version (both the `dotnet` install instruction and the `dotnet-ef` global-tool version).

## Implementation Notes / Decisions

### Files/areas to touch
- All `.csproj` files under `App/`, `IntegrationTests/`, and the WPF Launcher project. Search for `<TargetFramework>net7.0` and `<TargetFramework>net7.0-windows`.
- `global.json` at the repo root (if it exists) — check and update or delete.
- `README.md` — the "Install Postgres 14, .NET 7, Node.js…" line and the `dotnet tool install --global dotnet-ef --version 8.0.0` line must reflect the new SDK/tool major version.
- `TECHNICAL_SPECIFICATION.md` — §1.3, §7.1.1, §9.1.1 all reference .NET 7 explicitly. Updating the spec is a nice-to-have but not in the acceptance criteria; call out that it's stale.

### Package bumps to verify
- `Microsoft.EntityFrameworkCore.*` and `Npgsql.EntityFrameworkCore.PostgreSQL` — bump both together; Npgsql's major track aligns with EF Core majors.
- `Microsoft.AspNetCore.*` (JwtBearer, etc.).
- `Microsoft.Extensions.*` (Logging, Configuration).
- `Swashbuckle.AspNetCore` — often lags a release; pick a version compatible with the new target.
- `Serilog.AspNetCore` — bump to a version supporting the new target.
- Any `Microsoft.NET.Test.Sdk`, `NUnit`, and `Selenium.WebDriver` in the tests project — should keep working but update `Microsoft.NET.Test.Sdk` to something current.

### Gotchas — must call out
- **`global.json`**: even if you don't see it, `dotnet-ef` and `dotnet build` behave differently if one exists. Grep for it.
- **CI / Docker**: search the repo for `mcr.microsoft.com/dotnet/`, `dotnet-version:`, or `.github/workflows` — bump the image tags and workflow `dotnet-version` inputs. If there is no CI config today, note that in the report rather than skipping the check.
- **Packages pinned to .NET 7-era versions**: some transitive dependencies expose overloads only on newer targets — watch for new nullable-reference or trimming warnings and address the important ones.
- **WPF Launcher**: this is Windows-specific and must remain `netX.0-windows`. Keep `<UseWPF>true</UseWPF>`. Verify it still builds on Windows (`dotnet build` for the Launcher .csproj).
- **`dotnet-ef` global tool**: the README pins `--version 8.0.0`. This must match the chosen EF Core major (e.g. `9.0.0` if bumping to .NET 9 and EF Core 9).

### Open decision
- Target **.NET 8 (LTS)** unless a newer LTS is current when the work is done. Do not target a Current (non-LTS) release for a practice project — the goal is stability, not chasing the frontier.

---

## Prompt (paste into a clean Claude context)

````
You are working on the To-Do Lists project at C:\Code\ak\TodoLists on Windows. Use PowerShell for all shell commands (no `&&`, no Bash-isms). Start by reading `AGENTS.md`, `README.md`, and `TECHNICAL_SPECIFICATION.md` to load project context.

## Task
Upgrade the entire solution from .NET 7 to the current .NET LTS (target .NET 8 unless a newer LTS is out at the time of implementation). Bump matching Microsoft.* packages. Verify build, EF migrations, and integration tests all still succeed.

## Context you need
- The solution has three kinds of projects:
  - ASP.NET Core web API under `App/` — cross-platform, TFM `net7.0`.
  - WPF Launcher (Windows-only) — TFM `net7.0-windows`, keeps `<UseWPF>true</UseWPF>`.
  - `IntegrationTests/Tests.Integration` — NUnit + Selenium tests that require the web app to be running (see README).
- README currently instructs installing .NET 7 and pins `dotnet-ef --version 8.0.0`. Update both.
- TECHNICAL_SPECIFICATION.md §1.3, §7.1.1, §9.1.1 mention .NET 7 — noting they are stale is enough; updating them is not required for this task.

## What to change
1. Every `.csproj` `TargetFramework` → new version. Cross-platform → `net8.0`; WPF Launcher → `net8.0-windows`.
2. `global.json` at repo root (if present) → new SDK, or delete intentionally.
3. Bump `Microsoft.EntityFrameworkCore.*`, `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.AspNetCore.*`, `Microsoft.Extensions.*`, `Swashbuckle.AspNetCore`, `Serilog.AspNetCore`, `Microsoft.NET.Test.Sdk` to versions matching the new target.
4. `README.md` — update the `.NET 7` install line and the `dotnet-ef --version` line.
5. Search for CI/Docker references (`mcr.microsoft.com/dotnet/`, `dotnet-version:`, `.github/workflows/`) and update them too. If none exist, say so in the report.

## Definition of done — all must hold
- `dotnet build` succeeds for the whole solution with no new warnings-as-errors.
- `dotnet ef database update` succeeds on a fresh dev DB:
  ```powershell
  cd App
  .\LocalDevScripts\Drop_Create.ps1
  dotnet ef database update
  ```
- All `Tests.Integration` tests pass with the web app running (start the app per README, then `cd IntegrationTests\Tests.Integration; dotnet test`).
- `README.md` states the new required SDK and `dotnet-ef` versions.
- No `.csproj` still targets `net7.0` (grep to be sure).
- WPF Launcher still builds and keeps `net<N>.0-windows` with `<UseWPF>true</UseWPF>`.

## Report back
- Which .NET LTS version you chose and why (one sentence).
- List of files changed with a one-line description each.
- Output of `dotnet build`, the EF update step, and `dotnet test` for `Tests.Integration`.
- Any CI/Docker references you found (or a note that none exist).
````
