using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Renders a dimmed overlay with a cutout for the highlighted element.
/// </summary>
public class ShowcaseOverlay : Control
{
    private Geometry? _cachedGeometry;

    /// <summary>
    /// Defines the <see cref="OverlayBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> OverlayBrushProperty =
        AvaloniaProperty.Register<ShowcaseOverlay, IBrush?>(
            nameof(OverlayBrush),
            new SolidColorBrush(Colors.Black, 0.7));

    /// <summary>
    /// Defines the <see cref="TargetBounds"/> property.
    /// </summary>
    public static readonly StyledProperty<Rect?> TargetBoundsProperty =
        AvaloniaProperty.Register<ShowcaseOverlay, Rect?>(nameof(TargetBounds));

    /// <summary>
    /// Defines the <see cref="HighlightPadding"/> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> HighlightPaddingProperty =
        AvaloniaProperty.Register<ShowcaseOverlay, Thickness>(
            nameof(HighlightPadding),
            new Thickness(8));

    /// <summary>
    /// Defines the <see cref="HighlightShape"/> property.
    /// </summary>
    public static readonly StyledProperty<ShowcaseHighlightShape> HighlightShapeProperty =
        AvaloniaProperty.Register<ShowcaseOverlay, ShowcaseHighlightShape>(
            nameof(HighlightShape),
            ShowcaseHighlightShape.RoundedRectangle);

    /// <summary>
    /// Defines the <see cref="HighlightCornerRadius"/> property.
    /// </summary>
    public static readonly StyledProperty<double> HighlightCornerRadiusProperty =
        AvaloniaProperty.Register<ShowcaseOverlay, double>(
            nameof(HighlightCornerRadius),
            8);

    static ShowcaseOverlay()
    {
        AffectsRender<ShowcaseOverlay>(
            OverlayBrushProperty,
            TargetBoundsProperty,
            HighlightPaddingProperty,
            HighlightShapeProperty,
            HighlightCornerRadiusProperty);

        TargetBoundsProperty.Changed.AddClassHandler<ShowcaseOverlay>((x, _) => x._cachedGeometry = null);
        HighlightPaddingProperty.Changed.AddClassHandler<ShowcaseOverlay>((x, _) => x._cachedGeometry = null);
        HighlightShapeProperty.Changed.AddClassHandler<ShowcaseOverlay>((x, _) => x._cachedGeometry = null);
        HighlightCornerRadiusProperty.Changed.AddClassHandler<ShowcaseOverlay>((x, _) => x._cachedGeometry = null);
    }

    /// <summary>
    /// Gets or sets the brush used for the overlay.
    /// </summary>
    public IBrush? OverlayBrush
    {
        get => GetValue(OverlayBrushProperty);
        set => SetValue(OverlayBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the bounds of the target element to highlight.
    /// </summary>
    public Rect? TargetBounds
    {
        get => GetValue(TargetBoundsProperty);
        set => SetValue(TargetBoundsProperty, value);
    }

    /// <summary>
    /// Gets or sets the padding around the highlighted element.
    /// </summary>
    public Thickness HighlightPadding
    {
        get => GetValue(HighlightPaddingProperty);
        set => SetValue(HighlightPaddingProperty, value);
    }

    /// <summary>
    /// Gets or sets the shape of the highlight cutout.
    /// </summary>
    public ShowcaseHighlightShape HighlightShape
    {
        get => GetValue(HighlightShapeProperty);
        set => SetValue(HighlightShapeProperty, value);
    }

    /// <summary>
    /// Gets or sets the corner radius for rounded rectangle highlights.
    /// </summary>
    public double HighlightCornerRadius
    {
        get => GetValue(HighlightCornerRadiusProperty);
        set => SetValue(HighlightCornerRadiusProperty, value);
    }

    /// <inheritdoc />
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        _cachedGeometry = null;
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        var bounds = Bounds;
        if (bounds.Width <= 0 || bounds.Height <= 0) return;

        if (_cachedGeometry == null)
        {
            _cachedGeometry = BuildGeometry(bounds);
        }

        context.DrawGeometry(OverlayBrush, null, _cachedGeometry);
    }

    private Geometry BuildGeometry(Rect bounds)
    {
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(new Point(0, 0), true);
            ctx.LineTo(new Point(bounds.Width, 0));
            ctx.LineTo(new Point(bounds.Width, bounds.Height));
            ctx.LineTo(new Point(0, bounds.Height));
            ctx.EndFigure(true);

            if (TargetBounds.HasValue)
            {
                var highlightBounds = TargetBounds.Value.Inflate(HighlightPadding);
                DrawHighlightHole(ctx, highlightBounds);
            }
        }

        return geometry;
    }

    private void DrawHighlightHole(StreamGeometryContext ctx, Rect bounds)
    {
        switch (HighlightShape)
        {
            case ShowcaseHighlightShape.Rectangle:
                DrawRectangleHole(ctx, bounds);
                break;
            case ShowcaseHighlightShape.RoundedRectangle:
                DrawRoundedRectangleHole(ctx, bounds, HighlightCornerRadius);
                break;
            case ShowcaseHighlightShape.Circle:
                DrawCircleHole(ctx, bounds);
                break;
        }
    }

    private void DrawRectangleHole(StreamGeometryContext ctx, Rect bounds)
    {
        ctx.BeginFigure(bounds.TopLeft, true);
        ctx.LineTo(bounds.BottomLeft);
        ctx.LineTo(bounds.BottomRight);
        ctx.LineTo(bounds.TopRight);
        ctx.EndFigure(true);
    }

    private void DrawRoundedRectangleHole(StreamGeometryContext ctx, Rect bounds, double radius)
    {
        radius = Math.Min(radius, Math.Min(bounds.Width / 2, bounds.Height / 2));
        var arcSize = new Size(radius, radius);

        ctx.BeginFigure(new Point(bounds.Left, bounds.Top + radius), true);
        ctx.ArcTo(new Point(bounds.Left + radius, bounds.Top), arcSize, 0, false, SweepDirection.Clockwise);
        ctx.LineTo(new Point(bounds.Right - radius, bounds.Top));
        ctx.ArcTo(new Point(bounds.Right, bounds.Top + radius), arcSize, 0, false, SweepDirection.Clockwise);
        ctx.LineTo(new Point(bounds.Right, bounds.Bottom - radius));
        ctx.ArcTo(new Point(bounds.Right - radius, bounds.Bottom), arcSize, 0, false, SweepDirection.Clockwise);
        ctx.LineTo(new Point(bounds.Left + radius, bounds.Bottom));
        ctx.ArcTo(new Point(bounds.Left, bounds.Bottom - radius), arcSize, 0, false, SweepDirection.Clockwise);
        ctx.EndFigure(true);
    }

    private void DrawCircleHole(StreamGeometryContext ctx, Rect bounds)
    {
        var center = bounds.Center;
        var radius = Math.Max(bounds.Width, bounds.Height) / 2;
        var arcSize = new Size(radius, radius);

        ctx.BeginFigure(new Point(center.X, center.Y - radius), true);
        ctx.ArcTo(new Point(center.X, center.Y + radius), arcSize, 0, true, SweepDirection.CounterClockwise);
        ctx.ArcTo(new Point(center.X, center.Y - radius), arcSize, 0, true, SweepDirection.CounterClockwise);
        ctx.EndFigure(true);
    }
}
