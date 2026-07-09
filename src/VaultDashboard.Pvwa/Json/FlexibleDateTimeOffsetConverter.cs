using System.Text.Json;
using System.Text.Json.Serialization;

namespace VaultDashboard.Pvwa.Json;

/// <summary>
/// PVWA is inconsistent about how it serializes timestamps across endpoints/versions: some return
/// Unix epoch seconds as a number, some as a numeric string, some as ISO-8601. This converter accepts all three
/// so a single DTO shape keeps working regardless of which endpoint/version produced it.
/// </summary>
public sealed class FlexibleDateTimeOffsetConverter : JsonConverter<DateTimeOffset?>
{
    public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return null;
            case JsonTokenType.Number when reader.TryGetInt64(out var epochSeconds):
                return epochSeconds <= 0 ? null : DateTimeOffset.FromUnixTimeSeconds(epochSeconds);
            case JsonTokenType.String:
                {
                    var raw = reader.GetString();
                    if (string.IsNullOrWhiteSpace(raw))
                    {
                        return null;
                    }

                    if (long.TryParse(raw, out var epochFromString))
                    {
                        return epochFromString <= 0 ? null : DateTimeOffset.FromUnixTimeSeconds(epochFromString);
                    }

                    return DateTimeOffset.TryParse(raw, out var parsed) ? parsed : null;
                }
            default:
                return null;
        }
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteNumberValue(value.Value.ToUnixTimeSeconds());
        }
    }
}
