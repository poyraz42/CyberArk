using System.Windows.Controls;
using VaultDashboard.App.ViewModels.Pages;

namespace VaultDashboard.App.Views.Pages;

public partial class SessionsPage : UserControl
{
    public SessionsPage(SessionsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
