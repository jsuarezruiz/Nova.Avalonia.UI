using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// A panel that stacks children with configurable X/Y offsets, creating an overlapping card effect.
/// </summary>
public class OverlapPanel : Panel
{
    /// <summary>
    /// Defines the <see cref="OffsetX"/> property.
    /// </summary>
    public static readonly StyledProperty<double> OffsetXProperty =
        AvaloniaProperty.Register<OverlapPanel, double>(nameof(OffsetX), 10);

    /// <summary>
    /// Defines the <see cref="OffsetY"/> property.
    /// </summary>
    public static readonly StyledProperty<double> OffsetYProperty =
        AvaloniaProperty.Register<OverlapPanel, double>(nameof(OffsetY), 10);

    /// <summary>
    /// Defines the <see cref="ReverseZIndex"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> ReverseZIndexProperty =
        AvaloniaProperty.Register<OverlapPanel, bool>(nameof(ReverseZIndex), false);

    /// <summary>
    /// Gets or sets the horizontal offset between items.
    /// </summary>
    public double OffsetX
    {
        get => GetValue(OffsetXProperty);
        set => SetValue(OffsetXProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical offset between items.
    /// </summary>
    public double OffsetY
    {
        get => GetValue(OffsetYProperty);
        set => SetValue(OffsetYProperty, value);
    }

    /// <summary>
    /// Gets or sets whether to reverse the Z-index stacking order.
    /// When false (default), later items appear on top. When true, earlier items appear on top.
    /// </summary>
    public bool ReverseZIndex
    {
        get => GetValue(ReverseZIndexProperty);
        set => SetValue(ReverseZIndexProperty, value);
    }

    static OverlapPanel()
    {
        AffectsMeasure<OverlapPanel>(OffsetXProperty, OffsetYProperty);
        AffectsArrange<OverlapPanel>(OffsetXProperty, OffsetYProperty, ReverseZIndexProperty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double maxChildWidth = 0;
        double maxChildHeight = 0;
        int visibleCount = 0;

        foreach (var child in Children)
        {
            if (!child.IsVisible)
                continue;

            child.Measure(availableSize);
            maxChildWidth = Math.Max(maxChildWidth, child.DesiredSize.Width);
            maxChildHeight = Math.Max(maxChildHeight, child.DesiredSize.Height);
            visibleCount++;
        }

        if (visibleCount == 0)
            return new Size(0, 0);

        // Total size = largest child + offsets for all other children
        double totalWidth = maxChildWidth + Math.Abs(OffsetX) * (visibleCount - 1);
        double totalHeight = maxChildHeight + Math.Abs(OffsetY) * (visibleCount - 1);

        return new Size(totalWidth, totalHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        int visibleCount = 0;
        var visibleChildren = new List<Control>();

        foreach (var child in Children)
        {
            if (child.IsVisible)
            {
                visibleChildren.Add(child);
                visibleCount++;
            }
        }

        if (visibleCount == 0)
            return finalSize;

        double offsetX = OffsetX;
        double offsetY = OffsetY;
        bool reverseZ = ReverseZIndex;

        // Calculate starting position based on offset direction
        double startX = offsetX < 0 ? Math.Abs(offsetX) * (visibleCount - 1) : 0;
        double startY = offsetY < 0 ? Math.Abs(offsetY) * (visibleCount - 1) : 0;

        for (int i = 0; i < visibleChildren.Count; i++)
        {
            var child = visibleChildren[i];
            double x = startX + i * offsetX;
            double y = startY + i * offsetY;

            child.Arrange(new Rect(x, y, child.DesiredSize.Width, child.DesiredSize.Height));

            // Set ZIndex
            int zIndex = reverseZ ? (visibleCount - 1 - i) : i;
            child.ZIndex = zIndex;
        }

        return finalSize;
    }
}