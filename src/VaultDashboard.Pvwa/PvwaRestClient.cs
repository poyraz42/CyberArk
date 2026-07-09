using System.Net.Http.Json;
using System.Text.Json;
using VaultDashboard.Core.Connection;
using VaultDashboard.Core.Exceptions;
using VaultDashboard.Pvwa.Json;

namespace VaultDashboard.Pvwa;

/// <summary>
/// Thin, purpose-built client for the PVWA (PAM Self-Hosted) REST API.
/// Endpoint inventory and payload shapes follow docs.cyberark.com/pam-self-hosted/latest/en/content/webservices/.
/// One client instance == one authenticated session; call <see cref="LogonAsync"/> before anything else
/// and dispose the client (which calls Logoff) when done.
/// </summary>
public sealed partial class PvwaRestClient : IAsyncDisposable
{
    private readonly HttpClient _http;
    private readonly PvwaConnectionProfile _profile;
    private string? _sessionToken;
    private readonly bool _ownsHttpClient;

    public PvwaRestClient(PvwaConnectionProfile profile, HttpClient? httpClient = null)
    {
        _profile = profile ?? throw new ArgumentNullException(nameof(profile));
        _ownsHttpClient = httpClient is null;
        _http = httpClient ?? CreateHttpClient(profile);
        _http.BaseAddress = profile.BaseUri;
        _http.Timeout = profile.RequestTimeout;
    }

    public bool IsAuthenticated => _sessionToken is not null;

    private static HttpClient CreateHttpClient(PvwaConnectionProfile profile)
    {
        var handler = new HttpClientHandler();
        if (profile.AllowInsecureTls)
        {
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        }

        return new HttpClient(handler);
    }

    /// <summary>
    /// POST /PasswordVault/API/auth/{authType}/Logon
    /// https://docs.cyberark.com/pam-self-hosted/latest/en/content/webservices/logon.htm
    /// </summary>
    public async Task LogonAsync(CancellationToken ct = default)
    {
        var authTypeSegment = _profile.AuthenticationType switch
        {
            PvwaAuthenticationType.Cyberark => "Cyberark",
            PvwaAuthenticationType.Ldap => "LDAP",
            PvwaAuthenticationType.Radius => "RADIUS",
            PvwaAuthenticationType.Windows => "Windows",
            PvwaAuthenticationType.Saml => "SAML",
            PvwaAuthenticationType.Pta => "PTA",
            _ => "Cyberark",
        };

        var body = new
        {
            username = _profile.Username,
            password = _profile.Password,
            concurrentSession = _profile.ConcurrentSession,
        };

        using var response = await _http.PostAsJsonAsync($"API/auth/{authTypeSegment}/Logon", body, ct)
            .ConfigureAwait(false);
        var raw = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw BuildApiException(response.StatusCode, raw);
        }

        // The Logon endpoint returns the token as a bare JSON string, e.g. "\"7f3a...\"".
        var token = TryDeserialize<string>(raw) ?? raw.Trim('"');
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new PvwaApiException("PVWA Logon succeeded but returned an empty session token.");
        }

        _sessionToken = token;

        // CyberArk's documented convention is to send the raw token in the Authorization header
        // (no "Bearer " scheme prefix).
        _http.DefaultRequestHeaders.Remove("Authorization");
        _http.DefaultRequestHeaders.Add("Authorization", _sessionToken);
    }

    /// <summary>
    /// POST /PasswordVault/API/Auth/Logoff/
    /// https://docs.cyberark.com/pam-self-hosted/latest/en/content/webservices/logoff.htm
    /// </summary>
    public async Task LogoffAsync(CancellationToken ct = default)
    {
        if (!IsAuthenticated)
        {
            return;
        }

        try
        {
            using var response = await _http.PostAsync("API/Auth/Logoff/", content: null, ct).ConfigureAwait(false);
        }
        catch
        {
            // Best-effort: a failed logoff should never block app shutdown or profile switching.
        }
        finally
        {
            _sessionToken = null;
            _http.DefaultRequestHeaders.Remove("Authorization");
        }
    }

    private void EnsureAuthenticated()
    {
        if (!IsAuthenticated)
        {
            throw new PvwaApiException("Not authenticated. Call LogonAsync() first.");
        }
    }

    private async Task<T> GetJsonAsync<T>(string relativeUrl, CancellationToken ct)
    {
        EnsureAuthenticated();
        using var response = await _http.GetAsync(relativeUrl, ct).ConfigureAwait(false);
        var raw = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw BuildApiException(response.StatusCode, raw);
        }

        return JsonSerializer.Deserialize<T>(raw, PvwaJsonDefaults.Options)
               ?? throw new PvwaApiException($"Empty/unparsable response body from {relativeUrl}.");
    }

    private async Task<byte[]> GetBytesAsync(string relativeUrl, string acceptContentType, CancellationToken ct)
    {
        EnsureAuthenticated();
        using var request = new HttpRequestMessage(HttpMethod.Get, relativeUrl);
        request.Headers.TryAddWithoutValidation("Content-Type", acceptContentType);
        using var response = await _http.SendAsync(request, ct).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var raw = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            throw BuildApiException(response.StatusCode, raw);
        }

        return await response.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(false);
    }

    private static T? TryDeserialize<T>(string raw)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(raw, PvwaJsonDefaults.Options);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    private static PvwaApiException BuildApiException(System.Net.HttpStatusCode statusCode, string raw)
    {
        var errorCode = TryDeserialize<PvwaErrorDto>(raw)?.ErrorCode;
        var errorMessage = TryDeserialize<PvwaErrorDto>(raw)?.ErrorMessage;
        var message = errorMessage is not null
            ? $"PVWA API returned {(int)statusCode} {statusCode} ({errorCode}): {errorMessage}"
            : $"PVWA API returned {(int)statusCode} {statusCode}: {raw}";
        return new PvwaApiException(message, (int)statusCode, errorCode);
    }

    public async ValueTask DisposeAsync()
    {
        await LogoffAsync().ConfigureAwait(false);
        if (_ownsHttpClient)
        {
            _http.Dispose();
        }
    }

    private sealed class PvwaErrorDto
    {
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
