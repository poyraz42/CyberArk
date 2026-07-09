using System.Windows.Controls;
using VaultDashboard.App.ViewModels.Pages;

namespace VaultDashboard.App.Views.Pages;

public partial class SafesPage : UserControl
{
    public SafesPage(SafesViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
