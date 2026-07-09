using VaultDashboard.Core.Connection;
using VaultDashboard.Core.Models;
using Xunit;

namespace VaultDashboard.Pvwa.Tests;

public class PvwaSnapshotServiceTests
{
    [Fact]
    public async Task FetchSnapshotAsync_OneEndpointFailing_StillReturnsDataFromTheOthers()
    {
        var handler = new FakeHttpMessageHandler()
            .When(HttpMethod.Post, "auth/Cyberark/Logon", "\"tok\"")
            .When(HttpMethod.Get, "API/Safes", "{\"value\":[{\"safeName\":\"S1\"}],\"count\":1}")
            .When(HttpMethod.Get, "API/Accounts", "boom", System.Net.HttpStatusCode.InternalServerError);
        // Every other endpoint hit by FetchSnapshotAsync has no route registered, so it 404s -
        // that's the point: a handful of unavailable/unlicensed endpoints must not abort the whole pass.

        var profile = new PvwaConnectionProfile { Address = "fake-pvwa.test", Username = "a", Password = "b" };
        var client = new PvwaRestClient(profile, new HttpClient(handler));
        await client.LogonAsync();

        var snapshot = await new PvwaSnapshotService(client).FetchSnapshotAsync();

        Assert.Single(snapshot.Safes);
        Assert.Empty(snapshot.Accounts);

        var safesStatus = Assert.Single(snapshot.FetchLog, s => s.Endpoint == "Safes");
        Assert.True(safesStatus.Success);
        Assert.Equal(1, safesStatus.ItemCount);

        var accountsStatus = Assert.Single(snapshot.FetchLog, s => s.Endpoint == "Accounts");
        Assert.False(accountsStatus.Success);
        Assert.Equal(VaultDataSourceKind.Pvwa, accountsStatus.Source);

        // Every declared endpoint produced a log entry, success or not - nothing silently vanishes.
        Assert.True(snapshot.FetchLog.Count >= 13);
    }
}
