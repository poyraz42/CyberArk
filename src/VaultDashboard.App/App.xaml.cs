using System.IO;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using VaultDashboard.App.Services;
using VaultDashboard.App.ViewModels;
using VaultDashboard.App.ViewModels.Pages;
using VaultDashboard.App.Views;
using VaultDashboard.App.Views.Pages;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace VaultDashboard.App;

public partial class App : System.Windows.Application
{
    private static readonly string CrashLogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "CyberArkVaultDashboard", "startup-error.log");

    private IServiceProvider? _services;

    public App()
    {
        // Wire these up in the constructor - before OnStartup runs - so nothing during
        // startup can fail silently. A WinExe has no console, so without this the process
        // just exits with no visible error at all if anything throws before a window shows.
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            ReportFatalError(args.Exception, "Background task");
            args.SetObserved();
        };
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            ApplicationThemeManager.Apply(ApplicationTheme.Dark, WindowBackdropType.Mica, updateAccent: true);

            var services = new ServiceCollection();
            ConfigureServices(services);
            _services = services.BuildServiceProvider();

            var state = _services.GetRequiredService<DashboardStateService>();
            await state.LoadProfilesAsync();

            var mainWindow = _services.GetRequiredService<MainWindow>();
            MainWindow = mainWindow;
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            ReportFatalError(ex, "Startup");
            Shutdown(1);
        }
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        ReportFatalError(e.Exception, "UI thread");
        e.Handled = true;
    }

    private void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            ReportFatalError(ex, "AppDomain");
        }
    }

    private static void ReportFatalError(Exception ex, string source)
    {
        var message = $"[{DateTimeOffset.Now:u}] ({source}) {ex}\n\n";

        try
        {
            var dir = Path.GetDirectoryName(CrashLogPath)!;
            Directory.CreateDirectory(dir);
            File.AppendAllText(CrashLogPath, message);
        }
        catch
        {
            // If we can't even write the log, the MessageBox below is still shown.
        }

        System.Windows.MessageBox.Show(
            $"CyberArk Vault Dashboard hit an unexpected error and needs to close.\n\n" +
            $"Source: {source}\n{ex.GetType().Name}: {ex.Message}\n\n" +
            $"Full details were written to:\n{CrashLogPath}",
            "CyberArk Vault Dashboard - Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
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
