using System.Windows.Controls;
using VaultDashboard.App.ViewModels.Pages;

namespace VaultDashboard.App.Views.Pages;

public partial class OverviewPage : UserControl
{
    public OverviewPage(OverviewViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
