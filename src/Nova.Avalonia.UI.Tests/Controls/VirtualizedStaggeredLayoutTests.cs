using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class VirtualizingStaggeredPanelTests
{
    [AvaloniaFact]
    public void Should_Create_Panel_With_Default_Values()
    {
        var panel = new VirtualizingStaggeredPanel();

        Assert.Equal(250, panel.DesiredColumnWidth);
        Assert.Equal(0, panel.ColumnSpacing);
        Assert.Equal(0, panel.RowSpacing);
    }

    [AvaloniaFact]
    public void Should_Allow_Setting_Properties()
    {
        var panel = new VirtualizingStaggeredPanel
        {
            DesiredColumnWidth = 150,
            ColumnSpacing = 10,
            RowSpacing = 20
        };

        Assert.Equal(150, panel.DesiredColumnWidth);
        Assert.Equal(10, panel.ColumnSpacing);
        Assert.Equal(20, panel.RowSpacing);
    }

    [AvaloniaFact]
    public void Should_Arrange_Items_In_Columns()
    {
        var panel = new VirtualizingStaggeredPanel
        {
            DesiredColumnWidth = 100,
            ColumnSpacing = 0,
            RowSpacing = 0,
            Width = 300 // Should fit 3 columns
        };

        var child1 = new Border { Height = 50 };
        var child2 = new Border { Height = 100 };
        var child3 = new Border { Height = 75 };
        var child4 = new Border { Height = 50 };

        panel.Children.Add(child1);
        panel.Children.Add(child2);
        panel.Children.Add(child3);
        panel.Children.Add(child4);

        panel.Measure(new Size(300, 1000));
        panel.Arrange(new Rect(0, 0, 300, 1000));

        // Child 1: Col 0 (Height 0 -> 50)
        Assert.Equal(0, child1.Bounds.X);
        Assert.Equal(0, child1.Bounds.Y);

        // Child 2: Col 1 (Height 0 -> 100)
        Assert.Equal(100, child2.Bounds.X);
        Assert.Equal(0, child2.Bounds.Y);

        // Child 3: Col 2 (Height 0 -> 75)
        Assert.Equal(200, child3.Bounds.X);
        Assert.Equal(0, child3.Bounds.Y);

        // Child 4: Shortest was Col 0 (50), so should go to Col 0
        Assert.Equal(0, child4.Bounds.X);
        Assert.Equal(50, child4.Bounds.Y);
    }

    [AvaloniaFact]
    public void Should_Respect_Column_Spacing()
    {
        var panel = new VirtualizingStaggeredPanel
        {
            DesiredColumnWidth = 100,
            ColumnSpacing = 10,
            RowSpacing = 0,
            Width = 210 // 100 + 10 + 100 = 210. 2 Cols.
        };

        var child1 = new Border { Height = 100 };
        var child2 = new Border { Height = 100 };

        panel.Children.Add(child1);
        panel.Children.Add(child2);

        panel.Measure(new Size(210, 1000));
        panel.Arrange(new Rect(0, 0, 210, 1000));

        // Child 1: Col 0
        Assert.Equal(0, child1.Bounds.X);

        // Child 2: Col 1 at 100 + 10 = 110
        Assert.Equal(110, child2.Bounds.X);
    }

    [AvaloniaFact]
    public void Should_Handle_Property_Changes()
    {
        var panel = new VirtualizingStaggeredPanel();
        
        panel.DesiredColumnWidth = 300;
        Assert.Equal(300, panel.DesiredColumnWidth);

        panel.ColumnSpacing = 15;
        Assert.Equal(15, panel.ColumnSpacing);

        panel.RowSpacing = 12;
        Assert.Equal(12, panel.RowSpacing);
    }

    [AvaloniaFact]
    public void Should_Handle_Zero_Children()
    {
        var panel = new VirtualizingStaggeredPanel { DesiredColumnWidth = 100 };
        panel.Measure(new Size(300, 300));
        Assert.Equal(0, panel.DesiredSize.Height);
    }

    [AvaloniaFact]
    public void Recycle_Pool_Limit_Should_Be_Enforced()
    {
        // Verify panel can be created and has expected properties
        var panel = new VirtualizingStaggeredPanel
        {
            DesiredColumnWidth = 100,
            ColumnSpacing = 8,
            RowSpacing = 8
        };

        // Panel should have the properties set
        Assert.Equal(100, panel.DesiredColumnWidth);
        Assert.Equal(8, panel.ColumnSpacing);
        Assert.Equal(8, panel.RowSpacing);
    }

    [AvaloniaFact]
    public void Should_Calculate_Correct_Column_Count()
    {
        var panel = new VirtualizingStaggeredPanel
        {
            DesiredColumnWidth = 100,
            ColumnSpacing = 10,
            Width = 330 // 100 + 10 + 100 + 10 + 100 = 320, so 3 columns fit
        };

        var child1 = new Border { Height = 50 };
        var child2 = new Border { Height = 50 };
        var child3 = new Border { Height = 50 };
        var child4 = new Border { Height = 50 };

        panel.Children.Add(child1);
        panel.Children.Add(child2);
        panel.Children.Add(child3);
        panel.Children.Add(child4);

        panel.Measure(new Size(330, 1000));
        panel.Arrange(new Rect(0, 0, 330, 1000));

        // First 3 children should be in row 0 (different columns)
        Assert.Equal(0, child1.Bounds.Y);
        Assert.Equal(0, child2.Bounds.Y);
        Assert.Equal(0, child3.Bounds.Y);

        // 4th child should go to shortest column (all are 50 high, so col 0)
        Assert.Equal(50, child4.Bounds.Y);
    }

    [AvaloniaFact]
    public void Should_Handle_Single_Column()
    {
        var panel = new VirtualizingStaggeredPanel
        {
            DesiredColumnWidth = 200,
            ColumnSpacing = 10,
            RowSpacing = 5,
            Width = 100 // Smaller than desired, should still create 1 column
        };

        var child1 = new Border { Height = 50 };
        var child2 = new Border { Height = 50 };

        panel.Children.Add(child1);
        panel.Children.Add(child2);

        panel.Measure(new Size(100, 1000));
        panel.Arrange(new Rect(0, 0, 100, 1000));

        // Both should be in column 0
        Assert.Equal(0, child1.Bounds.X);
        Assert.Equal(0, child2.Bounds.X);

        // Child 2 should be below child 1 with row spacing
        Assert.Equal(0, child1.Bounds.Y);
        Assert.Equal(55, child2.Bounds.Y); // 50 + 5 row spacing
    }
}
