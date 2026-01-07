using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class ArcPanelTests
{
    [AvaloniaFact]
    public void Should_Create_Panel_With_Default_Values()
    {
        var panel = new ArcPanel();

        Assert.Equal(100, panel.Radius);
        Assert.Equal(0, panel.StartAngle);
        Assert.Equal(180, panel.SweepAngle);
        Assert.True(panel.DistributeEvenly);
    }

    [AvaloniaFact]
    public void Should_Position_Single_Child_At_Start()
    {
        var panel = new ArcPanel { Radius = 100, StartAngle = 0, SweepAngle = 180, Width = 300, Height = 300 };

        var child = new Border { Width = 20, Height = 20 };
        panel.Children.Add(child);

        panel.Measure(new Size(300, 300));
        panel.Arrange(new Rect(0, 0, 300, 300));

        // Single child at angle 0 should be to the right of center
        Assert.True(child.Bounds.X > 150);
    }

    [AvaloniaFact]
    public void Should_Distribute_Children_Along_Arc()
    {
        var panel = new ArcPanel { Radius = 100, StartAngle = -90, SweepAngle = 180, Width = 300, Height = 300 };

        var child1 = new Border { Width = 20, Height = 20 };
        var child2 = new Border { Width = 20, Height = 20 };
        var child3 = new Border { Width = 20, Height = 20 };

        panel.Children.Add(child1);
        panel.Children.Add(child2);
        panel.Children.Add(child3);

        panel.Measure(new Size(300, 300));
        panel.Arrange(new Rect(0, 0, 300, 300));

        // First child at -90° (top)
        Assert.True(child1.Bounds.Y < 150);

        // Last child at 90° (bottom)
        Assert.True(child3.Bounds.Y > 150);
    }

    [AvaloniaFact]
    public void Should_Handle_Full_Circle_Sweep()
    {
        var panel = new ArcPanel { Radius = 80, StartAngle = 0, SweepAngle = 360, Width = 250, Height = 250 };

        for (int i = 0; i < 4; i++)
        {
            panel.Children.Add(new Border { Width = 20, Height = 20 });
        }

        panel.Measure(new Size(250, 250));
        panel.Arrange(new Rect(0, 0, 250, 250));

        // 4 children at 360° should be similar to CircularPanel
        Assert.True(panel.Children[0].Bounds.Width > 0);
    }

    [AvaloniaFact]
    public void Should_Handle_Quarter_Arc()
    {
        var panel = new ArcPanel { Radius = 80, StartAngle = 0, SweepAngle = 90, Width = 250, Height = 250 };

        var child1 = new Border { Width = 20, Height = 20 };
        var child2 = new Border { Width = 20, Height = 20 };

        panel.Children.Add(child1);
        panel.Children.Add(child2);

        panel.Measure(new Size(250, 250));
        panel.Arrange(new Rect(0, 0, 250, 250));

        // Child 1 at 0° should be rightward
        // Child 2 at 90° should be downward
        Assert.True(child2.Bounds.Y > child1.Bounds.Y);
    }

    [AvaloniaFact]
    public void Should_Skip_Invisible_Children()
    {
        var panel = new ArcPanel { Radius = 80, Width = 250, Height = 250 };

        var visible = new Border { Width = 20, Height = 20 };
        var invisible = new Border { Width = 20, Height = 20, IsVisible = false };

        panel.Children.Add(visible);
        panel.Children.Add(invisible);

        panel.Measure(new Size(250, 250));
        panel.Arrange(new Rect(0, 0, 250, 250));

        Assert.True(visible.Bounds.Width > 0);
    }
}
