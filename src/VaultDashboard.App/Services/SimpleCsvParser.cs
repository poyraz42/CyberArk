namespace VaultDashboard.App.Services;

/// <summary>Minimal header+rows CSV reader (comma-separated, optional double-quote qualifier) for classic PVWA report downloads.</summary>
internal static class SimpleCsvParser
{
    public static IReadOnlyList<IReadOnlyDictionary<string, string>> Parse(string csv)
    {
        var lines = csv.Split('\n').Select(l => l.TrimEnd('\r')).Where(l => l.Length > 0).ToList();
        if (lines.Count == 0)
        {
            return Array.Empty<IReadOnlyDictionary<string, string>>();
        }

        var headers = SplitCsvLine(lines[0]);
        var records = new List<IReadOnlyDictionary<string, string>>();
        for (var i = 1; i < lines.Count; i++)
        {
            var columns = SplitCsvLine(lines[i]);
            var record = new Dictionary<string, string>(headers.Count, StringComparer.OrdinalIgnoreCase);
            for (var c = 0; c < headers.Count; c++)
            {
                record[headers[c]] = c < columns.Count ? columns[c] : string.Empty;
            }

            records.Add(record);
        }

        return records;
    }

    private static List<string> SplitCsvLine(string line)
    {
        var fields = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;

        foreach (var c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        fields.Add(current.ToString());
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

    public static int GetInt(this IReadOnlyDictionary<string, string> record, params string[] candidateKeys) =>
        int.TryParse(record.GetOrEmpty(candidateKeys), out var value) ? value : 0;
}
