namespace VaultDashboard.Core.Models;

/// <summary>An active/live PSM session, as returned by GET /PasswordVault/API/LiveSessions.</summary>
public sealed class LiveSessionInfo
{
    public string SessionId { get; set; } = string.Empty;
    public string? SafeName { get; set; }
    public string? PlatformId { get; set; }
    public string? AccountUsername { get; set; }
    public string? RemoteMachine { get; set; }
    public string? User { get; set; }
    public DateTimeOffset? Start { get; set; }
    public string? Protocol { get; set; }
    public int RiskScore { get; set; }
}
