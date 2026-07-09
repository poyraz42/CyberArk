using VaultDashboard.Core.Models;
using VaultDashboard.Pvwa.Dto;

namespace VaultDashboard.Pvwa;

public sealed partial class PvwaRestClient
{
    /// <summary>
    /// GET /PasswordVault/API/ComponentsMonitoringSummary/
    /// https://docs.cyberark.com/pam-self-hosted/latest/en/content/webservices/system%20health%20-%20summary.htm
    /// </summary>
    public async Task<IReadOnlyList<SystemHealthComponent>> GetSystemHealthSummaryAsync(CancellationToken ct = default)
    {
        var response = await GetJsonAsync<ComponentsMonitoringSummaryResponse>(
            "API/ComponentsMonitoringSummary/", ct).ConfigureAwait(false);
        return (response.Components ?? new List<ComponentSummaryDto>()).Select(c => c.ToModel()).ToList();
    }

    /// <summary>
    /// GET /PasswordVault/API/ComponentsMonitoringDetails/{componentID}
    /// https://docs.cyberark.com/pam-self-hosted/latest/en/content/webservices/system%20health%20-%20details.htm
    /// </summary>
    public async Task<IReadOnlyList<SystemHealthComponentDetail>> GetSystemHealthDetailsAsync(
        string componentId, CancellationToken ct = default)
    {
        var response = await GetJsonAsync<ComponentsMonitoringDetailsResponse>(
            $"API/ComponentsMonitoringDetails/{Uri.EscapeDataString(componentId)}", ct).ConfigureAwait(false);
        return (response.ComponentsDetails ?? new List<ComponentDetailDto>()).Select(d => d.ToModel()).ToList();
    }

    /// <summary>
    /// Convenience helper: fetches the summary, then the per-component detail rows for every component type,
    /// so the dashboard can show connected/disconnected component instances without a second manual round trip.
    /// </summary>
    public async Task<IReadOnlyList<SystemHealthComponent>> GetSystemHealthWithDetailsAsync(CancellationToken ct = default)
    {
        var summary = await GetSystemHealthSummaryAsync(ct).ConfigureAwait(false);
        var enriched = new List<SystemHealthComponent>(summary.Count);

        foreach (var component in summary)
        {
            try
            {
                var details = await GetSystemHealthDetailsAsync(component.ComponentType, ct).ConfigureAwait(false);
                enriched.Add(new SystemHealthComponent
                {
                    ComponentType = component.ComponentType,
                    ConnectedCount = component.ConnectedCount,
                    DisconnectedCount = component.DisconnectedCount,
                    Details = details,
                });
            }
            catch
            {
                enriched.Add(component);
            }
        }

        return enriched;
    }
}
