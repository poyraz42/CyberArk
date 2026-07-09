namespace VaultDashboard.Pvwa.Dto;

// GET /PasswordVault/API/Configuration/LDAP/Directories/
internal sealed class LdapDirectoriesResponse
{
    public List<LdapDirectoryDto>? Value { get; set; }
}

internal sealed class LdapDirectoryDto
{
    public string? DirectoryName { get; set; }
    public string? DomainName { get; set; }
    public List<string>? DirectoryServers { get; set; }
}

// GET WebServices/PIMServices.svc/Applications/
internal sealed class ApplicationsResponse
{
    public List<ApplicationDto>? Application { get; set; }
}

internal sealed class ApplicationDto
{
    public string? AppID { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public bool Disabled { get; set; }
    public List<object>? AuthenticationMethods { get; set; }
}

// GET /PasswordVault/API/AutomaticOnboardingRules/
internal sealed class OnboardingRulesResponse
{
    public List<OnboardingRuleDto>? Value { get; set; }
}

internal sealed class OnboardingRuleDto
{
    public string? RuleName { get; set; }
    public string? PlatformID { get; set; }
    public string? SafeName { get; set; }
    public bool RuleEnabled { get; set; }
}

// GET /PasswordVault/API/Reports
internal sealed class ReportsResponse
{
    public List<ReportDto>? Value { get; set; }
}

internal sealed class ReportDto
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public DateTimeOffset? CreationDate { get; set; }
}

// GET /PasswordVault/API/Tasks
internal sealed class TasksResponse
{
    public List<TaskDto>? Value { get; set; }
}

internal sealed class TaskDto
{
    public string? TaskId { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset? CreationDate { get; set; }
}

// GET /PasswordVault/API/pta/API/Risks/RisksEvents/ and .../pta/API/Events/
internal sealed class SecurityEventsResponse
{
    public List<SecurityEventDto>? Events { get; set; }
    public List<SecurityEventDto>? Value { get; set; }
}

internal sealed class SecurityEventDto
{
    public string? EventId { get; set; }
    public string? Id { get; set; }
    public string? Type { get; set; }
    public string? Score { get; set; }
    public string? Status { get; set; }
    public DateTimeOffset? DetectionTime { get; set; }
    public string? Description { get; set; }
}

// GET WebServices/PIMServices.svc/Server/
internal sealed class ServerInfoDto
{
    public string? ExternalVersion { get; set; }
    public string? InternalVersion { get; set; }
    public string? ServerName { get; set; }
    public bool? ServerId { get; set; }
    public string? SecurityHardeningLevel { get; set; }
}
