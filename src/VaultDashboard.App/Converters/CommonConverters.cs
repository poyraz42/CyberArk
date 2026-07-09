using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VaultDashboard.App.Converters;

public sealed class NullToCollapsedConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is null ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class InverseBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool b && !b;

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool b && !b;
}

public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture) =>
        value is Visibility v && v == Visibility.Visible;
}

public sealed class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is int count && count > 0 ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>True/false -&gt; the status-good / status-critical brush, for compliance-style booleans.</summary>
public sealed class BoolToStatusBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isGood = value is true;
        var key = isGood ? "StatusGoodBrush" : "StatusCriticalBrush";
        return System.Windows.Application.Current.Resources[key];
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
