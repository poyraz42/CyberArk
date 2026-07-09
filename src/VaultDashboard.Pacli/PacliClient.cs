using VaultDashboard.Core.Connection;
using VaultDashboard.Core.Exceptions;
using VaultDashboard.Core.Models;

namespace VaultDashboard.Pacli;

/// <summary>
/// Drives Pacli.exe through the INIT -&gt; DEFINE -&gt; LOGON -&gt; (list commands) -&gt; LOGOFF -&gt; TERM lifecycle.
/// Command syntax follows the PACLI Command Reference:
/// https://docs.cyberark.com/pam-self-hosted/latest/en/content/pacli/introduction.htm
/// Field names passed to output(...) are exactly the tokens documented for each command; if your PACLI
/// build supports additional/renamed fields, extend the *Fields arrays below rather than the parsing logic.
/// </summary>
public sealed class PacliClient : IAsyncDisposable
{
    private static readonly IReadOnlyList<string> UserFields = new[] { "NAME", "LOCATION", "TYPE", "DISABLED", "LDAPUSER", "USERID" };
    private static readonly IReadOnlyList<string> SafeFields = new[] { "NAME", "LOCATION", "SAFEID", "MAXSIZE", "ACCESSLEVEL" };
    private static readonly IReadOnlyList<string> OwnerFields = new[] { "NAME", "GROUP", "SAFENAME", "ACCESSLEVEL" };

    private readonly PacliConnectionProfile _profile;
    private readonly PacliProcessRunner _runner;
    private bool _initialized;
    private bool _loggedOn;

    public PacliClient(PacliConnectionProfile profile)
    {
        _profile = profile ?? throw new ArgumentNullException(nameof(profile));
        _runner = new PacliProcessRunner(profile.ExecutablePath, profile.CommandTimeout);
    }

    /// <summary>INIT then DEFINE VAULT=... ADDRESS=.... Must be called once before LogonAsync.</summary>
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        var initArgs = new List<string>();
        if (!string.IsNullOrEmpty(_profile.WorkingFolder))
        {
            initArgs.Add($"CTLFILENAME={Path.Combine(_profile.WorkingFolder, "pacli.ctl")}");
        }

        await ExecuteAsync("INIT", initArgs, ct).ConfigureAwait(false);
        _initialized = true;

