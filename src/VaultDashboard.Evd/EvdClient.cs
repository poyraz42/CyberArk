using VaultDashboard.Core.Connection;
using VaultDashboard.Core.Models;

namespace VaultDashboard.Evd;

/// <summary>
/// High-level façade over EvdExportRunner + EvdReportParser: runs a single ExportVaultData pass for
/// the reports the dashboard needs, then parses each into Core models.
/// </summary>
public sealed class EvdClient
{
    private readonly EvdConnectionProfile _profile;
    private readonly EvdExportRunner _runner;

    public EvdClient(EvdConnectionProfile profile)
    {
        _profile = profile ?? throw new ArgumentNullException(nameof(profile));
        _runner = new EvdExportRunner(profile);
    }

    /// <summary>
    /// Runs ExportVaultData once for SafesList/UsersList/GroupsList/GroupMembersList and returns everything
    /// parsed into Core models, keyed by report type so a caller can tell which reports actually produced data.
    /// </summary>
    public async Task<EvdSnapshot> ExportSnapshotAsync(CancellationToken ct = default)
    {
        var reportTypes = new[]
        {
            EvdReportType.SafesList,
            EvdReportType.UsersList,
            EvdReportType.GroupsList,
            EvdReportType.GroupMembersList,
        };

        var paths = await _runner.ExportAsync(reportTypes, ct).ConfigureAwait(false);

        var safes = EvdReportParser.ParseFile(paths[EvdReportType.SafesList], _profile.Separator)
            .Select(r => r.ToSafeInfo()).ToList();
        var users = EvdReportParser.ParseFile(paths[EvdReportType.UsersList], _profile.Separator)
            .Select(r => r.ToUserInfo()).ToList();
        var groups = EvdReportParser.ParseFile(paths[EvdReportType.GroupsList], _profile.Separator)
            .Select(r => r.ToGroupInfo()).ToList();
        var groupMembers = EvdReportParser.ParseFile(paths[EvdReportType.GroupMembersList], _profile.Separator);

        // Enrich group member counts from the GroupMembersList report (one row per group/member pair).
        var membersByGroup = groupMembers
            .GroupBy(r => r.GetOrEmpty("Groupname", "GroupName"))
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

        var enrichedGroups = groups.Select(g => membersByGroup.TryGetValue(g.GroupName, out var count)
            ? new GroupInfo
            {
                Id = g.Id,
                GroupName = g.GroupName,
                GroupType = g.GroupType,
                Location = g.Location,
                MembersCount = count,
                DataSource = g.DataSource,
            }
            : g).ToList();

        return new EvdSnapshot(safes, users, enrichedGroups, paths);
    }
}

public sealed record EvdSnapshot(
    IReadOnlyList<SafeInfo> Safes,
    IReadOnlyList<UserInfo> Users,
    IReadOnlyList<GroupInfo> Groups,
    IReadOnlyDictionary<EvdReportType, string> ReportFiles);
