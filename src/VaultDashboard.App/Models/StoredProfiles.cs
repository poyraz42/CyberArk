using VaultDashboard.Core.Connection;

namespace VaultDashboard.App.Models;

/// <summary>
/// Everything persisted to disk (%AppData%/CyberArkVaultDashboard/profiles.json). Password fields are
/// DPAPI-protected (current Windows user scope) by <see cref="Services.ProfileStore"/> before this
/// object is serialized, and unprotected right after it is deserialized - the JSON on disk never
/// contains a plaintext secret.
/// </summary>
public sealed class StoredProfiles
{
    public PvwaConnectionProfile? Pvwa { get; set; }
    public PacliConnectionProfile? Pacli { get; set; }
    public EvdConnectionProfile? Evd { get; set; }
}
