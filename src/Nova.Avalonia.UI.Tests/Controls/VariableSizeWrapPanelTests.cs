using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class VariableSizeWrapPanelTests
{
    [AvaloniaFact]
    public void Should_Create_Panel_With_Default_Values()
    {
        var panel = new VariableSizeWrapPanel();

        Assert.Equal(100, panel.TileSize);
        Assert.Equal(8, panel.Spacing);
        Assert.Equal(4, panel.Columns);
    }

    [AvaloniaFact]
    public void Should_Arrange_Single_Tile()
    {
        var panel = new VariableSizeWrapPanel { TileSize = 80, Spacing = 8, Columns = 4, Width = 360, Height = 200 };

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
    public void Should_Handle_ColumnSpan()
    {
        var panel = new VariableSizeWrapPanel { TileSize = 80, Spacing = 8, Columns = 4, Width = 360, Height = 200 };

        var child = new Border();
        VariableSizeWrapPanel.SetColumnSpan(child, 2);
        panel.Children.Add(child);

        panel.Measure(new Size(360, 200));
        panel.Arrange(new Rect(0, 0, 360, 200));

        // Width should be 2 * tileSize + 1 * spacing = 168
        Assert.Equal(168, child.Bounds.Width);
    }

    [AvaloniaFact]
    public void Should_Handle_RowSpan()
    {
        var panel = new VariableSizeWrapPanel { TileSize = 80, Spacing = 8, Columns = 4, Width = 360, Height = 300 };

        var child = new Border();
        VariableSizeWrapPanel.SetRowSpan(child, 2);
        panel.Children.Add(child);

        panel.Measure(new Size(360, 300));
        panel.Arrange(new Rect(0, 0, 360, 300));

        // Height should be 2 * tileSize + 1 * spacing = 168
        Assert.Equal(168, child.Bounds.Height);
    }

    [AvaloniaFact]
    public void Should_Flow_Around_Large_Tiles()
    {
        var panel = new VariableSizeWrapPanel { TileSize = 80, Spacing = 8, Columns = 4, Width = 360, Height = 400 };

        var large = new Border();
        VariableSizeWrapPanel.SetColumnSpan(large, 2);
        VariableSizeWrapPanel.SetRowSpan(large, 2);

        var small1 = new Border();
        var small2 = new Border();

        panel.Children.Add(large);
        panel.Children.Add(small1);
        panel.Children.Add(small2);

        panel.Measure(new Size(360, 400));
        panel.Arrange(new Rect(0, 0, 360, 400));

        // Small tiles should be to the right of the large one
        Assert.True(small1.Bounds.X > large.Bounds.X);
    }

    [AvaloniaFact]
    public void Should_Skip_Invisible_Children()
    {
        var panel = new VariableSizeWrapPanel { TileSize = 80, Spacing = 8, Columns = 4, Width = 360, Height = 200 };

        var visible = new Border();
        var invisible = new Border { IsVisible = false };

        panel.Children.Add(visible);
        panel.Children.Add(invisible);

        panel.Measure(new Size(360, 200));
        panel.Arrange(new Rect(0, 0, 360, 200));

        Assert.True(visible.Bounds.Width > 0);
    }
}
