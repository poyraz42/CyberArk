using VaultDashboard.App.ViewModels;
using Wpf.Ui.Controls;

namespace VaultDashboard.App.Views;

public partial class MainWindow : FluentWindow
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
