using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// A panel that arranges child elements using a circle packing algorithm.
/// Larger items are placed first and smaller items fill the gaps.
/// </summary>
public class BubblePanel : Panel
{
    /// <summary>
    /// Defines the <see cref="Padding"/> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> PaddingProperty =
        AvaloniaProperty.Register<BubblePanel, Thickness>(nameof(Padding));

    /// <summary>
    /// Defines the <see cref="Spacing"/> property.
    /// </summary>
    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<BubblePanel, double>(nameof(Spacing), 4);

    /// <summary>
    /// Gets or sets the padding around the panel content.
    /// </summary>
    public Thickness Padding
    {
        get => GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum spacing between bubbles.
    /// </summary>
    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    static BubblePanel()
    {
        AffectsMeasure<BubblePanel>(PaddingProperty, SpacingProperty);
        AffectsArrange<BubblePanel>(PaddingProperty, SpacingProperty);
    }

    private class PlacedCircle
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Radius { get; set; }
        public Control? Child { get; set; }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var visibleChildren = Children.Where(c => c.IsVisible).ToList();
        if (visibleChildren.Count == 0)
            return new Size(0, 0);

        foreach (var child in visibleChildren)
        {
            child.Measure(Size.Infinity);
        }

        // Estimate size based on children's actual visual size
        double totalArea = 0;
        foreach (var child in visibleChildren)
        {
            var radius = Math.Max(child.DesiredSize.Width, child.DesiredSize.Height) / 2;
            totalArea += Math.PI * radius * radius;
        }

        // Estimate a square that could contain all circles (with some padding)
        double side = Math.Sqrt(totalArea) * 1.5;
        var padding = Padding;

        return new Size(side + padding.Left + padding.Right, side + padding.Top + padding.Bottom);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var visibleChildren = Children.Where(c => c.IsVisible).ToList();
        if (visibleChildren.Count == 0)
            return finalSize;

        var padding = Padding;
        double availableWidth = finalSize.Width - padding.Left - padding.Right;
        double availableHeight = finalSize.Height - padding.Top - padding.Bottom;
        double spacing = Spacing;

        // Calculate radii and sort by size (largest first)
        // Use the actual visual radius (not scaled) for collision detection
        var childData = visibleChildren.Select(child =>
        {
            var visualRadius = Math.Max(child.DesiredSize.Width, child.DesiredSize.Height) / 2;
            return new { Child = child, Radius = Math.Max(10, visualRadius), DesiredSize = child.DesiredSize };
        }).OrderByDescending(c => c.Radius).ToList();

        var placedCircles = new List<PlacedCircle>();
        double centerX = availableWidth / 2;
        double centerY = availableHeight / 2;

        foreach (var item in childData)
        {
            var position = FindBestPosition(placedCircles, item.Radius, centerX, centerY, availableWidth, availableHeight, spacing);

            placedCircles.Add(new PlacedCircle
            {
                X = position.x,
                Y = position.y,
                Radius = item.Radius,
                Child = item.Child
            });

            // Arrange the child using its actual DesiredSize
            double childWidth = item.DesiredSize.Width;
            double childHeight = item.DesiredSize.Height;
            double x = padding.Left + position.x - childWidth / 2;
            double y = padding.Top + position.y - childHeight / 2;

            item.Child.Arrange(new Rect(x, y, childWidth, childHeight));
        }

        return finalSize;
    }

    private (double x, double y) FindBestPosition(
        List<PlacedCircle> placed,
        double radius,
        double centerX,
        double centerY,
        double width,
        double height,
        double spacing)
    {
        if (placed.Count == 0)
        {
            return (centerX, centerY);
        }

        double bestX = 0;
        double bestY = 0;
        double bestDistance = double.MaxValue;
        bool foundValid = false;

        // Try positions around each placed circle with finer granularity
        foreach (var circle in placed)
        {
            // Try 36 positions around this circle (every 10 degrees)
            for (int angle = 0; angle < 360; angle += 10)
            {
                double rad = angle * Math.PI / 180;
                double distance = circle.Radius + radius + spacing;

                double candidateX = circle.X + distance * Math.Cos(rad);
                double candidateY = circle.Y + distance * Math.Sin(rad);

                // Check bounds
                if (candidateX - radius < 0 || candidateX + radius > width ||
                    candidateY - radius < 0 || candidateY + radius > height)
                    continue;

                // Check overlap with all placed circles
                bool overlaps = false;
                foreach (var other in placed)
                {
                    double dx = candidateX - other.X;
                    double dy = candidateY - other.Y;
                    double dist = Math.Sqrt(dx * dx + dy * dy);
                    if (dist < radius + other.Radius + spacing)
                    {
                        overlaps = true;
                        break;
                    }
                }

                if (!overlaps)
                {
                    foundValid = true;
                    // Prefer positions closer to center
                    double distToCenter = Math.Sqrt(
                        Math.Pow(candidateX - centerX, 2) +
                        Math.Pow(candidateY - centerY, 2));

                    if (distToCenter < bestDistance)
                    {
                        bestDistance = distToCenter;
                        bestX = candidateX;
                        bestY = candidateY;
                    }
                }
            }
        }

        if (foundValid)
        {
            return (bestX, bestY);
        }

        // If no valid position found around existing circles, 
        // expand outward from center with finer step
        for (double r = radius + spacing; r < Math.Max(width, height) * 2; r += 10)
        {
            for (int angle = 0; angle < 360; angle += 15)
            {
                double rad = angle * Math.PI / 180;
                double candidateX = centerX + r * Math.Cos(rad);
                double candidateY = centerY + r * Math.Sin(rad);

                if (candidateX - radius < 0 || candidateX + radius > width ||
                    candidateY - radius < 0 || candidateY + radius > height)
                    continue;

                bool overlaps = false;
                foreach (var other in placed)
                {
                    double dx = candidateX - other.X;
                    double dy = candidateY - other.Y;
                    double dist = Math.Sqrt(dx * dx + dy * dy);
                    if (dist < radius + other.Radius + spacing)
                    {
                        overlaps = true;
                        break;
                    }
                }

                if (!overlaps)
                {
                    return (candidateX, candidateY);
                }
            }
        }

        // Last resort: place outside bounds rather than overlap
        // Find a position outside the current arrangement
        double maxRadius = placed.Max(c => Math.Sqrt(c.X * c.X + c.Y * c.Y) + c.Radius);
        return (centerX + maxRadius + radius + spacing, centerY);
    }
}
