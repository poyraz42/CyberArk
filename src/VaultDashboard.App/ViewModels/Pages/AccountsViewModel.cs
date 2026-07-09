using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using VaultDashboard.App.Charting;
using VaultDashboard.App.Services;
using VaultDashboard.Core.Models;

namespace VaultDashboard.App.ViewModels.Pages;

public sealed partial class AccountsViewModel : SnapshotPageViewModelBase
{
    [ObservableProperty] private ISeries[] _topPlatformsSeries = Array.Empty<ISeries>();
    [ObservableProperty] private Axis[] _topPlatformsXAxes = Array.Empty<Axis>();
    [ObservableProperty] private ISeries[] _topSafesSeries = Array.Empty<ISeries>();
    [ObservableProperty] private Axis[] _topSafesXAxes = Array.Empty<Axis>();

    public ObservableCollection<AccountInfo> Accounts { get; } = new();

    public AccountsViewModel(DashboardStateService state) : base(state)
    {
    }

    protected override void OnSnapshotChanged(EnvironmentSnapshot snapshot)
    {
        Accounts.Clear();
        foreach (var account in snapshot.Accounts.OrderBy(a => a.SafeName).ThenBy(a => a.Name))
        {
            Accounts.Add(account);
        }

        var topPlatforms = snapshot.Accounts
            .GroupBy(a => string.IsNullOrWhiteSpace(a.PlatformId) ? "(none)" : a.PlatformId!)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => (g.Key, (double)g.Count()))
            .ToList();
        TopPlatformsSeries = new[]
        {
            ChartFactory.ColumnSeries("Accounts", topPlatforms.Select(p => p.Item2), ChartPalette.Series1),
        };
        TopPlatformsXAxes = new[] { ChartFactory.CategoryAxis(topPlatforms.Select(p => p.Key)) };

        var topSafes = snapshot.Accounts
            .GroupBy(a => a.SafeName)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => (g.Key, (double)g.Count()))
            .ToList();
        TopSafesSeries = new[]
        {
            ChartFactory.ColumnSeries("Accounts", topSafes.Select(s => s.Item2), ChartPalette.Series2),
        };
        TopSafesXAxes = new[] { ChartFactory.CategoryAxis(topSafes.Select(s => s.Key)) };
    }
}
