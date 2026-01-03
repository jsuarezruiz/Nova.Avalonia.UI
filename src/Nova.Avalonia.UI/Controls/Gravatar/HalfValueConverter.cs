using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Converts a value to half its value (for circular corner radius).
/// </summary>
public class HalfValueConverter : IValueConverter
{
    /// <summary>
    /// Singleton instance of the converter.
    /// </summary>
    public static readonly HalfValueConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d)
            return new CornerRadius(d / 2);
        return new CornerRadius(0);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
