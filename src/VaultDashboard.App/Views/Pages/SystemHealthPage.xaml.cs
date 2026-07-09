using System.Windows.Controls;
using VaultDashboard.App.ViewModels.Pages;

namespace VaultDashboard.App.Views.Pages;

public partial class SystemHealthPage : UserControl
{
    public SystemHealthPage(SystemHealthViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
