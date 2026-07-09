using System.Text.Json.Serialization;

namespace VaultDashboard.Pvwa.Dto;

// GET /PasswordVault/API/Safes/
internal sealed class SafesListResponse
{
    public List<SafeDto>? Value { get; set; }
    public int? Count { get; set; }
    public string? NextLink { get; set; }
}

internal sealed class SafeDto
{
    public string? SafeName { get; set; }
    public string? SafeNumber { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string? ManagingCPM { get; set; }
    public int? NumberOfVersionsRetention { get; set; }
    public int? NumberOfDaysRetention { get; set; }
    public bool AutoPurgeEnabled { get; set; }
    public bool OLACEnabled { get; set; }
    public DateTimeOffset? CreationTime { get; set; }
    public SafeCreatorDto? Creator { get; set; }

    [JsonPropertyName("accounts")]
    public List<object>? Accounts { get; set; }
}

internal sealed class SafeCreatorDto
{
    public string? Id { get; set; }
    public string? Name { get; set; }
}
