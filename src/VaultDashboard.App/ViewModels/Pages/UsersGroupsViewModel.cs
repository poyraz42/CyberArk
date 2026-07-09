using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using VaultDashboard.App.Charting;
using VaultDashboard.App.Services;
using VaultDashboard.Core.Models;

namespace VaultDashboard.App.ViewModels.Pages;

public sealed partial class UsersGroupsViewModel : SnapshotPageViewModelBase
{
    [ObservableProperty] private ISeries[] _enabledVsDisabledSeries = Array.Empty<ISeries>();
    [ObservableProperty] private ISeries[] _sourceBreakdownSeries = Array.Empty<ISeries>();

    public ObservableCollection<UserInfo> Users { get; } = new();
    public ObservableCollection<GroupInfo> Groups { get; } = new();

    public UsersGroupsViewModel(DashboardStateService state) : base(state)
    {
    }

    protected override void OnSnapshotChanged(EnvironmentSnapshot snapshot)
    {
        Users.Clear();
        foreach (var user in snapshot.Users.OrderBy(u => u.Username))
        {
            Users.Add(user);
        }

        Groups.Clear();
        foreach (var group in snapshot.Groups.OrderByDescending(g => g.MembersCount))
        {
            Groups.Add(group);
        }

        var enabled = snapshot.Users.Count(u => u.Enabled);
        EnabledVsDisabledSeries = ChartFactory.PieSlices(new[]
        {
            ("Enabled", (double)enabled),
            ("Disabled", (double)snapshot.DisabledUsersCount),
        }).ToArray();

        var bySource = snapshot.Users
            .GroupBy(u => string.IsNullOrWhiteSpace(u.Source) ? "(unknown)" : u.Source!)
            .Select(g => (g.Key, (double)g.Count()))
            .ToList();
        SourceBreakdownSeries = ChartFactory.PieSlices(bySource).ToArray();
    }
}
