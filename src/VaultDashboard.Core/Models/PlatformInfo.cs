namespace VaultDashboard.Core.Models;

/// <summary>A target/rotational platform, as returned by GET /PasswordVault/API/Platforms.</summary>
public sealed class PlatformInfo
{
    public string PlatformId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? SystemType { get; set; }
    public bool Active { get; set; }
    public string? PlatformType { get; set; } // Regular / Group
    public int? AccountsCount { get; set; }

    /// <summary>Minutes between CPM task checks (Interval in the platform's CPM policy).</summary>
    public int? IntervalMinutes { get; set; }

    public bool PeriodicPasswordChangeEnabled { get; set; }
    public int? PasswordChangeIntervalDays { get; set; }
}
