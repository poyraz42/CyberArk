namespace VaultDashboard.Core.Models;

public enum VaultDataSourceKind
{
    Pvwa,
    Pacli,
    Evd,
}

/// <summary>
/// Per-endpoint fetch outcome. The dashboard shows a status row per call so a partial failure
/// (missing license, insufficient permissions, endpoint disabled) never hides the rest of the data.
/// </summary>
public sealed class DataSourceStatus
{
    public required VaultDataSourceKind Source { get; init; }
    public required string Endpoint { get; init; }
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public int ItemCount { get; init; }
    public DateTimeOffset FetchedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    public TimeSpan Duration { get; init; }

    public static DataSourceStatus Ok(VaultDataSourceKind source, string endpoint, int itemCount, TimeSpan duration) =>
        new() { Source = source, Endpoint = endpoint, Success = true, ItemCount = itemCount, Duration = duration };

    public static DataSourceStatus Fail(VaultDataSourceKind source, string endpoint, string error, TimeSpan duration) =>
        new() { Source = source, Endpoint = endpoint, Success = false, ErrorMessage = error, Duration = duration };
}
