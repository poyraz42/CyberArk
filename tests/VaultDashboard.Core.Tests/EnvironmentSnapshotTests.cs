using VaultDashboard.Core.Models;
using Xunit;

namespace VaultDashboard.Core.Tests;

public class EnvironmentSnapshotTests
{
    [Fact]
    public void ManagedAccountsCount_CountsOnlyAutomaticallyManaged()
    {
        var snapshot = new EnvironmentSnapshot
        {
            Accounts = new[]
            {
                new AccountInfo { Id = "1", AutomaticManagementEnabled = true },
                new AccountInfo { Id = "2", AutomaticManagementEnabled = false },
                new AccountInfo { Id = "3", AutomaticManagementEnabled = true },
            },
        };

        Assert.Equal(2, snapshot.ManagedAccountsCount);
    }

    [Fact]
    public void NonCompliantAccountsCount_CountsAccountsWithFailureStatus()
    {
        var snapshot = new EnvironmentSnapshot
        {
            Accounts = new[]
            {
                new AccountInfo { Id = "1", Status = "success" },
                new AccountInfo { Id = "2", Status = "failure" },
                new AccountInfo { Id = "3", Status = null },
            },
        };

        Assert.Equal(1, snapshot.NonCompliantAccountsCount);
    }

    [Fact]
    public void ActivePlatformsCount_CountsOnlyActivePlatforms()
    {
        var snapshot = new EnvironmentSnapshot
        {
            Platforms = new[]
            {
                new PlatformInfo { PlatformId = "A", Active = true },
                new PlatformInfo { PlatformId = "B", Active = false },
            },
        };

        Assert.Equal(1, snapshot.ActivePlatformsCount);
    }

    [Fact]
    public void DisabledUsersCount_CountsUsersNotEnabled()
    {
        var snapshot = new EnvironmentSnapshot
        {
            Users = new[]
            {
                new UserInfo { Username = "a", Enabled = true },
                new UserInfo { Username = "b", Enabled = false },
                new UserInfo { Username = "c", Enabled = false },
            },
        };

        Assert.Equal(2, snapshot.DisabledUsersCount);
    }
}
