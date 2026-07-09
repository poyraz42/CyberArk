using VaultDashboard.Core.Models;
using VaultDashboard.Pvwa.Dto;

namespace VaultDashboard.Pvwa;

public sealed partial class PvwaRestClient
{
    /// <summary>
    /// GET /PasswordVault/API/Accounts/
    /// https://docs.cyberark.com/pam-self-hosted/latest/en/content/webservices/get%20accounts.htm
    /// Pages through the API automatically and returns every matching account.
    /// </summary>
    public async Task<IReadOnlyList<AccountInfo>> GetAccountsAsync(
        string? search = null,
        string? filter = null,
        int pageSize = 1000,
        CancellationToken ct = default)
    {
        var results = new List<AccountInfo>();
        var offset = 0;

        while (true)
        {
            var query = new QueryStringBuilder()
                .Add("search", search)
                .Add("filter", filter)
                .Add("offset", offset)
                .Add("limit", pageSize)
                .ToString();

            var page = await GetJsonAsync<AccountsListResponse>($"API/Accounts/{query}", ct).ConfigureAwait(false);
            var accounts = page.Value ?? new List<AccountDto>();
            results.AddRange(accounts.Select(a => a.ToModel()));

            if (accounts.Count < pageSize)
            {
                break;
            }

            offset += pageSize;
        }

        return results;
    }

    /// <summary>
    /// GET /PasswordVault/API/Accounts/{accountId}/
    /// https://docs.cyberark.com/pam-self-hosted/latest/en/content/webservices/get%20account%20details.htm
    /// </summary>
    public async Task<AccountInfo> GetAccountDetailsAsync(string accountId, CancellationToken ct = default)
    {
        var dto = await GetJsonAsync<AccountDto>($"API/Accounts/{Uri.EscapeDataString(accountId)}/", ct)
            .ConfigureAwait(false);
        return dto.ToModel();
    }

    /// <summary>
    /// GET /PasswordVault/WebServices/PIMServices.svc/Accounts/{accountId}/Activities/
    /// https://docs.cyberark.com/pam-self-hosted/latest/en/content/webservices/get%20account%20activity.htm
    /// </summary>
    public async Task<IReadOnlyList<(string Action, string? User, DateTimeOffset? Time, string? Reason)>>
        GetAccountActivitiesAsync(string accountId, CancellationToken ct = default)
    {
        var response = await GetJsonAsync<AccountActivitiesResponse>(
            $"WebServices/PIMServices.svc/Accounts/{Uri.EscapeDataString(accountId)}/Activities/", ct)
            .ConfigureAwait(false);

        return (response.GetAccountActivitiesResult ?? new List<AccountActivityDto>())
            .Select(a => (a.Action ?? string.Empty, a.User, a.Time, a.Reason))
            .ToList();
    }
}
