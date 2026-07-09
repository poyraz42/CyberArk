using System.Windows.Controls;
using VaultDashboard.App.ViewModels.Pages;

namespace VaultDashboard.App.Views.Pages;

public partial class SettingsPage : UserControl
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
