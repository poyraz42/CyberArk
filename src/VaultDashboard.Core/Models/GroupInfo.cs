namespace VaultDashboard.Core.Models;

/// <summary>A user group, as returned by GET /PasswordVault/API/UserGroups or PACLI/EVD group reports.</summary>
public sealed class GroupInfo
{
    public string? Id { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string? GroupType { get; set; } // Vault / Directory
    public string? Location { get; set; }
    public int MembersCount { get; set; }
    public string DataSource { get; set; } = "PVWA";
}
