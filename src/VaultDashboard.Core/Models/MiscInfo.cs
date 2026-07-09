namespace VaultDashboard.Core.Models;

/// <summary>GET /PasswordVault/API/PSM/Servers/</summary>
public sealed class PsmServerInfo
{
    public string PsmServerId { get; set; } = string.Empty;
    public string PsmServerName { get; set; } = string.Empty;
    public bool IsDefaultPsmServer { get; set; }
}

/// <summary>GET /PasswordVault/API/Configuration/LDAP/Directories/</summary>
public sealed class LdapDirectoryInfo
{
    public string DirectoryName { get; set; } = string.Empty;
    public string? DomainName { get; set; }
    public IReadOnlyList<string> DirectoryServers { get; set; } = Array.Empty<string>();
}

/// <summary>WebServices/PIMServices.svc/Applications/</summary>
public sealed class ApplicationInfo
{
    public string AppId { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public bool Disabled { get; set; }
    public int AuthenticationMethodsCount { get; set; }
}

/// <summary>GET /PasswordVault/API/AutomaticOnboardingRules/</summary>
public sealed class OnboardingRuleInfo
{
    public string RuleName { get; set; } = string.Empty;
    public string? PlatformId { get; set; }
    public string? SafeName { get; set; }
    public bool RuleEnabled { get; set; }
}

/// <summary>GET /PasswordVault/API/Reports</summary>
public sealed class ReportInfo
{
    public string Name { get; set; } = string.Empty;
    public string? Type { get; set; }
    public DateTimeOffset? CreationDate { get; set; }
}

/// <summary>GET /PasswordVault/API/Tasks</summary>
public sealed class VaultTaskInfo
{
    public string TaskId { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset? CreationDate { get; set; }
}

/// <summary>One line from the PTA "Get Risk Events" / "Get Security Events" endpoints.</summary>
public sealed class SecurityEventInfo
{
    public string EventId { get; set; } = string.Empty;
    public string? EventType { get; set; }
    public string? Severity { get; set; }
    public string? Status { get; set; } // OPEN / CLOSED
    public DateTimeOffset? DetectionTime { get; set; }
    public string? Description { get; set; }
}

/// <summary>A single row of a PVWA "License Capacity" report (used vs. max seats per licensed object).</summary>
public sealed class LicenseUsageItem
{
    public string LicensedObject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Used { get; set; }
    public int Maximum { get; set; }
    public double UtilizationPercent => Maximum <= 0 ? 0 : Math.Round(Used * 100.0 / Maximum, 1);
}
