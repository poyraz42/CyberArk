using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using VaultDashboard.App.Charting;
using VaultDashboard.App.Services;
using VaultDashboard.Core.Models;

namespace VaultDashboard.App.ViewModels.Pages;

public sealed partial class SessionsViewModel : SnapshotPageViewModelBase
{
    [ObservableProperty] private ISeries[] _sessionsByProtocolSeries = Array.Empty<ISeries>();

    public ObservableCollection<LiveSessionInfo> LiveSessions { get; } = new();
    public ObservableCollection<PsmServerInfo> PsmServers { get; } = new();

    public SessionsViewModel(DashboardStateService state) : base(state)
    {
    }

    protected override void OnSnapshotChanged(EnvironmentSnapshot snapshot)
    {
        LiveSessions.Clear();
        foreach (var session in snapshot.LiveSessions.OrderByDescending(s => s.Start))
        {
            LiveSessions.Add(session);
        }

        PsmServers.Clear();
        foreach (var server in snapshot.PsmServers)
        {
            PsmServers.Add(server);
        }

        var byProtocol = snapshot.LiveSessions
            .GroupBy(s => string.IsNullOrWhiteSpace(s.Protocol) ? "(unknown)" : s.Protocol!)
            .Select(g => (g.Key, (double)g.Count()))
            .ToList();
        SessionsByProtocolSeries = ChartFactory.PieSlices(byProtocol).ToArray();
    }
}
