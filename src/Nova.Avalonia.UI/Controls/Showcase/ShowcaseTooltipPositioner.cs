using System;
using System.Collections.Generic;
using Avalonia;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Helper class for calculating optimal tooltip positions.
/// </summary>
public class ShowcaseTooltipPositioner
{
    private const double Margin = 16;
    
    /// <summary>
    /// Calculates the optimal position for a tooltip.
    /// </summary>
    /// <param name="targetBounds">Bounds of the highlighted element.</param>
    /// <param name="tooltipSize">Size of the tooltip.</param>
    /// <param name="containerBounds">Bounds of the container (usually the window).</param>
    /// <param name="preferredPosition">Preferred position, or Auto for automatic.</param>
    /// <returns>The calculated position for the tooltip.</returns>
    public Point CalculatePosition(
        Rect targetBounds,
        Size tooltipSize,
        Rect containerBounds,
        ShowcaseTooltipPosition preferredPosition)
    {
        if (preferredPosition == ShowcaseTooltipPosition.Center)
        {
            return new Point(
                (containerBounds.Width - tooltipSize.Width) / 2,
                (containerBounds.Height - tooltipSize.Height) / 2);
        }
        
        var positionsToTry = preferredPosition == ShowcaseTooltipPosition.Auto
            ? new[] { ShowcaseTooltipPosition.Bottom, ShowcaseTooltipPosition.Top, 
                      ShowcaseTooltipPosition.Right, ShowcaseTooltipPosition.Left }
            : GetFallbackOrder(preferredPosition);
        
        foreach (var position in positionsToTry)
        {
            var point = TryPosition(targetBounds, tooltipSize, position);
            if (IsPositionValid(point, tooltipSize, containerBounds))
            {
                return point;
            }
        }
        
        
        return new Point(
            (containerBounds.Width - tooltipSize.Width) / 2,
            (containerBounds.Height - tooltipSize.Height) / 2);
    }
    
    private Point TryPosition(Rect targetBounds, Size tooltipSize, ShowcaseTooltipPosition position)
    {
        return position switch
        {
            ShowcaseTooltipPosition.Top => new Point(
                targetBounds.Center.X - tooltipSize.Width / 2,
                targetBounds.Top - tooltipSize.Height - Margin),
                
            ShowcaseTooltipPosition.Bottom => new Point(
                targetBounds.Center.X - tooltipSize.Width / 2,
                targetBounds.Bottom + Margin),
                
            ShowcaseTooltipPosition.Left => new Point(
                targetBounds.Left - tooltipSize.Width - Margin,
                targetBounds.Center.Y - tooltipSize.Height / 2),
                
            ShowcaseTooltipPosition.Right => new Point(
                targetBounds.Right + Margin,
                targetBounds.Center.Y - tooltipSize.Height / 2),
                
            _ => new Point(
                targetBounds.Center.X - tooltipSize.Width / 2,
                targetBounds.Bottom + Margin)
        };
    }
    
    private bool IsPositionValid(Point position, Size tooltipSize, Rect containerBounds)
    {
        var tooltipBounds = new Rect(position, tooltipSize);
        return position.X >= 0 && 
               position.Y >= 0 && 
               tooltipBounds.Right <= containerBounds.Width &&
               tooltipBounds.Bottom <= containerBounds.Height;
    }
    
    private ShowcaseTooltipPosition[] GetFallbackOrder(ShowcaseTooltipPosition preferred)
    {
        return preferred switch
        {
            ShowcaseTooltipPosition.Top => new[] 
            { 
                ShowcaseTooltipPosition.Top,
                ShowcaseTooltipPosition.Bottom, 
                ShowcaseTooltipPosition.Right, 
                ShowcaseTooltipPosition.Left 
            },
            ShowcaseTooltipPosition.Bottom => new[] 
            { 
                ShowcaseTooltipPosition.Bottom,
                ShowcaseTooltipPosition.Top, 
                ShowcaseTooltipPosition.Right, 
                ShowcaseTooltipPosition.Left 
            },
            ShowcaseTooltipPosition.Left => new[] 
            { 
                ShowcaseTooltipPosition.Left,
                ShowcaseTooltipPosition.Right, 
                ShowcaseTooltipPosition.Top, 
                ShowcaseTooltipPosition.Bottom 
            },
            ShowcaseTooltipPosition.Right => new[] 
            { 
                ShowcaseTooltipPosition.Right,
                ShowcaseTooltipPosition.Left, 
                ShowcaseTooltipPosition.Top, 
                ShowcaseTooltipPosition.Bottom 
            },
            _ => new[] 
            { 
                ShowcaseTooltipPosition.Bottom, 
                ShowcaseTooltipPosition.Top, 
                ShowcaseTooltipPosition.Right, 
                ShowcaseTooltipPosition.Left 
            }
        };
    }
}
