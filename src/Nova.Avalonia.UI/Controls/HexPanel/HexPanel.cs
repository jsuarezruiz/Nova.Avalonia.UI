using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// A panel that arranges child elements in a honeycomb (hexagonal grid) pattern.
/// Based on https://github.com/AlexanderSharykin/HexGrid by Alexander Sharykin.
/// </summary>
public class HexPanel : Panel
{
    /// <summary>
    /// Defines the <see cref="Orientation"/> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<HexPanel, Orientation>(nameof(Orientation), Orientation.Vertical);

    /// <summary>
    /// Defines the <see cref="ColumnCount"/> property.
    /// </summary>
    public static readonly StyledProperty<int> ColumnCountProperty =
        AvaloniaProperty.Register<HexPanel, int>(nameof(ColumnCount), 3);

    /// <summary>
    /// Defines the <see cref="RowCount"/> property.
    /// </summary>
    public static readonly StyledProperty<int> RowCountProperty =
        AvaloniaProperty.Register<HexPanel, int>(nameof(RowCount), 3);

    /// <summary>
    /// Defines the Column attached property.
    /// </summary>
    public static readonly AttachedProperty<int> ColumnProperty =
        AvaloniaProperty.RegisterAttached<HexPanel, Control, int>("Column", 0);

    /// <summary>
    /// Defines the Row attached property.
    /// </summary>
    public static readonly AttachedProperty<int> RowProperty =
        AvaloniaProperty.RegisterAttached<HexPanel, Control, int>("Row", 0);

    /// <summary>
    /// Gets or sets the orientation of the hexagonal grid.
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Gets or sets the number of columns in the grid.
    /// </summary>
    public int ColumnCount
    {
        get => GetValue(ColumnCountProperty);
        set => SetValue(ColumnCountProperty, value);
    }

    /// <summary>
    /// Gets or sets the number of rows in the grid.
    /// </summary>
    public int RowCount
    {
        get => GetValue(RowCountProperty);
        set => SetValue(RowCountProperty, value);
    }

    /// <summary>
    /// Gets the Column attached property value for an element.
    /// </summary>
    public static int GetColumn(Control element) => element.GetValue(ColumnProperty);

    /// <summary>
    /// Sets the Column attached property value for an element.
    /// </summary>
    public static void SetColumn(Control element, int value) => element.SetValue(ColumnProperty, value);

    /// <summary>
    /// Gets the Row attached property value for an element.
    /// </summary>
    public static int GetRow(Control element) => element.GetValue(RowProperty);

    /// <summary>
    /// Sets the Row attached property value for an element.
    /// </summary>
    public static void SetRow(Control element, int value) => element.SetValue(RowProperty, value);

