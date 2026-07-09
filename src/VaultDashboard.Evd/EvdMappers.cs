using VaultDashboard.Core.Models;

namespace VaultDashboard.Evd;

/// <summary>
/// Maps parsed EVD report rows to Core models. Column names are looked up defensively (a couple of
/// historically-used spellings each) since EVD's exact header names have drifted slightly across
/// PAM Self-Hosted versions; see https://docs.cyberark.com/pam-self-hosted/latest/en/content/evd/.
/// </summary>
internal static class EvdMappers
{
    public static SafeInfo ToSafeInfo(this IReadOnlyDictionary<string, string> row) => new()
    {
        SafeName = row.GetOrEmpty("Safename", "SafeName"),
        Description = row.GetOrEmpty("Description"),
        ManagingCpm = row.GetOrEmpty("ManagingCPM", "CPM"),
        NumberOfVersionsRetention = row.GetInt("numVersionRetention", "NumOfVersionRetention"),
        NumberOfDaysRetention = row.GetInt("numDaysRetention", "NumOfDaysRetention"),
        OlacEnabled = row.GetBool("EnableOLAC", "OLACEnabled"),
        DataSource = "EVD",
    };

    public static UserInfo ToUserInfo(this IReadOnlyDictionary<string, string> row) => new()
    {
        Username = row.GetOrEmpty("Username", "User Name", "Name"),
        Location = row.GetOrEmpty("Location"),
        UserType = row.GetOrEmpty("UserType", "Type"),
        Enabled = !row.GetBool("Disabled"),
        Source = row.GetBool("LDAPUser", "IsLDAPUser") ? "LDAP" : "CyberArk",
        DataSource = "EVD",
    };

    public static GroupInfo ToGroupInfo(this IReadOnlyDictionary<string, string> row) => new()
    {
        GroupName = row.GetOrEmpty("Groupname", "GroupName", "Name"),
        Location = row.GetOrEmpty("Location"),
        GroupType = row.GetOrEmpty("GroupType", "Type"),
        DataSource = "EVD",
    };
}
