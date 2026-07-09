using System.Windows.Controls;
using VaultDashboard.App.ViewModels.Pages;

namespace VaultDashboard.App.Views.Pages;

public partial class LicenseUsagePage : UserControl
{
    public LicenseUsagePage(LicenseUsageViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
