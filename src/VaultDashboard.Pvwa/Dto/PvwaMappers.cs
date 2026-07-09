using VaultDashboard.Core.Models;

namespace VaultDashboard.Pvwa.Dto;

internal static class PvwaMappers
{
    public static SafeInfo ToModel(this SafeDto dto) => new()
    {
        SafeName = dto.SafeName ?? string.Empty,
        SafeNumber = dto.SafeNumber,
        Description = dto.Description,
        Location = dto.Location,
        ManagingCpm = dto.ManagingCPM,
        NumberOfVersionsRetention = dto.NumberOfVersionsRetention ?? 0,
        NumberOfDaysRetention = dto.NumberOfDaysRetention ?? 0,
        AutoPurgeEnabled = dto.AutoPurgeEnabled,
        OlacEnabled = dto.OLACEnabled,
        AccountsCount = dto.Accounts?.Count,
        CreationTime = dto.CreationTime,
        CreatorName = dto.Creator?.Name,
    };

    public static AccountInfo ToModel(this AccountDto dto) => new()
    {
        Id = dto.Id ?? string.Empty,
        Name = dto.Name ?? string.Empty,
        Address = dto.Address,
        UserName = dto.UserName,
        PlatformId = dto.PlatformId,
        SafeName = dto.SafeName ?? string.Empty,
        SecretType = dto.SecretType,
        CreatedTime = dto.CreatedTime,
        AutomaticManagementEnabled = dto.SecretManagement?.AutomaticManagementEnabled ?? true,
        ManualManagementReason = dto.SecretManagement?.ManualManagementReason,
        LastModifiedTime = dto.SecretManagement?.LastModifiedTime,
        LastVerifiedTime = dto.SecretManagement?.LastVerifiedTime,
        LastReconciledTime = dto.SecretManagement?.LastReconciledTime,
        Status = dto.SecretManagement?.Status,
        PlatformAccountProperties = dto.PlatformAccountProperties ?? new Dictionary<string, string>(),
    };

    public static PlatformInfo ToModel(this PlatformDto dto)
    {
        var general = dto.General;
        var cpmParams = dto.Properties is not null && dto.Properties.TryGetValue("CPM Parameters", out var cpm)
            ? cpm
            : null;

        int? interval = null;
        if (cpmParams is not null && cpmParams.TryGetValue("Interval", out var intervalStr) &&
            int.TryParse(intervalStr, out var parsedInterval))
        {
            interval = parsedInterval;
        }

        return new PlatformInfo
        {
            PlatformId = general?.Id ?? string.Empty,
            Name = general?.Name ?? general?.Id ?? string.Empty,
            SystemType = general?.SystemType,
            Active = general?.Active ?? false,
            PlatformType = general?.PlatformType,
            AccountsCount = general?.AccountsCount,
            IntervalMinutes = interval,
            PeriodicPasswordChangeEnabled = dto.CredentialsManagement?.PasswordChange?.PerformPeriodicChange ?? false,
            PasswordChangeIntervalDays = dto.CredentialsManagement?.PasswordChange?.RequirePasswordChangeEveryDays,
        };
    }

    public static UserInfo ToModel(this UserDto dto) => new()
    {
        Id = dto.Id?.ToString(),
        Username = dto.Username ?? string.Empty,
        Source = dto.Source,
        UserType = dto.UserType,
        Location = dto.Location,
        Enabled = dto.Enabled,
        Suspended = dto.Suspended,
        LastSuccessfulLogin = dto.LastSuccessfulLogin,
        AuthenticationMethods = dto.AuthenticationMethod ?? new List<string>(),
        ComponentUser = dto.ComponentUser,
    };

    public static GroupInfo ToModel(this GroupDto dto) => new()
    {
        Id = dto.Id?.ToString(),
        GroupName = dto.GroupName ?? string.Empty,
        GroupType = dto.GroupType,
        Location = dto.Location,
        MembersCount = dto.Members?.Count ?? 0,
    };

    public static SystemHealthComponent ToModel(this ComponentSummaryDto dto) => new()
    {
        ComponentType = dto.ComponentType ?? string.Empty,
        ConnectedCount = dto.ConnectedComponentCount,
        DisconnectedCount = dto.DisconnectedComponentCount,
    };

    public static SystemHealthComponentDetail ToModel(this ComponentDetailDto dto) => new()
    {
        ComponentUserName = dto.ComponentUserName ?? string.Empty,
        ComponentName = dto.ComponentName,
        IpAddress = dto.IpAddress ?? string.Empty,
        Version = dto.ComponentVersion,
        Connected = dto.Connected,
        LastLogonDate = dto.LastLogonDate,
    };

    public static LiveSessionInfo ToModel(this LiveSessionDto dto) => new()
    {
        SessionId = dto.Id ?? string.Empty,
        SafeName = dto.SafeName,
        PlatformId = dto.PlatformId,
        AccountUsername = dto.AccountUsername,
        RemoteMachine = dto.RemoteMachine,
        User = dto.User,
        Start = dto.Start,
        Protocol = dto.Protocol,
        RiskScore = dto.RiskScore,
    };

    public static PsmServerInfo ToModel(this PsmServerDto dto) => new()
    {
        PsmServerId = dto.PSMServerId ?? string.Empty,
        PsmServerName = dto.PSMServerName ?? string.Empty,
        IsDefaultPsmServer = dto.IsDefaultPSMServer,
    };

    public static LdapDirectoryInfo ToModel(this LdapDirectoryDto dto) => new()
    {
        DirectoryName = dto.DirectoryName ?? string.Empty,
        DomainName = dto.DomainName,
        DirectoryServers = dto.DirectoryServers ?? new List<string>(),
    };

    public static ApplicationInfo ToModel(this ApplicationDto dto) => new()
    {
        AppId = dto.AppID ?? string.Empty,
        Description = dto.Description,
        Location = dto.Location,
        Disabled = dto.Disabled,
        AuthenticationMethodsCount = dto.AuthenticationMethods?.Count ?? 0,
    };

    public static OnboardingRuleInfo ToModel(this OnboardingRuleDto dto) => new()
    {
        RuleName = dto.RuleName ?? string.Empty,
        PlatformId = dto.PlatformID,
        SafeName = dto.SafeName,
        RuleEnabled = dto.RuleEnabled,
    };

    public static ReportInfo ToModel(this ReportDto dto) => new()
    {
        Name = dto.Name ?? string.Empty,
        Type = dto.Type,
        CreationDate = dto.CreationDate,
    };

    public static VaultTaskInfo ToModel(this TaskDto dto) => new()
    {
        TaskId = dto.TaskId ?? string.Empty,
        Category = dto.Category,
        Description = dto.Description,
        CreationDate = dto.CreationDate,
    };

    public static SecurityEventInfo ToModel(this SecurityEventDto dto) => new()
    {
        EventId = dto.EventId ?? dto.Id ?? string.Empty,
        EventType = dto.Type,
        Severity = dto.Score,
        Status = dto.Status,
        DetectionTime = dto.DetectionTime,
        Description = dto.Description,
    };
}
