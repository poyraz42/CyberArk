namespace VaultDashboard.Core.Connection;

/// <summary>
/// Parameters required to drive the PACLI (Privileged Account CLI) executable against a Vault.
/// See https://docs.cyberark.com/pam-self-hosted/latest/en/content/pacli/introduction.htm
/// </summary>
public sealed class PacliConnectionProfile
{
    public string Name { get; set; } = "Default";

    /// <summary>Full path to Pacli.exe on the machine running the dashboard.</summary>
    public string ExecutablePath { get; set; } = @"C:\PACLI\Pacli.exe";

    /// <summary>Logical vault name used in DEFINE VAULT=... and every subsequent command.</summary>
    public string VaultName { get; set; } = "Vault";

    /// <summary>Vault server host name or IP used in DEFINE ADDRESS=...</summary>
    public string VaultAddress { get; set; } = string.Empty;

    public int VaultPort { get; set; } = 1858;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    /// <summary>Working folder PACLI uses for its local session/control files (CTLFILENAME).</summary>
    public string? WorkingFolder { get; set; }

    public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromSeconds(60);
}
