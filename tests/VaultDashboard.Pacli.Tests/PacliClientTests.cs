using VaultDashboard.Core.Connection;
using VaultDashboard.Core.Exceptions;
using Xunit;

namespace VaultDashboard.Pacli.Tests;

public class PacliClientTests
{
    [Fact]
    public async Task InitializeAsync_ExecutableMissing_ThrowsPacliCommandExceptionWithHelpfulMessage()
    {
        var profile = new PacliConnectionProfile
        {
            ExecutablePath = "/nonexistent/Pacli.exe",
            VaultName = "Vault",
            VaultAddress = "vault.example.com",
        };

        await using var client = new PacliClient(profile);

        var ex = await Assert.ThrowsAsync<PacliCommandException>(() => client.InitializeAsync());
        Assert.Contains("Pacli.exe was not found", ex.Message);
    }

    [Fact]
    public async Task ListUsersAsync_BeforeLogon_ThrowsPacliCommandException()
    {
        var profile = new PacliConnectionProfile { ExecutablePath = "/nonexistent/Pacli.exe" };
        await using var client = new PacliClient(profile);

        await Assert.ThrowsAsync<PacliCommandException>(() => client.ListUsersAsync());
    }
}
