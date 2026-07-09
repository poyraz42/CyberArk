namespace VaultDashboard.Core.Connection;

/// <summary>
/// Everything needed to reach a PVWA instance and authenticate against the REST API.
/// Corresponds to POST /PasswordVault/API/auth/{authType}/Logon.
/// </summary>
public sealed class PvwaConnectionProfile
{
    public string Name { get; set; } = "Default";

    /// <summary>Host name or FQDN of the PVWA, e.g. pvwa.corp.example.com (no scheme/path).</summary>
    public string Address { get; set; } = string.Empty;

    public PvwaAuthenticationType AuthenticationType { get; set; } = PvwaAuthenticationType.Cyberark;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    /// <summary>Allow more than one concurrent logon session for this user.</summary>
    public bool ConcurrentSession { get; set; } = true;

    /// <summary>Skip TLS certificate validation. Only ever use this for lab/dev PVWA instances.</summary>
    public bool AllowInsecureTls { get; set; }

    /// <summary>Request timeout applied to every REST call.</summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(100);

    /// <summary>Optional separate PTA session token, required for the Security/Risk endpoints.</summary>
    public string? PtaSessionToken { get; set; }

    public Uri BaseUri => new($"https://{Address.Trim().TrimEnd('/')}/PasswordVault/");
}
