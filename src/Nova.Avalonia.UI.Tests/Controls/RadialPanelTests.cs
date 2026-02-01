using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class RadialPanelTests
{
    [AvaloniaFact]
    public void Should_Create_Panel_With_Default_Values()
    {
        var panel = new RadialPanel();

        Assert.Equal(100, panel.Radius);
        Assert.Equal(0, panel.StartAngle);
        Assert.Equal(360, panel.SweepAngle);
        Assert.True(panel.RotateItems);
    }

    [AvaloniaFact]
    public void Should_Position_Children_Radially()
    {
        var panel = new RadialPanel { Radius = 100, Width = 300, Height = 300 };

        for (int i = 0; i < 4; i++)
        {
            panel.Children.Add(new Border { Width = 20, Height = 20 });
        }

        panel.Measure(new Size(300, 300));
        panel.Arrange(new Rect(0, 0, 300, 300));

        // 4 children at 360° should be 90° apart
        // child 0 at 0° (right), child 1 at 90° (bottom)
        Assert.True(panel.Children[0].Bounds.X > 150);
        Assert.True(panel.Children[1].Bounds.Y > 150);
    }

    [AvaloniaFact]
    public void Should_Handle_Zero_Children()
    {
        var panel = new RadialPanel();
        panel.Measure(new Size(300, 300));
        Assert.Equal(0, panel.DesiredSize.Width);
        Assert.Equal(0, panel.DesiredSize.Height);
    }

    [AvaloniaFact]
    public void Should_Skip_Invisible_Children()
    {
        var panel = new RadialPanel { Radius = 100, SweepAngle = 180 };

        var visible1 = new Border { Width = 20, Height = 20 };
        var invisible = new Border { Width = 20, Height = 20, IsVisible = false };
        var visible2 = new Border { Width = 20, Height = 20 };

        panel.Children.Add(visible1);
        panel.Children.Add(invisible);
        panel.Children.Add(visible2);

        panel.Measure(new Size(300, 300));
        panel.Arrange(new Rect(0, 0, 300, 300));

        // Only 2 visible children -> 180° apart
        Assert.True(visible1.Bounds.Center.X > 150);
        Assert.True(visible2.Bounds.Center.X < 150);
    }

    [AvaloniaFact]
    public void Should_Respect_StartAngle()
    {
        var panel = new RadialPanel { Radius = 100, Width = 300, Height = 300, StartAngle = 90 };
        var child = new Border { Width = 20, Height = 20 };
        panel.Children.Add(child);

        panel.Measure(new Size(300, 300));
        panel.Arrange(new Rect(0, 0, 300, 300));

        // Start angle 90° is down. Center is 150,150. Radius 100 -> Child at 150, 250
        Assert.Equal(150, child.Bounds.Center.X, 1);
        Assert.Equal(250, child.Bounds.Center.Y, 1);
    }

    [AvaloniaFact]
    public void Should_Respect_SweepAngle_Distribution()
    {
        var panel = new RadialPanel { Radius = 100, Width = 300, Height = 300, SweepAngle = 180 };
        
        var child1 = new Border { Width = 20, Height = 20 };
        var child2 = new Border { Width = 20, Height = 20 };
        var child3 = new Border { Width = 20, Height = 20 };

        panel.Children.Add(child1);
        panel.Children.Add(child2);
        panel.Children.Add(child3);

        panel.Measure(new Size(300, 300));
        panel.Arrange(new Rect(0, 0, 300, 300));

        // 3 children, 180° sweep -> 0°, 90°, 180°
        Assert.Equal(250, child1.Bounds.Center.X, 1); // 0°
        Assert.Equal(150, child2.Bounds.Center.X, 1); // 90° (down)
        Assert.Equal(250, child2.Bounds.Center.Y, 1);
        Assert.Equal(50, child3.Bounds.Center.X, 1);  // 180°
    }

    [AvaloniaFact]
    public void Should_Handle_ItemAngle_And_RotateItems()
    {
        var panel = new RadialPanel { Radius = 100, RotateItems = true, ItemAngle = 45 };
        var child = new Border { Width = 20, Height = 20 };
        panel.Children.Add(child);

        panel.Measure(new Size(300, 300));
        panel.Arrange(new Rect(0, 0, 300, 300));

        Assert.NotNull(child.RenderTransform);
        // Default angle 0 + 90 + ItemAngle 45 = 135
        var rotation = child.RenderTransform as global::Avalonia.Media.RotateTransform;
        Assert.NotNull(rotation);
        Assert.Equal(135, rotation.Angle);
    }

    [AvaloniaFact]
    public void Should_Disable_Rotation_When_Requested()
    {
        var panel = new RadialPanel { Radius = 100, RotateItems = false };
        var child = new Border { Width = 20, Height = 20 };
        panel.Children.Add(child);

        panel.Measure(new Size(300, 300));
        panel.Arrange(new Rect(0, 0, 300, 300));

        Assert.Null(child.RenderTransform);
    }
}
