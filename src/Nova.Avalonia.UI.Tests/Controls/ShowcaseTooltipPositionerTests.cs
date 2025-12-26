using Avalonia;
using Avalonia.Headless.XUnit;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class ShowcaseTooltipPositionerTests
{
    private readonly ShowcaseTooltipPositioner _positioner = new();
    
    [AvaloniaFact]
    public void CalculatePosition_Bottom_Should_Place_Below_Target()
    {
        var targetBounds = new Rect(100, 100, 50, 30);
        var tooltipSize = new Size(100, 50);
        var containerBounds = new Rect(0, 0, 800, 600);
        
        var position = _positioner.CalculatePosition(
            targetBounds, tooltipSize, containerBounds, 
            ShowcaseTooltipPosition.Bottom);
        
        // Tooltip should be below the target
        Assert.True(position.Y > targetBounds.Bottom);
    }
    
    [AvaloniaFact]
    public void CalculatePosition_Top_Should_Place_Above_Target()
    {
        var targetBounds = new Rect(100, 200, 50, 30);
        var tooltipSize = new Size(100, 50);
        var containerBounds = new Rect(0, 0, 800, 600);
        
        var position = _positioner.CalculatePosition(
            targetBounds, tooltipSize, containerBounds, 
            ShowcaseTooltipPosition.Top);
        
        // Tooltip should be above the target
        Assert.True(position.Y + tooltipSize.Height < targetBounds.Top);
    }
    
    [AvaloniaFact]
    public void CalculatePosition_Right_Should_Place_Right_Of_Target()
    {
        var targetBounds = new Rect(100, 200, 50, 30);
        var tooltipSize = new Size(100, 50);
        var containerBounds = new Rect(0, 0, 800, 600);
        
        var position = _positioner.CalculatePosition(
            targetBounds, tooltipSize, containerBounds, 
            ShowcaseTooltipPosition.Right);
        
        // Tooltip should be right of the target
        Assert.True(position.X > targetBounds.Right);
    }
    
    [AvaloniaFact]
    public void CalculatePosition_Left_Should_Place_Left_Of_Target()
    {
        var targetBounds = new Rect(300, 200, 50, 30);
        var tooltipSize = new Size(100, 50);
        var containerBounds = new Rect(0, 0, 800, 600);
        
        var position = _positioner.CalculatePosition(
            targetBounds, tooltipSize, containerBounds, 
            ShowcaseTooltipPosition.Left);
        
        // Tooltip should be left of the target
        Assert.True(position.X + tooltipSize.Width < targetBounds.Left);
    }
    
    [AvaloniaFact]
    public void CalculatePosition_Center_Should_Center_On_Screen()
    {
        var targetBounds = new Rect(100, 100, 50, 30);
        var tooltipSize = new Size(200, 100);
        var containerBounds = new Rect(0, 0, 800, 600);
        
        var position = _positioner.CalculatePosition(
            targetBounds, tooltipSize, containerBounds, 
            ShowcaseTooltipPosition.Center);
        
        Assert.Equal((containerBounds.Width - tooltipSize.Width) / 2, position.X);
        Assert.Equal((containerBounds.Height - tooltipSize.Height) / 2, position.Y);
    }
    
    [AvaloniaFact]
    public void CalculatePosition_Should_Fallback_If_Preferred_Overflows()
    {
        // Target at the top of the container
        var targetBounds = new Rect(100, 10, 50, 30);
        var tooltipSize = new Size(100, 100); // Too tall for Top position
        var containerBounds = new Rect(0, 0, 800, 600);
        
        // Prefer top, but should fallback since there's no room
        var position = _positioner.CalculatePosition(
            targetBounds, tooltipSize, containerBounds, 
            ShowcaseTooltipPosition.Top);
        
        // Should have fallen back to another position (likely bottom)
        Assert.True(position.Y >= 0);
    }
    
    [AvaloniaFact]
    public void CalculatePosition_Auto_Should_Not_Overflow()
    {
        var targetBounds = new Rect(100, 100, 50, 30);
        var tooltipSize = new Size(100, 50);
        var containerBounds = new Rect(0, 0, 800, 600);
        
        var position = _positioner.CalculatePosition(
            targetBounds, tooltipSize, containerBounds, 
            ShowcaseTooltipPosition.Auto);
        
        // Position should be within bounds
        Assert.True(position.X >= 0);
        Assert.True(position.Y >= 0);
        Assert.True(position.X + tooltipSize.Width <= containerBounds.Width);
        Assert.True(position.Y + tooltipSize.Height <= containerBounds.Height);
    }
}
