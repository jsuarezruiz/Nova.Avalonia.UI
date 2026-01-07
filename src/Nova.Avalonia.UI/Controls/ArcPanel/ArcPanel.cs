using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// A panel that arranges child elements along an arc (partial circle).
/// </summary>
public class ArcPanel : Panel
{
    /// <summary>
    /// Defines the <see cref="Radius"/> property.
    /// </summary>
    public static readonly StyledProperty<double> RadiusProperty =
        AvaloniaProperty.Register<ArcPanel, double>(nameof(Radius), 100);

    /// <summary>
    /// Defines the <see cref="StartAngle"/> property.
    /// </summary>
    public static readonly StyledProperty<double> StartAngleProperty =
        AvaloniaProperty.Register<ArcPanel, double>(nameof(StartAngle), 0);

    /// <summary>
    /// Defines the <see cref="SweepAngle"/> property.
    /// </summary>
    public static readonly StyledProperty<double> SweepAngleProperty =
        AvaloniaProperty.Register<ArcPanel, double>(nameof(SweepAngle), 180);

    /// <summary>
    /// Defines the <see cref="DistributeEvenly"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> DistributeEvenlyProperty =
        AvaloniaProperty.Register<ArcPanel, bool>(nameof(DistributeEvenly), true);

    /// <summary>
    /// Gets or sets the radius of the arc.
    /// </summary>
    public double Radius
    {
        get => GetValue(RadiusProperty);
        set => SetValue(RadiusProperty, value);
    }

    /// <summary>
    /// Gets or sets the starting angle in degrees (-90 = top).
    /// </summary>
    public double StartAngle
    {
        get => GetValue(StartAngleProperty);
        set => SetValue(StartAngleProperty, value);
    }

    /// <summary>
    /// Gets or sets the sweep angle in degrees (how much of the arc to use).
    /// </summary>
    public double SweepAngle
    {
        get => GetValue(SweepAngleProperty);
        set => SetValue(SweepAngleProperty, value);
    }

    /// <summary>
    /// Gets or sets whether to distribute children evenly across the arc.
    /// </summary>
    public bool DistributeEvenly
    {
        get => GetValue(DistributeEvenlyProperty);
        set => SetValue(DistributeEvenlyProperty, value);
    }

    static ArcPanel()
    {
        AffectsMeasure<ArcPanel>(RadiusProperty, StartAngleProperty, SweepAngleProperty);
        AffectsArrange<ArcPanel>(RadiusProperty, StartAngleProperty, SweepAngleProperty, DistributeEvenlyProperty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double maxChildSize = 0;

        foreach (var child in Children)
        {
            if (!child.IsVisible)
                continue;

            child.Measure(Size.Infinity);
            maxChildSize = Math.Max(maxChildSize, Math.Max(child.DesiredSize.Width, child.DesiredSize.Height));
        }

        // Calculate bounding box based on arc
        double totalRadius = Radius + maxChildSize / 2;
        
        // For a partial arc, we need to calculate the actual bounds
        double startRad = StartAngle * Math.PI / 180.0;
        double endRad = (StartAngle + SweepAngle) * Math.PI / 180.0;

        double minX = 0, maxX = 0, minY = 0, maxY = 0;

        // Check endpoints and cardinal points within range
        var angles = new[] { startRad, endRad };
        var cardinals = new[] { 0, Math.PI / 2, Math.PI, 3 * Math.PI / 2, 2 * Math.PI };

        foreach (var angle in angles.Concat(cardinals.Where(a => IsAngleInRange(a, startRad, endRad))))
        {
            double x = totalRadius * Math.Cos(angle);
            double y = totalRadius * Math.Sin(angle);
            minX = Math.Min(minX, x);
            maxX = Math.Max(maxX, x);
            minY = Math.Min(minY, y);
            maxY = Math.Max(maxY, y);
        }

        // Add center point
        minX = Math.Min(minX, 0);
        maxX = Math.Max(maxX, 0);
        minY = Math.Min(minY, 0);
        maxY = Math.Max(maxY, 0);

        return new Size(maxX - minX + maxChildSize, maxY - minY + maxChildSize);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var visibleChildren = Children.Where(c => c.IsVisible).ToList();
        if (visibleChildren.Count == 0)
            return finalSize;

        var centerX = finalSize.Width / 2;
        var centerY = finalSize.Height / 2;

        double angleStep;
        if (DistributeEvenly && visibleChildren.Count > 1)
        {
            angleStep = SweepAngle / (visibleChildren.Count - 1);
        }
        else if (visibleChildren.Count > 1)
        {
            angleStep = SweepAngle / visibleChildren.Count;
        }
        else
        {
            angleStep = 0;
        }

        double currentAngle = StartAngle;

        foreach (var child in visibleChildren)
        {
            var angleRad = currentAngle * Math.PI / 180.0;
            var x = centerX + Radius * Math.Cos(angleRad) - child.DesiredSize.Width / 2;
            var y = centerY + Radius * Math.Sin(angleRad) - child.DesiredSize.Height / 2;

            child.Arrange(new Rect(x, y, child.DesiredSize.Width, child.DesiredSize.Height));
            currentAngle += angleStep;
        }

        return finalSize;
    }

    private static bool IsAngleInRange(double angle, double start, double end)
    {
        // Normalize angles
        while (angle < start) angle += 2 * Math.PI;
        while (angle > start + 2 * Math.PI) angle -= 2 * Math.PI;
        return angle >= start && angle <= end;
    }
}
