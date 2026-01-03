using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// A control that renders repeating text or image watermarks as a tiled overlay.
/// </summary>
public class Watermark : ContentControl
{
    /// <summary>
    /// Defines the text to display as the watermark.
    /// </summary>
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<Watermark, string?>(nameof(Text));

    /// <summary>
    /// Defines the image source for image-based watermarks.
    /// </summary>
    public static readonly StyledProperty<IImage?> SourceProperty =
        AvaloniaProperty.Register<Watermark, IImage?>(nameof(Source));

    /// <summary>
    /// Defines the rotation angle of the watermark pattern in degrees.
    /// </summary>
    public static readonly StyledProperty<double> AngleProperty =
        AvaloniaProperty.Register<Watermark, double>(nameof(Angle), defaultValue: -30.0);

    /// <summary>
    /// Defines the horizontal spacing between watermark tiles.
    /// </summary>
    public static readonly StyledProperty<double> HorizontalSpacingProperty =
        AvaloniaProperty.Register<Watermark, double>(nameof(HorizontalSpacing), defaultValue: 50.0);

    /// <summary>
    /// Defines the vertical spacing between watermark tiles.
    /// </summary>
    public static readonly StyledProperty<double> VerticalSpacingProperty =
        AvaloniaProperty.Register<Watermark, double>(nameof(VerticalSpacing), defaultValue: 30.0);

    /// <summary>
    /// Defines the opacity of the watermark overlay (0.0-1.0).
    /// </summary>
    public static readonly StyledProperty<double> WatermarkOpacityProperty =
        AvaloniaProperty.Register<Watermark, double>(nameof(WatermarkOpacity), defaultValue: 0.15, coerce: CoerceOpacity);

    /// <summary>
    /// Defines the font size for text watermarks.
    /// </summary>
    public static readonly StyledProperty<double> WatermarkFontSizeProperty =
        AvaloniaProperty.Register<Watermark, double>(nameof(WatermarkFontSize), defaultValue: 14.0);

    /// <summary>
    /// Defines the font family for text watermarks.
    /// </summary>
    public static readonly StyledProperty<FontFamily> WatermarkFontFamilyProperty =
        AvaloniaProperty.Register<Watermark, FontFamily>(nameof(WatermarkFontFamily), defaultValue: FontFamily.Default);

    /// <summary>
    /// Defines the foreground brush for text watermarks.
    /// </summary>
    public static readonly StyledProperty<IBrush?> WatermarkForegroundProperty =
        AvaloniaProperty.Register<Watermark, IBrush?>(nameof(WatermarkForeground));

    /// <summary>
    /// Defines the flow direction for text watermarks (LeftToRight or RightToLeft).
    /// </summary>
    public static readonly StyledProperty<FlowDirection> WatermarkFlowDirectionProperty =
        AvaloniaProperty.Register<Watermark, FlowDirection>(nameof(WatermarkFlowDirection), defaultValue: FlowDirection.LeftToRight);

    private FormattedText? _cachedFormattedText;

    /// <inheritdoc cref="TextProperty"/>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <inheritdoc cref="SourceProperty"/>
    public IImage? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <inheritdoc cref="AngleProperty"/>
    public double Angle
    {
        get => GetValue(AngleProperty);
        set => SetValue(AngleProperty, value);
    }

    /// <inheritdoc cref="HorizontalSpacingProperty"/>
    public double HorizontalSpacing
    {
        get => GetValue(HorizontalSpacingProperty);
        set => SetValue(HorizontalSpacingProperty, value);
    }

    /// <inheritdoc cref="VerticalSpacingProperty"/>
    public double VerticalSpacing
    {
        get => GetValue(VerticalSpacingProperty);
        set => SetValue(VerticalSpacingProperty, value);
    }

    /// <inheritdoc cref="WatermarkOpacityProperty"/>
    public double WatermarkOpacity
    {
        get => GetValue(WatermarkOpacityProperty);
        set => SetValue(WatermarkOpacityProperty, value);
    }

    /// <inheritdoc cref="WatermarkFontSizeProperty"/>
    public double WatermarkFontSize
    {
        get => GetValue(WatermarkFontSizeProperty);
        set => SetValue(WatermarkFontSizeProperty, value);
    }

