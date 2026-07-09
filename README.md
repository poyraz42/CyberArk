# CyberArk Vault Dashboard

A Windows desktop application (WPF, .NET 8, Fluent/modern UI) that pulls data from a CyberArk
Self-Hosted PAM environment through three channels — **PVWA REST API**, **PACLI**, and
**ExportVaultData (EVD)** — and turns it into a live, charted dashboard: safes, accounts,
platforms, sessions/PSM, users & groups, system health, and license usage. It's built to answer
the same questions a manual Vault assessment report does, but pulled live from the environment
instead of compiled by hand.

## Architecture

```
src/
  VaultDashboard.Core    Domain models, connection profiles, exceptions (net8.0, no dependencies)
  VaultDashboard.Pvwa    PVWA REST API client + fault-tolerant snapshot orchestrator (net8.0)
  VaultDashboard.Pacli   Pacli.exe process wrapper (INIT/DEFINE/LOGON/LOGOFF/TERM + list commands) (net8.0)
  VaultDashboard.Evd     ExportVaultData.exe wrapper + report parser (net8.0)
  VaultDashboard.App     WPF dashboard (net8.0-windows) — the only Windows-only project
tests/
  *.Tests                xUnit tests for Core/Pvwa/Pacli/Evd (all run on any OS)
```

The data-access layer (`Core`, `Pvwa`, `Pacli`, `Evd`) is plain `net8.0` with zero Windows
dependency and is fully unit tested — `dotnet test` runs it on Linux/macOS/Windows alike. Only
`VaultDashboard.App` targets `net8.0-windows` (WPF) and therefore can only be **built and run on
Windows** with the .NET 8 SDK's Windows Desktop workload. This sandbox environment doesn't have
Windows, so while every line of the app was written and cross-checked against the real compiled
NuGet packages (see "How this was verified" below), the WPF project itself hasn't been through
`dotnet build`/`dotnet run` — please build it on a Windows machine and report anything the
compiler flags.

## Data sources

### PVWA REST API (primary)
`VaultDashboard.Pvwa.PvwaRestClient` authenticates via `POST /PasswordVault/API/auth/{type}/Logon`
(CyberArk / LDAP / RADIUS / Windows / SAML) and pulls: Safes, Accounts (+ activity), Platforms,
Users, User Groups, System Health (`ComponentsMonitoringSummary`/`Details`), Live PSM Sessions,
PSM Servers, LDAP Directories, Applications, Onboarding Rules, Reports/Tasks, classic report
downloads (e.g. the License Capacity report), and PTA risk/security events. Every endpoint is
called independently by `PvwaSnapshotService` — one unlicensed/forbidden/down endpoint never
aborts the rest of the refresh; the Overview page's "endpoint status" table shows exactly what
succeeded and what didn't. Endpoint shapes and the exact REST conventions follow
docs.cyberark.com (the on-prem "Self-Hosted PAM" API), cross-referenced against the
`CyberArk-REST-API-Bruno-main.zip` collection already in this repo.

### PACLI (supplemental)
`VaultDashboard.Pacli.PacliClient` drives `Pacli.exe` through `INIT` → `DEFINE` → `LOGON` →
`USERSLIST` / `SAFESLIST` / `OWNERSLIST` / `FINDFILES` → `LOGOFF` → `TERM`, using the documented
PACLI Command Reference syntax. Enable it in **Connections** to enrich the dashboard with
safes/users PVWA didn't report (e.g. a REST user with narrower visibility, or a fully offline
audit workstation).

### ExportVaultData / EVD (supplemental)
`VaultDashboard.Evd.EvdClient` runs `ExportVaultData.exe \VaultFile=... \CredFile=... \Target=File
\SafesList=... \UsersList=... \GroupsList=... \GroupMembersList=...` in one pass and parses the
resulting header+rows report files (header-driven parsing, so it tolerates the column-set
differences between PAM versions).

