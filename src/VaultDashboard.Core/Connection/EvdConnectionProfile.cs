namespace VaultDashboard.Core.Connection;

/// <summary>
/// Parameters required to drive the ExportVaultData (EVD) utility.
/// See https://docs.cyberark.com/pam-self-hosted/latest/en/content/evd/exporting-data-to-files.htm
/// </summary>
public sealed class EvdConnectionProfile
{
    public string Name { get; set; } = "Default";

    /// <summary>Full path to ExportVaultData.exe.</summary>
    public string ExecutablePath { get; set; } = @"C:\ExportVaultData\ExportVaultData.exe";

    /// <summary>Path to the Vault.ini used by EVD (\VaultFile=).</summary>
    public string VaultFilePath { get; set; } = @"C:\ExportVaultData\Vault.ini";

    /// <summary>Path to the auditor .cred file created with CreateCredFile (\CredFile=).</summary>
    public string CredFilePath { get; set; } = @"C:\ExportVaultData\auditor.cred";

    /// <summary>Folder where the dashboard asks EVD to drop its report files, and where it reads them back from.</summary>
    public string OutputFolder { get; set; } = @"C:\ExportVaultData\Output";

    /// <summary>Field separator EVD should use (\Separator=). Tab by default.</summary>
    public char Separator { get; set; } = '\t';

    public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromMinutes(10);
}
