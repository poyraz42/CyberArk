using System.Diagnostics;
using System.Text;
using VaultDashboard.Core.Exceptions;

namespace VaultDashboard.Pacli;

/// <summary>Result of a single Pacli.exe invocation.</summary>
internal sealed record PacliProcessResult(int ExitCode, string StandardOutput, string StandardError);

/// <summary>
/// Low-level process launcher for Pacli.exe. Every PACLI command (INIT, DEFINE, LOGON, USERSLIST, ...) is a
/// separate process invocation; PACLI keeps session state locally between invocations for a given VAULT/USER
/// until LOGOFF/TERM is issued, so callers only need to serialize calls, not keep a process alive.
/// </summary>
internal sealed class PacliProcessRunner
{
    private readonly string _executablePath;
    private readonly TimeSpan _timeout;

    public PacliProcessRunner(string executablePath, TimeSpan timeout)
    {
        _executablePath = executablePath;
        _timeout = timeout;
    }

    /// <summary>
    /// Runs "Pacli.exe &lt;command&gt; KEY=value ...". Arguments are passed via <see cref="ProcessStartInfo.ArgumentList"/>
    /// so values containing spaces (safe names, descriptions, ...) never need manual quoting/escaping.
    /// </summary>
    public async Task<PacliProcessResult> RunAsync(string command, IEnumerable<string> parameters, CancellationToken ct)
    {
        if (!File.Exists(_executablePath))
        {
            throw new PacliCommandException(command, -1,
                $"Pacli.exe was not found at '{_executablePath}'. Check the PACLI connection profile.");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = _executablePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        startInfo.ArgumentList.Add(command);
        foreach (var parameter in parameters)
        {
            startInfo.ArgumentList.Add(parameter);
        }

        using var process = new Process { StartInfo = startInfo };
        var stdOut = new StringBuilder();
        var stdErr = new StringBuilder();

        process.OutputDataReceived += (_, e) => { if (e.Data is not null) stdOut.AppendLine(e.Data); };
        process.ErrorDataReceived += (_, e) => { if (e.Data is not null) stdErr.AppendLine(e.Data); };

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(_timeout);

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync(timeoutCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            TryKill(process);
            throw new PacliCommandException(command, -1,
                $"PACLI command '{command}' timed out after {_timeout.TotalSeconds:0}s.");
        }
        catch (OperationCanceledException)
        {
            TryKill(process);
            throw;
        }

        return new PacliProcessResult(process.ExitCode, stdOut.ToString(), stdErr.ToString());
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch
        {
            // best effort
        }
    }
}
