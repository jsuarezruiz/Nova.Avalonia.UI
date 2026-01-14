using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class VirtualizingVariableSizeWrapPanelTests
{
    [AvaloniaFact]
    public void Should_Create_Panel_With_Default_Values()
    {
        var panel = new VirtualizingVariableSizeWrapPanel();

        Assert.Equal(100, panel.TileSize);
        Assert.Equal(8, panel.Spacing);
        Assert.Equal(4, panel.Columns);
    }

    [AvaloniaFact]
    public void Should_Allow_Setting_Properties()
    {
        var panel = new VirtualizingVariableSizeWrapPanel
        {
            TileSize = 80,
            Spacing = 6,
            Columns = 5
        };

        Assert.Equal(80, panel.TileSize);
        Assert.Equal(6, panel.Spacing);
        Assert.Equal(5, panel.Columns);
    }

    [AvaloniaFact]
    public void Should_Arrange_Single_Tile()
    {
        var panel = new VirtualizingVariableSizeWrapPanel { TileSize = 80, Spacing = 8, Columns = 4, Width = 360, Height = 200 };

        var child = new Border();
        panel.Children.Add(child);

        panel.Measure(new Size(360, 200));
        panel.Arrange(new Rect(0, 0, 360, 200));

        Assert.Equal(0, child.Bounds.X);
        Assert.Equal(0, child.Bounds.Y);
        Assert.Equal(80, child.Bounds.Width);
        Assert.Equal(80, child.Bounds.Height);
    }

    [AvaloniaFact]
    public void Should_Have_Attached_Properties_For_Spans()
    {
        var border = new Border();
        
        VirtualizingVariableSizeWrapPanel.SetColumnSpan(border, 2);
        VirtualizingVariableSizeWrapPanel.SetRowSpan(border, 3);

        Assert.Equal(2, VirtualizingVariableSizeWrapPanel.GetColumnSpan(border));
        Assert.Equal(3, VirtualizingVariableSizeWrapPanel.GetRowSpan(border));
    }

    [AvaloniaFact]
    public void Should_Default_Span_To_One()
    {
        var border = new Border();

        Assert.Equal(1, VirtualizingVariableSizeWrapPanel.GetColumnSpan(border));
        Assert.Equal(1, VirtualizingVariableSizeWrapPanel.GetRowSpan(border));
    }

    [AvaloniaFact]
    public void Should_Handle_ColumnSpan()
    {
        var panel = new VirtualizingVariableSizeWrapPanel { TileSize = 80, Spacing = 8, Columns = 4, Width = 360, Height = 200 };

        var child = new Border();
        VirtualizingVariableSizeWrapPanel.SetColumnSpan(child, 2);
        panel.Children.Add(child);

        panel.Measure(new Size(360, 200));
        panel.Arrange(new Rect(0, 0, 360, 200));

        // Width should be 2 * tileSize + 1 * spacing = 168
        Assert.Equal(168, child.Bounds.Width);
    }

    [AvaloniaFact]
    public void Should_Handle_RowSpan()
    {
        var panel = new VirtualizingVariableSizeWrapPanel { TileSize = 80, Spacing = 8, Columns = 4, Width = 360, Height = 300 };

        var child = new Border();
        VirtualizingVariableSizeWrapPanel.SetRowSpan(child, 2);
        panel.Children.Add(child);

        panel.Measure(new Size(360, 300));
        panel.Arrange(new Rect(0, 0, 360, 300));

        // Height should be 2 * tileSize + 1 * spacing = 168
        Assert.Equal(168, child.Bounds.Height);
    }

    [AvaloniaFact]
    public void Should_Handle_Property_Changes()
    {
        var panel = new VirtualizingVariableSizeWrapPanel();
        
        panel.TileSize = 120;
        Assert.Equal(120, panel.TileSize);

        panel.Spacing = 12;
        Assert.Equal(12, panel.Spacing);

        panel.Columns = 6;
        Assert.Equal(6, panel.Columns);
    }

    [AvaloniaFact]
    public void Should_Handle_Zero_Spacing()
    {
        var panel = new VirtualizingVariableSizeWrapPanel
        {
            TileSize = 100,
            Spacing = 0,
            Columns = 4
        };

        Assert.Equal(0, panel.Spacing);
    }
}
