namespace VaultDashboard.Evd;

/// <summary>
/// EVD text reports always start with a header line naming each column, followed by one data line per record,
/// all using the configured \Separator (and, if \UseQualifier is set, values may be wrapped in \Qualifier chars).
/// Parsing off the header - rather than a hardcoded column order - keeps this working across EVD/PAM versions
/// that add or reorder columns.
/// </summary>
public static class EvdReportParser
{
    public static IReadOnlyList<IReadOnlyDictionary<string, string>> ParseFile(
        string filePath, char separator = '\t', char? qualifier = null)
    {
        if (!File.Exists(filePath))
        {
            return Array.Empty<IReadOnlyDictionary<string, string>>();
        }

        using var reader = new StreamReader(filePath);
        return Parse(reader.ReadToEnd(), separator, qualifier);
    }

    public static IReadOnlyList<IReadOnlyDictionary<string, string>> Parse(
        string content, char separator = '\t', char? qualifier = null)
    {
        var lines = content.Split('\n')
            .Select(l => l.TrimEnd('\r'))
            .Where(l => l.Length > 0)
            .ToList();

        if (lines.Count == 0)
        {
            return Array.Empty<IReadOnlyDictionary<string, string>>();
        }

        var headers = SplitLine(lines[0], separator, qualifier);
        var records = new List<IReadOnlyDictionary<string, string>>(lines.Count - 1);

        for (var i = 1; i < lines.Count; i++)
        {
            var columns = SplitLine(lines[i], separator, qualifier);
            var record = new Dictionary<string, string>(headers.Count, StringComparer.OrdinalIgnoreCase);
            for (var c = 0; c < headers.Count; c++)
            {
                record[headers[c]] = c < columns.Count ? columns[c] : string.Empty;
            }

            records.Add(record);
        }

        return records;
    }

    private static List<string> SplitLine(string line, char separator, char? qualifier)
    {
        var fields = line.Split(separator).ToList();
        if (qualifier is not { } q)
        {
            return fields;
        }

        for (var i = 0; i < fields.Count; i++)
        {
            var f = fields[i];
            if (f.Length >= 2 && f[0] == q && f[^1] == q)
            {
                fields[i] = f[1..^1];
            }
        }

        return fields;
    }

    public static string GetOrEmpty(this IReadOnlyDictionary<string, string> record, params string[] candidateKeys)
    {
        foreach (var key in candidateKeys)
        {
            if (record.TryGetValue(key, out var value))
            {
                return value;
            }
        }

        return string.Empty;
    }

    public static bool GetBool(this IReadOnlyDictionary<string, string> record, params string[] candidateKeys)
    {
        var raw = record.GetOrEmpty(candidateKeys);
        return raw.Equals("yes", StringComparison.OrdinalIgnoreCase) || raw == "1" ||
               raw.Equals("true", StringComparison.OrdinalIgnoreCase);
    }

    public static int GetInt(this IReadOnlyDictionary<string, string> record, params string[] candidateKeys)
        => int.TryParse(record.GetOrEmpty(candidateKeys), out var value) ? value : 0;
}
