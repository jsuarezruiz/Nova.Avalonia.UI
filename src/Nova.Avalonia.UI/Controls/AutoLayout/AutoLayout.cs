using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Defines how items are distributed in the <see cref="AutoLayout"/> panel.
/// </summary>
public enum AutoLayoutJustify
{
    /// <summary>
    /// Items are packed together with the specified <see cref="AutoLayout.Spacing"/>.
    /// </summary>
    Packed,

    /// <summary>
    /// Items are distributed evenly across the available space, creating equal spacing between them.
    /// The <see cref="AutoLayout.Spacing"/> property is ignored in this mode.
    /// </summary>
    SpaceBetween
}

/// <summary>
/// A layout panel inspired by Figma's Auto Layout, allowing simple direction-based stacking
/// with easy control over spacing, alignment, and distribution.
/// </summary>
public class AutoLayout : Panel
{
    /// <summary>
    /// Defines the <see cref="Orientation"/> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        StackPanel.OrientationProperty.AddOwner<AutoLayout>();

    /// <summary>
    /// Defines the <see cref="Spacing"/> property.
    /// </summary>
    public static readonly StyledProperty<double> SpacingProperty =
        StackPanel.SpacingProperty.AddOwner<AutoLayout>();

    /// <summary>
    /// Defines the <see cref="Padding"/> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> PaddingProperty =
        Decorator.PaddingProperty.AddOwner<AutoLayout>();

    /// <summary>
    /// Defines the <see cref="HorizontalContentAlignment"/> property.
    /// </summary>
    public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
        ContentControl.HorizontalContentAlignmentProperty.AddOwner<AutoLayout>();

    /// <summary>
    /// Defines the <see cref="VerticalContentAlignment"/> property.
    /// </summary>
    public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
        ContentControl.VerticalContentAlignmentProperty.AddOwner<AutoLayout>();

    /// <summary>
    /// Defines the <see cref="Justification"/> property.
    /// </summary>
    public static readonly StyledProperty<AutoLayoutJustify> JustificationProperty =
        AvaloniaProperty.Register<AutoLayout, AutoLayoutJustify>(nameof(Justification), AutoLayoutJustify.Packed);

    /// <summary>
    /// Defines the <see cref="IsReverseZIndex"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsReverseZIndexProperty =
        AvaloniaProperty.Register<AutoLayout, bool>(nameof(IsReverseZIndex), false);
        
    /// <summary>
    /// Defines the IsAbsolute attached property.
    /// </summary>
    public static readonly AttachedProperty<bool> IsAbsoluteProperty =
        AvaloniaProperty.RegisterAttached<AutoLayout, Control, bool>("IsAbsolute", false);

    /// <summary>
    /// Gets or sets the primary direction of the layout.
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Gets or sets the spacing between items.
    /// </summary>
    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the padding around the content.
    /// </summary>
    public Thickness Padding
    {
        get => GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }

