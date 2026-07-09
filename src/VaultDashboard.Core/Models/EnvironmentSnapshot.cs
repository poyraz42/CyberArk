namespace VaultDashboard.Core.Models;

/// <summary>
/// The full, merged picture of a Vault environment used to feed the dashboard, assembled from
/// whichever of PVWA / PACLI / EVD were configured and reachable. Every collection is independently
/// optional so the UI degrades gracefully instead of failing the whole refresh.
/// </summary>
public sealed class EnvironmentSnapshot
{
    public DateTimeOffset GeneratedAtUtc { get; init; } = DateTimeOffset.UtcNow;

    public IReadOnlyList<SafeInfo> Safes { get; set; } = Array.Empty<SafeInfo>();
    public IReadOnlyList<AccountInfo> Accounts { get; set; } = Array.Empty<AccountInfo>();
    public IReadOnlyList<PlatformInfo> Platforms { get; set; } = Array.Empty<PlatformInfo>();
    public IReadOnlyList<UserInfo> Users { get; set; } = Array.Empty<UserInfo>();
    public IReadOnlyList<GroupInfo> Groups { get; set; } = Array.Empty<GroupInfo>();
    public IReadOnlyList<SystemHealthComponent> SystemHealth { get; set; } = Array.Empty<SystemHealthComponent>();
    public IReadOnlyList<LiveSessionInfo> LiveSessions { get; set; } = Array.Empty<LiveSessionInfo>();
    public IReadOnlyList<PsmServerInfo> PsmServers { get; set; } = Array.Empty<PsmServerInfo>();
    public IReadOnlyList<LdapDirectoryInfo> LdapDirectories { get; set; } = Array.Empty<LdapDirectoryInfo>();
    public IReadOnlyList<ApplicationInfo> Applications { get; set; } = Array.Empty<ApplicationInfo>();
    public IReadOnlyList<OnboardingRuleInfo> OnboardingRules { get; set; } = Array.Empty<OnboardingRuleInfo>();
    public IReadOnlyList<ReportInfo> Reports { get; set; } = Array.Empty<ReportInfo>();
    public IReadOnlyList<VaultTaskInfo> Tasks { get; set; } = Array.Empty<VaultTaskInfo>();
    public IReadOnlyList<SecurityEventInfo> SecurityEvents { get; set; } = Array.Empty<SecurityEventInfo>();
    public IReadOnlyList<LicenseUsageItem> LicenseUsage { get; set; } = Array.Empty<LicenseUsageItem>();

    public IReadOnlyList<DataSourceStatus> FetchLog { get; set; } = Array.Empty<DataSourceStatus>();

    public int ManagedAccountsCount => Accounts.Count(a => a.AutomaticManagementEnabled);
    public int NonCompliantAccountsCount => Accounts.Count(a => !a.IsCompliant);
    public int ActivePlatformsCount => Platforms.Count(p => p.Active);
    public int DisabledUsersCount => Users.Count(u => !u.Enabled);
}
