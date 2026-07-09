using VaultDashboard.Core.Models;

namespace VaultDashboard.App.Services;

/// <summary>
/// Merges supplemental PACLI/EVD data into a PVWA-sourced snapshot. PVWA is always the primary source
/// (it has the richest, most current data); PACLI/EVD entries are only added for names PVWA didn't
/// already report - useful when the REST API user lacks visibility into some safes, or when running
/// fully offline against an EVD export with no PVWA available at all.
/// </summary>
internal static class SnapshotMerger
{
    public static IReadOnlyList<SafeInfo> MergeSafes(IReadOnlyList<SafeInfo> primary, IReadOnlyList<SafeInfo> supplemental)
    {
        var known = new HashSet<string>(primary.Select(s => s.SafeName), StringComparer.OrdinalIgnoreCase);
        return primary.Concat(supplemental.Where(s => known.Add(s.SafeName))).ToList();
    }

    public static IReadOnlyList<UserInfo> MergeUsers(IReadOnlyList<UserInfo> primary, IReadOnlyList<UserInfo> supplemental)
    {
        var known = new HashSet<string>(primary.Select(u => u.Username), StringComparer.OrdinalIgnoreCase);
        return primary.Concat(supplemental.Where(u => known.Add(u.Username))).ToList();
    }

    public static IReadOnlyList<GroupInfo> MergeGroups(IReadOnlyList<GroupInfo> primary, IReadOnlyList<GroupInfo> supplemental)
    {
        var known = new HashSet<string>(primary.Select(g => g.GroupName), StringComparer.OrdinalIgnoreCase);
        return primary.Concat(supplemental.Where(g => known.Add(g.GroupName))).ToList();
    }
}
