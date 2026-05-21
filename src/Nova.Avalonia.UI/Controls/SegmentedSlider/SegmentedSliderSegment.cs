using Avalonia;
using Avalonia.Media;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Defines an optional segment for a <see cref="SegmentedSlider"/>, including title, proportional width, and per-segment brushes.
/// </summary>
public class SegmentedSliderSegment : AvaloniaObject
{
    /// <summary>
    /// Defines the <see cref="Title"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<SegmentedSliderSegment, string?>(nameof(Title));

    /// <summary>
    /// Defines the <see cref="WidthRatio"/> property.
    /// </summary>
    public static readonly StyledProperty<double> WidthRatioProperty =
        AvaloniaProperty.Register<SegmentedSliderSegment, double>(
            nameof(WidthRatio),
            1.0,
            coerce: static (_, value) => SegmentedSlider.CoerceSegmentRatio(value));

    /// <summary>
    /// Defines the <see cref="FillBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> FillBrushProperty =
        AvaloniaProperty.Register<SegmentedSliderSegment, IBrush?>(nameof(FillBrush));

    /// <summary>
    /// Defines the <see cref="TrackBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> TrackBrushProperty =
        AvaloniaProperty.Register<SegmentedSliderSegment, IBrush?>(nameof(TrackBrush));

    /// <summary>
    /// Gets or sets the title displayed for the segment.
    /// </summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the proportional segment width.
    /// </summary>
    public double WidthRatio
    {
        get => GetValue(WidthRatioProperty);
        set => SetValue(WidthRatioProperty, value);
    }

    /// <summary>
    /// Gets or sets the filled portion brush for this segment.
    /// </summary>
    public IBrush? FillBrush
    {
        get => GetValue(FillBrushProperty);
        set => SetValue(FillBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the unfilled track brush for this segment.
    /// </summary>
    public IBrush? TrackBrush
    {
        get => GetValue(TrackBrushProperty);
        set => SetValue(TrackBrushProperty, value);
    }
}
