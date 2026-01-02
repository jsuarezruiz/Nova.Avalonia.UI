using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Media;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class AutoLayoutTests
{
    [AvaloniaFact]
    public void Measures_Vertical_Packed_Correctly()
    {
        var target = new AutoLayout
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Padding = new Thickness(10),
            Justification = AutoLayoutJustify.Packed
        };

        var child1 = new Border { Width = 100, Height = 50 };
        var child2 = new Border { Width = 100, Height = 50 };

        target.Children.Add(child1);
        target.Children.Add(child2);

        target.Measure(Size.Infinity);
        
        // Height: 10(Pad) + 50 + 10(Space) + 50 + 10(Pad) = 130
        // Width: 10(Pad) + 100 + 10(Pad) = 120
        Assert.Equal(new Size(120, 130), target.DesiredSize);
    }

    [AvaloniaFact]
    public void Measures_Horizontal_Packed_Correctly()
    {
        var target = new AutoLayout
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Padding = new Thickness(0),
            Justification = AutoLayoutJustify.Packed
        };

        var child1 = new Border { Width = 50, Height = 100 };
        var child2 = new Border { Width = 50, Height = 120 };

        target.Children.Add(child1);
        target.Children.Add(child2);

        target.Measure(Size.Infinity);

        // Width: 50 + 5 + 50 = 105
        // Height: Max(100, 120) = 120
        Assert.Equal(new Size(105, 120), target.DesiredSize);
    }

    [AvaloniaFact]
    public void SpaceBetween_Distributes_Items()
    {
        var target = new AutoLayout
        {
            Orientation = Orientation.Horizontal,
            Justification = AutoLayoutJustify.SpaceBetween,
            Width = 300, 
            Height = 100
        };

        var child1 = new Border { Width = 50, Height = 50 };
        var child2 = new Border { Width = 50, Height = 50 };
        var child3 = new Border { Width = 50, Height = 50 };

        target.Children.Add(child1);
        target.Children.Add(child2);
        target.Children.Add(child3);

        target.Measure(new Size(300, 100));
        target.Arrange(new Rect(0, 0, 300, 100));

        // Available width for spacing: 300 - (50*3) = 150
        // Gaps: 2 gaps. Spacing = 75.
        // Child1 X: 0
        // Child2 X: 50 + 75 = 125
        // Child3 X: 125 + 50 + 75 = 250
        
        Assert.Equal(0, child1.Bounds.X);
        Assert.Equal(125, child2.Bounds.X);
        Assert.Equal(250, child3.Bounds.X);
    }

    [AvaloniaFact]
    public void Alignment_Horizontal_Center_Moves_Items()
    {
        var target = new AutoLayout
        {
            Orientation = Orientation.Vertical,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Width = 200
        };

        var child = new Border { Width = 100, Height = 50 };
        target.Children.Add(child);

        target.Measure(new Size(200, 200));
        target.Arrange(new Rect(0, 0, 200, 200));

        // Center of 200 is 100. Child width 100.
        // X = (200 - 100) / 2 = 50.
        Assert.Equal(50, child.Bounds.X);
    }

    [AvaloniaFact]
    public void IsAbsolute_Removes_From_Flow()
    {
        var target = new AutoLayout
        {
            Orientation = Orientation.Vertical,
            Spacing = 10
        };

        var child1 = new Border { Width = 100, Height = 50 };
        var child2 = new Border { Width = 100, Height = 50 };
        var absoluteChild = new Border 
        { 
            Width = 20, 
            Height = 20,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };
        AutoLayout.SetIsAbsolute(absoluteChild, true);

        target.Children.Add(child1);
        target.Children.Add(child2);
        target.Children.Add(absoluteChild);

        target.Measure(Size.Infinity);

        // DesiredSize should ignore absoluteChild
        // Height: 50 + 10 + 50 = 110.
        Assert.Equal(110, target.DesiredSize.Height);
        
        target.Arrange(new Rect(0, 0, 100, 110));
        
        // Absolute child gets arranged to full rect (0,0) by default
        Assert.Equal(0, absoluteChild.Bounds.X);
        Assert.Equal(0, absoluteChild.Bounds.Y);
    }
}
