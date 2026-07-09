using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using VaultDashboard.App.Charting;
using VaultDashboard.App.Services;
using VaultDashboard.Core.Models;
using VaultDashboard.Pvwa;

namespace VaultDashboard.App.ViewModels.Pages;

/// <summary>
/// The License Capacity report is a "classic" PVWA report that must already have been generated once
/// inside PVWA (Reports section) before it can be downloaded - it's not a live endpoint, so this page
/// fetches it on demand via its own command rather than as part of the main dashboard refresh.
/// </summary>
public sealed partial class LicenseUsageViewModel : SnapshotPageViewModelBase
{
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string? _statusMessage;
    [ObservableProperty] private ISeries[] _usageSeries = Array.Empty<ISeries>();
    [ObservableProperty] private Axis[] _usageXAxes = Array.Empty<Axis>();

    public ObservableCollection<LicenseUsageItem> Items { get; } = new();

    public IAsyncRelayCommand DownloadReportCommand { get; }

    public LicenseUsageViewModel(DashboardStateService state) : base(state)
    {
        DownloadReportCommand = new AsyncRelayCommand(DownloadReportAsync);
    }

    protected override void OnSnapshotChanged(EnvironmentSnapshot snapshot)
    {
        var licenseReport = snapshot.Reports.FirstOrDefault(r =>
            r.Name.Contains("LicenseCapacityReport", StringComparison.OrdinalIgnoreCase) ||
            (r.Type?.Contains("LicenseCapacityReport", StringComparison.OrdinalIgnoreCase) ?? false));

        StatusMessage = licenseReport is null
            ? "No License Capacity report was found under PVWAReports. Generate it once in PVWA, then Refresh and try again."
            : $"Found report '{licenseReport.Name}'. Click Download to pull the latest usage numbers.";
    }

    private async Task DownloadReportAsync()
    {
        var snapshot = State.Snapshot;
        var licenseReport = snapshot?.Reports.FirstOrDefault(r =>
            r.Name.Contains("LicenseCapacityReport", StringComparison.OrdinalIgnoreCase));

        if (licenseReport is null)
        {
            StatusMessage = "No License Capacity report available. Refresh the dashboard first.";
            return;
        }

        IsLoading = true;
        StatusMessage = "Downloading license capacity report...";
        try
        {
            await using var client = new PvwaRestClient(State.PvwaProfile);
            await client.LogonAsync().ConfigureAwait(false);
            var bytes = await client.DownloadClassicReportAsync(
                "PVWAReports", "Root", licenseReport.Name, "CyberArk.Reports.LicenseCapacityReport.LicenseCapacityReportUI",
                ClassicReportFormat.Csv).ConfigureAwait(false);

            var csv = System.Text.Encoding.UTF8.GetString(bytes);
            var rows = SimpleCsvParser.Parse(csv);

            Items.Clear();
            foreach (var row in rows)
            {
                var name = row.GetOrEmpty("Licensed Object", "LicensedObject", "Component");
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                Items.Add(new LicenseUsageItem
                {
                    LicensedObject = name,
                    Description = row.GetOrEmpty("Description"),
                    Used = row.GetInt("Used", "Currently Used", "CurrentlyUsed"),
                    Maximum = row.GetInt("Maximum", "Maximum Allowed", "MaximumAllowed", "Total"),
                });
            }

            UsageSeries = new[]
            {
                ChartFactory.ColumnSeries("Used", Items.Select(i => (double)i.Used), ChartPalette.Series1),
                ChartFactory.ColumnSeries("Maximum", Items.Select(i => (double)i.Maximum), ChartPalette.Series2),
            };
            UsageXAxes = new[] { ChartFactory.CategoryAxis(Items.Select(i => i.LicensedObject)) };

            StatusMessage = $"Loaded {Items.Count} licensed object(s).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Download failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
