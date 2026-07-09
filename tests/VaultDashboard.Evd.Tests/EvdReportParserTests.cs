using Xunit;

namespace VaultDashboard.Evd.Tests;

public class EvdReportParserTests
{
    [Fact]
    public void Parse_TabSeparatedWithHeader_ReturnsOneRecordPerDataLine()
    {
        const string content = "Safename\tDescription\tManagingCPM\n" +
                                "Safe1\tFirst safe\tPasswordManager\n" +
                                "Safe2\t\tPasswordManager_DMZ\n";

        var records = EvdReportParser.Parse(content, '\t');

        Assert.Equal(2, records.Count);
        Assert.Equal("Safe1", records[0]["Safename"]);
        Assert.Equal("First safe", records[0]["Description"]);
        Assert.Equal("PasswordManager_DMZ", records[1]["ManagingCPM"]);
    }

    [Fact]
    public void Parse_WithQualifier_StripsQuoteCharacters()
    {
        const string content = "Name,Location\n\"John Doe\",\"\\Finance\"\n";

        var records = EvdReportParser.Parse(content, ',', '"');

        Assert.Equal("John Doe", records[0]["Name"]);
        Assert.Equal(@"\Finance", records[0]["Location"]);
    }

    [Fact]
    public void Parse_EmptyContent_ReturnsEmptyList()
    {
        Assert.Empty(EvdReportParser.Parse(string.Empty));
    }

    [Fact]
    public void ToSafeInfo_MapsKnownColumnsAndBooleans()
    {
        var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Safename"] = "Safe1",
            ["ManagingCPM"] = "PasswordManager",
            ["numVersionRetention"] = "7",
            ["EnableOLAC"] = "Yes",
        };

        var safe = row.ToSafeInfo();

        Assert.Equal("Safe1", safe.SafeName);
        Assert.Equal("PasswordManager", safe.ManagingCpm);
        Assert.Equal(7, safe.NumberOfVersionsRetention);
        Assert.True(safe.OlacEnabled);
        Assert.Equal("EVD", safe.DataSource);
    }

    [Fact]
    public void GetOrEmpty_FallsBackThroughCandidateKeys()
    {
        var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["Name"] = "value" };
        Assert.Equal("value", row.GetOrEmpty("Username", "User Name", "Name"));
        Assert.Equal(string.Empty, row.GetOrEmpty("DoesNotExist"));
    }
}
