using System.Windows.Controls;
using VaultDashboard.App.ViewModels.Pages;

namespace VaultDashboard.App.Views.Pages;

public partial class PlatformsPage : UserControl
{
    public PlatformsPage(PlatformsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
