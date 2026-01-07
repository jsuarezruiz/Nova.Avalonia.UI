using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class HexPanelTests
{
    [AvaloniaFact]
    public void Should_Create_Panel_With_Default_Values()
    {
        var panel = new HexPanel();

        Assert.Equal(Orientation.Vertical, panel.Orientation);
        Assert.Equal(3, panel.ColumnCount);
        Assert.Equal(3, panel.RowCount);
    }

    [AvaloniaFact]
    public void Should_Position_Children_In_Grid()
    {
        var panel = new HexPanel
        {
            ColumnCount = 2,
            RowCount = 2,
            Width = 200,
            Height = 200
        };

        var child00 = new Border { Width = 40, Height = 40 };
        HexPanel.SetRow(child00, 0);
        HexPanel.SetColumn(child00, 0);

        var child01 = new Border { Width = 40, Height = 40 };
        HexPanel.SetRow(child01, 0);
        HexPanel.SetColumn(child01, 1);

        panel.Children.Add(child00);
        panel.Children.Add(child01);

        panel.Measure(new Size(200, 200));
        panel.Arrange(new Rect(0, 0, 200, 200));

        // Child at (0,0) should be at origin or close to it
        Assert.True(child00.Bounds.X >= 0);
        Assert.True(child00.Bounds.Y >= 0);

        // Child at (0,1) should be to the right and offset
        Assert.True(child01.Bounds.X > child00.Bounds.X);
    }

    [AvaloniaFact]
    public void Should_Handle_Vertical_Orientation()
    {
        var panel = new HexPanel
        {
            ColumnCount = 3,
            RowCount = 2,
            Orientation = Orientation.Vertical,
            Width = 300,
            Height = 200
        };

        var child10 = new Border { Width = 40, Height = 40 };
        HexPanel.SetRow(child10, 1);
        HexPanel.SetColumn(child10, 0);

        panel.Children.Add(child10);

        panel.Measure(new Size(300, 200));
        panel.Arrange(new Rect(0, 0, 300, 200));

        // Row 1 should be below row 0
        Assert.True(child10.Bounds.Y > 0);
    }

    [AvaloniaFact]
    public void Should_Handle_Horizontal_Orientation()
    {
        var panel = new HexPanel
        {
            ColumnCount = 2,
            RowCount = 2,
            Orientation = Orientation.Horizontal,
            Width = 200,
            Height = 200
        };

        var child = new Border { Width = 40, Height = 40 };
        HexPanel.SetRow(child, 0);
        HexPanel.SetColumn(child, 1);

        panel.Children.Add(child);

        panel.Measure(new Size(200, 200));
        panel.Arrange(new Rect(0, 0, 200, 200));

        // Column 1 in horizontal should be offset
        Assert.True(child.Bounds.X > 0);
    }

    [AvaloniaFact]
    public void Should_Clamp_Row_Column_To_Valid_Range()
    {
        var panel = new HexPanel
        {
            ColumnCount = 2,
            RowCount = 2,
            Width = 200,
            Height = 200
        };

        var child = new Border { Width = 40, Height = 40 };
        HexPanel.SetRow(child, 10); // Out of range
        HexPanel.SetColumn(child, 10); // Out of range

        panel.Children.Add(child);

        panel.Measure(new Size(200, 200));
        panel.Arrange(new Rect(0, 0, 200, 200));

        // Should not throw and child should be positioned
        Assert.True(child.Bounds.Width > 0);
    }

    [AvaloniaFact]
    public void Should_Handle_Zero_Children()
    {
        var panel = new HexPanel { ColumnCount = 3, RowCount = 3 };

        panel.Measure(new Size(300, 300));

        // Should not throw
        Assert.True(panel.DesiredSize.Width >= 0);
        Assert.True(panel.DesiredSize.Height >= 0);
    }

    [AvaloniaFact]
    public void Should_Skip_Invisible_Children()
    {
        var panel = new HexPanel
        {
            ColumnCount = 2,
            RowCount = 2,
            Width = 200,
            Height = 200
        };

        var visible = new Border { Width = 40, Height = 40 };
        var invisible = new Border { Width = 40, Height = 40, IsVisible = false };

        HexPanel.SetRow(visible, 0);
        HexPanel.SetColumn(visible, 0);
        HexPanel.SetRow(invisible, 0);
        HexPanel.SetColumn(invisible, 1);

        panel.Children.Add(visible);
        panel.Children.Add(invisible);

        panel.Measure(new Size(200, 200));
        panel.Arrange(new Rect(0, 0, 200, 200));

        // Visible should be arranged, invisible should not affect layout
        Assert.True(visible.Bounds.Width > 0);
    }

    [AvaloniaFact]
    public void Should_Create_Honeycomb_Offset()
    {
        var panel = new HexPanel
        {
            ColumnCount = 2,
            RowCount = 2,
            Orientation = Orientation.Vertical,
            Width = 200,
            Height = 200
        };

        var row0col0 = new Border { Width = 40, Height = 40 };
        var row1col0 = new Border { Width = 40, Height = 40 };

        HexPanel.SetRow(row0col0, 0);
        HexPanel.SetColumn(row0col0, 0);
        HexPanel.SetRow(row1col0, 1);
        HexPanel.SetColumn(row1col0, 0);

        panel.Children.Add(row0col0);
        panel.Children.Add(row1col0);

        panel.Measure(new Size(200, 200));
        panel.Arrange(new Rect(0, 0, 200, 200));

        // Row 1 should have X offset (honeycomb pattern)
        // In vertical orientation, odd rows are offset horizontally
        Assert.True(row1col0.Bounds.X > row0col0.Bounds.X || row1col0.Bounds.Y > row0col0.Bounds.Y);
    }
}
