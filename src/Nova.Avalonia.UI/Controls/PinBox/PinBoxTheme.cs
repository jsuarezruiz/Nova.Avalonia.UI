using Avalonia;
using Avalonia.Media;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Defines the visual theme for a PinBox item.
/// </summary>
public class PinBoxTheme : AvaloniaObject
{
    /// <summary>
    /// Defines the <see cref="Width"/> property.
    /// </summary>
    public static readonly StyledProperty<double> WidthProperty =
        AvaloniaProperty.Register<PinBoxTheme, double>(nameof(Width), 56);

    /// <summary>
    /// Gets or sets the width of the pin box item.
    /// </summary>
    public double Width
    {
        get => GetValue(WidthProperty);
        set => SetValue(WidthProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="Height"/> property.
    /// </summary>
    public static readonly StyledProperty<double> HeightProperty =
        AvaloniaProperty.Register<PinBoxTheme, double>(nameof(Height), 60);

    /// <summary>
    /// Gets or sets the height of the pin box item.
    /// </summary>
    public double Height
    {
        get => GetValue(HeightProperty);
        set => SetValue(HeightProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="Background"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        AvaloniaProperty.Register<PinBoxTheme, IBrush?>(nameof(Background));

    /// <summary>
    /// Gets or sets the background brush.
    /// </summary>
    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="BorderBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> BorderBrushProperty =
        AvaloniaProperty.Register<PinBoxTheme, IBrush?>(nameof(BorderBrush));

    /// <summary>
    /// Gets or sets the border brush.
    /// </summary>
    public IBrush? BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="BorderThickness"/> property.
    /// </summary>
    public static readonly StyledProperty<double> BorderThicknessProperty =
        AvaloniaProperty.Register<PinBoxTheme, double>(nameof(BorderThickness), 2);

    /// <summary>
    /// Gets or sets the border thickness.
    /// </summary>
    public double BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="CornerRadius"/> property.
    /// </summary>
    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<PinBoxTheme, CornerRadius>(nameof(CornerRadius), new CornerRadius(8));

    /// <summary>
    /// Gets or sets the corner radius.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="FontSize"/> property.
    /// </summary>
    public static readonly StyledProperty<double> FontSizeProperty =
        AvaloniaProperty.Register<PinBoxTheme, double>(nameof(FontSize), 24);

    /// <summary>
    /// Gets or sets the font size for the character.
    /// </summary>
    public double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="FontWeight"/> property.
    /// </summary>
    public static readonly StyledProperty<FontWeight> FontWeightProperty =
        AvaloniaProperty.Register<PinBoxTheme, FontWeight>(nameof(FontWeight), FontWeight.SemiBold);

    /// <summary>
    /// Gets or sets the font weight for the character.
    /// </summary>
    public FontWeight FontWeight
    {
        get => GetValue(FontWeightProperty);
        set => SetValue(FontWeightProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="Foreground"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        AvaloniaProperty.Register<PinBoxTheme, IBrush?>(nameof(Foreground));

    /// <summary>
    /// Gets or sets the foreground brush for the character.
    /// </summary>
    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="BoxShadow"/> property.
    /// </summary>
    public static readonly StyledProperty<BoxShadows> BoxShadowProperty =
        AvaloniaProperty.Register<PinBoxTheme, BoxShadows>(nameof(BoxShadow));

    /// <summary>
    /// Gets or sets the box shadow.
    /// </summary>
    public BoxShadows BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="IsUnderline"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsUnderlineProperty =
        AvaloniaProperty.Register<PinBoxTheme, bool>(nameof(IsUnderline), false);

    /// <summary>
    /// Gets or sets whether to draw only a bottom underline instead of a full border.
    /// </summary>
    public bool IsUnderline
    {
        get => GetValue(IsUnderlineProperty);
        set => SetValue(IsUnderlineProperty, value);
    }

    /// <summary>
    /// Gets the default theme configuration.
    /// </summary>
    public static PinBoxTheme Default => new()
    {
        Width = 56,
        Height = 60,
        Background = new SolidColorBrush(Colors.White),
        BorderBrush = new SolidColorBrush(Color.Parse("#EAEFF3")),
        BorderThickness = 2,
        CornerRadius = new CornerRadius(8),
        FontSize = 24,
        FontWeight = FontWeight.SemiBold,
        Foreground = new SolidColorBrush(Color.Parse("#1E3C57"))
    };
}
