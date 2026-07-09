namespace VaultDashboard.Core.Models;

/// <summary>A Vault/directory user, as returned by GET /PasswordVault/API/Users or PACLI USERSLIST.</summary>
public sealed class UserInfo
{
    public string? Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Source { get; set; } // CyberArk / LDAP / RADIUS ...
    public string? UserType { get; set; }
    public string? Location { get; set; }
    public bool Enabled { get; set; } = true;
    public bool Suspended { get; set; }
    public DateTimeOffset? LastSuccessfulLogin { get; set; }
    public IReadOnlyList<string> AuthenticationMethods { get; set; } = Array.Empty<string>();
    public bool ComponentUser { get; set; }
    public string DataSource { get; set; } = "PVWA";
}
