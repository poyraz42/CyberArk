namespace VaultDashboard.Pvwa.Dto;

// GET /PasswordVault/API/Accounts/
internal sealed class AccountsListResponse
{
    public List<AccountDto>? Value { get; set; }
    public int? Count { get; set; }
    public string? NextLink { get; set; }
}

internal sealed class AccountDto
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? UserName { get; set; }
    public string? PlatformId { get; set; }
    public string? SafeName { get; set; }
    public string? SecretType { get; set; }
    public DateTimeOffset? CreatedTime { get; set; }
    public Dictionary<string, string>? PlatformAccountProperties { get; set; }
    public SecretManagementDto? SecretManagement { get; set; }
}

internal sealed class SecretManagementDto
{
    public bool AutomaticManagementEnabled { get; set; } = true;
    public string? ManualManagementReason { get; set; }
    public DateTimeOffset? LastModifiedTime { get; set; }
    public DateTimeOffset? LastVerifiedTime { get; set; }
    public DateTimeOffset? LastReconciledTime { get; set; }
    public string? Status { get; set; }
}

// GET WebServices/PIMServices.svc/Accounts/{id}/Activities/
internal sealed class AccountActivitiesResponse
{
    public List<AccountActivityDto>? GetAccountActivitiesResult { get; set; }
}

internal sealed class AccountActivityDto
{
    public string? Action { get; set; }
    public string? ClientId { get; set; }
    public string? Reason { get; set; }
    public string? User { get; set; }
    public DateTimeOffset? Time { get; set; }
}
