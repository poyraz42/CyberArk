using CommunityToolkit.Mvvm.ComponentModel;
using VaultDashboard.App.Models;
using VaultDashboard.Core.Connection;
using VaultDashboard.Core.Exceptions;
using VaultDashboard.Core.Models;
using VaultDashboard.Evd;
using VaultDashboard.Pacli;
using VaultDashboard.Pvwa;

namespace VaultDashboard.App.Services;

/// <summary>
/// App-wide, single-instance state: current connection profiles and the most recently fetched
/// <see cref="EnvironmentSnapshot"/>. Every dashboard page reads from here rather than owning its
/// own copy of the data, so one refresh updates every page at once.
/// </summary>
public sealed partial class DashboardStateService : ObservableObject
{
    private readonly ProfileStore _profileStore;

    [ObservableProperty] private EnvironmentSnapshot? _snapshot;
    [ObservableProperty] private bool _isRefreshing;
    [ObservableProperty] private string? _statusMessage;
    [ObservableProperty] private DateTimeOffset? _lastRefreshedUtc;
    [ObservableProperty] private string? _lastError;

    public PvwaConnectionProfile PvwaProfile { get; set; } = new();
    public PacliConnectionProfile PacliProfile { get; set; } = new();
    public EvdConnectionProfile EvdProfile { get; set; } = new();
    public bool IncludePacli { get; set; }
    public bool IncludeEvd { get; set; }

    public DashboardStateService(ProfileStore profileStore)
    {
        _profileStore = profileStore;
    }

    public async Task LoadProfilesAsync(CancellationToken ct = default)
    {
        var stored = await _profileStore.LoadAsync(ct).ConfigureAwait(false);
        PvwaProfile = stored.Pvwa ?? new PvwaConnectionProfile();
        PacliProfile = stored.Pacli ?? new PacliConnectionProfile();
        EvdProfile = stored.Evd ?? new EvdConnectionProfile();
    }

    public Task SaveProfilesAsync(CancellationToken ct = default) =>
        _profileStore.SaveAsync(new StoredProfiles { Pvwa = PvwaProfile, Pacli = PacliProfile, Evd = EvdProfile }, ct);

    /// <summary>
    /// Logs on to PVWA, pulls every endpoint the dashboard understands, then optionally enriches the
    /// result with PACLI and/or EVD data. Never throws for a single failed PVWA endpoint (see
    /// <see cref="PvwaSnapshotService"/>) - it only throws if PVWA logon itself fails, since without a
    /// session there is nothing to show.
    /// </summary>
    public async Task RefreshAsync(CancellationToken ct = default)
    {
        IsRefreshing = true;
        LastError = null;
        StatusMessage = "Connecting to PVWA...";

        try
        {
            await using var client = new PvwaRestClient(PvwaProfile);
            await client.LogonAsync(ct).ConfigureAwait(false);

            var progress = new Progress<string>(msg => StatusMessage = msg);
            var snapshot = await new PvwaSnapshotService(client).FetchSnapshotAsync(progress, ct).ConfigureAwait(false);

            if (IncludePacli)
            {
                snapshot = await MergePacliAsync(snapshot, ct).ConfigureAwait(false);
            }

            if (IncludeEvd)
            {
                snapshot = await MergeEvdAsync(snapshot, ct).ConfigureAwait(false);
            }

            Snapshot = snapshot;
            LastRefreshedUtc = DateTimeOffset.UtcNow;
            var failed = snapshot.FetchLog.Count(l => !l.Success);
            StatusMessage = failed == 0
                ? $"Refreshed successfully ({snapshot.FetchLog.Count} endpoints)."
                : $"Refreshed with {failed} of {snapshot.FetchLog.Count} endpoints unavailable.";
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            StatusMessage = "Refresh failed.";
            throw;
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private async Task<EnvironmentSnapshot> MergePacliAsync(EnvironmentSnapshot snapshot, CancellationToken ct)
    {
        var log = snapshot.FetchLog.ToList();
        try
        {
            StatusMessage = "Fetching supplemental data via PACLI...";
            await using var pacli = new PacliClient(PacliProfile);
            await pacli.LogonAsync(ct).ConfigureAwait(false);

            var users = await pacli.ListUsersAsync(ct).ConfigureAwait(false);
            var safes = await pacli.ListSafesAsync(ct).ConfigureAwait(false);

            log.Add(DataSourceStatus.Ok(VaultDataSourceKind.Pacli, "USERSLIST", users.Count, TimeSpan.Zero));
            log.Add(DataSourceStatus.Ok(VaultDataSourceKind.Pacli, "SAFESLIST", safes.Count, TimeSpan.Zero));

            return CloneWith(snapshot, log,
                users: SnapshotMerger.MergeUsers(snapshot.Users, users),
                safes: SnapshotMerger.MergeSafes(snapshot.Safes, safes));
        }
        catch (VaultDataException ex)
        {
            log.Add(DataSourceStatus.Fail(VaultDataSourceKind.Pacli, "USERSLIST/SAFESLIST", ex.Message, TimeSpan.Zero));
            return CloneWith(snapshot, log);
        }
    }

    private async Task<EnvironmentSnapshot> MergeEvdAsync(EnvironmentSnapshot snapshot, CancellationToken ct)
    {
        var log = snapshot.FetchLog.ToList();
        try
        {
            StatusMessage = "Fetching supplemental data via ExportVaultData...";
            var evd = await new EvdClient(EvdProfile).ExportSnapshotAsync(ct).ConfigureAwait(false);

            log.Add(DataSourceStatus.Ok(VaultDataSourceKind.Evd, "SafesList/UsersList/GroupsList", evd.Safes.Count + evd.Users.Count + evd.Groups.Count, TimeSpan.Zero));

            return CloneWith(snapshot, log,
                users: SnapshotMerger.MergeUsers(snapshot.Users, evd.Users),
                safes: SnapshotMerger.MergeSafes(snapshot.Safes, evd.Safes),
                groups: SnapshotMerger.MergeGroups(snapshot.Groups, evd.Groups));
        }
        catch (VaultDataException ex)
        {
            log.Add(DataSourceStatus.Fail(VaultDataSourceKind.Evd, "ExportVaultData", ex.Message, TimeSpan.Zero));
            return CloneWith(snapshot, log);
        }
    }

    private static EnvironmentSnapshot CloneWith(
        EnvironmentSnapshot source,
        IReadOnlyList<DataSourceStatus> log,
        IReadOnlyList<SafeInfo>? safes = null,
        IReadOnlyList<AccountInfo>? accounts = null,
        IReadOnlyList<UserInfo>? users = null,
        IReadOnlyList<GroupInfo>? groups = null) => new()
    {
        Safes = safes ?? source.Safes,
        Accounts = accounts ?? source.Accounts,
        Platforms = source.Platforms,
        Users = users ?? source.Users,
        Groups = groups ?? source.Groups,
        SystemHealth = source.SystemHealth,
        LiveSessions = source.LiveSessions,
        PsmServers = source.PsmServers,
        LdapDirectories = source.LdapDirectories,
        Applications = source.Applications,
        OnboardingRules = source.OnboardingRules,
        Reports = source.Reports,
        Tasks = source.Tasks,
        SecurityEvents = source.SecurityEvents,
        LicenseUsage = source.LicenseUsage,
        FetchLog = log,
    };
}
