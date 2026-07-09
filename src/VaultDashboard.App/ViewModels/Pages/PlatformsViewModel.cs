using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using VaultDashboard.App.Charting;
using VaultDashboard.App.Services;
using VaultDashboard.Core.Models;

namespace VaultDashboard.App.ViewModels.Pages;

public sealed partial class PlatformsViewModel : SnapshotPageViewModelBase
{
    [ObservableProperty] private ISeries[] _activeVsDisabledSeries = Array.Empty<ISeries>();
    [ObservableProperty] private ISeries[] _rotationIntervalSeries = Array.Empty<ISeries>();
    [ObservableProperty] private Axis[] _rotationIntervalXAxes = Array.Empty<Axis>();

    public ObservableCollection<PlatformInfo> Platforms { get; } = new();

    public PlatformsViewModel(DashboardStateService state) : base(state)
    {
    }

    protected override void OnSnapshotChanged(EnvironmentSnapshot snapshot)
    {
        Platforms.Clear();
        foreach (var platform in snapshot.Platforms.OrderByDescending(p => p.AccountsCount ?? 0))
        {
            Platforms.Add(platform);
        }

        var active = snapshot.Platforms.Count(p => p.Active);
        var disabled = snapshot.Platforms.Count - active;
        ActiveVsDisabledSeries = ChartFactory.PieSlices(new[]
        {
            ("Active", (double)active),
            ("Disabled", (double)disabled),
        }).ToArray();

        var byInterval = snapshot.Platforms
            .Where(p => p.PasswordChangeIntervalDays.HasValue)
            .GroupBy(p => p.PasswordChangeIntervalDays!.Value)
            .OrderBy(g => g.Key)
            .Select(g => (g.Key.ToString() + "d", (double)g.Count()))
            .ToList();
        RotationIntervalSeries = new[]
        {
            ChartFactory.ColumnSeries("Platforms", byInterval.Select(b => b.Item2), ChartPalette.Series3),
        };
        RotationIntervalXAxes = new[] { ChartFactory.CategoryAxis(byInterval.Select(b => b.Item1)) };
    }
}
