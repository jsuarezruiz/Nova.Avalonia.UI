using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class BubblePanelTests
{
    [AvaloniaFact]
    public void Should_Create_Panel_With_Default_Values()
    {
        var panel = new BubblePanel();

        Assert.Equal(4, panel.Spacing);
        Assert.Equal(new Thickness(0), panel.Padding);
    }

    [AvaloniaFact]
    public void Should_Position_Single_Child_At_Center()
    {
        var panel = new BubblePanel { Width = 200, Height = 200 };

        var child = new Border { Width = 40, Height = 40 };
        panel.Children.Add(child);

        panel.Measure(new Size(200, 200));
        panel.Arrange(new Rect(0, 0, 200, 200));

        // Child should be approximately centered
        Assert.True(child.Bounds.X > 60 && child.Bounds.X < 100);
        Assert.True(child.Bounds.Y > 60 && child.Bounds.Y < 100);
    }

    [AvaloniaFact]
    public void Should_Pack_Multiple_Children()
    {
        var panel = new BubblePanel { Width = 300, Height = 300, Spacing = 4 };

        var child1 = new Border { Width = 60, Height = 60 };
        var child2 = new Border { Width = 40, Height = 40 };

        panel.Children.Add(child1);
        panel.Children.Add(child2);

        panel.Measure(new Size(300, 300));
        panel.Arrange(new Rect(0, 0, 300, 300));

        // Both children should be positioned
        Assert.True(child1.Bounds.Width > 0);
        Assert.True(child2.Bounds.Width > 0);

        // They should not overlap (with spacing)
        // Check center distance > sum of radii + spacing
        double dx = (child1.Bounds.X + 30) - (child2.Bounds.X + 20);
        double dy = (child1.Bounds.Y + 30) - (child2.Bounds.Y + 20);
        double distance = System.Math.Sqrt(dx * dx + dy * dy);

        Assert.True(distance >= 30 + 20); // sum of radii
    }

    [AvaloniaFact]
    public void Should_Handle_Different_Sized_Items()
    {
        var panel = new BubblePanel { Width = 300, Height = 300 };

        var large = new Border { Width = 80, Height = 80 };
        var small = new Border { Width = 40, Height = 40 };

        panel.Children.Add(large);
        panel.Children.Add(small);

        panel.Measure(new Size(300, 300));
        panel.Arrange(new Rect(0, 0, 300, 300));

        // Both should be arranged with their respective sizes
        Assert.Equal(80, large.Bounds.Width);
        Assert.Equal(40, small.Bounds.Width);
    }

    [AvaloniaFact]
    public void Should_Skip_Invisible_Children()
    {
        var panel = new BubblePanel { Width = 200, Height = 200 };

        var visible = new Border { Width = 40, Height = 40 };
        var invisible = new Border { Width = 40, Height = 40, IsVisible = false };

        panel.Children.Add(visible);
        panel.Children.Add(invisible);

        panel.Measure(new Size(200, 200));
        panel.Arrange(new Rect(0, 0, 200, 200));

        Assert.True(visible.Bounds.Width > 0);
    }

    [AvaloniaFact]
    public void Should_Handle_Zero_Children()
    {
        var panel = new BubblePanel();

        panel.Measure(new Size(200, 200));

        Assert.Equal(0, panel.DesiredSize.Width);
        Assert.Equal(0, panel.DesiredSize.Height);
    }

    [AvaloniaFact]
    public void Should_Respect_Padding()
    {
        var padding = new Thickness(20);
        var panel = new BubblePanel { Width = 300, Height = 300, Padding = padding };

        var child = new Border { Width = 50, Height = 50 };
        panel.Children.Add(child);

        panel.Measure(new Size(300, 300));
        panel.Arrange(new Rect(0, 0, 300, 300));

        // Child should be within the padded area
        Assert.True(child.Bounds.X >= padding.Left);
        Assert.True(child.Bounds.Y >= padding.Top);
        Assert.True(child.Bounds.Right <= 300 - padding.Right);
        Assert.True(child.Bounds.Bottom <= 300 - padding.Bottom);
    }
}
