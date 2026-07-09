namespace VaultDashboard.Core.Models;

/// <summary>One row from GET /PasswordVault/API/ComponentsMonitoringSummary.</summary>
public sealed class SystemHealthComponent
{
    public string ComponentType { get; set; } = string.Empty; // CPM, PVWA, PSM, PSMP, AIM, ...
    public int ConnectedCount { get; set; }
    public int DisconnectedCount { get; set; }
    public int TotalCount => ConnectedCount + DisconnectedCount;

    public IReadOnlyList<SystemHealthComponentDetail> Details { get; set; } =
        Array.Empty<SystemHealthComponentDetail>();
}

/// <summary>One row from GET /PasswordVault/API/ComponentsMonitoringDetails/{componentID}.</summary>
public sealed class SystemHealthComponentDetail
{
    public string ComponentUserName { get; set; } = string.Empty;
    public string? ComponentName { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string? Version { get; set; }
    public bool Connected { get; set; }
    public DateTimeOffset? LastLogonDate { get; set; }
}
