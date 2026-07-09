using System.Diagnostics;
using System.Text;
using VaultDashboard.Core.Connection;
using VaultDashboard.Core.Exceptions;

namespace VaultDashboard.Evd;

/// <summary>
/// Invokes ExportVaultData.exe to export one or more reports to files in a single pass.
/// Command syntax: https://docs.cyberark.com/pam-self-hosted/latest/en/content/evd/exporting-data-to-files.htm
/// </summary>
public sealed class EvdExportRunner
{
    private readonly EvdConnectionProfile _profile;

    public EvdExportRunner(EvdConnectionProfile profile)
    {
        _profile = profile ?? throw new ArgumentNullException(nameof(profile));
    }

    /// <summary>
    /// Runs ExportVaultData once, requesting every report in <paramref name="reports"/>, and returns the
    /// full path written for each. Throws <see cref="EvdExportException"/> on a non-zero exit code.
    /// </summary>
    public async Task<IReadOnlyDictionary<EvdReportType, string>> ExportAsync(
        IEnumerable<EvdReportType> reports, CancellationToken ct = default)
    {
        if (!File.Exists(_profile.ExecutablePath))
        {
            throw new EvdExportException(
                $"ExportVaultData.exe was not found at '{_profile.ExecutablePath}'. Check the EVD connection profile.");
        }

        Directory.CreateDirectory(_profile.OutputFolder);
        var logFilePath = Path.Combine(_profile.OutputFolder, "evd.log");
        var outputPaths = new Dictionary<EvdReportType, string>();

        var startInfo = new ProcessStartInfo
        {
            FileName = _profile.ExecutablePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        startInfo.ArgumentList.Add($"\\VaultFile={_profile.VaultFilePath}");
        startInfo.ArgumentList.Add($"\\CredFile={_profile.CredFilePath}");
        startInfo.ArgumentList.Add($"\\LogFile={logFilePath}");
        startInfo.ArgumentList.Add("\\Target=File");
        startInfo.ArgumentList.Add($"\\Separator={_profile.Separator}");

        foreach (var report in reports.Distinct())
        {
            var fileName = Path.Combine(_profile.OutputFolder, $"{report}.txt");
            startInfo.ArgumentList.Add($"\\{report}={fileName}");
            outputPaths[report] = fileName;
        }

        using var process = new Process { StartInfo = startInfo };
        var stdOut = new StringBuilder();
        var stdErr = new StringBuilder();
        process.OutputDataReceived += (_, e) => { if (e.Data is not null) stdOut.AppendLine(e.Data); };
        process.ErrorDataReceived += (_, e) => { if (e.Data is not null) stdErr.AppendLine(e.Data); };

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(_profile.CommandTimeout);

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            await process.WaitForExitAsync(timeoutCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            TryKill(process);
            throw new EvdExportException(
                $"ExportVaultData timed out after {_profile.CommandTimeout.TotalMinutes:0} minute(s).");
        }

        if (process.ExitCode != 0)
        {
            throw new EvdExportException(
                $"ExportVaultData exited with code {process.ExitCode}: {stdErr} {stdOut}".Trim());
        }

        return outputPaths;
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