All three sources feed one merged `EnvironmentSnapshot` (PVWA wins on conflicts; PACLI/EVD entries
are only added for names PVWA didn't already report).

## Building

### Data layer (works on any OS)
```bash
dotnet build CyberArkVaultDashboard.sln   # builds Core/Pvwa/Pacli/Evd + all tests (App is skipped without Windows)
dotnet test tests/VaultDashboard.Core.Tests
dotnet test tests/VaultDashboard.Pvwa.Tests
dotnet test tests/VaultDashboard.Evd.Tests
dotnet test tests/VaultDashboard.Pacli.Tests
```

### Dashboard app (requires Windows + .NET 8 SDK)
```powershell
dotnet build src\VaultDashboard.App\VaultDashboard.App.csproj
dotnet run --project src\VaultDashboard.App\VaultDashboard.App.csproj
```

### Publishing the .exe
```powershell
dotnet publish src\VaultDashboard.App\VaultDashboard.App.csproj -c Release -r win-x64 `
  --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:EnableCompressionInSingleFile=true
```
This produces a single `CyberArkVaultDashboard.exe` under
`src\VaultDashboard.App\bin\Release\net8.0-windows\win-x64\publish\` that runs on a machine
without the .NET runtime installed. Drop `--self-contained true` (and the `PublishReadyToRun`
flag) for a smaller, framework-dependent exe if the target machine already has the .NET 8 Desktop
Runtime.

## Configuring connections

Open the **Connections** page in the app:
- **PVWA** — host name (no scheme/path), authentication type, username/password, optional
  "allow insecure TLS" for lab environments. Use **Test connection** to confirm before saving.
- **PACLI** — path to `Pacli.exe`, vault name/address/port, credentials. Requires PACLI to be
  installed on the machine running the dashboard.
- **EVD** — path to `ExportVaultData.exe`, the `Vault.ini` used by EVD, the auditor `.cred` file
  (see `CreateCredFile`), and an output folder for the generated report files.

Profiles are saved to `%AppData%\CyberArkVaultDashboard\profiles.json`; passwords are encrypted at
rest with Windows DPAPI (current-user scope) before being written to disk.

## License Usage page

The License Capacity report is a "classic" PVWA report — it has to already exist under the
`PVWAReports` safe (PVWA generates it periodically) before it can be downloaded; it isn't a live
endpoint. Refresh the dashboard first so the Reports list is populated, then use **Download
report** on the License Usage page.

## How this was verified

Since this sandbox can't run Windows/WPF, the WPF-UI and LiveCharts2 NuGet packages pinned in
`VaultDashboard.App.csproj` were downloaded directly from nuget.org and inspected with
`System.Reflection.MetadataLoadContext` against the real `Microsoft.WindowsDesktop.App.Ref`
reference assemblies — every WPF-UI/LiveCharts2 type, property, constructor and enum member used
in the XAML/C# (`FluentWindow`, `TitleBar`, `PasswordBox.Password`, `Button.Appearance` +
`ControlAppearance.Primary`, `ApplicationThemeManager.Apply(...)`, `WindowBackdropType.Mica`,
`CartesianChart`/`PieChart`, `ColumnSeries<T>`, `PieSeries<T>`, `Axis`, `SolidColorPaint`, `ISeries`)
was confirmed to exist with the exact signature used, catching several `using`-directive bugs
before they could ship. The data-access layer (`Core`/`Pvwa`/`Pacli`/`Evd`) is fully compiled and
unit-tested in this environment (30 passing tests). What has **not** been verified here is a full
XAML markup compile (`dotnet build` needs the Windows Desktop workload) or a live run against a
real PVWA/PACLI/EVD — please do both on Windows before relying on this for a real assessment.

## Reference material

- `CyberArk-REST-API-Bruno-main.zip` — the Bruno collection already in this repo; its
  `CyberArk Self-Hosted REST API/Self-Hosted PAM` folder is the primary source for every PVWA
  endpoint URL/method/params used by `VaultDashboard.Pvwa`.
- PACLI command syntax (`LOGON`, `USERSLIST`, `SAFESLIST`, `OWNERSLIST`, `DEFINE`) and EVD syntax
  (`\VaultFile=`, `\CredFile=`, `\Target=File`, `\<OutputName>=<file>`) follow
  docs.cyberark.com's PACLI Command Reference and Export Vault Data (EVD) utility documentation.
