using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// A panel that arranges child elements in a grid with variable-size tiles using RowSpan and ColumnSpan.
/// </summary>
public class VariableSizeWrapPanel : Panel
{
    /// <summary>
    /// Defines the <see cref="TileSize"/> property.
    /// </summary>
    public static readonly StyledProperty<double> TileSizeProperty =
        AvaloniaProperty.Register<VariableSizeWrapPanel, double>(nameof(TileSize), 100);

    /// <summary>
    /// Defines the <see cref="Spacing"/> property.
    /// </summary>
    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<VariableSizeWrapPanel, double>(nameof(Spacing), 8);

    /// <summary>
    /// Defines the <see cref="Columns"/> property.
    /// </summary>
    public static readonly StyledProperty<int> ColumnsProperty =
        AvaloniaProperty.Register<VariableSizeWrapPanel, int>(nameof(Columns), 4);

    /// <summary>
    /// Defines the ColumnSpan attached property.
    /// </summary>
    public static readonly AttachedProperty<int> ColumnSpanProperty =
        AvaloniaProperty.RegisterAttached<VariableSizeWrapPanel, Control, int>("ColumnSpan", 1);

    /// <summary>
    /// Defines the RowSpan attached property.
    /// </summary>
    public static readonly AttachedProperty<int> RowSpanProperty =
        AvaloniaProperty.RegisterAttached<VariableSizeWrapPanel, Control, int>("RowSpan", 1);

    /// <summary>
    /// Gets or sets the base size of a single tile unit.
    /// </summary>
    public double TileSize
    {
        get => GetValue(TileSizeProperty);
        set => SetValue(TileSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the spacing between tiles.
    /// </summary>
    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the number of columns in the grid.
    /// </summary>
    public int Columns
    {
        get => GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    /// <summary>
    /// Gets the ColumnSpan attached property value.
    /// </summary>
    public static int GetColumnSpan(Control element) => element.GetValue(ColumnSpanProperty);

    /// <summary>
    /// Sets the ColumnSpan attached property value.
    /// </summary>
    public static void SetColumnSpan(Control element, int value) => element.SetValue(ColumnSpanProperty, value);

    /// <summary>
    /// Gets the RowSpan attached property value.
    /// </summary>
    public static int GetRowSpan(Control element) => element.GetValue(RowSpanProperty);

    /// <summary>
    /// Sets the RowSpan attached property value.
    /// </summary>
    public static void SetRowSpan(Control element, int value) => element.SetValue(RowSpanProperty, value);

    static VariableSizeWrapPanel()
    {
        AffectsMeasure<VariableSizeWrapPanel>(TileSizeProperty, SpacingProperty, ColumnsProperty);
        AffectsArrange<VariableSizeWrapPanel>(TileSizeProperty, SpacingProperty, ColumnsProperty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        int columns = Math.Max(1, Columns);
        double tileSize = TileSize;
        double spacing = Spacing;

        // List of rows, where each row is a bool[] of size 'columns'
        var grid = new List<bool[]>();
        int maxRow = 0;
        bool hasVisible = false;

        foreach (var child in Children)
        {
            if (!child.IsVisible) continue;
            hasVisible = true;

            int colSpan = Math.Max(1, Math.Min(GetColumnSpan(child), columns));
            int rowSpan = Math.Max(1, GetRowSpan(child));

            // Find first available position
            var (row, col) = FindAvailablePosition(grid, columns, colSpan, rowSpan);

            // Mark cells as occupied
            MarkCells(grid, columns, row, col, rowSpan, colSpan, true);

            maxRow = Math.Max(maxRow, row + rowSpan);

            // Measure child
            double childWidth = colSpan * tileSize + (colSpan - 1) * spacing;
            double childHeight = rowSpan * tileSize + (rowSpan - 1) * spacing;
            child.Measure(new Size(childWidth, childHeight));
        }

        if (!hasVisible)
            return new Size(0, 0);

        double totalWidth = columns * tileSize + (columns - 1) * spacing;
        double totalHeight = maxRow * tileSize + Math.Max(0, maxRow - 1) * spacing;

        return new Size(totalWidth, totalHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        int columns = Math.Max(1, Columns);
        double tileSize = TileSize;
        double spacing = Spacing;

        var grid = new List<bool[]>();
        int maxRow = 0;

        foreach (var child in Children)
        {
            if (!child.IsVisible) continue;

            int colSpan = Math.Max(1, Math.Min(GetColumnSpan(child), columns));
            int rowSpan = Math.Max(1, GetRowSpan(child));

            var (row, col) = FindAvailablePosition(grid, columns, colSpan, rowSpan);
            MarkCells(grid, columns, row, col, rowSpan, colSpan, true);

            maxRow = Math.Max(maxRow, row + rowSpan);

            double x = col * (tileSize + spacing);
            double y = row * (tileSize + spacing);
            double width = colSpan * tileSize + (colSpan - 1) * spacing;
            double height = rowSpan * tileSize + (rowSpan - 1) * spacing;

            child.Arrange(new Rect(x, y, width, height));
        }

        double totalHeight = maxRow * tileSize + Math.Max(0, maxRow - 1) * spacing;
        return new Size(finalSize.Width, totalHeight);
    }

    private static (int row, int col) FindAvailablePosition(List<bool[]> grid, int columns, int colSpan, int rowSpan)
    {
        // Try to fit in existing rows
        for (int row = 0; row < grid.Count; row++)
        {
            for (int col = 0; col <= columns - colSpan; col++)
            {
                if (CanPlace(grid, row, col, rowSpan, colSpan, columns))
                {
                    return (row, col);
                }
            }
        }

        // If not found in existing rows, place at the start of the first new row
        return (grid.Count, 0);
    }

    private static bool CanPlace(List<bool[]> grid, int startRow, int startCol, int rowSpan, int colSpan, int maxCols)
    {
        if (startCol + colSpan > maxCols)
            return false;

        for (int r = startRow; r < startRow + rowSpan; r++)
        {
            // If the row doesn't exist yet, it's effectively empty, so we can place here (assuming subsequent rows also valid)
            if (r >= grid.Count) 
                continue;

            for (int c = startCol; c < startCol + colSpan; c++)
            {
                if (grid[r][c])
                    return false;
            }
        }
        return true;
    }

    private static void MarkCells(List<bool[]> grid, int columns, int startRow, int startCol, int rowSpan, int colSpan, bool value)
    {
        // Ensure rows exist
        while (grid.Count < startRow + rowSpan)
        {
            grid.Add(new bool[columns]);
        }

        for (int r = startRow; r < startRow + rowSpan; r++)
        {
            for (int c = startCol; c < startCol + colSpan; c++)
            {
                grid[r][c] = value;
            }
        }
    }
}