using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using VaultDashboard.App.Models;

namespace VaultDashboard.App.Services;

/// <summary>
/// Loads/saves connection profiles from %AppData%/CyberArkVaultDashboard/profiles.json.
/// Password fields are encrypted at rest with Windows DPAPI (current-user scope) so the JSON file
/// on disk never holds a plaintext PVWA/PACLI/EVD credential.
/// </summary>
public sealed class ProfileStore
{
    private static readonly string FolderPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CyberArkVaultDashboard");

    private static readonly string FilePath = Path.Combine(FolderPath, "profiles.json");

    private const string DpapiPrefix = "dpapi:";

    public async Task<StoredProfiles> LoadAsync(CancellationToken ct = default)
    {
        if (!File.Exists(FilePath))
        {
            return new StoredProfiles();
        }

        await using var stream = File.OpenRead(FilePath);
        var stored = await JsonSerializer.DeserializeAsync<StoredProfiles>(stream, cancellationToken: ct)
                     ?? new StoredProfiles();

        if (stored.Pvwa is not null)
        {
            stored.Pvwa.Password = Unprotect(stored.Pvwa.Password);
        }

        if (stored.Pacli is not null)
        {
            stored.Pacli.Password = Unprotect(stored.Pacli.Password);
        }

        return stored;
    }

    public async Task SaveAsync(StoredProfiles profiles, CancellationToken ct = default)
    {
        Directory.CreateDirectory(FolderPath);

        // Work on shallow copies so the caller's live, in-memory profile objects (e.g. the ones a
        // connected client is currently using) never get their plaintext password overwritten.
        var toSave = new StoredProfiles
        {
            Pvwa = profiles.Pvwa is null ? null : CloneWithProtectedPassword(profiles.Pvwa),
            Pacli = profiles.Pacli is null ? null : CloneWithProtectedPassword(profiles.Pacli),
            Evd = profiles.Evd,
        };

        await using var stream = File.Create(FilePath);
        await JsonSerializer.SerializeAsync(stream, toSave, new JsonSerializerOptions { WriteIndented = true }, ct);
    }

    private static Core.Connection.PvwaConnectionProfile CloneWithProtectedPassword(Core.Connection.PvwaConnectionProfile p) =>
        new()
        {
            Name = p.Name,
            Address = p.Address,
            AuthenticationType = p.AuthenticationType,
            Username = p.Username,
            Password = Protect(p.Password),
            ConcurrentSession = p.ConcurrentSession,
            AllowInsecureTls = p.AllowInsecureTls,
            RequestTimeout = p.RequestTimeout,
        };

    private static Core.Connection.PacliConnectionProfile CloneWithProtectedPassword(Core.Connection.PacliConnectionProfile p) =>
        new()
        {
            Name = p.Name,
            ExecutablePath = p.ExecutablePath,
            VaultName = p.VaultName,
            VaultAddress = p.VaultAddress,
            VaultPort = p.VaultPort,
            Username = p.Username,
            Password = Protect(p.Password),
            WorkingFolder = p.WorkingFolder,
            CommandTimeout = p.CommandTimeout,
        };

    private static string Protect(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            return string.Empty;
        }

        var encrypted = ProtectedData.Protect(Encoding.UTF8.GetBytes(plainText), optionalEntropy: null,
            DataProtectionScope.CurrentUser);
        return DpapiPrefix + Convert.ToBase64String(encrypted);
    }

    private static string Unprotect(string storedValue)
    {
        if (string.IsNullOrEmpty(storedValue) || !storedValue.StartsWith(DpapiPrefix, StringComparison.Ordinal))
        {
            return storedValue;
        }

        try
        {
            var bytes = Convert.FromBase64String(storedValue[DpapiPrefix.Length..]);
            var decrypted = ProtectedData.Unprotect(bytes, optionalEntropy: null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decrypted);
        }
        catch (CryptographicException)
        {
            // Profile file was copied from another machine/user profile - DPAPI can't decrypt it there.
            return string.Empty;
        }
    }
}
