using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using VaultDashboard.App.Charting;
using VaultDashboard.App.Services;
using VaultDashboard.Core.Models;

namespace VaultDashboard.App.ViewModels.Pages;

public sealed partial class OverviewViewModel : SnapshotPageViewModelBase
{
    [ObservableProperty] private int _safesCount;
    [ObservableProperty] private int _accountsCount;
    [ObservableProperty] private double _managedAccountsPercent;
    [ObservableProperty] private int _nonCompliantAccountsCount;
    [ObservableProperty] private int _usersCount;
    [ObservableProperty] private int _disabledUsersCount;
    [ObservableProperty] private int _groupsCount;
    [ObservableProperty] private int _activePlatformsCount;
    [ObservableProperty] private int _liveSessionsCount;

    [ObservableProperty] private ISeries[] _accountsComplianceSeries = Array.Empty<ISeries>();
    [ObservableProperty] private ISeries[] _accountsManagedSeries = Array.Empty<ISeries>();
    [ObservableProperty] private ISeries[] _topSafeRetentionSeries = Array.Empty<ISeries>();
    [ObservableProperty] private Axis[] _topSafeRetentionXAxes = Array.Empty<Axis>();

    public ObservableCollection<DataSourceStatus> FetchLog { get; } = new();

    public OverviewViewModel(DashboardStateService state) : base(state)
    {
    }

    protected override void OnSnapshotChanged(EnvironmentSnapshot snapshot)
    {
        SafesCount = snapshot.Safes.Count;
        AccountsCount = snapshot.Accounts.Count;
        ManagedAccountsPercent = snapshot.Accounts.Count == 0
            ? 0
            : Math.Round(snapshot.ManagedAccountsCount * 100.0 / snapshot.Accounts.Count, 1);
        NonCompliantAccountsCount = snapshot.NonCompliantAccountsCount;
        UsersCount = snapshot.Users.Count;
        DisabledUsersCount = snapshot.DisabledUsersCount;
        GroupsCount = snapshot.Groups.Count;
        ActivePlatformsCount = snapshot.ActivePlatformsCount;
        LiveSessionsCount = snapshot.LiveSessions.Count;

        var compliant = snapshot.Accounts.Count - snapshot.NonCompliantAccountsCount;
        AccountsComplianceSeries = ChartFactory.PieSlices(new[]
        {
            ("Compliant", (double)compliant),
            ("Non-compliant", (double)snapshot.NonCompliantAccountsCount),
        }).ToArray();

        var unmanaged = snapshot.Accounts.Count - snapshot.ManagedAccountsCount;
        AccountsManagedSeries = ChartFactory.PieSlices(new[]
        {
            ("Managed", (double)snapshot.ManagedAccountsCount),
            ("Unmanaged", (double)unmanaged),
        }).ToArray();

        var topSafes = snapshot.Safes
            .OrderByDescending(s => s.NumberOfDaysRetention)
            .Take(8)
            .ToList();
        TopSafeRetentionSeries = new[]
        {
            ChartFactory.ColumnSeries("Retention (days)", topSafes.Select(s => (double)s.NumberOfDaysRetention),
                ChartPalette.Series1),
        };
        TopSafeRetentionXAxes = new[] { ChartFactory.CategoryAxis(topSafes.Select(s => s.SafeName)) };

        FetchLog.Clear();
        foreach (var entry in snapshot.FetchLog.OrderBy(l => l.Success).ThenBy(l => l.Endpoint))
        {
            FetchLog.Add(entry);
        }
    }
}
