using System.Net;
using VaultDashboard.Core.Connection;
using VaultDashboard.Core.Exceptions;
using Xunit;

namespace VaultDashboard.Pvwa.Tests;

public class PvwaRestClientTests
{
    private static PvwaConnectionProfile MakeProfile() => new()
    {
        Address = "fake-pvwa.test",
        AuthenticationType = PvwaAuthenticationType.Cyberark,
        Username = "auditor",
        Password = "secret",
    };

    [Fact]
    public async Task LogonAsync_ParsesTokenAndSetsRawAuthorizationHeader()
    {
        var handler = new FakeHttpMessageHandler()
            .When(HttpMethod.Post, "auth/Cyberark/Logon", "\"abc123token\"");
        var client = new PvwaRestClient(MakeProfile(), new HttpClient(handler));

        await client.LogonAsync();

        Assert.True(client.IsAuthenticated);
        handler
            .When(HttpMethod.Get, "API/Safes", "{\"value\":[],\"count\":0}");
        // Verify the header is the raw token with no "Bearer " scheme prefix.
        await client.GetSafesAsync();
        var safesRequest = handler.Requests.Last(r => r.RequestUri!.PathAndQuery.Contains("API/Safes"));
        Assert.Equal("abc123token", safesRequest.Headers.Authorization?.ToString());
    }

    [Fact]
    public async Task LogonAsync_NonSuccessStatus_ThrowsPvwaApiExceptionWithErrorDetails()
    {
        var handler = new FakeHttpMessageHandler()
            .When(HttpMethod.Post, "auth/Cyberark/Logon",
                "{\"ErrorCode\":\"CAWS00001E\",\"ErrorMessage\":\"Invalid credentials\"}",
                HttpStatusCode.Unauthorized);
        var client = new PvwaRestClient(MakeProfile(), new HttpClient(handler));

        var ex = await Assert.ThrowsAsync<PvwaApiException>(() => client.LogonAsync());

        Assert.Equal(401, ex.HttpStatusCode);
        Assert.Equal("CAWS00001E", ex.CyberArkErrorCode);
        Assert.Contains("Invalid credentials", ex.Message);
    }

    [Fact]
    public async Task GetSafesAsync_MapsFieldsAndStopsWhenPageShorterThanLimit()
    {
        const string json = """
        {
          "value": [
            { "safeName": "Safe1", "description": "d1", "managingCPM": "PasswordManager",
              "numberOfVersionsRetention": 7, "numberOfDaysRetention": 0, "OLACEnabled": true,
              "creationTime": 1700000000 }
          ],
          "count": 1
        }
        """;
        var handler = new FakeHttpMessageHandler()
            .When(HttpMethod.Post, "auth/Cyberark/Logon", "\"tok\"")
            .When(HttpMethod.Get, "API/Safes", json);
        var client = new PvwaRestClient(MakeProfile(), new HttpClient(handler));
        await client.LogonAsync();

        var safes = await client.GetSafesAsync(pageSize: 10);

        var safe = Assert.Single(safes);
        Assert.Equal("Safe1", safe.SafeName);
        Assert.Equal("PasswordManager", safe.ManagingCpm);
        Assert.Equal(7, safe.NumberOfVersionsRetention);
        Assert.True(safe.OlacEnabled);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1700000000), safe.CreationTime);
    }

    [Fact]
    public async Task GetAccountsAsync_NonSuccessStatus_ThrowsPvwaApiException()
    {
        var handler = new FakeHttpMessageHandler()
            .When(HttpMethod.Post, "auth/Cyberark/Logon", "\"tok\"")
            .When(HttpMethod.Get, "API/Accounts", "{\"ErrorMessage\":\"forbidden\"}", HttpStatusCode.Forbidden);
        var client = new PvwaRestClient(MakeProfile(), new HttpClient(handler));
        await client.LogonAsync();

        await Assert.ThrowsAsync<PvwaApiException>(() => client.GetAccountsAsync());
    }

    [Fact]
    public async Task AnyCall_BeforeLogon_ThrowsPvwaApiException()
    {
        var handler = new FakeHttpMessageHandler();
        var client = new PvwaRestClient(MakeProfile(), new HttpClient(handler));

        await Assert.ThrowsAsync<PvwaApiException>(() => client.GetSafesAsync());
    }
}
