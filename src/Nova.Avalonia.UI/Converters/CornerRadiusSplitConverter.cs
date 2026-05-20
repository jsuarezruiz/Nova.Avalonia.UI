using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Nova.Avalonia.UI.Converters;

/// <summary>
/// Converts a <see cref="CornerRadius"/> value by selectively applying corners based on a parameter string.
/// </summary>
/// <remarks>
/// <para>
/// This converter is used to split a corner radius value into individual corners,
/// typically for controls that need different rounded corners on different parts
/// (e.g., a shield badge with rounded left corners on one border and rounded right corners on another).
/// </para>
/// <para>
/// The converter parameter should be a comma-separated string of four values (1 or 0),
/// representing: TopLeft, TopRight, BottomRight, BottomLeft.
/// A value of 1 means the corner radius is applied; 0 means the corner is set to 0.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;!-- Apply only left corners: TopLeft and BottomLeft --&gt;
/// &lt;Border CornerRadius="{Binding CornerRadius,
///     Converter={StaticResource CornerRadiusSplitConverter},
///     ConverterParameter='1,0,0,1'}" /&gt;
///
/// &lt;!-- Apply only right corners: TopRight and BottomRight --&gt;
/// &lt;Border CornerRadius="{Binding CornerRadius,
///     Converter={StaticResource CornerRadiusSplitConverter},
///     ConverterParameter='0,1,1,0'}" /&gt;
/// </code>
/// </example>
public class CornerRadiusSplitConverter : IValueConverter
{
    /// <summary>
    /// Converts a <see cref="CornerRadius"/> value by selectively zeroing out corners based on the parameter.
    /// </summary>
    /// <param name="value">The source <see cref="CornerRadius"/> value.</param>
    /// <param name="targetType">The target type (should be <see cref="CornerRadius"/>).</param>
    /// <param name="parameter">
    /// A comma-separated string of four 1/0 values: "TopLeft,TopRight,BottomRight,BottomLeft".
    /// Example: "1,0,0,1" applies only the left corners.
    /// </param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>
    /// A new <see cref="CornerRadius"/> with the selected corners applied and others set to 0.
    /// Returns <see cref="CornerRadius"/> with all zeros if the conversion fails.
    /// </returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is CornerRadius cornerRadius && parameter is string param)
        {
            var parts = param.Split(',');
            if (parts.Length == 4 &&
                int.TryParse(parts[0], out int tl) &&
                int.TryParse(parts[1], out int tr) &&
                int.TryParse(parts[2], out int br) &&
                int.TryParse(parts[3], out int bl))
            {
                return new CornerRadius(
                    tl == 1 ? cornerRadius.TopLeft : 0,
                    tr == 1 ? cornerRadius.TopRight : 0,
                    br == 1 ? cornerRadius.BottomRight : 0,
                    bl == 1 ? cornerRadius.BottomLeft : 0);
            }
        }
        return new CornerRadius(0);
    }

    /// <summary>
    /// Not implemented. This converter does not support two-way binding.
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown as this converter is one-way only.</exception>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException($"{nameof(CornerRadiusSplitConverter)} does not support ConvertBack.");
    }
}