        await ExecuteAsync("DEFINE", new[]
        {
            $"VAULT={_profile.VaultName}",
            $"ADDRESS={_profile.VaultAddress}",
            $"PORT={_profile.VaultPort}",
        }, ct).ConfigureAwait(false);
    }

    /// <summary>LOGON VAULT=... USER=... PASSWORD=....</summary>
    public async Task LogonAsync(CancellationToken ct = default)
    {
        if (!_initialized)
        {
            await InitializeAsync(ct).ConfigureAwait(false);
        }

        await ExecuteAsync("LOGON", new[]
        {
            $"VAULT={_profile.VaultName}",
            $"USER={_profile.Username}",
            $"PASSWORD={_profile.Password}",
            "FAILIFCONNECTED=NO",
        }, ct).ConfigureAwait(false);

        _loggedOn = true;
    }

    /// <summary>USERSLIST VAULT=... USER=... output(NAME,LOCATION,TYPE,DISABLED,LDAPUSER,USERID).</summary>
    public async Task<IReadOnlyList<UserInfo>> ListUsersAsync(CancellationToken ct = default)
    {
        EnsureLoggedOn();
        var stdOut = await ExecuteAsync("USERSLIST", new[]
        {
            $"VAULT={_profile.VaultName}",
            $"USER={_profile.Username}",
            "INCLUDEDISABLEDUSERS=YES",
            $"output({string.Join(',', UserFields)})",
        }, ct).ConfigureAwait(false);

        return PacliRecordParser.Parse(stdOut, UserFields).Select(r => new UserInfo
        {
            Id = r.GetOrEmpty("USERID"),
            Username = r.GetOrEmpty("NAME"),
            Location = r.GetOrEmpty("LOCATION"),
            UserType = r.GetOrEmpty("TYPE"),
            Enabled = !r.TryGetBool("DISABLED"),
            Source = r.TryGetBool("LDAPUSER") ? "LDAP" : "CyberArk",
            DataSource = "PACLI",
        }).ToList();
    }

    /// <summary>SAFESLIST VAULT=... USER=... output(NAME,LOCATION,SAFEID,MAXSIZE,ACCESSLEVEL).</summary>
    public async Task<IReadOnlyList<SafeInfo>> ListSafesAsync(CancellationToken ct = default)
    {
        EnsureLoggedOn();
        var stdOut = await ExecuteAsync("SAFESLIST", new[]
        {
            $"VAULT={_profile.VaultName}",
            $"USER={_profile.Username}",
            "INCLUDESUBLOCATIONS=YES",
            $"output({string.Join(',', SafeFields)})",
        }, ct).ConfigureAwait(false);

        return PacliRecordParser.Parse(stdOut, SafeFields).Select(r => new SafeInfo
        {
            SafeName = r.GetOrEmpty("NAME"),
            SafeNumber = r.GetOrEmpty("SAFEID"),
            Location = r.GetOrEmpty("LOCATION"),
            DataSource = "PACLI",
        }).ToList();
    }

    /// <summary>
    /// OWNERSLIST VAULT=... USER=... SAFEPATTERN=... OWNERPATTERN=* output(NAME,GROUP,SAFENAME,ACCESSLEVEL).
    /// Returns the safe-owner (permission) matrix for every safe matching <paramref name="safePattern"/>.
    /// </summary>
    public async Task<IReadOnlyList<(string Owner, string? Group, string SafeName, string? AccessLevel)>> ListOwnersAsync(
        string safePattern = "*", CancellationToken ct = default)
    {
        EnsureLoggedOn();
        var stdOut = await ExecuteAsync("OWNERSLIST", new[]
        {
            $"VAULT={_profile.VaultName}",
            $"USER={_profile.Username}",
            $"SAFEPATTERN={safePattern}",
            "OWNERPATTERN=*",
            $"output({string.Join(',', OwnerFields)})",
        }, ct).ConfigureAwait(false);

        return PacliRecordParser.Parse(stdOut, OwnerFields)
            .Select(r => (r.GetOrEmpty("NAME"), (string?)r.GetOrEmpty("GROUP"), r.GetOrEmpty("SAFENAME"), (string?)r.GetOrEmpty("ACCESSLEVEL")))
            .ToList();
    }

    /// <summary>
    /// FINDFILES VAULT=... USER=... SAFE=... FOLDER=Root FILEPATTERN=*. Returns file names found in the safe;
    /// used for the workspace/object-count panels rather than full metadata (FINDFILES' output(...) tokens are
    /// version-dependent — adjust the parsing below if your PACLI build emits additional columns).
    /// </summary>
    public async Task<IReadOnlyList<string>> FindFilesAsync(
        string safeName, string folder = "Root", CancellationToken ct = default)
    {
        EnsureLoggedOn();
        var stdOut = await ExecuteAsync("FINDFILES", new[]
        {
            $"VAULT={_profile.VaultName}",
            $"USER={_profile.Username}",
            $"SAFE={safeName}",
            $"FOLDER={folder}",
            "FILEPATTERN=*",
            "INCLUDESUBFOLDERS=YES",
        }, ct).ConfigureAwait(false);

        return stdOut.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Split('\t')[0].Trim('\r', '"'))
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToList();
    }

    /// <summary>LOGOFF VAULT=... USER=....</summary>
    public async Task LogoffAsync(CancellationToken ct = default)
    {
        if (!_loggedOn)
        {
            return;
        }

        try
        {
            await ExecuteAsync("LOGOFF", new[] { $"VAULT={_profile.VaultName}", $"USER={_profile.Username}" }, ct)
                .ConfigureAwait(false);
        }
        finally
        {
            _loggedOn = false;
        }
    }

    /// <summary>TERM — ends the local PACLI session. Always call this last.</summary>
    public async Task TerminateAsync(CancellationToken ct = default)
    {
        if (!_initialized)
        {
            return;
        }

        try
        {
            await ExecuteAsync("TERM", Array.Empty<string>(), ct).ConfigureAwait(false);
        }
        finally
        {
            _initialized = false;
        }
    }

    private void EnsureLoggedOn()
    {
        if (!_loggedOn)
        {
            throw new PacliCommandException("(session)", -1, "Not logged on. Call LogonAsync() first.");
        }
    }

    private async Task<string> ExecuteAsync(string command, IEnumerable<string> parameters, CancellationToken ct)
    {
        var result = await _runner.RunAsync(command, parameters, ct).ConfigureAwait(false);
        if (result.ExitCode != 0)
        {
            throw new PacliCommandException(command, result.ExitCode,
                $"PACLI command '{command}' exited with code {result.ExitCode}: {result.StandardError.Trim()}");
        }

        return result.StandardOutput;
    }

    public async ValueTask DisposeAsync()
    {
        await LogoffAsync().ConfigureAwait(false);
        await TerminateAsync().ConfigureAwait(false);
    }
}
