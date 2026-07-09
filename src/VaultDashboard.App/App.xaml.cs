using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using VaultDashboard.App.Services;
using VaultDashboard.App.ViewModels;
using VaultDashboard.App.ViewModels.Pages;
using VaultDashboard.App.Views;
using VaultDashboard.App.Views.Pages;
using Wpf.Ui.Appearance;

namespace VaultDashboard.App;

public partial class App : System.Windows.Application
{
    private IServiceProvider? _services;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ApplicationThemeManager.Apply(ApplicationTheme.Dark, WindowBackdropType.Mica, updateAccent: true);

        var services = new ServiceCollection();
        ConfigureServices(services);
        _services = services.BuildServiceProvider();

        var state = _services.GetRequiredService<DashboardStateService>();
        await state.LoadProfilesAsync();

        var mainWindow = _services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ProfileStore>();
        services.AddSingleton<DashboardStateService>();

        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<OverviewViewModel>();
        services.AddTransient<LicenseUsageViewModel>();
        services.AddTransient<SafesViewModel>();
        services.AddTransient<AccountsViewModel>();
        services.AddTransient<PlatformsViewModel>();
        services.AddTransient<SessionsViewModel>();
        services.AddTransient<UsersGroupsViewModel>();
        services.AddTransient<SystemHealthViewModel>();
        services.AddTransient<SettingsViewModel>();

        services.AddTransient<OverviewPage>();
        services.AddTransient<LicenseUsagePage>();
        services.AddTransient<SafesPage>();
        services.AddTransient<AccountsPage>();
        services.AddTransient<PlatformsPage>();
        services.AddTransient<SessionsPage>();
        services.AddTransient<UsersGroupsPage>();
        services.AddTransient<SystemHealthPage>();
        services.AddTransient<SettingsPage>();

        services.AddSingleton<MainWindow>();
    }
}
