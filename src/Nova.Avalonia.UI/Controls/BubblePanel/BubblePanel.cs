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

    protected override Size MeasureOverride(Size availableSize)
    {
        double totalArea = 0;
        double maxChildRadius = 0;
        bool hasVisible = false;

        foreach (var child in Children)
        {
            if (!child.IsVisible) continue;
            
            hasVisible = true;
            child.Measure(Size.Infinity);
            
            var radius = Math.Max(child.DesiredSize.Width, child.DesiredSize.Height) / 2;
            totalArea += Math.PI * radius * radius;
            maxChildRadius = Math.Max(maxChildRadius, radius);
        }

        if (!hasVisible)
            return new Size(0, 0);
        
        double side = Math.Sqrt(totalArea) * 1.5;
        // Ensure side is at least max diameter to fit the largest item
        side = Math.Max(side, maxChildRadius * 2);

        var padding = Padding;
        return new Size(side + padding.Left + padding.Right, side + padding.Top + padding.Bottom);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        int visibleCount = 0;
        foreach (var child in Children)
        {
            if (child.IsVisible) visibleCount++;
        }

        if (visibleCount == 0)
            return finalSize;

        var padding = Padding;
        double availableWidth = finalSize.Width - padding.Left - padding.Right;
        double availableHeight = finalSize.Height - padding.Top - padding.Bottom;
        double spacing = Spacing;

        var childData = new List<(Control Child, double Radius, Size DesiredSize)>(visibleCount);
        foreach (var child in Children)
        {
            if (!child.IsVisible) continue;
            var visualRadius = Math.Max(child.DesiredSize.Width, child.DesiredSize.Height) / 2;
            childData.Add((child, Math.Max(10, visualRadius), child.DesiredSize));
        }

        // Sorting is acceptable here as it's part of the algorithm, 
        // but we've avoided the extra LINQ Where/ToList.
        childData.Sort((a, b) => b.Radius.CompareTo(a.Radius));

        var placedCircles = new List<PlacedCircle>(visibleCount);
        double centerX = availableWidth / 2;
        double centerY = availableHeight / 2;

        foreach (var item in childData)
        {
            var position = FindBestPosition(placedCircles, item.Radius, centerX, centerY, availableWidth, availableHeight, spacing);

            placedCircles.Add(new PlacedCircle
            {
                X = position.X,
                Y = position.Y,
                Radius = item.Radius,
                Child = item.Child
            });

            double childWidth = item.DesiredSize.Width;
            double childHeight = item.DesiredSize.Height;
            double x = padding.Left + position.X - childWidth / 2;
            double y = padding.Top + position.Y - childHeight / 2;

            item.Child.Arrange(new Rect(x, y, childWidth, childHeight));
        }

        return finalSize;
    }

    private (double X, double Y) FindBestPosition(
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


        foreach (var circle in placed)
        {
            for (int angle = 0; angle < 360; angle += 10)
            {
                double rad = angle * Math.PI / 180;
                double distance = circle.Radius + radius + spacing;

                double candidateX = circle.X + distance * Math.Cos(rad);
                double candidateY = circle.Y + distance * Math.Sin(rad);


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
                    foundValid = true;

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
                for (int i = 0; i < placed.Count; i++)
                {
                    var other = placed[i];
                    double dx = candidateX - other.X;
                    double dy = candidateY - other.Y;
                    double distSq = dx * dx + dy * dy;
                    double minRelativeDist = radius + other.Radius + spacing;
                    if (distSq < minRelativeDist * minRelativeDist)
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

        double maxRadius = 0;
        for (int i = 0; i < placed.Count; i++)
        {
            var c = placed[i];
            double dist = Math.Sqrt(c.X * c.X + c.Y * c.Y) + c.Radius;
            if (dist > maxRadius) maxRadius = dist;
        }

        return (centerX + maxRadius + radius + spacing, centerY);
    }
    
    private class PlacedCircle
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Radius { get; set; }
        public Control? Child { get; set; }
    }
}