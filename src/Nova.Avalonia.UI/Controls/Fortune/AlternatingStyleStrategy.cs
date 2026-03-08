using Avalonia;
using Avalonia.Media;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// A style strategy that alternates between two colors.
/// </summary>
public class AlternatingStyleStrategy : AvaloniaObject, IStyleStrategy
{
    /// <summary>
    /// Defines the <see cref="PrimaryBackground"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush> PrimaryBackgroundProperty =
        AvaloniaProperty.Register<AlternatingStyleStrategy, IBrush>(nameof(PrimaryBackground), new SolidColorBrush(Color.Parse("#FF6B35")));

    /// <summary>
    /// Defines the <see cref="SecondaryBackground"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush> SecondaryBackgroundProperty =
        AvaloniaProperty.Register<AlternatingStyleStrategy, IBrush>(nameof(SecondaryBackground), new SolidColorBrush(Color.Parse("#F7931E")));

    /// <summary>
    /// Defines the <see cref="BorderBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush> BorderBrushProperty =
        AvaloniaProperty.Register<AlternatingStyleStrategy, IBrush>(nameof(BorderBrush), Brushes.White);

    /// <summary>
    /// Defines the <see cref="BorderThickness"/> property.
    /// </summary>
    public static readonly StyledProperty<double> BorderThicknessProperty =
        AvaloniaProperty.Register<AlternatingStyleStrategy, double>(nameof(BorderThickness), 2.0);

    /// <summary>
    /// Defines the <see cref="Foreground"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush> ForegroundProperty =
        AvaloniaProperty.Register<AlternatingStyleStrategy, IBrush>(nameof(Foreground), Brushes.White);

    /// <summary>
    /// Gets or sets the background brush for even-indexed items.
    /// </summary>
    public IBrush PrimaryBackground
    {
        get => GetValue(PrimaryBackgroundProperty);
        set => SetValue(PrimaryBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the background brush for odd-indexed items.
    /// </summary>
    public IBrush SecondaryBackground
    {
        get => GetValue(SecondaryBackgroundProperty);
        set => SetValue(SecondaryBackgroundProperty, value);
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

        return new FortuneItemStyle
        {
            Background = index % 2 == 0 ? PrimaryBackground : SecondaryBackground,
            BorderBrush = BorderBrush,
            BorderThickness = BorderThickness,
            Foreground = Foreground
        };
    }
}
