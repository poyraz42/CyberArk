using System.Collections.ObjectModel;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VaultDashboard.App.Services;
using VaultDashboard.App.Views.Pages;

namespace VaultDashboard.App.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    private readonly IServiceProvider _services;
    private readonly Dictionary<Type, UserControl> _pageCache = new();

    public DashboardStateService State { get; }

    public ObservableCollection<NavItem> NavItems { get; } = new(new[]
    {
        new NavItem("overview", "Overview", typeof(OverviewPage)),
        new NavItem("license", "License Usage", typeof(LicenseUsagePage)),
        new NavItem("safes", "Safes", typeof(SafesPage)),
        new NavItem("accounts", "Accounts", typeof(AccountsPage)),
        new NavItem("platforms", "Platforms", typeof(PlatformsPage)),
        new NavItem("sessions", "Sessions & PSM", typeof(SessionsPage)),
        new NavItem("users", "Users & Groups", typeof(UsersGroupsPage)),
        new NavItem("health", "System Health", typeof(SystemHealthPage)),
        new NavItem("settings", "Connections", typeof(SettingsPage)),
    });

    [ObservableProperty]
    private NavItem? _selectedNavItem;

    [ObservableProperty]
    private object? _currentPage;

    public IAsyncRelayCommand RefreshCommand { get; }

    public MainWindowViewModel(DashboardStateService state, IServiceProvider services)
    {
        State = state;
        _services = services;
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        SelectedNavItem = NavItems[0];
    }

    partial void OnSelectedNavItemChanged(NavItem? value)
    {
        if (value is null)
        {
            return;
        }

        if (!_pageCache.TryGetValue(value.PageType, out var page))
        {
            page = (UserControl)_services.GetService(value.PageType)!;
            _pageCache[value.PageType] = page;
        }

        CurrentPage = page;
    }

    private async Task RefreshAsync()
    {
        try
        {
            await State.RefreshAsync();
        }
        catch
        {
            // DashboardStateService.LastError / StatusMessage already reflect the failure for the UI to show.
        }
    }
}
