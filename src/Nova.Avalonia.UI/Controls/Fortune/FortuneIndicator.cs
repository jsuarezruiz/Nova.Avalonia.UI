using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// A visual indicator (pointer/arrow) for fortune controls.
/// </summary>
public class FortuneIndicator : Control
{
    /// <summary>
    /// Defines the <see cref="Fill"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> FillProperty =
        AvaloniaProperty.Register<FortuneIndicator, IBrush?>(nameof(Fill), Brushes.Red);

    /// <summary>
    /// Defines the <see cref="Stroke"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> StrokeProperty =
        AvaloniaProperty.Register<FortuneIndicator, IBrush?>(nameof(Stroke), Brushes.White);

    /// <summary>
    /// Defines the <see cref="StrokeThickness"/> property.
    /// </summary>
    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<FortuneIndicator, double>(nameof(StrokeThickness), 2.0);

    /// <summary>
    /// Defines the <see cref="IndicatorSize"/> property.
    /// </summary>
    public static readonly StyledProperty<double> IndicatorSizeProperty =
        AvaloniaProperty.Register<FortuneIndicator, double>(nameof(IndicatorSize), 24.0);

    /// <summary>
    /// Defines the <see cref="Position"/> property.
    /// </summary>
    public static readonly StyledProperty<IndicatorPosition> PositionProperty =
        AvaloniaProperty.Register<FortuneIndicator, IndicatorPosition>(nameof(Position), IndicatorPosition.Top);

    /// <summary>
    /// Gets or sets the fill brush for the indicator.
    /// </summary>
    public IBrush? Fill
    {
        get => GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    /// <summary>
    /// Gets or sets the stroke brush for the indicator.
    /// </summary>
    public IBrush? Stroke
    {
        get => GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    /// <summary>
    /// Gets or sets the stroke thickness for the indicator.
    /// </summary>
    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets the size of the indicator.
    /// </summary>
    public double IndicatorSize
    {
        get => GetValue(IndicatorSizeProperty);
        set => SetValue(IndicatorSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the position of the indicator relative to the fortune control.
    /// </summary>
    public IndicatorPosition Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    static FortuneIndicator()
    {
        AffectsRender<FortuneIndicator>(FillProperty, StrokeProperty, StrokeThicknessProperty, IndicatorSizeProperty, PositionProperty);
    }

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        var size = IndicatorSize;
        var geometry = new StreamGeometry();

        using (var ctx = geometry.Open())
        {
            switch (Position)
            {
                case IndicatorPosition.Top:
                    // Triangle pointing down
                    ctx.BeginFigure(new Point(size / 2, size), true);
                    ctx.LineTo(new Point(0, 0));
                    ctx.LineTo(new Point(size, 0));
                    ctx.EndFigure(true);
                    break;

                case IndicatorPosition.Bottom:
                    // Triangle pointing up
                    ctx.BeginFigure(new Point(size / 2, 0), true);
                    ctx.LineTo(new Point(0, size));
                    ctx.LineTo(new Point(size, size));
                    ctx.EndFigure(true);
                    break;

                case IndicatorPosition.Left:
                    // Triangle pointing right
                    ctx.BeginFigure(new Point(size, size / 2), true);
                    ctx.LineTo(new Point(0, 0));
                    ctx.LineTo(new Point(0, size));
                    ctx.EndFigure(true);
                    break;

                case IndicatorPosition.Right:
                    // Triangle pointing left
                    ctx.BeginFigure(new Point(0, size / 2), true);
                    ctx.LineTo(new Point(size, 0));
                    ctx.LineTo(new Point(size, size));
                    ctx.EndFigure(true);
                    break;
            }
        }

        var pen = Stroke != null ? new Pen(Stroke, StrokeThickness) : null;
        context.DrawGeometry(Fill, pen, geometry);
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        return new Size(IndicatorSize, IndicatorSize);
    }
}

/// <summary>
/// Specifies the position of a fortune indicator.
/// </summary>
public enum IndicatorPosition
{
    /// <summary>Indicator at the top, pointing down.</summary>
    Top,
    /// <summary>Indicator at the bottom, pointing up.</summary>
    Bottom,
    /// <summary>Indicator at the left, pointing right.</summary>
    Left,
    /// <summary>Indicator at the right, pointing left.</summary>
    Right
}
