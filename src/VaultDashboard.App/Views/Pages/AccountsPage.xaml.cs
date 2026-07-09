using System.Windows.Controls;
using VaultDashboard.App.ViewModels.Pages;

namespace VaultDashboard.App.Views.Pages;

public partial class AccountsPage : UserControl
{
    public AccountsPage(AccountsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
