using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VaultDashboard.App.Services;
using VaultDashboard.Core.Connection;
using VaultDashboard.Pvwa;

namespace VaultDashboard.App.ViewModels.Pages;

public sealed partial class SettingsViewModel : ObservableObject
{
    public DashboardStateService State { get; }

    public IReadOnlyList<PvwaAuthenticationType> AuthenticationTypes { get; } =
        Enum.GetValues<PvwaAuthenticationType>();

    [ObservableProperty] private string? _testResultMessage;
    [ObservableProperty] private bool _testSucceeded;
    [ObservableProperty] private bool _isBusy;

    public IAsyncRelayCommand TestPvwaConnectionCommand { get; }
    public IAsyncRelayCommand SaveCommand { get; }

    public SettingsViewModel(DashboardStateService state)
    {
        State = state;
        TestPvwaConnectionCommand = new AsyncRelayCommand(TestPvwaConnectionAsync);
        SaveCommand = new AsyncRelayCommand(SaveAsync);
    }

    private async Task TestPvwaConnectionAsync()
    {
        IsBusy = true;
        TestResultMessage = "Connecting...";
        try
        {
            await using var client = new PvwaRestClient(State.PvwaProfile);
            await client.LogonAsync().ConfigureAwait(false);
            var (version, serverName) = await client.GetServerInfoAsync().ConfigureAwait(false);
            TestSucceeded = true;
            TestResultMessage = $"Connected to {serverName ?? State.PvwaProfile.Address} (version {version ?? "unknown"}).";
        }
        catch (Exception ex)
        {
            TestSucceeded = false;
            TestResultMessage = $"Connection failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveAsync()
    {
        IsBusy = true;
        try
        {
            await State.SaveProfilesAsync().ConfigureAwait(false);
            TestResultMessage = "Connection profiles saved.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
