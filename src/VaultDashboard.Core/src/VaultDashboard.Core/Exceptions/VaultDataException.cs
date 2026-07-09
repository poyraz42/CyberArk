namespace VaultDashboard.Core.Exceptions;

/// <summary>Base type for failures raised by the Pvwa/Pacli/Evd data-access layers.</summary>
public class VaultDataException : Exception
{
    public VaultDataException(string message) : base(message)
    {
    }

    public VaultDataException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>Raised when a PVWA REST call fails, carrying the HTTP status and CyberArk error code when available.</summary>
public sealed class PvwaApiException : VaultDataException
{
    public int? HttpStatusCode { get; }
    public string? CyberArkErrorCode { get; }

    public PvwaApiException(string message, int? httpStatusCode = null, string? cyberArkErrorCode = null)
        : base(message)
    {
        HttpStatusCode = httpStatusCode;
        CyberArkErrorCode = cyberArkErrorCode;
    }
}

/// <summary>Raised when a PACLI command exits with a non-zero return code.</summary>
public sealed class PacliCommandException : VaultDataException
{
    public int ExitCode { get; }
    public string Command { get; }

    public PacliCommandException(string command, int exitCode, string message)
        : base(message)
    {
        Command = command;
        ExitCode = exitCode;
    }
}

/// <summary>Raised when ExportVaultData fails or an expected output report file is missing/malformed.</summary>
public sealed class EvdExportException : VaultDataException
{
    public EvdExportException(string message) : base(message)
    {
    }

    public EvdExportException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
