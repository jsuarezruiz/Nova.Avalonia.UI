using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class StaggeredPanelTests
{
    [AvaloniaFact]
    public void Should_Arrange_Items_In_Columns()
    {
        var panel = new StaggeredPanel
        {
            DesiredColumnWidth = 100,
            ColumnSpacing = 0,
            RowSpacing = 0,
            Width = 300 // Should fit 3 columns
        };

        var child1 = new Border { Width = 100, Height = 50 };
        var child2 = new Border { Width = 100, Height = 100 };
        var child3 = new Border { Width = 100, Height = 75 };
        var child4 = new Border { Width = 100, Height = 50 };

        panel.Children.Add(child1);
        panel.Children.Add(child2);
        panel.Children.Add(child3);
        panel.Children.Add(child4);

        panel.Measure(new Size(300, 1000));
        panel.Arrange(new Rect(0, 0, 300, 1000));

        // Child 1: Col 0 (Height 0 -> 50)
        Assert.Equal(0, child1.Bounds.X);
        Assert.Equal(0, child1.Bounds.Y);

        // Child 2: Col 1 (Height 0 -> 100) - Shortest is Col 1 (0)
        Assert.Equal(100, child2.Bounds.X);
        Assert.Equal(0, child2.Bounds.Y);

        // Child 3: Col 2 (Height 0 -> 75) - Shortest is Col 2 (0)
        Assert.Equal(200, child3.Bounds.X);
        Assert.Equal(0, child3.Bounds.Y);

        // Child 4: Shortest was Col 0 (50), Col 1 (100), Col 2 (75) -> Should go to Col 0
        Assert.Equal(0, child4.Bounds.X);
        Assert.Equal(50, child4.Bounds.Y);
    }

    [AvaloniaFact]
    public void Should_Respect_Spacing()
    {
        var panel = new StaggeredPanel
        {
            DesiredColumnWidth = 100,
            ColumnSpacing = 10,
            RowSpacing = 20,
            Width = 210 // 100 + 10 + 100 = 210. 2 Cols.
        };

        var child1 = new Border { Width = 100, Height = 100 };
        var child2 = new Border { Width = 100, Height = 100 };
        var child3 = new Border { Width = 100, Height = 100 };

        panel.Children.Add(child1);
        panel.Children.Add(child2);
        panel.Children.Add(child3);

        panel.Measure(new Size(210, 1000));
        panel.Arrange(new Rect(0, 0, 210, 1000));

        // Child 1: Col 0
        Assert.Equal(0, child1.Bounds.X);

        // Child 2: Col 1 at 100 + 10 = 110
        Assert.Equal(110, child2.Bounds.X);

        // Child 3: Col 0 at 100 + 20 (RowSpacing) = 120
        Assert.Equal(0, child3.Bounds.X);
        Assert.Equal(120, child3.Bounds.Y);
    }

    [AvaloniaFact]
    public void Should_Handle_Padding()
    {
        var panel = new StaggeredPanel
        {
            DesiredColumnWidth = 100,
            ColumnSpacing = 0,
            RowSpacing = 0,
            Padding = new Thickness(10, 20, 30, 40),
            Width = 240 // 240 - 10(L) - 30(R) = 200 available. 2 columns of 100.
        };

        var child1 = new Border { Width = 100, Height = 50 };
        panel.Children.Add(child1);

        panel.Measure(new Size(240, 1000));
        panel.Arrange(new Rect(0, 0, 240, 1000));

        // Top-Left should be at (Padding.Left, Padding.Top)
        Assert.Equal(10, child1.Bounds.X);
        Assert.Equal(20, child1.Bounds.Y);
        
        // Panel DesiredSize should include padding
        Assert.Equal(240, panel.Bounds.Width);
        Assert.Equal(50 + 20 + 40, panel.Bounds.Height); // Height + Top + Bottom
    }

    [AvaloniaFact]
    public void Should_Determine_Columns_With_Infinite_Width()
    {
        // When width is infinite (e.g., inside horizontal scroll viewer), 
        // fallback logic should apply (typically 1 column or based on desired width if we implemented max width constraint logic)
        // Our implementation currently defaults to 1 column or children count logic fallback if available width is infinite.
        
        var panel = new StaggeredPanel
        {
            DesiredColumnWidth = 100,
            ColumnSpacing = 10
        };

        var child1 = new Border { Width = 100, Height = 50 };
        var child2 = new Border { Width = 100, Height = 50 };
        panel.Children.Add(child1);
        panel.Children.Add(child2);

        // Infinite width measure
        panel.Measure(new Size(double.PositiveInfinity, 1000));
        panel.Arrange(new Rect(panel.DesiredSize)); 

        // Current implementation: if infinite width, calc columns based on available size? No, it used fallback.
        // Let's verify what it actually did. Our code: Math.Max(1, (int)(available / desired)). If infinity -> infinity?
        // Wait, standard double cast of infinity is int.MinValue or something.
        // Actually (int)double.PositiveInfinity is usually int.MinValue in C# unchecked context.
        // Let's check the code:
        // if (double.IsInfinity(availableWidth)) columnCount = Math.Max(1, Children.Count);
        
        // So with 2 children, it should be 1 column (vertical stack)
        Assert.Equal(0, child1.Bounds.X);
        Assert.Equal(0, child2.Bounds.X); 
        Assert.Equal(50, child2.Bounds.Y); // Below child1 (Height 50)
    }

    [AvaloniaFact]
    public void Should_Handle_Zero_Children()
    {
        var panel = new StaggeredPanel { DesiredColumnWidth = 100 };
        panel.Measure(new Size(300, 300));
        Assert.Equal(0, panel.DesiredSize.Height);
    }
    
    [AvaloniaFact]
    public void Should_Handle_Single_Child()
    {
         var panel = new StaggeredPanel
        {
            DesiredColumnWidth = 100,
            Width = 300
        };
        var child = new Border { Width = 100, Height = 50 };
        panel.Children.Add(child);

        panel.Measure(new Size(300, 1000));
        panel.Arrange(new Rect(0, 0, 300, 1000));
        
        Assert.Equal(0, child.Bounds.X);
        Assert.Equal(0, child.Bounds.Y);
        Assert.Equal(50, panel.Bounds.Height);
    }

    [AvaloniaFact]
    public void Should_Distribute_Updates_Dynamically()
    {
         var panel = new StaggeredPanel
        {
            DesiredColumnWidth = 100, 
            Width = 200 // 2 Columns
        };

        var child1 = new Border { Width = 100, Height = 100 };
        var child2 = new Border { Width = 100, Height = 100 };
        
        panel.Children.Add(child1);
        panel.Children.Add(child2);

        panel.Measure(new Size(200, 1000));
        panel.Arrange(new Rect(0, 0, 200, 1000));

        // 2 cols -> items side by side
        Assert.Equal(0, child1.Bounds.X);
        Assert.Equal(100, child2.Bounds.X);
        Assert.Equal(0, child2.Bounds.Y);

        // Change width to force 1 column
        panel.Width = 100;
        
        // Measure/Arrange again
        panel.Measure(new Size(100, 1000));
        panel.Arrange(new Rect(0, 0, 100, 1000));

        // 1 col -> items stacked
        Assert.Equal(0, child1.Bounds.X);
        Assert.Equal(0, child2.Bounds.X);
        Assert.Equal(100, child2.Bounds.Y); // Below child1
    }

    [AvaloniaFact]
    public void Should_Stretch_Children_To_Column_Width()
    {
        var panel = new StaggeredPanel
        {
            DesiredColumnWidth = 100,
            ColumnSpacing = 0,
            Width = 100 // 1 column
        };

        var child = new Border { Height = 50 };
        panel.Children.Add(child);

        panel.Measure(new Size(100, 1000));
        panel.Arrange(new Rect(0, 0, 100, 1000));

        // Child should be stretched to column width (100)
        Assert.Equal(100, child.Bounds.Width);
    }

    [AvaloniaFact]
    public void Should_Handle_Large_Number_of_Items()
    {
        var panel = new StaggeredPanel
        {
            DesiredColumnWidth = 10,
            Width = 100 // 10 columns
        };

        for (int i = 0; i < 1000; i++)
        {
            panel.Children.Add(new Border { Width = 10, Height = 10 });
        }

        panel.Measure(new Size(100, 5000));
        panel.Arrange(new Rect(0, 0, 100, 5000));

        // 1000 items in 10 columns -> approx 100 per column -> 100 * 10 = 1000 height
        Assert.Equal(1000, panel.DesiredSize.Height);
    }
}
