namespace VaultDashboard.Pacli;

/// <summary>
/// Parses the tab-delimited stdout PACLI list commands (USERSLIST, SAFESLIST, FINDFILES, OWNERSLIST) produce
/// when given an explicit output(field1,field2,...) clause: one line per record, one column per requested field,
/// in the same order they were requested.
/// </summary>
internal static class PacliRecordParser
{
    public static IReadOnlyList<IReadOnlyDictionary<string, string>> Parse(string stdOut, IReadOnlyList<string> fields)
    {
        var records = new List<IReadOnlyDictionary<string, string>>();

        foreach (var rawLine in stdOut.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var line = rawLine.TrimEnd('\r');
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var columns = line.Split('\t');
            var record = new Dictionary<string, string>(fields.Count, StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < fields.Count; i++)
            {
                var value = i < columns.Length ? columns[i].Trim('"') : string.Empty;
                record[fields[i]] = value;
            }

            records.Add(record);
        }

        return records;
    }

    public static bool TryGetBool(this IReadOnlyDictionary<string, string> record, string key)
        => record.TryGetValue(key, out var value) &&
           (string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase) || value == "1");

    public static string GetOrEmpty(this IReadOnlyDictionary<string, string> record, string key)
        => record.TryGetValue(key, out var value) ? value : string.Empty;

    public static int? TryGetInt(this IReadOnlyDictionary<string, string> record, string key)
        => record.TryGetValue(key, out var value) && int.TryParse(value, out var parsed) ? parsed : null;
}
