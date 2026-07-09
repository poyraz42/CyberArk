namespace VaultDashboard.Pvwa.Dto;

// GET /PasswordVault/API/ComponentsMonitoringSummary/
internal sealed class ComponentsMonitoringSummaryResponse
{
    public int? ComponentsCount { get; set; }
    public List<ComponentSummaryDto>? Components { get; set; }
}

internal sealed class ComponentSummaryDto
{
    public string? ComponentType { get; set; }
    public int ConnectedComponentCount { get; set; }
    public int DisconnectedComponentCount { get; set; }
}

// GET /PasswordVault/API/ComponentsMonitoringDetails/{componentID}
internal sealed class ComponentsMonitoringDetailsResponse
{
    public string? ComponentType { get; set; }
    public List<ComponentDetailDto>? ComponentsDetails { get; set; }
}

internal sealed class ComponentDetailDto
{
    public string? ComponentUserName { get; set; }
    public string? ComponentName { get; set; }
    public string? IpAddress { get; set; }
    public string? ComponentVersion { get; set; }
    public bool Connected { get; set; }
    public DateTimeOffset? LastLogonDate { get; set; }
}

// GET /PasswordVault/API/LiveSessions
internal sealed class LiveSessionsResponse
{
    public List<LiveSessionDto>? Value { get; set; }
    public int? Count { get; set; }
}

internal sealed class LiveSessionDto
{
    public string? Id { get; set; }
    public string? SafeName { get; set; }
    public string? PlatformId { get; set; }
    public string? AccountUsername { get; set; }
    public string? RemoteMachine { get; set; }
    public string? User { get; set; }
    public DateTimeOffset? Start { get; set; }
    public string? Protocol { get; set; }
    public int RiskScore { get; set; }
}

// GET /PasswordVault/API/PSM/Servers/
internal sealed class PsmServersResponse
{
    public List<PsmServerDto>? PSMServers { get; set; }
}

internal sealed class PsmServerDto
{
    public string? PSMServerId { get; set; }
    public string? PSMServerName { get; set; }
    public bool IsDefaultPSMServer { get; set; }
}
