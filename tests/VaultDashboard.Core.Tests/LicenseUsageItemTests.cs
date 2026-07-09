using VaultDashboard.Core.Models;
using Xunit;

namespace VaultDashboard.Core.Tests;

public class LicenseUsageItemTests
{
    [Theory]
    [InlineData(8, 15, 53.3)]
    [InlineData(0, 10, 0)]
    [InlineData(10, 10, 100)]
    public void UtilizationPercent_ComputesRoundedPercentage(int used, int max, double expected)
    {
        var item = new LicenseUsageItem { Used = used, Maximum = max };
        Assert.Equal(expected, item.UtilizationPercent);
    }

    [Fact]
    public void UtilizationPercent_ZeroMaximum_ReturnsZeroInsteadOfDividingByZero()
    {
        var item = new LicenseUsageItem { Used = 5, Maximum = 0 };
        Assert.Equal(0, item.UtilizationPercent);
    }
}
