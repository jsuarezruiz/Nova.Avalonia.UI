using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class LoopPanelTests
{
    [AvaloniaFact]
    public void LoopPanel_Defaults()
    {
        var panel = new LoopPanel();
        Assert.Equal(Orientation.Horizontal, panel.Orientation);
        Assert.Equal(0.0, panel.Offset);
        Assert.Equal(0.5, panel.AnchorPosition);
        Assert.Equal(0.0, panel.Spacing);
        Assert.True(panel.IsInertiaEnabled);
        Assert.True(panel.SnapToItems);
        Assert.Equal(1.0, panel.ScrollFactor);
    }

    [AvaloniaFact]
    public void LoopPanel_Sets_Properties()
    {
        var panel = new LoopPanel
        {
            Orientation = Orientation.Vertical,
            Offset = 2.5,
            AnchorPosition = 0.0,
            Spacing = 10,
            IsInertiaEnabled = false,
            SnapToItems = false,
            ScrollFactor = 2.0
        };

        Assert.Equal(Orientation.Vertical, panel.Orientation);
        Assert.Equal(2.5, panel.Offset);
        Assert.Equal(0.0, panel.AnchorPosition);
        Assert.Equal(10.0, panel.Spacing);
        Assert.False(panel.IsInertiaEnabled);
        Assert.False(panel.SnapToItems);
        Assert.Equal(2.0, panel.ScrollFactor);
    }

    [AvaloniaFact]
    public void LoopPanel_AnchorPosition_Coerced_Above_One()
    {
        var panel = new LoopPanel { AnchorPosition = 1.5 };
        Assert.Equal(1.0, panel.AnchorPosition);
    }

    [AvaloniaFact]
    public void LoopPanel_AnchorPosition_Coerced_Below_Zero()
    {
        var panel = new LoopPanel { AnchorPosition = -0.5 };
        Assert.Equal(0.0, panel.AnchorPosition);
    }

    [AvaloniaFact]
    public void LoopPanel_Offset_Accepts_Negative()
    {
        var panel = new LoopPanel { Offset = -2.5 };
        Assert.Equal(-2.5, panel.Offset);
    }

    [AvaloniaFact]
    public void LoopPanel_Offset_Accepts_Large_Values()
    {
        var panel = new LoopPanel { Offset = 100.5 };
        Assert.Equal(100.5, panel.Offset);
    }

    [AvaloniaFact]
    public void LoopPanel_Children_Can_Be_Added()
    {
        var panel = new LoopPanel();
        var child1 = new Border { Width = 100, Height = 100 };
        var child2 = new Border { Width = 100, Height = 100 };
        
        panel.Children.Add(child1);
        panel.Children.Add(child2);
        
        Assert.Equal(2, panel.Children.Count);
        Assert.Same(child1, panel.Children[0]);
        Assert.Same(child2, panel.Children[1]);
    }

    [AvaloniaFact]
    public void LoopPanel_Children_Can_Be_Removed()
    {
        var panel = new LoopPanel();
        var child = new Border { Width = 100, Height = 100 };
        
        panel.Children.Add(child);
        Assert.Single(panel.Children);
        
        panel.Children.Remove(child);
        Assert.Empty(panel.Children);
    }

    [AvaloniaFact]
    public void LoopPanel_Children_Can_Be_Cleared()
    {
        var panel = new LoopPanel();
        panel.Children.Add(new Border());
        panel.Children.Add(new Border());
        panel.Children.Add(new Border());
        
        Assert.Equal(3, panel.Children.Count);
        
        panel.Children.Clear();
        Assert.Empty(panel.Children);
    }

    [AvaloniaFact]
    public void LoopPanel_ScrollBy_Changes_Offset()
    {
        var panel = new LoopPanel();
        var child = new Border { Width = 100, Height = 100 };
        panel.Children.Add(child);
        
        // Measure the child so it has a DesiredSize
        child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        
        double initialOffset = panel.Offset;
        panel.ScrollBy(50);
        
        Assert.NotEqual(initialOffset, panel.Offset);
    }

    [AvaloniaFact]
    public void LoopPanel_ScrollBy_With_No_Children_Does_Nothing()
    {
        var panel = new LoopPanel();
        double initialOffset = panel.Offset;
        
        panel.ScrollBy(100);
        
        Assert.Equal(initialOffset, panel.Offset);
    }

    [AvaloniaFact]
    public void LoopPanel_ScrollToIndex_Sets_Offset()
    {
        var panel = new LoopPanel { IsInertiaEnabled = false };
        panel.Children.Add(new Border { Width = 100, Height = 100 });
        panel.Children.Add(new Border { Width = 100, Height = 100 });
        panel.Children.Add(new Border { Width = 100, Height = 100 });
        
        panel.ScrollToIndex(2, animate: false);
        
        Assert.Equal(2.0, panel.Offset);
    }

    [AvaloniaFact]
    public void LoopPanel_ScrollToIndex_Wraps_Negative_Index()
    {
        var panel = new LoopPanel { IsInertiaEnabled = false };
        panel.Children.Add(new Border { Width = 100, Height = 100 });
        panel.Children.Add(new Border { Width = 100, Height = 100 });
        panel.Children.Add(new Border { Width = 100, Height = 100 });
        
        panel.ScrollToIndex(-1, animate: false);
        
        Assert.Equal(2.0, panel.Offset); // -1 wraps to last index (2)
    }

    [AvaloniaFact]
    public void LoopPanel_ScrollToIndex_Wraps_Overflow_Index()
    {
        var panel = new LoopPanel { IsInertiaEnabled = false };
        panel.Children.Add(new Border { Width = 100, Height = 100 });
        panel.Children.Add(new Border { Width = 100, Height = 100 });
        panel.Children.Add(new Border { Width = 100, Height = 100 });
        
        panel.ScrollToIndex(5, animate: false);
        
        Assert.Equal(2.0, panel.Offset); // 5 % 3 = 2
    }

    [AvaloniaFact]
    public void LoopPanel_ScrollToIndex_With_No_Children_Does_Nothing()
    {
        var panel = new LoopPanel();
        double initialOffset = panel.Offset;
        
        panel.ScrollToIndex(5);
        
        Assert.Equal(initialOffset, panel.Offset);
    }

    [AvaloniaFact]
    public void LoopPanel_CurrentIndexChanged_Raised()
    {
        var panel = new LoopPanel { IsInertiaEnabled = false };
        panel.Children.Add(new Border { Width = 100, Height = 100 });
        panel.Children.Add(new Border { Width = 100, Height = 100 });
        
        int? raisedIndex = null;
        panel.CurrentIndexChanged += (s, index) => raisedIndex = index;
        
        // Trigger arrange which updates _pivotalChildIndex
        panel.Measure(new Size(500, 200));
        panel.Arrange(new Rect(0, 0, 500, 200));
        
        // Change offset to trigger CurrentIndexChanged
        panel.Offset = 1.0;
        panel.Arrange(new Rect(0, 0, 500, 200));
        
        Assert.NotNull(raisedIndex);
        Assert.Equal(1, raisedIndex);
    }

    [AvaloniaFact]
    public void LoopPanel_Horizontal_Measures_Correctly()
    {
        var panel = new LoopPanel { Orientation = Orientation.Horizontal };
        panel.Children.Add(new Border { Width = 100, Height = 50 });
        panel.Children.Add(new Border { Width = 100, Height = 60 });
        panel.Children.Add(new Border { Width = 100, Height = 40 });
        
        panel.Measure(new Size(500, 200));
        
        Assert.Equal(60, panel.DesiredSize.Height); // Max child height
        Assert.Equal(300, panel.DesiredSize.Width); // Sum of child widths
    }

    [AvaloniaFact]
    public void LoopPanel_Vertical_Measures_Correctly()
    {
        var panel = new LoopPanel { Orientation = Orientation.Vertical };
        panel.Children.Add(new Border { Width = 50, Height = 100 });
        panel.Children.Add(new Border { Width = 60, Height = 100 });
        panel.Children.Add(new Border { Width = 40, Height = 100 });
        
        panel.Measure(new Size(200, 500));
        
        Assert.Equal(60, panel.DesiredSize.Width); // Max child width
        Assert.Equal(300, panel.DesiredSize.Height); // Sum of child heights
    }

    [AvaloniaFact]
    public void LoopPanel_Spacing_Affects_Measure()
    {
        var panel = new LoopPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        panel.Children.Add(new Border { Width = 100, Height = 50 });
        panel.Children.Add(new Border { Width = 100, Height = 50 });
        panel.Children.Add(new Border { Width = 100, Height = 50 });
        
        panel.Measure(new Size(500, 200));
        
        // 3 children * 100px + 2 gaps * 10px = 320px
        Assert.Equal(320, panel.DesiredSize.Width);
    }

    [AvaloniaFact]
    public void LoopPanel_ClipToBounds_Is_Enabled()
    {
        var panel = new LoopPanel();
        Assert.True(panel.ClipToBounds);
    }

    [AvaloniaFact]
    public void LoopPanel_Empty_Panel_Arranges_Without_Error()
    {
        var panel = new LoopPanel();
        
        panel.Measure(new Size(500, 200));
        panel.Arrange(new Rect(0, 0, 500, 200));
        
        // Should complete without throwing
        Assert.True(true);
    }

    [AvaloniaFact]
    public void LoopPanel_Single_Child_Arranges_At_Anchor()
    {
        var panel = new LoopPanel { AnchorPosition = 0.5 };
        var child = new Border { Width = 100, Height = 100 };
        panel.Children.Add(child);
        
        panel.Measure(new Size(500, 200));
        panel.Arrange(new Rect(0, 0, 500, 200));
        
        // At AnchorPosition 0.5, child should start at (500 * 0.5) - (100 * 0) = 250
        Assert.Equal(250, child.Bounds.X);
    }

    [AvaloniaFact]
    public void LoopPanel_AnchorPosition_Zero_Starts_At_Left()
    {
        var panel = new LoopPanel { AnchorPosition = 0.0 };
        var child = new Border { Width = 100, Height = 100 };
        panel.Children.Add(child);
        
        panel.Measure(new Size(500, 200));
        panel.Arrange(new Rect(0, 0, 500, 200));
        
        Assert.Equal(0, child.Bounds.X);
    }

    [AvaloniaFact]
    public void LoopPanel_AnchorPosition_One_Starts_At_Right()
    {
        var panel = new LoopPanel { AnchorPosition = 1.0 };
        var child = new Border { Width = 100, Height = 100 };
        panel.Children.Add(child);
        
        panel.Measure(new Size(500, 200));
        panel.Arrange(new Rect(0, 0, 500, 200));
        
        // At AnchorPosition 1.0, anchor point is at 500, child starts at 500
        Assert.Equal(500, child.Bounds.X);
    }
}
