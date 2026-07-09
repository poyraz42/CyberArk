namespace VaultDashboard.Core.Models;

/// <summary>A Safe, as returned by GET /PasswordVault/API/Safes and enriched by PACLI SAFESLIST / EVD SafesList.</summary>
public sealed class SafeInfo
{
    public string SafeName { get; set; } = string.Empty;
    public string? SafeNumber { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string? ManagingCpm { get; set; }
    public int NumberOfVersionsRetention { get; set; }
    public int NumberOfDaysRetention { get; set; }
    public bool AutoPurgeEnabled { get; set; }
    public bool OlacEnabled { get; set; }
    public int? AccountsCount { get; set; }
    public DateTimeOffset? CreationTime { get; set; }
    public string? CreatorName { get; set; }
    public string DataSource { get; set; } = "PVWA";
}