    /// <summary>
    /// Gets or sets the horizontal alignment of the content items.
    /// </summary>
    public HorizontalAlignment HorizontalContentAlignment
    {
        get => GetValue(HorizontalContentAlignmentProperty);
        set => SetValue(HorizontalContentAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical alignment of the content items.
    /// </summary>
    public VerticalAlignment VerticalContentAlignment
    {
        get => GetValue(VerticalContentAlignmentProperty);
        set => SetValue(VerticalContentAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets how items are distributed along the primary axis.
    /// </summary>
    public AutoLayoutJustify Justification
    {
        get => GetValue(JustificationProperty);
        set => SetValue(JustificationProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the visual stacking order is reversed.
    /// </summary>
    public bool IsReverseZIndex
    {
        get => GetValue(IsReverseZIndexProperty);
        set => SetValue(IsReverseZIndexProperty, value);
    }

    /// <summary>
    /// Gets the IsAbsolute attached property.
    /// </summary>
    public static bool GetIsAbsolute(Control element) => element.GetValue(IsAbsoluteProperty);

    /// <summary>
    /// Sets the IsAbsolute attached property.
    /// </summary>
    public static void SetIsAbsolute(Control element, bool value) => element.SetValue(IsAbsoluteProperty, value);

    static AutoLayout()
    {
        AffectsMeasure<AutoLayout>(OrientationProperty, SpacingProperty, PaddingProperty, HorizontalContentAlignmentProperty, VerticalContentAlignmentProperty, JustificationProperty);
        AffectsArrange<AutoLayout>(OrientationProperty, SpacingProperty, PaddingProperty, HorizontalContentAlignmentProperty, VerticalContentAlignmentProperty, JustificationProperty, IsReverseZIndexProperty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var spacing = Spacing;
        var padding = Padding;
        var orientation = Orientation;
        var children = Children;
        var parentWidth = availableSize.Width;
        var parentHeight = availableSize.Height;
        
        double totalWidth = 0;
        double totalHeight = 0;
        double accumulatedSize = 0;
        double maxSecondary = 0;
        int visibleCount = 0;

        double availableWidthRaw = parentWidth - padding.Left - padding.Right;
        double availableHeightRaw = parentHeight - padding.Top - padding.Bottom;
        
        var availableConstraint = new Size(Math.Max(0, availableWidthRaw), Math.Max(0, availableHeightRaw));

        foreach (var child in children)
        {
            if (!child.IsVisible) continue;
            if (GetIsAbsolute(child))
            {
                // Measure absolute items with full available size but don't affect layout
                child.Measure(availableConstraint);
                continue;
            }

            visibleCount++;
            
            // For children that want to stretch on the secondary axis, we need to pass the full constraint
            // But for SpaceBetween on primary axis, we usually let them auto-size first.
            // Simplified: Just measure with available constraint.
            child.Measure(availableConstraint);

            var desired = child.DesiredSize;
            if (orientation == Orientation.Horizontal)
            {
                accumulatedSize += desired.Width;
                maxSecondary = Math.Max(maxSecondary, desired.Height);
            }
            else
            {
                accumulatedSize += desired.Height;
                maxSecondary = Math.Max(maxSecondary, desired.Width);
            }
        }

        if (visibleCount > 0)
        {
            // Add spacing only if packed (SpaceBetween ignores Spacing property during Measure size calc, effectively)
            // But wait, for DesiredSize of the Panel, if Packed, we need spacing.
            // If SpaceBetween, the DesiredSize depends on whether we are constrained.
            // If Infinity constraint (Auto width), SpaceBetween behaves like Packed (min size).
            
            double totalSpacing = (visibleCount - 1) * spacing;
            
            if (orientation == Orientation.Horizontal)
            {
                if (Justification == AutoLayoutJustify.Packed)
                {
                    totalWidth = accumulatedSize + totalSpacing;
                }
                else
                {
                    // SpaceBetween: Items grow to fill available space.
                    totalWidth = accumulatedSize; 
                }
                totalHeight = maxSecondary;
            }
            else
            {
                if (Justification == AutoLayoutJustify.Packed)
                {
                    totalHeight = accumulatedSize + totalSpacing;
                }
                else
                {
                    totalHeight = accumulatedSize;
                }
                totalWidth = maxSecondary;
            }
        }

        return new Size(
            totalWidth + padding.Left + padding.Right,
            totalHeight + padding.Top + padding.Bottom);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var orientation = Orientation;
        var padding = Padding;
        var justify = Justification;
        var children = Children;
        var spacing = Spacing;
        var horizontalAlign = HorizontalContentAlignment;
        var verticalAlign = VerticalContentAlignment;
        var isReverse = IsReverseZIndex;

        double currentX = padding.Left;
        double currentY = padding.Top;
        double contentWidth = finalSize.Width - padding.Left - padding.Right;
        double contentHeight = finalSize.Height - padding.Top - padding.Bottom;

        int visibleCount = 0;
        foreach (var child in Children)
        {
            if (child.IsVisible && !GetIsAbsolute(child)) visibleCount++;
        }

        if (visibleCount == 0) return finalSize;

        double actualSpacing = spacing;
        double totalItemSize = 0;

        foreach (var child in Children)
        {
            if (!child.IsVisible || GetIsAbsolute(child)) continue;
            totalItemSize += (orientation == Orientation.Horizontal) ? child.DesiredSize.Width : child.DesiredSize.Height;
        }

        if (justify == AutoLayoutJustify.SpaceBetween)
        {
            if (visibleCount > 1)
            {
                double availableSpace = (orientation == Orientation.Horizontal) ? contentWidth : contentHeight;
                double remainingSpace = Math.Max(0, availableSpace - totalItemSize);
                actualSpacing = remainingSpace / (visibleCount - 1);
            }
            else
            {
                actualSpacing = 0;
            }
        }
        else // Packed
        {
            double groupSize = totalItemSize + (visibleCount - 1) * spacing;
            
            if (orientation == Orientation.Horizontal)
            {
                if (horizontalAlign == HorizontalAlignment.Center)
                    currentX += (contentWidth - groupSize) / 2;
                else if (horizontalAlign == HorizontalAlignment.Right)
                    currentX += (contentWidth - groupSize);
            }
            else
            {
                if (verticalAlign == VerticalAlignment.Center)
                    currentY += (contentHeight - groupSize) / 2;
                else if (verticalAlign == VerticalAlignment.Bottom)
                    currentY += (contentHeight - groupSize);
            }
        }

        if (isReverse)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].ZIndex = Children.Count - i;
            }
        }
        else
        {
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].ZIndex = 0; // Reset
            }
        }

        foreach (var child in Children)
        {
            if (!child.IsVisible || GetIsAbsolute(child)) continue;

            var desired = child.DesiredSize;
            double childW = desired.Width;
            double childH = desired.Height;
            
            // Secondary Axis Alignment (Counter Axis)
            // If child is Stretch, force it to fill secondary dimension
            if (orientation == Orientation.Horizontal)
            {
                double arrangeY = currentY;
                double arrangeH = childH;

                // Align child based on VerticalContentAlignment or child's own VerticalAlignment.
                var childVAlign = child.VerticalAlignment;
                
                if (VerticalContentAlignment == VerticalAlignment.Stretch || childVAlign == VerticalAlignment.Stretch)
                {
                    arrangeY = padding.Top;
                    arrangeH = contentHeight;
                }
                else
                {
                    // Align within the row height (which is full content height)
                    if (VerticalContentAlignment == VerticalAlignment.Center)
                        arrangeY = padding.Top + (contentHeight - childH) / 2;
                    else if (VerticalContentAlignment == VerticalAlignment.Bottom)
                        arrangeY = padding.Top + (contentHeight - childH);
                    else // Top
                        arrangeY = padding.Top;
                }
                
                child.Arrange(new Rect(currentX, arrangeY, childW, arrangeH));
                currentX += childW + actualSpacing;
            }
            else // Vertical
            {
                double arrangeX = currentX;
                double arrangeW = childW;
                var childHAlign = child.HorizontalAlignment;

                if (HorizontalContentAlignment == HorizontalAlignment.Stretch || childHAlign == HorizontalAlignment.Stretch)
                {
                    arrangeX = padding.Left;
                    arrangeW = contentWidth;
                }
                else
                {
                     if (HorizontalContentAlignment == HorizontalAlignment.Center)
                        arrangeX = padding.Left + (contentWidth - childW) / 2;
                    else if (HorizontalContentAlignment == HorizontalAlignment.Right)
                        arrangeX = padding.Left + (contentWidth - childW);
                    else // Left
                        arrangeX = padding.Left;
                }

                child.Arrange(new Rect(arrangeX, currentY, arrangeW, childH));
                currentY += childH + actualSpacing;
            }
        }
        
        foreach (var child in Children)
        {
            if (child.IsVisible && GetIsAbsolute(child))
            {
                child.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
            }
        }

        return finalSize;
    }
}