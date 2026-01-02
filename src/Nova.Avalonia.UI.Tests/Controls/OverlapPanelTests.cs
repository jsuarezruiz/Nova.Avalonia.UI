using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class OverlapPanelTests
{
    [AvaloniaFact]
    public void OverlapPanel_DefaultValues_AreCorrect()
    {
        var panel = new OverlapPanel();

        Assert.Equal(10, panel.OffsetX);
        Assert.Equal(10, panel.OffsetY);
        Assert.False(panel.ReverseZIndex);
    }

    [AvaloniaFact]
    public void OverlapPanel_WithNoChildren_ReturnsEmptySize()
    {
        var panel = new OverlapPanel();
        
        panel.Measure(new Size(500, 500));
        
        Assert.Equal(new Size(0, 0), panel.DesiredSize);
    }

    [AvaloniaFact]
    public void OverlapPanel_MeasuresCorrectly_WithChildren()
    {
        var panel = new OverlapPanel
        {
            OffsetX = 20,
            OffsetY = 10
        };

        panel.Children.Add(new Border { Width = 100, Height = 50 });
        panel.Children.Add(new Border { Width = 100, Height = 50 });
        panel.Children.Add(new Border { Width = 100, Height = 50 });

        panel.Measure(new Size(1000, 1000));

        // Expected: 100 + 20*2 = 140 width, 50 + 10*2 = 70 height
        Assert.Equal(140, panel.DesiredSize.Width);
        Assert.Equal(70, panel.DesiredSize.Height);
    }

    [AvaloniaFact]
    public void OverlapPanel_ArrangesChildren_WithCorrectOffsets()
    {
        var panel = new OverlapPanel
        {
            OffsetX = 15,
            OffsetY = 10
        };

        var child1 = new Border { Width = 80, Height = 60 };
        var child2 = new Border { Width = 80, Height = 60 };
        var child3 = new Border { Width = 80, Height = 60 };

        panel.Children.Add(child1);
        panel.Children.Add(child2);
        panel.Children.Add(child3);

        panel.Measure(new Size(500, 500));
        panel.Arrange(new Rect(0, 0, 500, 500));

        Assert.Equal(0, child1.Bounds.X);
        Assert.Equal(0, child1.Bounds.Y);

        Assert.Equal(15, child2.Bounds.X);
        Assert.Equal(10, child2.Bounds.Y);

        Assert.Equal(30, child3.Bounds.X);
        Assert.Equal(20, child3.Bounds.Y);
    }

    [AvaloniaFact]
    public void OverlapPanel_SetsZIndex_Correctly()
    {
        var panel = new OverlapPanel { ReverseZIndex = false };

        var child1 = new Border { Width = 50, Height = 50 };
        var child2 = new Border { Width = 50, Height = 50 };
        var child3 = new Border { Width = 50, Height = 50 };

        panel.Children.Add(child1);
        panel.Children.Add(child2);
        panel.Children.Add(child3);

        panel.Measure(new Size(500, 500));
        panel.Arrange(new Rect(0, 0, 500, 500));

        Assert.Equal(0, child1.ZIndex);
        Assert.Equal(1, child2.ZIndex);
        Assert.Equal(2, child3.ZIndex);
    }

    [AvaloniaFact]
    public void OverlapPanel_ReverseZIndex_ReversesOrder()
    {
        var panel = new OverlapPanel { ReverseZIndex = true };

        var child1 = new Border { Width = 50, Height = 50 };
        var child2 = new Border { Width = 50, Height = 50 };
        var child3 = new Border { Width = 50, Height = 50 };

        panel.Children.Add(child1);
        panel.Children.Add(child2);
        panel.Children.Add(child3);

        panel.Measure(new Size(500, 500));
        panel.Arrange(new Rect(0, 0, 500, 500));

        Assert.Equal(2, child1.ZIndex);
        Assert.Equal(1, child2.ZIndex);
        Assert.Equal(0, child3.ZIndex);
    }

    [AvaloniaFact]
    public void OverlapPanel_NegativeOffsets_PositionsCorrectly()
    {
        var panel = new OverlapPanel
        {
            OffsetX = -20,
            OffsetY = -10
        };

        var child1 = new Border { Width = 80, Height = 60 };
        var child2 = new Border { Width = 80, Height = 60 };
        var child3 = new Border { Width = 80, Height = 60 };

        panel.Children.Add(child1);
        panel.Children.Add(child2);
        panel.Children.Add(child3);

        panel.Measure(new Size(500, 500));
        panel.Arrange(new Rect(0, 0, 500, 500));

        // With negative offsets, first item starts at the offset position
        Assert.Equal(40, child1.Bounds.X); // 2 * 20
        Assert.Equal(20, child1.Bounds.Y); // 2 * 10

        Assert.Equal(20, child2.Bounds.X);
        Assert.Equal(10, child2.Bounds.Y);

        Assert.Equal(0, child3.Bounds.X);
        Assert.Equal(0, child3.Bounds.Y);
    }
}
