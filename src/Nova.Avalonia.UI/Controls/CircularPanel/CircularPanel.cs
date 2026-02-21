using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// A layout panel that arranges child elements in a circular pattern around a center point.
/// </summary>
public class CircularPanel : Panel
{
    /// <summary>
    /// Defines the <see cref="Radius"/> property.
    /// </summary>
    public static readonly StyledProperty<double> RadiusProperty =
        AvaloniaProperty.Register<CircularPanel, double>(nameof(Radius), 100);

    /// <summary>
    /// Defines the <see cref="StartAngle"/> property.
    /// </summary>
    public static readonly StyledProperty<double> StartAngleProperty =
        AvaloniaProperty.Register<CircularPanel, double>(nameof(StartAngle), 0);

    /// <summary>
    /// Defines the <see cref="AngleStep"/> property.
    /// </summary>
    public static readonly StyledProperty<double> AngleStepProperty =
        AvaloniaProperty.Register<CircularPanel, double>(nameof(AngleStep), double.NaN);

    /// <summary>
    /// Defines the <see cref="SweepDirection"/> property.
    /// </summary>
    public static readonly StyledProperty<SweepDirection> SweepDirectionProperty =
        AvaloniaProperty.Register<CircularPanel, SweepDirection>(nameof(SweepDirection), SweepDirection.Clockwise);

    /// <summary>
    /// Defines the <see cref="KeepInBounds"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> KeepInBoundsProperty =
        AvaloniaProperty.Register<CircularPanel, bool>(nameof(KeepInBounds), true);

    /// <summary>
    /// Defines the Angle attached property.
    /// </summary>
    public static readonly AttachedProperty<double> AngleProperty =
        AvaloniaProperty.RegisterAttached<CircularPanel, Control, double>("Angle", double.NaN);

    /// <summary>
    /// Defines the ItemRadius attached property.
    /// </summary>
    public static readonly AttachedProperty<double> ItemRadiusProperty =
        AvaloniaProperty.RegisterAttached<CircularPanel, Control, double>("ItemRadius", double.NaN);

    /// <summary>
    /// Defines the Alignment attached property.
    /// </summary>
    public static readonly AttachedProperty<CircularAlignment> AlignmentProperty =
        AvaloniaProperty.RegisterAttached<CircularPanel, Control, CircularAlignment>("Alignment", CircularAlignment.Center);

    /// <summary>
    /// Gets or sets the default radius for all children.
    /// </summary>
    public double Radius
    {
        get => GetValue(RadiusProperty);
        set => SetValue(RadiusProperty, value);
    }

    /// <summary>
    /// Gets or sets the starting angle in degrees (0° = right, 90° = bottom).
    /// </summary>
    public double StartAngle
    {
        get => GetValue(StartAngleProperty);
        set => SetValue(StartAngleProperty, value);
    }

    /// <summary>
    /// Gets or sets the angle increment between children. Use NaN for auto (360° / count).
    /// </summary>
    public double AngleStep
    {
        get => GetValue(AngleStepProperty);
        set => SetValue(AngleStepProperty, value);
    }

    /// <summary>
    /// Gets or sets the direction to arrange children.
    /// </summary>
    public SweepDirection SweepDirection
    {
        get => GetValue(SweepDirectionProperty);
        set => SetValue(SweepDirectionProperty, value);
    }

    /// <summary>
    /// Gets or sets whether to ensure children stay within panel bounds.
    /// </summary>
    public bool KeepInBounds
    {
        get => GetValue(KeepInBoundsProperty);
        set => SetValue(KeepInBoundsProperty, value);
    }

    /// <summary>
    /// Gets the Angle attached property value for an element.
    /// </summary>
    public static double GetAngle(Control element) => element.GetValue(AngleProperty);

    /// <summary>
    /// Sets the Angle attached property value for an element.
    /// </summary>
    public static void SetAngle(Control element, double value) => element.SetValue(AngleProperty, value);

    /// <summary>
    /// Gets the ItemRadius attached property value for an element.
    /// </summary>
    public static double GetItemRadius(Control element) => element.GetValue(ItemRadiusProperty);