    static HexPanel()
    {
        AffectsMeasure<HexPanel>(OrientationProperty, ColumnCountProperty, RowCountProperty);
        AffectsArrange<HexPanel>(OrientationProperty, ColumnCountProperty, RowCountProperty);
        AffectsParentMeasure<HexPanel>(ColumnProperty, RowProperty);
        AffectsParentArrange<HexPanel>(ColumnProperty, RowProperty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double maxChildWidth = 0;
        double maxChildHeight = 0;

        foreach (var child in Children)
        {
            if (!child.IsVisible)
                continue;

            child.Measure(Size.Infinity);

            if (child.DesiredSize.Height > maxChildHeight)
                maxChildHeight = child.DesiredSize.Height;

            if (child.DesiredSize.Width > maxChildWidth)
                maxChildWidth = child.DesiredSize.Width;
        }

        // Calculate total size based on orientation
        if (Orientation == Orientation.Horizontal)
        {
            // Horizontal: hexes have pointy tops
            var totalWidth = maxChildWidth * (ColumnCount * 3 + 1) / 4;
            var totalHeight = maxChildHeight * (RowCount * 2 + 1) / 2;
            return new Size(totalWidth, totalHeight);
        }
        else
        {
            // Vertical: hexes have flat tops
            var totalWidth = maxChildWidth * (ColumnCount * 2 + 1) / 2;
            var totalHeight = maxChildHeight * (RowCount * 3 + 1) / 4;
            return new Size(totalWidth, totalHeight);
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        HasShift(out bool firstShift, out bool _);

        var hexSize = GetHexSize(finalSize);

        double columnWidth, rowHeight;

        if (Orientation == Orientation.Horizontal)
        {
            rowHeight = 0.5 * hexSize.Height;
            columnWidth = 0.25 * hexSize.Width;
        }
        else
        {
            rowHeight = 0.25 * hexSize.Height;
            columnWidth = 0.5 * hexSize.Width;
        }

        foreach (var child in Children)
        {
            if (!child.IsVisible)
                continue;

            LayoutChild(child, hexSize, columnWidth, rowHeight, firstShift);
        }

        return finalSize;
    }

    private void LayoutChild(Control element, Size hexSize, double columnWidth, double rowHeight, bool shift)
    {
        int row = Math.Min(GetRow(element), Math.Max(0, RowCount - 1));
        int column = Math.Min(GetColumn(element), Math.Max(0, ColumnCount - 1));

        double x, y;

        if (Orientation == Orientation.Horizontal)
        {
            x = 3 * columnWidth * column;
            y = rowHeight * (2 * row + (column % 2 == 1 ? 1 : 0) + (shift ? -1 : 0));
        }
        else
        {
            x = columnWidth * (2 * column + (row % 2 == 1 ? 1 : 0) + (shift ? -1 : 0));
            y = 3 * rowHeight * row;
        }

        element.Arrange(new Rect(x, y, hexSize.Width, hexSize.Height));
    }

    private void HasShift(out bool first, out bool last)
    {
        if (Orientation == Orientation.Horizontal)
            HasRowShift(out first, out last);
        else
            HasColumnShift(out first, out last);
    }

    private void HasRowShift(out bool firstRow, out bool lastRow)
    {
        firstRow = lastRow = true;

        foreach (var child in Children)
        {
            if (!child.IsVisible)
                continue;

            int row = GetRow(child);
            int column = GetColumn(child);
            int mod = column % 2;

            if (row == 0 && mod == 0)
                firstRow = false;

            if (row == RowCount - 1 && mod == 1)
                lastRow = false;
        }
    }

    private void HasColumnShift(out bool firstColumn, out bool lastColumn)
    {
        firstColumn = lastColumn = true;

        foreach (var child in Children)
        {
            if (!child.IsVisible)
                continue;

            int row = GetRow(child);
            int column = GetColumn(child);
            int mod = row % 2;

            if (column == 0 && mod == 0)
                firstColumn = false;

            if (column == ColumnCount - 1 && mod == 1)
                lastColumn = false;
        }
    }

    private Size GetHexSize(Size gridSize)
    {
        HasShift(out bool first, out bool last);

        var possibleSize = GetPossibleSize(gridSize, first, last);
        return new Size(Math.Max(1, possibleSize.Width), Math.Max(1, possibleSize.Height));
    }

    private Size GetPossibleSize(Size gridSize, bool first, bool last)
    {
        if (Orientation == Orientation.Horizontal)
            return GetPossibleSizeHorizontal(gridSize, first, last);

        return GetPossibleSizeVertical(gridSize, first, last);
    }

    private Size GetPossibleSizeVertical(Size gridSize, bool first, bool last)
    {
        int columns = (first ? 0 : 1) + 2 * ColumnCount - (last ? 1 : 0);
        double w = columns > 0 ? 2 * (gridSize.Width / columns) : gridSize.Width;

        int rows = 1 + 3 * RowCount;
        double h = rows > 0 ? 4 * (gridSize.Height / rows) : gridSize.Height;

        return new Size(w, h);
    }

    private Size GetPossibleSizeHorizontal(Size gridSize, bool first, bool last)
    {
        int columns = 1 + 3 * ColumnCount;
        double w = columns > 0 ? 4 * (gridSize.Width / columns) : gridSize.Width;

        int rows = (first ? 0 : 1) + 2 * RowCount - (last ? 1 : 0);
        double h = rows > 0 ? 2 * (gridSize.Height / rows) : gridSize.Height;

        return new Size(w, h);
    }
}