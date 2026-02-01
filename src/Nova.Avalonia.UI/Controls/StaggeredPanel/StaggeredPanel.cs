using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// A panel that arranges child elements into a staggered grid pattern, usually 
/// adding items to the column with the least amount of space used.
/// </summary>
public class StaggeredPanel : Panel
{
    /// <summary>
    /// Defines the <see cref="DesiredColumnWidth"/> property.
    /// </summary>
    public static readonly StyledProperty<double> DesiredColumnWidthProperty =
        AvaloniaProperty.Register<StaggeredPanel, double>(nameof(DesiredColumnWidth), 250);

    /// <summary>
    /// Defines the <see cref="ColumnSpacing"/> property.
    /// </summary>
    public static readonly StyledProperty<double> ColumnSpacingProperty =
        AvaloniaProperty.Register<StaggeredPanel, double>(nameof(ColumnSpacing), 0);

    /// <summary>
    /// Defines the <see cref="RowSpacing"/> property.
    /// </summary>
    public static readonly StyledProperty<double> RowSpacingProperty =
        AvaloniaProperty.Register<StaggeredPanel, double>(nameof(RowSpacing), 0);

    /// <summary>
    /// Defines the <see cref="Padding"/> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> PaddingProperty =
        AvaloniaProperty.Register<StaggeredPanel, Thickness>(nameof(Padding));



    /// <summary>
    /// Gets or sets the desired width for each column.
    /// </summary>
    public double DesiredColumnWidth
    {
        get => GetValue(DesiredColumnWidthProperty);
        set => SetValue(DesiredColumnWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the spacing between columns.
    /// </summary>
    public double ColumnSpacing
    {
        get => GetValue(ColumnSpacingProperty);
        set => SetValue(ColumnSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the spacing between rows of items.
    /// </summary>
    public double RowSpacing
    {
        get => GetValue(RowSpacingProperty);
        set => SetValue(RowSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the padding around the panel content.
    /// </summary>
    public Thickness Padding
    {
        get => GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }

    static StaggeredPanel()
    {
        AffectsMeasure<StaggeredPanel>(DesiredColumnWidthProperty, ColumnSpacingProperty, RowSpacingProperty, PaddingProperty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var padding = Padding;
        double availableWidth = Math.Max(0, availableSize.Width - padding.Left - padding.Right);
        double columnSpacing = ColumnSpacing;
        double rowSpacing = RowSpacing;
        double desiredColumnWidth = Math.Max(1, DesiredColumnWidth);

        // Calculate column count
        var (columnCount, actualColumnWidth) = GetColumnMetrics(availableWidth, desiredColumnWidth, columnSpacing);

        var columnHeights = new double[columnCount];
        
        foreach (var child in Children)
        {
            if (!child.IsVisible)
                continue;
                
            child.Measure(new Size(actualColumnWidth, double.PositiveInfinity));
            
            int columnIndex = GetShortestColumnIndex(columnHeights);
            
            if (columnHeights[columnIndex] > 0)
            {
                columnHeights[columnIndex] += rowSpacing;
            }

            columnHeights[columnIndex] += child.DesiredSize.Height;
        }

        double maxHeight = 0;
        foreach (var h in columnHeights)
        {
            if (h > maxHeight) maxHeight = h;
        }

        double resultWidth = double.IsInfinity(availableSize.Width) 
            ? (columnCount * actualColumnWidth + (columnCount - 1) * columnSpacing) + padding.Left + padding.Right 
            : availableSize.Width;
            
        return new Size(resultWidth, maxHeight + padding.Top + padding.Bottom);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var padding = Padding;
        double availableWidth = Math.Max(0, finalSize.Width - padding.Left - padding.Right);
        double rowSpacing = RowSpacing;
        double columnSpacing = ColumnSpacing;
        double desiredColumnWidth = Math.Max(1, DesiredColumnWidth);

        // Recalculate values ensures consistency even if Measure was called with different constraints
        var (columnCount, actualColumnWidth) = GetColumnMetrics(availableWidth, desiredColumnWidth, columnSpacing);

        var columnHeights = new double[columnCount];

        foreach (var child in Children)
        {
            if (!child.IsVisible)
                continue;
                
            int columnIndex = GetShortestColumnIndex(columnHeights);
            
            if (columnHeights[columnIndex] > 0)
            {
                columnHeights[columnIndex] += rowSpacing;
            }

            double x = padding.Left + columnIndex * (actualColumnWidth + columnSpacing);
            double y = padding.Top + columnHeights[columnIndex];

            child.Arrange(new Rect(x, y, actualColumnWidth, child.DesiredSize.Height));

            columnHeights[columnIndex] += child.DesiredSize.Height;
        }

        double maxHeight = 0;
        foreach (var h in columnHeights)
        {
            if (h > maxHeight) maxHeight = h;
        }

        return new Size(finalSize.Width, maxHeight + padding.Top + padding.Bottom);
    }

    private (int count, double width) GetColumnMetrics(double availableWidth, double desiredWidth, double spacing)
    {
        int count = 1;

        double actualWidth = desiredWidth;
        double denominator = desiredWidth + spacing;
        if (!double.IsInfinity(availableWidth) && !double.IsNaN(availableWidth))
        {
            if (denominator > 0)
            {
                count = Math.Max(1, (int)((availableWidth + spacing) / denominator));
            }
            
            if (count > 0)
            {
                actualWidth = Math.Max(1, (availableWidth - (count - 1) * spacing) / count);
            }
        }

        return (count, actualWidth);
    }

    private int GetShortestColumnIndex(double[] heights)
    {
        int index = 0;
        double minHeight = heights[0];
        
        for (int i = 1; i < heights.Length; i++)
        {
            if (heights[i] < minHeight)
            {
                minHeight = heights[i];
                index = i;
            }
        }
        return index;
    }
}
