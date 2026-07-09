namespace VaultDashboard.Core.Connection;

/// <summary>
/// Authentication methods exposed by the PVWA "Logon" endpoint.
/// See https://docs.cyberark.com/pam-self-hosted/latest/en/content/webservices/logon.htm
/// </summary>
public enum PvwaAuthenticationType
{
    Cyberark,
    Ldap,
    Radius,
    Windows,
    Saml,
    Pta,
}
