using Avalonia;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Helper class for calculating optimal tooltip positions.
/// </summary>
public class ShowcaseTooltipPositioner
{
    /// <summary>
    /// Gap between the target highlight edge and the tooltip.
    /// </summary>
    public const double TooltipGap = 16;

    private static readonly ShowcaseTooltipPosition[] AutoOrder =
    {
        ShowcaseTooltipPosition.Bottom,
        ShowcaseTooltipPosition.Top,
        ShowcaseTooltipPosition.Right,
        ShowcaseTooltipPosition.Left
    };

    private static readonly ShowcaseTooltipPosition[] TopFallback =
    {
        ShowcaseTooltipPosition.Top,
        ShowcaseTooltipPosition.Bottom,
        ShowcaseTooltipPosition.Right,
        ShowcaseTooltipPosition.Left
    };

    private static readonly ShowcaseTooltipPosition[] BottomFallback =
    {
        ShowcaseTooltipPosition.Bottom,
        ShowcaseTooltipPosition.Top,
        ShowcaseTooltipPosition.Right,
        ShowcaseTooltipPosition.Left
    };

    private static readonly ShowcaseTooltipPosition[] LeftFallback =
    {
        ShowcaseTooltipPosition.Left,
        ShowcaseTooltipPosition.Right,
        ShowcaseTooltipPosition.Top,
        ShowcaseTooltipPosition.Bottom
    };

    private static readonly ShowcaseTooltipPosition[] RightFallback =
    {
        ShowcaseTooltipPosition.Right,
        ShowcaseTooltipPosition.Left,
        ShowcaseTooltipPosition.Top,
        ShowcaseTooltipPosition.Bottom
    };

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
                containerBounds.Left + (containerBounds.Width - tooltipSize.Width) / 2,
                containerBounds.Top + (containerBounds.Height - tooltipSize.Height) / 2);
        }

        var positionsToTry = preferredPosition == ShowcaseTooltipPosition.Auto
            ? AutoOrder
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
            containerBounds.Left + (containerBounds.Width - tooltipSize.Width) / 2,
            containerBounds.Top + (containerBounds.Height - tooltipSize.Height) / 2);
    }

    private static Point TryPosition(Rect targetBounds, Size tooltipSize, ShowcaseTooltipPosition position)
    {
        return position switch
        {
            ShowcaseTooltipPosition.Top => new Point(
                targetBounds.Center.X - tooltipSize.Width / 2,
                targetBounds.Top - tooltipSize.Height - TooltipGap),

            ShowcaseTooltipPosition.Bottom => new Point(
                targetBounds.Center.X - tooltipSize.Width / 2,
                targetBounds.Bottom + TooltipGap),

            ShowcaseTooltipPosition.Left => new Point(
                targetBounds.Left - tooltipSize.Width - TooltipGap,
                targetBounds.Center.Y - tooltipSize.Height / 2),

            ShowcaseTooltipPosition.Right => new Point(
                targetBounds.Right + TooltipGap,
                targetBounds.Center.Y - tooltipSize.Height / 2),

            _ => new Point(
                targetBounds.Center.X - tooltipSize.Width / 2,
                targetBounds.Bottom + TooltipGap)
        };
    }

    private static bool IsPositionValid(Point position, Size tooltipSize, Rect containerBounds)
    {
        var tooltipBounds = new Rect(position, tooltipSize);
        return position.X >= containerBounds.Left &&
               position.Y >= containerBounds.Top &&
               tooltipBounds.Right <= containerBounds.Right &&
               tooltipBounds.Bottom <= containerBounds.Bottom;
    }

    private static ShowcaseTooltipPosition[] GetFallbackOrder(ShowcaseTooltipPosition preferred)
    {
        return preferred switch
        {
            ShowcaseTooltipPosition.Top => TopFallback,
            ShowcaseTooltipPosition.Bottom => BottomFallback,
            ShowcaseTooltipPosition.Left => LeftFallback,
            ShowcaseTooltipPosition.Right => RightFallback,
            _ => AutoOrder
        };
    }
}
