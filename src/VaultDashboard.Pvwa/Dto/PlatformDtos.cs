namespace VaultDashboard.Pvwa.Dto;

// GET /PasswordVault/API/Platforms/
internal sealed class PlatformsListResponse
{
    public List<PlatformDto>? Platforms { get; set; }
}

// GET /PasswordVault/API/Platforms/{platformName}/  (single-object shape)
internal sealed class PlatformDto
{
    public PlatformGeneralDto? General { get; set; }
    public Dictionary<string, Dictionary<string, string>>? Properties { get; set; }
    public PlatformCredentialsManagementDto? CredentialsManagement { get; set; }
}

internal sealed class PlatformGeneralDto
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? SystemType { get; set; }
    public bool Active { get; set; }
    public string? PlatformType { get; set; }
    public string? PlatformBaseID { get; set; }
    public int? AccountsCount { get; set; }
}

internal sealed class PlatformCredentialsManagementDto
{
    public PlatformPasswordChangeDto? PasswordChange { get; set; }
}

internal sealed class PlatformPasswordChangeDto
{
    public bool PerformPeriodicChange { get; set; }
    public int? RequirePasswordChangeEveryDays { get; set; }
}
