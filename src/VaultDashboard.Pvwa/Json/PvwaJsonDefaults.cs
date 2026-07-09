using System.Text.Json;

namespace VaultDashboard.Pvwa.Json;

internal static class PvwaJsonDefaults
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new FlexibleDateTimeOffsetConverter() },
    };
}
