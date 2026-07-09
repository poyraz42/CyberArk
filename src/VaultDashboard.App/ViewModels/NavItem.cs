namespace VaultDashboard.App.ViewModels;

/// <summary>One entry in the left navigation rail.</summary>
public sealed record NavItem(string Key, string Title, Type PageType);
