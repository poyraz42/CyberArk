using VaultDashboard.Pacli;
using Xunit;

namespace VaultDashboard.Pacli.Tests;

public class PacliRecordParserTests
{
    [Fact]
    public void Parse_TabDelimitedLines_MapsColumnsToRequestedFieldOrder()
    {
        var fields = new[] { "NAME", "LOCATION", "TYPE", "DISABLED" };
        var stdOut = "jsmith\t\\\tEPVUser\tno\nasvc\t\\Applications\tEPVUser\tyes\n";

        var records = PacliRecordParser.Parse(stdOut, fields);

        Assert.Equal(2, records.Count);
        Assert.Equal("jsmith", records[0]["NAME"]);
        Assert.Equal("EPVUser", records[0]["TYPE"]);
        Assert.Equal("asvc", records[1]["NAME"]);
    }

    [Fact]
    public void Parse_QuotedFields_StripsSurroundingQuotes()
    {
        var fields = new[] { "NAME" };
        var records = PacliRecordParser.Parse("\"jsmith\"\n", fields);

        Assert.Equal("jsmith", records[0]["NAME"]);
    }

    [Fact]
    public void Parse_MissingTrailingColumns_FillsEmptyString()
    {
        var fields = new[] { "NAME", "LOCATION", "TYPE" };
        var records = PacliRecordParser.Parse("jsmith\t\\\n", fields);

        Assert.Equal(string.Empty, records[0]["TYPE"]);
    }

    [Theory]
    [InlineData("yes", true)]
    [InlineData("YES", true)]
    [InlineData("1", true)]
    [InlineData("no", false)]
    [InlineData("", false)]
    public void TryGetBool_ParsesPacliYesNoConvention(string raw, bool expected)
    {
        var record = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["DISABLED"] = raw };
        Assert.Equal(expected, record.TryGetBool("DISABLED"));
    }

    [Fact]
    public void Parse_EmptyStdOut_ReturnsNoRecords()
    {
        Assert.Empty(PacliRecordParser.Parse(string.Empty, new[] { "NAME" }));
    }
}
