using VaultDashboard.Core.Models;
using VaultDashboard.Pvwa.Dto;

namespace VaultDashboard.Pvwa;

public sealed partial class PvwaRestClient
{
    /// <summary>
    /// GET /PasswordVault/API/Configuration/LDAP/Directories/
    /// https://docs.cyberark.com/pam-self-hosted/latest/en/content/webservices/get%20directories.htm
    /// </summary>
    public async Task<IReadOnlyList<LdapDirectoryInfo>> GetLdapDirectoriesAsync(CancellationToken ct = default)
    {
        var response = await GetJsonAsync<LdapDirectoriesResponse>(
            "API/Configuration/LDAP/Directories/", ct).ConfigureAwait(false);
        return (response.Value ?? new List<LdapDirectoryDto>()).Select(d => d.ToModel()).ToList();
    }

    /// <summary>
    /// GET /PasswordVault/WebServices/PIMServices.svc/Applications/
    /// https://docs.cyberark.com/pam-self-hosted/latest/en/content/webservices/get%20applications.htm
    /// </summary>
    public async Task<IReadOnlyList<ApplicationInfo>> GetApplicationsAsync(CancellationToken ct = default)
    {
        var response = await GetJsonAsync<ApplicationsResponse>(
            "WebServices/PIMServices.svc/Applications/", ct).ConfigureAwait(false);
        return (response.Application ?? new List<ApplicationDto>()).Select(a => a.ToModel()).ToList();
    }

    /// <summary>
    /// GET /PasswordVault/API/AutomaticOnboardingRules/
    /// https://docs.cyberark.com/pam-self-hosted/latest/en/content/webservices/get%20onboarding%20rule.htm
    /// </summary>
    public async Task<IReadOnlyList<OnboardingRuleInfo>> GetOnboardingRulesAsync(CancellationToken ct = default)
    {
        var response = await GetJsonAsync<OnboardingRulesResponse>(
            "API/AutomaticOnboardingRules/", ct).ConfigureAwait(false);
        return (response.Value ?? new List<OnboardingRuleDto>()).Select(r => r.ToModel()).ToList();
    }

    /// <summary>
    /// GET /PasswordVault/API/Reports
    /// https://docs.cyberark.com/pam-self-hosted/latest/en/content/webservices/get-reports.htm
    /// </summary>
    public async Task<IReadOnlyList<ReportInfo>> GetReportsAsync(CancellationToken ct = default)
    {
        var response = await GetJsonAsync<ReportsResponse>("API/Reports", ct).ConfigureAwait(false);
        return (response.Value ?? new List<ReportDto>()).Select(r => r.ToModel()).ToList();
    }

    /// <summary>
    /// GET /PasswordVault/API/Tasks
    /// https://docs.cyberark.com/pam-self-hosted/latest/en/content/webservices/get-tasks.htm
    /// </summary>
    public async Task<IReadOnlyList<VaultTaskInfo>> GetTasksAsync(CancellationToken ct = default)
    {
        var response = await GetJsonAsync<TasksResponse>("API/Tasks", ct).ConfigureAwait(false);
        return (response.Value ?? new List<TaskDto>()).Select(t => t.ToModel()).ToList();
    }

    /// <summary>
    /// GET /PasswordVault/API/ClassicReports?data=&lt;base64 report descriptor&gt;
    /// https://docs.cyberark.com/pam-self-hosted/latest/en/content/webservices/download-report.htm
    /// Downloads a "classic" PVWA report (e.g. the License Capacity report) as CSV/XLS/XLSX bytes.
    /// </summary>
    public async Task<byte[]> DownloadClassicReportAsync(
        string safeName,
        string folder,
        string fileName,
        string reportType,
        ClassicReportFormat format = ClassicReportFormat.Csv,
        CancellationToken ct = default)
    {
        var formatToken = format == ClassicReportFormat.Csv ? "csv" : "excel";
        var descriptor = $"Safe={safeName}^@^Folder={folder}^@^Name={fileName}^@^Format={formatToken}^@^Type={reportType}";
        var encoded = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(descriptor));
        var contentType = format switch
        {
            ClassicReportFormat.Csv => "text/csv",
            ClassicReportFormat.Xls => "application/vnd.ms-excel",
            ClassicReportFormat.Xlsx => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => "text/csv",
        };

        return await GetBytesAsync($"API/ClassicReports?data={Uri.EscapeDataString(encoded)}", contentType, ct)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// GET /PasswordVault/WebServices/PIMServices.svc/Server/
    /// https://docs.cyberark.com/pam-self-hosted/latest/en/content/webservices/server.htm
    /// Returns the PVWA/vault version, used for the "Version Control" panel of the dashboard.
    /// </summary>
    public async Task<(string? ExternalVersion, string? ServerName)> GetServerInfoAsync(CancellationToken ct = default)
    {
        var dto = await GetJsonAsync<ServerInfoDto>("WebServices/PIMServices.svc/Server/", ct).ConfigureAwait(false);
        return (dto.ExternalVersion, dto.ServerName);
    }

    /// <summary>
    /// GET /PasswordVault/API/pta/API/Risks/RisksEvents/  — requires a PTA session token (separate Logon call
    /// against the PTA auth type). Returns an empty list rather than throwing if PTA isn't configured/licensed.
    /// https://docs.cyberark.com/pam-self-hosted/latest/en/content/webservices/get%20risk%20events.htm
    /// </summary>
    public async Task<IReadOnlyList<SecurityEventInfo>> GetRiskEventsAsync(CancellationToken ct = default)
    {
        var response = await GetJsonAsync<SecurityEventsResponse>("API/pta/API/Risks/RisksEvents/", ct)
            .ConfigureAwait(false);
        var events = response.Events ?? response.Value ?? new List<SecurityEventDto>();
        return events.Select(e => e.ToModel()).ToList();
    }

    /// <summary>
    /// GET /PasswordVault/API/pta/API/Events/
    /// https://docs.cyberark.com/pam-self-hosted/latest/en/content/webservices/get%20security%20events.htm
    /// </summary>
    public async Task<IReadOnlyList<SecurityEventInfo>> GetSecurityEventsAsync(CancellationToken ct = default)
    {
        var response = await GetJsonAsync<SecurityEventsResponse>("API/pta/API/Events/", ct).ConfigureAwait(false);
        var events = response.Events ?? response.Value ?? new List<SecurityEventDto>();
        return events.Select(e => e.ToModel()).ToList();
    }
}

public enum ClassicReportFormat
{
    Csv,
    Xls,
    Xlsx,
}
