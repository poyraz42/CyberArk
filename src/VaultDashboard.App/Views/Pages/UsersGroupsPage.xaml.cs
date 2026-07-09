using System.Windows.Controls;
using VaultDashboard.App.ViewModels.Pages;

namespace VaultDashboard.App.Views.Pages;

public partial class UsersGroupsPage : UserControl
{
    public UsersGroupsPage(UsersGroupsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
