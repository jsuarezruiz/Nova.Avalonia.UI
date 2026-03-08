using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class TimelinePanelTests
{
    [AvaloniaFact]
    public void Should_Create_Panel_With_Default_Values()
    {
        var panel = new TimelinePanel();

        Assert.Equal(Orientation.Vertical, panel.Orientation);
        Assert.Equal(20, panel.Spacing);
        Assert.Equal(40, panel.ConnectorWidth);
        Assert.False(panel.AlternateItems);
    }

    [AvaloniaFact]
    public void Should_Stack_Vertically()
    {
        var panel = new TimelinePanel { Orientation = Orientation.Vertical, Width = 300, Height = 400 };

        var child1 = new Border { Width = 100, Height = 50 };
        var child2 = new Border { Width = 100, Height = 50 };

        panel.Children.Add(child1);
        panel.Children.Add(child2);

        panel.Measure(new Size(300, 400));
        panel.Arrange(new Rect(0, 0, 300, 400));

        // Child 2 should be below child 1
        Assert.True(child2.Bounds.Y > child1.Bounds.Y);
    }

    [AvaloniaFact]
    public void Should_Stack_Horizontally()
    {
        var panel = new TimelinePanel { Orientation = Orientation.Horizontal, Width = 400, Height = 200 };

        var child1 = new Border { Width = 80, Height = 60 };
        var child2 = new Border { Width = 80, Height = 60 };

        panel.Children.Add(child1);
        panel.Children.Add(child2);

        panel.Measure(new Size(400, 200));
        panel.Arrange(new Rect(0, 0, 400, 200));

        // Child 2 should be to the right of child 1
        Assert.True(child2.Bounds.X > child1.Bounds.X);
    }

    [AvaloniaFact]
    public void Should_Alternate_Items_Vertically()
    {
        var panel = new TimelinePanel
        {
            Orientation = Orientation.Vertical,
            AlternateItems = true,
            ConnectorWidth = 40,
            Width = 300,
            Height = 400
        };

        var child1 = new Border { Width = 100, Height = 50 };
        var child2 = new Border { Width = 100, Height = 50 };

        panel.Children.Add(child1);
        panel.Children.Add(child2);

        panel.Measure(new Size(300, 400));
        panel.Arrange(new Rect(0, 0, 300, 400));

        // Child 1 on left, child 2 on right (alternating)
        Assert.True(child2.Bounds.X > child1.Bounds.X);
    }

    [AvaloniaFact]
    public void Should_Reserve_Connector_Width()
    {
        var panel = new TimelinePanel
        {
            Orientation = Orientation.Vertical,
            ConnectorWidth = 50,
            Width = 300,
            Height = 400
        };

        var child = new Border { Width = 100, Height = 50 };
        panel.Children.Add(child);

        panel.Measure(new Size(300, 400));
        panel.Arrange(new Rect(0, 0, 300, 400));

        // Child should start after connector width
        Assert.True(child.Bounds.X >= 50);
    }

    [AvaloniaFact]
    public void Should_Apply_Item_Spacing()
    {
        var panel = new TimelinePanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 30,
            Width = 300,
            Height = 400
        };

        var child1 = new Border { Width = 100, Height = 50 };
        var child2 = new Border { Width = 100, Height = 50 };

        panel.Children.Add(child1);
        panel.Children.Add(child2);

        panel.Measure(new Size(300, 400));
        panel.Arrange(new Rect(0, 0, 300, 400));

        // Gap should be at least Spacing
        double gap = child2.Bounds.Y - child1.Bounds.Bottom;
        Assert.True(gap >= 30);
    }

    [AvaloniaFact]
    public void Should_Skip_Invisible_Children()
    {
        var panel = new TimelinePanel { Width = 300, Height = 400 };

        var visible = new Border { Width = 100, Height = 50 };
        var invisible = new Border { Width = 100, Height = 50, IsVisible = false };

        panel.Children.Add(visible);
        panel.Children.Add(invisible);

        panel.Measure(new Size(300, 400));
        panel.Arrange(new Rect(0, 0, 300, 400));

        Assert.True(visible.Bounds.Width > 0);
    }

    [AvaloniaFact]
    public void Should_Alternate_Items_Horizontally()
    {
        var panel = new TimelinePanel
        {
            Orientation = Orientation.Horizontal,
            AlternateItems = true,
            ConnectorWidth = 40,
            Width = 600,
            Height = 300
        };

        var child1 = new Border { Width = 100, Height = 50 };
        var child2 = new Border { Width = 100, Height = 50 };

        panel.Children.Add(child1);
        panel.Children.Add(child2);

        panel.Measure(new Size(600, 300));
        panel.Arrange(new Rect(0, 0, 600, 300));

        // Child 1 on top, child 2 on bottom (alternating)
        Assert.True(child2.Bounds.Y > child1.Bounds.Y);
    }

    [AvaloniaFact]
    public void Should_Handle_Infinite_Available_Size()
    {
        var panel = new TimelinePanel { Orientation = Orientation.Vertical, Spacing = 10 };

        panel.Children.Add(new Border { Width = 100, Height = 50 });
        panel.Children.Add(new Border { Width = 100, Height = 50 });

        panel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        // Height should be 50 + 10 + 50 = 110
        // Width should be ConnectorWidth (40) + maxSecondary (100) = 140
        Assert.Equal(110, panel.DesiredSize.Height);
        Assert.Equal(140, panel.DesiredSize.Width);
    }

    [AvaloniaFact]
    public void Should_Handle_Varying_Child_Sizes()
    {
        var panel = new TimelinePanel { Orientation = Orientation.Vertical, Spacing = 10 };

        var child1 = new Border { Width = 50, Height = 100 };
        var child2 = new Border { Width = 150, Height = 50 };

        panel.Children.Add(child1);
        panel.Children.Add(child2);

        panel.Measure(new Size(300, 300));
        panel.Arrange(new Rect(0, 0, 300, 300));

        // Total height = 100 + 10 + 50 = 160
        Assert.Equal(160, panel.DesiredSize.Height);
        // Max width = max(50, 150) + 40 = 190
        Assert.Equal(190, panel.DesiredSize.Width);
    }

    [AvaloniaFact]
    public void Should_Handle_Zero_Children()
    {
        var panel = new TimelinePanel();

        panel.Measure(new Size(300, 300));

        Assert.Equal(0, panel.DesiredSize.Width);
        Assert.Equal(0, panel.DesiredSize.Height);
    }
}
