using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using VaultDashboard.App.Services;
using VaultDashboard.Core.Models;

namespace VaultDashboard.App.ViewModels.Pages;

/// <summary>
/// Base for every dashboard page's view model: subscribes to <see cref="DashboardStateService"/> and
/// calls <see cref="OnSnapshotChanged"/> whenever a refresh produces new data (including immediately,
/// if a snapshot already exists when the page is first shown).
/// </summary>
public abstract partial class SnapshotPageViewModelBase : ObservableObject, IDisposable
{
    protected readonly DashboardStateService State;

    protected SnapshotPageViewModelBase(DashboardStateService state)
    {
        State = state;
        State.PropertyChanged += OnStatePropertyChanged;
        if (State.Snapshot is not null)
        {
            OnSnapshotChanged(State.Snapshot);
        }
    }

    private void OnStatePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DashboardStateService.Snapshot) && State.Snapshot is not null)
        {
            OnSnapshotChanged(State.Snapshot);
        }
    }

    protected abstract void OnSnapshotChanged(EnvironmentSnapshot snapshot);

    public void Dispose() => State.PropertyChanged -= OnStatePropertyChanged;
}
