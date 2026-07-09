using System.Diagnostics;
using VaultDashboard.Core.Models;

namespace VaultDashboard.Pvwa;

/// <summary>
/// Orchestrates the full "pull everything the dashboard can show" pass over the PVWA REST API.
/// Every call is isolated: one endpoint being unlicensed/forbidden/down never aborts the rest of the
/// refresh, it just shows up as a failed row in <see cref="EnvironmentSnapshot.FetchLog"/>.
/// </summary>
public sealed class PvwaSnapshotService
{
    private readonly PvwaRestClient _client;

    public PvwaSnapshotService(PvwaRestClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task<EnvironmentSnapshot> FetchSnapshotAsync(
        IProgress<string>? progress = null,
        CancellationToken ct = default)
    {
        var log = new List<DataSourceStatus>();

        var safes = await RunAsync("Safes", () => _client.GetSafesAsync(ct: ct), log, progress, ct).ConfigureAwait(false);
        var accounts = await RunAsync("Accounts", () => _client.GetAccountsAsync(ct: ct), log, progress, ct).ConfigureAwait(false);
        var platforms = await RunAsync("Platforms", () => _client.GetPlatformsAsync(ct: ct), log, progress, ct).ConfigureAwait(false);
        var users = await RunAsync("Users", () => _client.GetUsersAsync(ct: ct), log, progress, ct).ConfigureAwait(false);
        var groups = await RunAsync("UserGroups", () => _client.GetGroupsAsync(ct: ct), log, progress, ct).ConfigureAwait(false);
        var health = await RunAsync("ComponentsMonitoringSummary", () => _client.GetSystemHealthWithDetailsAsync(ct), log, progress, ct).ConfigureAwait(false);
        var sessions = await RunAsync("LiveSessions", () => _client.GetActiveSessionsAsync(ct: ct), log, progress, ct).ConfigureAwait(false);
        var psmServers = await RunAsync("PSM/Servers", () => _client.GetPsmServersAsync(ct), log, progress, ct).ConfigureAwait(false);
        var ldap = await RunAsync("Configuration/LDAP/Directories", () => _client.GetLdapDirectoriesAsync(ct), log, progress, ct).ConfigureAwait(false);
        var apps = await RunAsync("Applications", () => _client.GetApplicationsAsync(ct), log, progress, ct).ConfigureAwait(false);
        var onboardingRules = await RunAsync("AutomaticOnboardingRules", () => _client.GetOnboardingRulesAsync(ct), log, progress, ct).ConfigureAwait(false);
        var reports = await RunAsync("Reports", () => _client.GetReportsAsync(ct), log, progress, ct).ConfigureAwait(false);
        var tasks = await RunAsync("Tasks", () => _client.GetTasksAsync(ct), log, progress, ct).ConfigureAwait(false);
        var securityEvents = await RunAsync("pta/API/Events", () => _client.GetSecurityEventsAsync(ct), log, progress, ct).ConfigureAwait(false);

        return new EnvironmentSnapshot
        {
            Safes = safes ?? Array.Empty<SafeInfo>(),
            Accounts = accounts ?? Array.Empty<AccountInfo>(),
            Platforms = platforms ?? Array.Empty<PlatformInfo>(),
            Users = users ?? Array.Empty<UserInfo>(),
            Groups = groups ?? Array.Empty<GroupInfo>(),
            SystemHealth = health ?? Array.Empty<SystemHealthComponent>(),
            LiveSessions = sessions ?? Array.Empty<LiveSessionInfo>(),
            PsmServers = psmServers ?? Array.Empty<PsmServerInfo>(),
            LdapDirectories = ldap ?? Array.Empty<LdapDirectoryInfo>(),
            Applications = apps ?? Array.Empty<ApplicationInfo>(),
            OnboardingRules = onboardingRules ?? Array.Empty<OnboardingRuleInfo>(),
            Reports = reports ?? Array.Empty<ReportInfo>(),
            Tasks = tasks ?? Array.Empty<VaultTaskInfo>(),
            SecurityEvents = securityEvents ?? Array.Empty<SecurityEventInfo>(),
            FetchLog = log,
        };
    }

    private static async Task<IReadOnlyList<T>?> RunAsync<T>(
        string endpointName,
        Func<Task<IReadOnlyList<T>>> call,
        List<DataSourceStatus> log,
        IProgress<string>? progress,
        CancellationToken ct)
    {
        progress?.Report($"Fetching {endpointName}...");
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await call().ConfigureAwait(false);
            stopwatch.Stop();
            log.Add(DataSourceStatus.Ok(VaultDataSourceKind.Pvwa, endpointName, result.Count, stopwatch.Elapsed));
            return result;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            log.Add(DataSourceStatus.Fail(VaultDataSourceKind.Pvwa, endpointName, ex.Message, stopwatch.Elapsed));
            return null;
        }
    }
}
