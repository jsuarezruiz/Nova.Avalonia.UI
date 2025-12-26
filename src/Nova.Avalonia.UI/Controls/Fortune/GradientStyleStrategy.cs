using Avalonia;
using Avalonia.Media;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// A style strategy that creates a smooth color gradient across items.
/// </summary>
public class GradientStyleStrategy : AvaloniaObject, IStyleStrategy
{
    /// <summary>
    /// Defines the <see cref="StartColor"/> property.
    /// </summary>
    public static readonly StyledProperty<Color> StartColorProperty =
        AvaloniaProperty.Register<GradientStyleStrategy, Color>(nameof(StartColor), Colors.Purple);

    /// <summary>
    /// Defines the <see cref="EndColor"/> property.
    /// </summary>
    public static readonly StyledProperty<Color> EndColorProperty =
        AvaloniaProperty.Register<GradientStyleStrategy, Color>(nameof(EndColor), Colors.Orange);

    /// <summary>
    /// Defines the <see cref="BorderBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush> BorderBrushProperty =
        AvaloniaProperty.Register<GradientStyleStrategy, IBrush>(nameof(BorderBrush), Brushes.White);

    /// <summary>
    /// Defines the <see cref="BorderThickness"/> property.
    /// </summary>
    public static readonly StyledProperty<double> BorderThicknessProperty =
        AvaloniaProperty.Register<GradientStyleStrategy, double>(nameof(BorderThickness), 2.0);

    /// <summary>
    /// Defines the <see cref="Foreground"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush> ForegroundProperty =
        AvaloniaProperty.Register<GradientStyleStrategy, IBrush>(nameof(Foreground), Brushes.White);

    /// <summary>
    /// Gets or sets the starting color of the gradient.
    /// </summary>
    public Color StartColor
    {
        get => GetValue(StartColorProperty);
        set => SetValue(StartColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the ending color of the gradient.
    /// </summary>
    public Color EndColor
    {
        get => GetValue(EndColorProperty);
        set => SetValue(EndColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the border brush for all items.
    /// </summary>
    public IBrush BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the border thickness for all items.
    /// </summary>
    public double BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets the foreground brush for all items.
    /// </summary>
    public IBrush Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    /// <inheritdoc/>
    public FortuneItemStyle GetStyle(int index, int totalCount, FortuneItemStyle? itemStyle)
    {
        if (itemStyle != null)
            return itemStyle;

        var ratio = totalCount > 1 ? (double)index / (totalCount - 1) : 0;
        var r = (byte)(StartColor.R + (EndColor.R - StartColor.R) * ratio);
        var g = (byte)(StartColor.G + (EndColor.G - StartColor.G) * ratio);
        var b = (byte)(StartColor.B + (EndColor.B - StartColor.B) * ratio);

        return new FortuneItemStyle
        {
            Background = new SolidColorBrush(Color.FromRgb(r, g, b)),
            BorderBrush = BorderBrush,
            BorderThickness = BorderThickness,
            Foreground = Foreground
        };
    }
}
