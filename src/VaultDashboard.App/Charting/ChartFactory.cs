using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace VaultDashboard.App.Charting;

/// <summary>Small helpers so every page builds LiveCharts2 series the same way, with the same palette.</summary>
public static class ChartFactory
{
    public static ISeries ColumnSeries(string name, IEnumerable<double> values, SKColor color) =>
        new ColumnSeries<double>
        {
            Name = name,
            Values = values.ToArray(),
            Fill = new SolidColorPaint(color),
            Stroke = null,
            MaxBarWidth = 32,
            Rx = 4,
            Ry = 4,
        };

    /// <summary>One column series per category, each colored from the fixed categorical order.</summary>
    public static IReadOnlyList<ISeries> CategoricalColumns(IReadOnlyList<(string Label, double Value)> items)
    {
        var series = new List<ISeries>(items.Count);
        for (var i = 0; i < items.Count; i++)
        {
            series.Add(new ColumnSeries<double>
            {
                Name = items[i].Label,
                Values = new[] { items[i].Value },
                Fill = new SolidColorPaint(ChartPalette.ForIndex(i)),
                Stroke = null,
                MaxBarWidth = 48,
                Rx = 4,
                Ry = 4,
            });
        }

        return series;
    }

    public static IReadOnlyList<ISeries> PieSlices(IReadOnlyList<(string Label, double Value)> items)
    {
        var series = new List<ISeries>(items.Count);
        for (var i = 0; i < items.Count; i++)
        {
            series.Add(new PieSeries<double>
            {
                Name = items[i].Label,
                Values = new[] { items[i].Value },
                Fill = new SolidColorPaint(ChartPalette.ForIndex(i)),
                InnerRadius = 45,
                DataLabelsPaint = new SolidColorPaint(ChartPalette.SecondaryInk),
            });
        }

        return series;
    }

    public static Axis CategoryAxis(IEnumerable<string> labels) => new()
    {
        Labels = labels.ToArray(),
        LabelsPaint = new SolidColorPaint(ChartPalette.MutedInk),
        SeparatorsPaint = null,
        TextSize = 12,
    };

    public static Axis ValueAxis(string? name = null) => new()
    {
        Name = name,
        LabelsPaint = new SolidColorPaint(ChartPalette.MutedInk),
        SeparatorsPaint = new SolidColorPaint(ChartPalette.Gridline) { StrokeThickness = 1 },
        TextSize = 12,
        MinLimit = 0,
    };
}
