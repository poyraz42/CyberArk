using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using VaultDashboard.App.Charting;
using VaultDashboard.App.Services;
using VaultDashboard.Core.Models;

namespace VaultDashboard.App.ViewModels.Pages;

public sealed partial class SafesViewModel : SnapshotPageViewModelBase
{
    [ObservableProperty] private ISeries[] _retentionDistributionSeries = Array.Empty<ISeries>();
    [ObservableProperty] private Axis[] _retentionDistributionXAxes = Array.Empty<Axis>();
    [ObservableProperty] private ISeries[] _managingCpmSeries = Array.Empty<ISeries>();

    public ObservableCollection<SafeInfo> Safes { get; } = new();

    public SafesViewModel(DashboardStateService state) : base(state)
    {
    }

    protected override void OnSnapshotChanged(EnvironmentSnapshot snapshot)
    {
        Safes.Clear();
        foreach (var safe in snapshot.Safes.OrderBy(s => s.SafeName))
        {
            Safes.Add(safe);
        }

        // Bucket retention (days) the same way the assessment-report "Safes" section groups them.
        var buckets = snapshot.Safes
            .GroupBy(s => s.NumberOfDaysRetention switch
            {
                <= 0 => "Versions only",
                <= 7 => "1-7 days",
                <= 30 => "8-30 days",
                <= 90 => "31-90 days",
                <= 180 => "91-180 days",
                _ => "180+ days",
            })
            .ToDictionary(g => g.Key, g => g.Count());
        var order = new[] { "Versions only", "1-7 days", "8-30 days", "31-90 days", "91-180 days", "180+ days" };
        var orderedBuckets = order.Where(buckets.ContainsKey).Select(k => (k, (double)buckets[k])).ToList();

        RetentionDistributionSeries = new[]
        {
            ChartFactory.ColumnSeries("Safes", orderedBuckets.Select(b => b.Item2), ChartPalette.Series1),
        };
        RetentionDistributionXAxes = new[] { ChartFactory.CategoryAxis(orderedBuckets.Select(b => b.k)) };

        var byCpm = snapshot.Safes
            .GroupBy(s => string.IsNullOrWhiteSpace(s.ManagingCpm) ? "(none)" : s.ManagingCpm!)
            .OrderByDescending(g => g.Count())
            .Take(8)
            .Select(g => (g.Key, (double)g.Count()))
            .ToList();
        ManagingCpmSeries = ChartFactory.PieSlices(byCpm).ToArray();
    }
}
