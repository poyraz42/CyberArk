using VaultDashboard.Core.Models;
using VaultDashboard.Pvwa.Dto;

namespace VaultDashboard.Pvwa;

public sealed partial class PvwaRestClient
{
    /// <summary>
    /// GET /PasswordVault/API/Safes/
    /// https://docs.cyberark.com/pam-self-hosted/latest/en/content/webservices/get%20safes.htm
    /// Pages through the API automatically and returns every safe.
    /// </summary>
    public async Task<IReadOnlyList<SafeInfo>> GetSafesAsync(
        string? search = null,
        bool includeAccounts = false,
        int pageSize = 500,
        CancellationToken ct = default)
    {
        var results = new List<SafeInfo>();
        var offset = 0;

        while (true)
        {
            var query = new QueryStringBuilder()
                .Add("search", search)
                .Add("offset", offset)
                .Add("limit", pageSize)
                .Add("includeAccounts", includeAccounts)
                .ToString();

            var page = await GetJsonAsync<SafesListResponse>($"API/Safes/{query}", ct).ConfigureAwait(false);
            var safes = page.Value ?? new List<SafeDto>();
            results.AddRange(safes.Select(s => s.ToModel()));

            if (safes.Count < pageSize || string.IsNullOrEmpty(page.NextLink))
            {
                break;
            }

            offset += pageSize;
        }

        return results;
    }

    /// <summary>
    /// GET /PasswordVault/API/Safes/{safeUrlId}/
    /// https://docs.cyberark.com/pam-self-hosted/latest/en/content/webservices/get%20safe%20details.htm
    /// </summary>
    public async Task<SafeInfo> GetSafeDetailsAsync(string safeId, CancellationToken ct = default)
    {
        var dto = await GetJsonAsync<SafeDto>($"API/Safes/{Uri.EscapeDataString(safeId)}/", ct).ConfigureAwait(false);
        return dto.ToModel();
    }
}
