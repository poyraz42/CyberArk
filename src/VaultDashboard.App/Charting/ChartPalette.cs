using SkiaSharp;

namespace VaultDashboard.App.Charting;

/// <summary>
/// The dark-surface slice of the validated reference palette used across every chart in the app.
/// Categorical colors are a FIXED order (never cycled/reassigned when a filter changes the item
/// count) and status colors are reserved for compliance/health state - never reused as series color.
/// </summary>
public static class ChartPalette
{
    public static readonly SKColor Series1 = SKColor.Parse("#3987E5"); // blue
    public static readonly SKColor Series2 = SKColor.Parse("#199E70"); // aqua
    public static readonly SKColor Series3 = SKColor.Parse("#C98500"); // yellow
    public static readonly SKColor Series4 = SKColor.Parse("#008300"); // green
    public static readonly SKColor Series5 = SKColor.Parse("#9085E9"); // violet
    public static readonly SKColor Series6 = SKColor.Parse("#E66767"); // red
    public static readonly SKColor Series7 = SKColor.Parse("#D55181"); // magenta
    public static readonly SKColor Series8 = SKColor.Parse("#D95926"); // orange

    public static readonly IReadOnlyList<SKColor> Categorical = new[]
    {
        Series1, Series2, Series3, Series4, Series5, Series6, Series7, Series8,
    };

    public static readonly SKColor StatusGood = SKColor.Parse("#0CA30C");
    public static readonly SKColor StatusWarning = SKColor.Parse("#FAB219");
    public static readonly SKColor StatusSerious = SKColor.Parse("#EC835A");
    public static readonly SKColor StatusCritical = SKColor.Parse("#D03B3B");

    /// <summary>Single-hue sequential ramp (blue, light to dark) for magnitude-only encodings.</summary>
    public static readonly IReadOnlyList<SKColor> SequentialBlue = new[]
    {
        SKColor.Parse("#86B6EF"),
        SKColor.Parse("#5598E7"),
        SKColor.Parse("#3987E5"),
        SKColor.Parse("#256ABF"),
        SKColor.Parse("#1C5CAB"),
        SKColor.Parse("#184F95"),
    };

    public static readonly SKColor MutedInk = SKColor.Parse("#898781");
    public static readonly SKColor SecondaryInk = SKColor.Parse("#C3C2B7");
    public static readonly SKColor Gridline = SKColor.Parse("#2C2C2A");

    /// <summary>Returns the Nth fixed categorical color, wrapping into "Other" gray past the 8th slot.</summary>
    public static SKColor ForIndex(int index) =>
        index >= 0 && index < Categorical.Count ? Categorical[index] : MutedInk;
}
