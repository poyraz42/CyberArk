namespace VaultDashboard.Pvwa.Dto;

// GET /PasswordVault/API/Users
internal sealed class UsersListResponse
{
    public List<UserDto>? Users { get; set; }
    public int? Total { get; set; }
}

internal sealed class UserDto
{
    public int? Id { get; set; }
    public string? Username { get; set; }
    public string? Source { get; set; }
    public string? UserType { get; set; }
    public string? Location { get; set; }
    public bool Suspended { get; set; }
    public bool Enabled { get; set; } = true;
    public bool ComponentUser { get; set; }
    public DateTimeOffset? LastSuccessfulLogin { get; set; }
    public List<string>? AuthenticationMethod { get; set; }
}

// GET /PasswordVault/API/UserGroups/
internal sealed class GroupsListResponse
{
    public List<GroupDto>? Value { get; set; }
    public int? Count { get; set; }
}

internal sealed class GroupDto
{
    public int? Id { get; set; }
    public string? GroupName { get; set; }
    public string? GroupType { get; set; }
    public string? Location { get; set; }
    public List<object>? Members { get; set; }
}
