using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// A panel that arranges child elements in concentric orbit rings around a center point.
/// Each child can be assigned to a specific orbit (ring) and will be distributed evenly within that orbit.
/// </summary>
public class OrbitPanel : Panel
{
    /// <summary>
    /// Defines the <see cref="OrbitSpacing"/> property.
    /// </summary>
    public static readonly StyledProperty<double> OrbitSpacingProperty =
        AvaloniaProperty.Register<OrbitPanel, double>(nameof(OrbitSpacing), 60);

    /// <summary>
    /// Defines the <see cref="StartAngle"/> property.
    /// </summary>
    public static readonly StyledProperty<double> StartAngleProperty =
        AvaloniaProperty.Register<OrbitPanel, double>(nameof(StartAngle), 0);

    /// <summary>
    /// Defines the <see cref="InnerRadius"/> property.
    /// </summary>
    public static readonly StyledProperty<double> InnerRadiusProperty =
        AvaloniaProperty.Register<OrbitPanel, double>(nameof(InnerRadius), 50);

    /// <summary>
    /// Defines the Orbit attached property (0 = center, 1 = first ring, etc.).
    /// </summary>
    public static readonly AttachedProperty<int> OrbitProperty =
        AvaloniaProperty.RegisterAttached<OrbitPanel, Control, int>("Orbit", 0);

    /// <summary>
    /// Gets or sets the spacing between orbit rings.
    /// </summary>
    public double OrbitSpacing
    {
        get => GetValue(OrbitSpacingProperty);
        set => SetValue(OrbitSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the starting angle in degrees.
    /// </summary>
    public double StartAngle
    {
        get => GetValue(StartAngleProperty);
        set => SetValue(StartAngleProperty, value);
    }

    /// <summary>
    /// Gets or sets the radius of the innermost orbit ring.
    /// </summary>
    public double InnerRadius
    {
        get => GetValue(InnerRadiusProperty);
        set => SetValue(InnerRadiusProperty, value);
    }

    /// <summary>
    /// Gets the Orbit attached property value.
    /// </summary>
    public static int GetOrbit(Control element)
    {
        // Check if the element has the Orbit property set directly
        var orbit = element.GetValue(OrbitProperty);
        if (orbit != 0)
            return orbit;

        // If element is a ContentPresenter (from ItemsControl), check its content
        if (element is ContentPresenter cp && cp.Child is Control childControl)
        {
            return childControl.GetValue(OrbitProperty);
        }

        return orbit;
    }

    /// <summary>
    /// Sets the Orbit attached property value.
    /// </summary>
    public static void SetOrbit(Control element, int value) => element.SetValue(OrbitProperty, value);

    static OrbitPanel()
    {
        AffectsMeasure<OrbitPanel>(OrbitSpacingProperty, StartAngleProperty, InnerRadiusProperty);
        AffectsArrange<OrbitPanel>(OrbitSpacingProperty, StartAngleProperty, InnerRadiusProperty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        int maxOrbit = 0;
        double maxChildSize = 0;
        bool hasVisible = false;

        foreach (var child in Children)
        {
            if (!child.IsVisible)
                continue;

            hasVisible = true;
            child.Measure(Size.Infinity);
            maxOrbit = Math.Max(maxOrbit, GetOrbit(child));
            maxChildSize = Math.Max(maxChildSize, Math.Max(child.DesiredSize.Width, child.DesiredSize.Height));
        }

        if (!hasVisible)
            return new Size(0, 0);

        double maxRadius = InnerRadius + maxOrbit * OrbitSpacing + maxChildSize / 2;
        double diameter = maxRadius * 2;

        return new Size(diameter, diameter);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var centerX = finalSize.Width / 2;
        var centerY = finalSize.Height / 2;

        int minOrbit = int.MaxValue;
        int maxOrbit = int.MinValue;
        bool hasVisible = false;

        foreach (var child in Children)
        {
            if (!child.IsVisible) continue;
            int orbit = GetOrbit(child);
            if (orbit < minOrbit) minOrbit = orbit;
            if (orbit > maxOrbit) maxOrbit = orbit;
            hasVisible = true;
        }

        if (!hasVisible) return finalSize;

        for (int orbitIndex = minOrbit; orbitIndex <= maxOrbit; orbitIndex++)
        {
            // Count children in this orbit
            int childrenInOrbitCount = 0;
            foreach (var child in Children)
            {
                if (child.IsVisible && GetOrbit(child) == orbitIndex)
                {
                    childrenInOrbitCount++;
                }
            }

            if (childrenInOrbitCount == 0) continue;

            if (orbitIndex == 0)
            {
                foreach (var child in Children)
                {
                    if (child.IsVisible && GetOrbit(child) == 0)
                    {
                        var x = centerX - child.DesiredSize.Width / 2;
                        var y = centerY - child.DesiredSize.Height / 2;
                        child.Arrange(new Rect(x, y, child.DesiredSize.Width, child.DesiredSize.Height));
                    }
                }
            }
            else
            {
                double radius = InnerRadius + (orbitIndex - 1) * OrbitSpacing;
                double angleStep = 360.0 / childrenInOrbitCount;
                double currentAngle = StartAngle;

                foreach (var child in Children)
                {
                    if (child.IsVisible && GetOrbit(child) == orbitIndex)
                    {
                        var angleRad = currentAngle * Math.PI / 180.0;
                        var x = centerX + radius * Math.Cos(angleRad) - child.DesiredSize.Width / 2;
                        var y = centerY + radius * Math.Sin(angleRad) - child.DesiredSize.Height / 2;

                        child.Arrange(new Rect(x, y, child.DesiredSize.Width, child.DesiredSize.Height));
                        currentAngle += angleStep;
                    }
                }
            }
        }

        return finalSize;
    }
}