    /// <summary>
    /// Sets the ItemRadius attached property value for an element.
    /// </summary>
    public static void SetItemRadius(Control element, double value) => element.SetValue(ItemRadiusProperty, value);

    /// <summary>
    /// Gets the Alignment attached property value for an element.
    /// </summary>
    public static CircularAlignment GetAlignment(Control element) => element.GetValue(AlignmentProperty);

    /// <summary>
    /// Sets the Alignment attached property value for an element.
    /// </summary>
    public static void SetAlignment(Control element, CircularAlignment value) => element.SetValue(AlignmentProperty, value);

    static CircularPanel()
    {
        AffectsMeasure<CircularPanel>(RadiusProperty, StartAngleProperty, AngleStepProperty, SweepDirectionProperty);
        AffectsArrange<CircularPanel>(RadiusProperty, StartAngleProperty, AngleStepProperty, SweepDirectionProperty, KeepInBoundsProperty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var maxRadius = 0.0;

        foreach (var child in Children)
        {
            if (!child.IsVisible)
                continue;

            child.Measure(Size.Infinity);

            var radius = GetItemRadius(child);
            if (double.IsNaN(radius))
                radius = Radius;

            var childSize = Math.Max(child.DesiredSize.Width, child.DesiredSize.Height);
            maxRadius = Math.Max(maxRadius, radius + childSize / 2);
        }

        var diameter = maxRadius * 2;
        return new Size(diameter, diameter);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var centerX = finalSize.Width / 2;
        var centerY = finalSize.Height / 2;

        int visibleCount = 0;
        foreach (var child in Children)
        {
            if (child.IsVisible) visibleCount++;
        }

        var autoAngle = AngleStep;
        if (double.IsNaN(autoAngle) || autoAngle == 0)
        {
            autoAngle = visibleCount > 0 ? 360.0 / visibleCount : 0;
        }

        var currentAngle = StartAngle;
        var angleMultiplier = SweepDirection == SweepDirection.Clockwise ? 1 : -1;

        foreach (var child in Children)
        {
            if (!child.IsVisible) continue;


            var angle = GetAngle(child);
            if (double.IsNaN(angle))
            {
                angle = currentAngle;
                currentAngle += autoAngle * angleMultiplier;
            }
            
            var radius = GetItemRadius(child);
            if (double.IsNaN(radius))
                radius = Radius;
            
            var angleRad = angle * Math.PI / 180.0;
            var x = centerX + radius * Math.Cos(angleRad);
            var y = centerY + radius * Math.Sin(angleRad);


            var alignment = GetAlignment(child);
            var childRect = CalculateChildRect(child, x, y, alignment, angleRad);
            
            if (KeepInBounds)
            {
                childRect = ClampToBounds(childRect, finalSize);
            }

            child.Arrange(childRect);
        }

        return finalSize;
    }

    private static Rect CalculateChildRect(
        Control child,
        double x,
        double y,
        CircularAlignment alignment,
        double angleRad)
    {
        var width = child.DesiredSize.Width;
        var height = child.DesiredSize.Height;

        switch (alignment)
        {
            case CircularAlignment.Center:
                return new Rect(x - width / 2, y - height / 2, width, height);

            case CircularAlignment.Inner:
                var innerX = x - (width / 2) * Math.Cos(angleRad);
                var innerY = y - (height / 2) * Math.Sin(angleRad);
                return new Rect(innerX - width / 2, innerY - height / 2, width, height);

            case CircularAlignment.Outer:
                var outerX = x + (width / 2) * Math.Cos(angleRad);
                var outerY = y + (height / 2) * Math.Sin(angleRad);
                return new Rect(outerX - width / 2, outerY - height / 2, width, height);

            default:
                return new Rect(x - width / 2, y - height / 2, width, height);
        }
    }

    private static Rect ClampToBounds(Rect rect, Size bounds)
    {
        var x = Math.Max(0, Math.Min(rect.X, bounds.Width - rect.Width));
        var y = Math.Max(0, Math.Min(rect.Y, bounds.Height - rect.Height));
        return new Rect(x, y, rect.Width, rect.Height);
    }
}
