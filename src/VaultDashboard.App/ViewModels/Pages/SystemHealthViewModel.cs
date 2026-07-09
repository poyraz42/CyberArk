using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using VaultDashboard.App.Charting;
using VaultDashboard.App.Services;
using VaultDashboard.Core.Models;

namespace VaultDashboard.App.ViewModels.Pages;

public sealed partial class SystemHealthViewModel : SnapshotPageViewModelBase
{
    [ObservableProperty] private ISeries[] _connectedVsDisconnectedSeries = Array.Empty<ISeries>();
    [ObservableProperty] private Axis[] _connectedVsDisconnectedXAxes = Array.Empty<Axis>();

    public ObservableCollection<SystemHealthComponent> Components { get; } = new();

    public SystemHealthViewModel(DashboardStateService state) : base(state)
    {
    }

    protected override void OnSnapshotChanged(EnvironmentSnapshot snapshot)
    {
        Components.Clear();
        foreach (var component in snapshot.SystemHealth.OrderBy(c => c.ComponentType))
        {
            Components.Add(component);
        }

        ConnectedVsDisconnectedSeries = new ISeries[]
        {
            ChartFactory.ColumnSeries("Connected", snapshot.SystemHealth.Select(c => (double)c.ConnectedCount), ChartPalette.StatusGood),
            ChartFactory.ColumnSeries("Disconnected", snapshot.SystemHealth.Select(c => (double)c.DisconnectedCount), ChartPalette.StatusCritical),
        };
        ConnectedVsDisconnectedXAxes = new[] { ChartFactory.CategoryAxis(snapshot.SystemHealth.Select(c => c.ComponentType)) };
    }
}
