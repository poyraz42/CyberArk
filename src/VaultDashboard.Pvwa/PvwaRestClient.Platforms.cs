using VaultDashboard.Core.Models;
using VaultDashboard.Pvwa.Dto;

namespace VaultDashboard.Pvwa;

public sealed partial class PvwaRestClient
{
    /// <summary>
    /// GET /PasswordVault/API/Platforms/
    /// https://docs.cyberark.com/pam-self-hosted/latest/en/content/webservices/get%20platforms.htm
    /// </summary>
    public async Task<IReadOnlyList<PlatformInfo>> GetPlatformsAsync(
        bool? active = null,
        string? platformType = null,
        CancellationToken ct = default)
    {
        var query = new QueryStringBuilder()
            .Add("Active", active)
            .Add("PlatformType", platformType)
            .ToString();

        var response = await GetJsonAsync<PlatformsListResponse>($"API/Platforms/{query}", ct).ConfigureAwait(false);
        return (response.Platforms ?? new List<PlatformDto>()).Select(p => p.ToModel()).ToList();
    }

    /// <summary>
    /// GET /PasswordVault/API/Platforms/{platformName}/
    /// https://docs.cyberark.com/pam-self-hosted/latest/en/content/webservices/get%20platform%20details.htm
    /// </summary>
    public async Task<PlatformInfo> GetPlatformDetailsAsync(string platformName, CancellationToken ct = default)
    {
        var dto = await GetJsonAsync<PlatformDto>($"API/Platforms/{Uri.EscapeDataString(platformName)}/", ct)
            .ConfigureAwait(false);
        return dto.ToModel();
    }
}
