using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class OrbitPanelTests
{
    [AvaloniaFact]
    public void Should_Create_Panel_With_Default_Values()
    {
        var panel = new OrbitPanel();

        Assert.Equal(60, panel.OrbitSpacing);
        Assert.Equal(50, panel.InnerRadius);
        Assert.Equal(0, panel.StartAngle);
    }

    [AvaloniaFact]
    public void Should_Position_Center_Orbit_At_Center()
    {
        var panel = new OrbitPanel { Width = 300, Height = 300 };

        var centerChild = new Border { Width = 40, Height = 40 };
        OrbitPanel.SetOrbit(centerChild, 0);
        panel.Children.Add(centerChild);

        panel.Measure(new Size(300, 300));
        panel.Arrange(new Rect(0, 0, 300, 300));

        // Center should be at (150 - 20, 150 - 20) = (130, 130)
        Assert.True(Math.Abs(centerChild.Bounds.X - 130) < 1);
        Assert.True(Math.Abs(centerChild.Bounds.Y - 130) < 1);
    }

    [AvaloniaFact]
    public void Should_Distribute_Orbit_Children_Evenly()
    {
        var panel = new OrbitPanel { Width = 300, Height = 300, InnerRadius = 60 };

        var child1 = new Border { Width = 20, Height = 20 };
        var child2 = new Border { Width = 20, Height = 20 };
        OrbitPanel.SetOrbit(child1, 1);
        OrbitPanel.SetOrbit(child2, 1);

        panel.Children.Add(child1);
        panel.Children.Add(child2);

        panel.Measure(new Size(300, 300));
        panel.Arrange(new Rect(0, 0, 300, 300));

        // Two children on same orbit should be 180° apart
        double dx = child1.Bounds.X - child2.Bounds.X;
        double dy = child1.Bounds.Y - child2.Bounds.Y;
        double distance = Math.Sqrt(dx * dx + dy * dy);

        // Should be approximately 2 * radius (diameter) apart
        Assert.True(distance > 100);
    }

    [AvaloniaFact]
    public void Should_Handle_Multiple_Orbits()
    {
        var panel = new OrbitPanel { Width = 400, Height = 400, OrbitSpacing = 50, InnerRadius = 50 };

        var orbit1 = new Border { Width = 20, Height = 20 };
        var orbit2 = new Border { Width = 20, Height = 20 };
        OrbitPanel.SetOrbit(orbit1, 1);
        OrbitPanel.SetOrbit(orbit2, 2);

        panel.Children.Add(orbit1);
        panel.Children.Add(orbit2);

        panel.Measure(new Size(400, 400));
        panel.Arrange(new Rect(0, 0, 400, 400));

        // Orbit 2 should be farther from center than orbit 1
        double center = 200;
        double dist1 = Math.Sqrt(Math.Pow(orbit1.Bounds.X + 10 - center, 2) + Math.Pow(orbit1.Bounds.Y + 10 - center, 2));
        double dist2 = Math.Sqrt(Math.Pow(orbit2.Bounds.X + 10 - center, 2) + Math.Pow(orbit2.Bounds.Y + 10 - center, 2));

        Assert.True(dist2 > dist1);
    }

    [AvaloniaFact]
    public void Should_Skip_Invisible_Children()
    {
        var panel = new OrbitPanel { Width = 300, Height = 300 };

        var visible = new Border { Width = 20, Height = 20 };
        var invisible = new Border { Width = 20, Height = 20, IsVisible = false };
        OrbitPanel.SetOrbit(visible, 1);
        OrbitPanel.SetOrbit(invisible, 1);

        panel.Children.Add(visible);
        panel.Children.Add(invisible);

        panel.Measure(new Size(300, 300));
        panel.Arrange(new Rect(0, 0, 300, 300));

        Assert.True(visible.Bounds.Width > 0);
    }
    [AvaloniaFact]
    public void Should_Handle_Zero_Children()
    {
        var panel = new OrbitPanel();
        panel.Measure(new Size(300, 300));
        panel.Arrange(new Rect(0, 0, 300, 300));
        
        Assert.Equal(0, panel.DesiredSize.Width);
        Assert.Equal(0, panel.DesiredSize.Height);
    }

    [AvaloniaFact]
    public void Should_Stack_Multiple_Children_In_Orbit_Zero()
    {
        var panel = new OrbitPanel { Width = 300, Height = 300 };

        var child1 = new Border { Width = 40, Height = 40 };
        var child2 = new Border { Width = 40, Height = 40 };
        OrbitPanel.SetOrbit(child1, 0);
        OrbitPanel.SetOrbit(child2, 0);
        
        panel.Children.Add(child1);
        panel.Children.Add(child2);

        panel.Measure(new Size(300, 300));
        panel.Arrange(new Rect(0, 0, 300, 300));

        // Both should be at center
        Assert.Equal(130, child1.Bounds.X);
        Assert.Equal(130, child2.Bounds.X);
    }

    [AvaloniaFact]
    public void Should_Handle_Sparse_Orbits()
    {
        var panel = new OrbitPanel { Width = 300, Height = 300 };

        var orbit1 = new Border { Width = 20, Height = 20 };
        var orbit10 = new Border { Width = 20, Height = 20 };
        OrbitPanel.SetOrbit(orbit1, 1);
        OrbitPanel.SetOrbit(orbit10, 10);
        
        panel.Children.Add(orbit1);
        panel.Children.Add(orbit10);

        // This should not crash or be extremely slow despite the gap (1 to 10 is fine)
        panel.Measure(new Size(300, 300));
        panel.Arrange(new Rect(0, 0, 300, 300));

        Assert.True(orbit10.Bounds.X != orbit1.Bounds.X);
    }
}
