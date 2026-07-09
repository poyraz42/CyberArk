namespace VaultDashboard.Evd;

/// <summary>
/// The report ("OutputName") types ExportVaultData can produce, per
/// https://docs.cyberark.com/pam-self-hosted/latest/en/content/evd/exporting-data-to-files.htm
/// The enum name matches the \OutputName= token used on the command line.
/// </summary>
public enum EvdReportType
{
    SafesList,
    UsersList,
    GroupsList,
    GroupMembersList,
    FilesList,
    OwnersList,
    LogList,
    LocationsList,
    ConfirmationsList,
    RequestsList,
}
