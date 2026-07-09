using VaultDashboard.Core.Models;
using VaultDashboard.Pvwa.Dto;

namespace VaultDashboard.Pvwa;

public sealed partial class PvwaRestClient
{
    /// <summary>
    /// GET /PasswordVault/API/LiveSessions
    /// https://docs.cyberark.com/pam-self-hosted/latest/en/content/webservices/get%20active%20sessions.htm
    /// </summary>
    public async Task<IReadOnlyList<LiveSessionInfo>> GetActiveSessionsAsync(
        string? safe = null,
        int limit = 500,
        CancellationToken ct = default)
    {
        var query = new QueryStringBuilder()
            .Add("Safe", safe)
            .Add("Limit", limit)
            .ToString();

        var response = await GetJsonAsync<LiveSessionsResponse>($"API/LiveSessions{query}", ct).ConfigureAwait(false);
        return (response.Value ?? new List<LiveSessionDto>()).Select(s => s.ToModel()).ToList();
    }

    /// <summary>
    /// GET /PasswordVault/API/PSM/Servers/
    /// https://docs.cyberark.com/pam-self-hosted/latest/en/content/webservices/get%20all%20psm%20servers.htm
    /// </summary>
    public async Task<IReadOnlyList<PsmServerInfo>> GetPsmServersAsync(CancellationToken ct = default)
    {
        var response = await GetJsonAsync<PsmServersResponse>("API/PSM/Servers/", ct).ConfigureAwait(false);
        return (response.PSMServers ?? new List<PsmServerDto>()).Select(s => s.ToModel()).ToList();
    }
}
