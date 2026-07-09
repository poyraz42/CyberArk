using VaultDashboard.Core.Models;
using VaultDashboard.Pvwa.Dto;

namespace VaultDashboard.Pvwa;

public sealed partial class PvwaRestClient
{
    /// <summary>
    /// GET /PasswordVault/API/Users
    /// https://docs.cyberark.com/pam-self-hosted/latest/en/content/webservices/get%20users.htm
    /// </summary>
    public async Task<IReadOnlyList<UserInfo>> GetUsersAsync(
        string? search = null,
        bool extendedDetails = true,
        int pageSize = 1000,
        CancellationToken ct = default)
    {
        var results = new List<UserInfo>();
        var offset = 0;

        while (true)
        {
            var query = new QueryStringBuilder()
                .Add("search", search)
                .Add("ExtendedDetails", extendedDetails)
                .Add("pageOffset", offset)
                .Add("pageSize", pageSize)
                .ToString();

            var page = await GetJsonAsync<UsersListResponse>($"API/Users{query}", ct).ConfigureAwait(false);
            var users = page.Users ?? new List<UserDto>();
            results.AddRange(users.Select(u => u.ToModel()));

            if (users.Count < pageSize)
            {
                break;
            }

            offset += pageSize;
        }

        return results;
    }

    /// <summary>
    /// GET /PasswordVault/API/Users/{userId}
    /// https://docs.cyberark.com/pam-self-hosted/latest/en/content/webservices/get%20user%20details.htm
    /// </summary>
    public async Task<UserInfo> GetUserDetailsAsync(string userId, CancellationToken ct = default)
    {
        var dto = await GetJsonAsync<UserDto>($"API/Users/{Uri.EscapeDataString(userId)}", ct).ConfigureAwait(false);
        return dto.ToModel();
    }

    /// <summary>
    /// GET /PasswordVault/API/UserGroups/
    /// https://docs.cyberark.com/pam-self-hosted/latest/en/content/webservices/get%20groups.htm
    /// </summary>
    public async Task<IReadOnlyList<GroupInfo>> GetGroupsAsync(
        string? search = null,
        bool includeMembers = false,
        CancellationToken ct = default)
    {
        var query = new QueryStringBuilder()
            .Add("search", search)
            .Add("includeMembers", includeMembers)
            .ToString();

        var response = await GetJsonAsync<GroupsListResponse>($"API/UserGroups/{query}", ct).ConfigureAwait(false);
        return (response.Value ?? new List<GroupDto>()).Select(g => g.ToModel()).ToList();
    }
}