    /// <inheritdoc cref="WatermarkFontFamilyProperty"/>
    public FontFamily WatermarkFontFamily
    {
        get => GetValue(WatermarkFontFamilyProperty);
        set => SetValue(WatermarkFontFamilyProperty, value);
    }

    /// <inheritdoc cref="WatermarkForegroundProperty"/>
    public IBrush? WatermarkForeground
    {
        get => GetValue(WatermarkForegroundProperty);
        set => SetValue(WatermarkForegroundProperty, value);
    }

    /// <inheritdoc cref="WatermarkFlowDirectionProperty"/>
    public FlowDirection WatermarkFlowDirection
    {
        get => GetValue(WatermarkFlowDirectionProperty);
        set => SetValue(WatermarkFlowDirectionProperty, value);
    }

    static Watermark()
    {
        AffectsRender<Watermark>(
            TextProperty,
            SourceProperty,
            AngleProperty,
            HorizontalSpacingProperty,
            VerticalSpacingProperty,
            WatermarkOpacityProperty,
            WatermarkFontSizeProperty,
            WatermarkFontFamilyProperty,
            WatermarkForegroundProperty,
            WatermarkFlowDirectionProperty
        );
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TextProperty ||
            change.Property == WatermarkFontSizeProperty ||
            change.Property == WatermarkFontFamilyProperty ||
            change.Property == WatermarkFlowDirectionProperty)
        {
            _cachedFormattedText = null;
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = Bounds;
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return;

        Size tileSize;
        bool isImage = Source != null;

        if (isImage)
        {
            tileSize = new Size(Source!.Size.Width + HorizontalSpacing, Source.Size.Height + VerticalSpacing);
        }
        else if (!string.IsNullOrEmpty(Text))
        {
            EnsureFormattedText();
            if (_cachedFormattedText == null) return;
            tileSize = new Size(_cachedFormattedText.Width + HorizontalSpacing, _cachedFormattedText.Height + VerticalSpacing);
        }
        else
        {
            return;
        }

        if (tileSize.Width <= 0 || tileSize.Height <= 0)
            return;

        using (context.PushOpacity(WatermarkOpacity))
        {
            double angleRad = Angle * Math.PI / 180.0;
            double cos = Math.Abs(Math.Cos(angleRad));
            double sin = Math.Abs(Math.Sin(angleRad));

            double expandedWidth = bounds.Width * cos + bounds.Height * sin;
            double expandedHeight = bounds.Width * sin + bounds.Height * cos;

            double diagonal = Math.Sqrt(bounds.Width * bounds.Width + bounds.Height * bounds.Height);
            expandedWidth = Math.Max(expandedWidth, diagonal);
            expandedHeight = Math.Max(expandedHeight, diagonal);

            double centerX = bounds.Width / 2;
            double centerY = bounds.Height / 2;

            using (context.PushTransform(
                Matrix.CreateTranslation(-centerX, -centerY) *
                Matrix.CreateRotation(angleRad) *
                Matrix.CreateTranslation(centerX, centerY)))
            {
                double startX = centerX - expandedWidth / 2;
                double startY = centerY - expandedHeight / 2;

                int cols = (int)Math.Ceiling(expandedWidth / tileSize.Width) + 2;
                int rows = (int)Math.Ceiling(expandedHeight / tileSize.Height) + 2;

                for (int row = 0; row < rows; row++)
                {
                    for (int col = 0; col < cols; col++)
                    {
                        double x = startX + col * tileSize.Width;
                        double y = startY + row * tileSize.Height;

                        if (isImage && Source != null)
                        {
                            var destRect = new Rect(x, y, Source.Size.Width, Source.Size.Height);
                            context.DrawImage(Source, destRect);
                        }
                        else if (_cachedFormattedText != null)
                        {
                            context.DrawText(_cachedFormattedText, new Point(x, y));
                        }
                    }
                }
            }
        }
    }

    private void EnsureFormattedText()
    {
        if (_cachedFormattedText != null || string.IsNullOrEmpty(Text))
            return;

        var foreground = WatermarkForeground ?? Brushes.Gray;

        _cachedFormattedText = new FormattedText(
            Text,
            System.Globalization.CultureInfo.CurrentCulture,
            WatermarkFlowDirection,
            new Typeface(WatermarkFontFamily),
            WatermarkFontSize,
            foreground);
    }

    private static double CoerceOpacity(AvaloniaObject sender, double value)
    {
        return Math.Clamp(value, 0.0, 1.0);
    }
}
