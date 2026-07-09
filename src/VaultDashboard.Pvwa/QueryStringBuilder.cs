using System.Text;

namespace VaultDashboard.Pvwa;

internal sealed class QueryStringBuilder
{
    private readonly List<(string Key, string Value)> _parameters = new();

    public QueryStringBuilder Add(string key, string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _parameters.Add((key, value));
        }

        return this;
    }

    public QueryStringBuilder Add(string key, int? value)
    {
        if (value.HasValue)
        {
            _parameters.Add((key, value.Value.ToString()));
        }

        return this;
    }

    public QueryStringBuilder Add(string key, bool? value)
    {
        if (value.HasValue)
        {
            _parameters.Add((key, value.Value ? "true" : "false"));
        }

        return this;
    }

    public override string ToString()
    {
        if (_parameters.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder("?");
        for (var i = 0; i < _parameters.Count; i++)
        {
            if (i > 0)
            {
                sb.Append('&');
            }

            sb.Append(Uri.EscapeDataString(_parameters[i].Key));
            sb.Append('=');
            sb.Append(Uri.EscapeDataString(_parameters[i].Value));
        }

        return sb.ToString();
    }
}
