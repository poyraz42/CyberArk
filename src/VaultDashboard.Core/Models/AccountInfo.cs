namespace VaultDashboard.Core.Models;

/// <summary>An Account, as returned by GET /PasswordVault/API/Accounts.</summary>
public sealed class AccountInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? UserName { get; set; }
    public string? PlatformId { get; set; }
    public string SafeName { get; set; } = string.Empty;
    public string? SecretType { get; set; }
    public DateTimeOffset? CreatedTime { get; set; }

    // secretManagement block
    public bool AutomaticManagementEnabled { get; set; } = true;
    public string? ManualManagementReason { get; set; }
    public DateTimeOffset? LastModifiedTime { get; set; }
    public DateTimeOffset? LastVerifiedTime { get; set; }
    public DateTimeOffset? LastReconciledTime { get; set; }
    public string? Status { get; set; }

    public IReadOnlyDictionary<string, string> PlatformAccountProperties { get; set; } =
        new Dictionary<string, string>();

    public bool IsCompliant => !string.Equals(Status, "failure", StringComparison.OrdinalIgnoreCase);

    public string DataSource { get; set; } = "PVWA";
}
