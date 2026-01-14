using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class ResponsivePanelTests
{
    [AvaloniaFact]
    public void Should_Show_Only_Narrow_Content_When_Small()
    {
        var panel = new ResponsivePanel
        {
            NarrowBreakpoint = 600,
            WideBreakpoint = 1000,
            Width = 400,
            Height = 400
        };

        var narrowChild = new Border { Width = 100, Height = 100 };
        ResponsivePanel.SetCondition(narrowChild, ResponsiveBreakpoint.Narrow);

        var normalChild = new Border { Width = 100, Height = 100 };
        ResponsivePanel.SetCondition(normalChild, ResponsiveBreakpoint.Normal);

        panel.Children.Add(narrowChild);
        panel.Children.Add(normalChild);

        panel.Measure(new Size(400, 400));
        panel.Arrange(new Rect(0, 0, 400, 400));

        // Narrow child should be visible
        Assert.True(narrowChild.IsVisible);
        Assert.Equal(100, narrowChild.Bounds.Width);

        // Normal child should be hidden
        Assert.False(normalChild.IsVisible);
    }

    [AvaloniaFact]
    public void Should_Show_Only_Normal_Content_When_Medium()
    {
        var panel = new ResponsivePanel
        {
            NarrowBreakpoint = 600,
            WideBreakpoint = 1000,
            Width = 800
        };

        var narrowChild = new Border();
        ResponsivePanel.SetCondition(narrowChild, ResponsiveBreakpoint.Narrow);

        var normalChild = new Border();
        ResponsivePanel.SetCondition(normalChild, ResponsiveBreakpoint.Normal);

        panel.Children.Add(narrowChild);
        panel.Children.Add(normalChild);

        panel.Measure(new Size(800, 800));

        Assert.False(narrowChild.IsVisible);
        Assert.True(normalChild.IsVisible);
    }

    [AvaloniaFact]
    public void Should_Show_Only_Wide_Content_When_Large()
    {
        var panel = new ResponsivePanel
        {
            NarrowBreakpoint = 600,
            WideBreakpoint = 1000,
            Width = 1200
        };

        var wideChild = new Border();
        ResponsivePanel.SetCondition(wideChild, ResponsiveBreakpoint.Wide);

        var normalChild = new Border();
        ResponsivePanel.SetCondition(normalChild, ResponsiveBreakpoint.Normal);

        panel.Children.Add(wideChild);
        panel.Children.Add(normalChild);

        panel.Measure(new Size(1200, 800));

        Assert.True(wideChild.IsVisible);
        Assert.False(normalChild.IsVisible);
    }

    [AvaloniaFact]
    public void Should_Support_Combined_Conditions()
    {
        var panel = new ResponsivePanel
        {
            NarrowBreakpoint = 600,
            WideBreakpoint = 1000
        };

        var flexibleChild = new Border();
        // Visible on Narrow AND Normal, but not Wide
        ResponsivePanel.SetCondition(flexibleChild, ResponsiveBreakpoint.Narrow | ResponsiveBreakpoint.Normal);

        panel.Children.Add(flexibleChild);

        // Case 1: Narrow
        panel.Measure(new Size(500, 500));
        Assert.True(flexibleChild.IsVisible, "Should be visible on Narrow");

        // Case 2: Normal
        panel.Measure(new Size(800, 800));
        Assert.True(flexibleChild.IsVisible, "Should be visible on Normal");

        // Case 3: Wide
        panel.Measure(new Size(1200, 1200));
        Assert.False(flexibleChild.IsVisible, "Should be hidden on Wide");
    }

    [AvaloniaFact]
    public void Should_Default_To_All_Breakpoints()
    {
        var panel = new ResponsivePanel();
        var child = new Border();
        // No condition set -> Default is All

        panel.Children.Add(child);

        panel.Measure(new Size(100, 100)); // Narrow
        Assert.True(child.IsVisible);

        panel.Measure(new Size(2000, 2000)); // Wide
        Assert.True(child.IsVisible);
    }
    [AvaloniaFact]
    public void Should_Handle_Zero_Children()
    {
        var panel = new ResponsivePanel();
        panel.Measure(new Size(800, 600));
        
        Assert.Equal(0, panel.DesiredSize.Width);
        Assert.Equal(0, panel.DesiredSize.Height);
    }

    [AvaloniaFact]
    public void Should_Default_To_Wide_On_Infinite_Width()
    {
        var panel = new ResponsivePanel();
        
        var wideChild = new Border { Width = 100, Height = 100 };
        ResponsivePanel.SetCondition(wideChild, ResponsiveBreakpoint.Wide);
        
        var narrowChild = new Border { Width = 100, Height = 100 };
        ResponsivePanel.SetCondition(narrowChild, ResponsiveBreakpoint.Narrow);

        panel.Children.Add(wideChild);
        panel.Children.Add(narrowChild);

        panel.Measure(new Size(double.PositiveInfinity, 800));

        Assert.True(wideChild.IsVisible);
        Assert.False(narrowChild.IsVisible);
    }
}
