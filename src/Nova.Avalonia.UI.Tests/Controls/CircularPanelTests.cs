using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class CircularPanelTests
{
    [AvaloniaFact]
    public void Should_Create_Panel_With_Default_Values()
    {
        var panel = new CircularPanel();

        Assert.Equal(100, panel.Radius);
        Assert.Equal(0, panel.StartAngle);
        Assert.True(double.IsNaN(panel.AngleStep));
        Assert.Equal(SweepDirection.Clockwise, panel.SweepDirection);
        Assert.True(panel.KeepInBounds);
    }

    [AvaloniaFact]
    public void Should_Distribute_Children_Evenly()
    {
        var panel = new CircularPanel { Radius = 100, Width = 300, Height = 300 };

        for (int i = 0; i < 4; i++)
        {
            panel.Children.Add(new Border { Width = 20, Height = 20 });
        }

        panel.Measure(new Size(300, 300));
        panel.Arrange(new Rect(0, 0, 300, 300));

        // 4 children should be 90° apart
        // At 0°: x = center + radius = 150 + 100 = 250 (adjusted for child size)
        var child0 = panel.Children[0];
        var child1 = panel.Children[1];

        // Child 0 at 0° should be on the right
        Assert.True(child0.Bounds.X > 150); // Right of center

        // Child 1 at 90° should be at the bottom
        Assert.True(child1.Bounds.Y > 150); // Below center
    }

    [AvaloniaFact]
    public void Should_Respect_Custom_Angle()
    {
        var panel = new CircularPanel { Radius = 100, Width = 300, Height = 300 };

        var child = new Border { Width = 20, Height = 20 };
        CircularPanel.SetAngle(child, 180); // Left side
        panel.Children.Add(child);

        panel.Measure(new Size(300, 300));
        panel.Arrange(new Rect(0, 0, 300, 300));

        // At 180°, x should be left of center (150 - 100 = 50)
        Assert.True(child.Bounds.X < 150);
    }

    [AvaloniaFact]
    public void Should_Respect_Custom_Radius()
    {
        var panel = new CircularPanel { Radius = 100, Width = 300, Height = 300 };

        var child = new Border { Width = 20, Height = 20 };
        CircularPanel.SetItemRadius(child, 50); // Closer to center
        panel.Children.Add(child);

        panel.Measure(new Size(300, 300));
        panel.Arrange(new Rect(0, 0, 300, 300));

        // At radius 50 and angle 0°, x = 150 + 50 - 10 = 190 (center + radius - half width)
        Assert.True(child.Bounds.X < 200); // Closer to center than default radius would be
    }

    [AvaloniaFact]
    public void Should_Handle_CounterClockwise_Orientation()
    {
        var panel = new CircularPanel
        {
            Radius = 100,
            Width = 300,
            Height = 300,
            SweepDirection = SweepDirection.CounterClockwise
        };

        for (int i = 0; i < 4; i++)
        {
            panel.Children.Add(new Border { Width = 20, Height = 20 });
        }

        panel.Measure(new Size(300, 300));
        panel.Arrange(new Rect(0, 0, 300, 300));

        var child1 = panel.Children[1];

        // Counter-clockwise: child 1 at -90° should be at the TOP (y < center)
        Assert.True(child1.Bounds.Y < 150);
    }

    [AvaloniaFact]
    public void Should_Handle_StartAngle()
    {
        var panel = new CircularPanel
        {
            Radius = 100,
            Width = 300,
            Height = 300,
            StartAngle = -90 // Start at top
        };

        var child = new Border { Width = 20, Height = 20 };
        panel.Children.Add(child);

        panel.Measure(new Size(300, 300));
        panel.Arrange(new Rect(0, 0, 300, 300));

        // At -90°, y should be above center
        Assert.True(child.Bounds.Y < 150);
    }

    [AvaloniaFact]
    public void Should_Handle_Zero_Children()
    {
        var panel = new CircularPanel { Radius = 100 };

        panel.Measure(new Size(300, 300));

        // Should not throw and return valid size
        Assert.True(panel.DesiredSize.Width >= 0);
        Assert.True(panel.DesiredSize.Height >= 0);
    }

    [AvaloniaFact]
    public void Should_Handle_Single_Child()
    {
        var panel = new CircularPanel { Radius = 100, Width = 300, Height = 300 };
        panel.Children.Add(new Border { Width = 20, Height = 20 });

        panel.Measure(new Size(300, 300));
        panel.Arrange(new Rect(0, 0, 300, 300));

        // Single child should be at angle 0 (right side)
        var child = panel.Children[0];
        Assert.True(child.Bounds.X > 150);
    }

    [AvaloniaFact]
    public void Should_Skip_Invisible_Children()
    {
        var panel = new CircularPanel { Radius = 100, Width = 300, Height = 300 };

        var visible1 = new Border { Width = 20, Height = 20 };
        var invisible = new Border { Width = 20, Height = 20, IsVisible = false };
        var visible2 = new Border { Width = 20, Height = 20 };

        panel.Children.Add(visible1);
        panel.Children.Add(invisible);
        panel.Children.Add(visible2);

        panel.Measure(new Size(300, 300));
        panel.Arrange(new Rect(0, 0, 300, 300));

        // Only 2 visible children, so they should be 180° apart
        // visible1 at 0° (right), visible2 at 180° (left)
        Assert.True(visible1.Bounds.X > 150); // Right
        Assert.True(visible2.Bounds.X < 150); // Left
    }

    [AvaloniaFact]
    public void Should_Keep_Children_In_Bounds()
    {
        var panel = new CircularPanel
        {
            Radius = 150, // Larger than half container
            Width = 200,
            Height = 200,
            KeepInBounds = true
        };

        var child = new Border { Width = 40, Height = 40 };
        CircularPanel.SetAngle(child, 0); // Right edge
        panel.Children.Add(child);

        panel.Measure(new Size(200, 200));
        panel.Arrange(new Rect(0, 0, 200, 200));

        // Child should be clamped to stay within bounds
        Assert.True(child.Bounds.Right <= 200);
        Assert.True(child.Bounds.Left >= 0);
    }

    [AvaloniaFact]
    public void Should_Respect_AngleStep()
    {
        var panel = new CircularPanel { Radius = 100, Width = 300, Height = 300, AngleStep = 45 };

        for (int i = 0; i < 3; i++)
        {
            panel.Children.Add(new Border { Width = 20, Height = 20 });
        }

        panel.Measure(new Size(300, 300));
        panel.Arrange(new Rect(0, 0, 300, 300));

        // Child 0 at 0°, Child 1 at 45°, Child 2 at 90°
        // We can check if child 1 is between child 0 and child 2
        Assert.True(panel.Children[1].Bounds.X > 150 && panel.Children[1].Bounds.Y > 150);
    }

    [AvaloniaTheory]
    [InlineData(CircularAlignment.Inner)]
    [InlineData(CircularAlignment.Outer)]
    public void Should_Handle_CircularAlignment(CircularAlignment alignment)
    {
        var panel = new CircularPanel { Radius = 100, Width = 300, Height = 300 };
        var child = new Border { Width = 40, Height = 40 };
        CircularPanel.SetAlignment(child, alignment);
        panel.Children.Add(child);

        panel.Measure(new Size(300, 300));
        panel.Arrange(new Rect(0, 0, 300, 300));

        // Center is at 150,150. Radius 100.
        // At Angle 0: Center of item usually at 250, 150.
        // If Alignment is Inner: item should be shifted left (towards center)
        // If Alignment is Outer: item should be shifted right (away from center)
        
        if (alignment == CircularAlignment.Inner)
            Assert.True(child.Bounds.Center.X < 250);
        else if (alignment == CircularAlignment.Outer)
            Assert.True(child.Bounds.Center.X > 250);
    }
}
