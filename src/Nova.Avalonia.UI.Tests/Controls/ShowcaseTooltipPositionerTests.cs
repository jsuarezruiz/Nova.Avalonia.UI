using Avalonia;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class ShowcaseTooltipPositionerTests
{
    private readonly ShowcaseTooltipPositioner _positioner = new();
    private static readonly Rect ContainerBounds = new(0, 0, 800, 600);
    private static readonly Size TooltipSize = new(100, 50);

    [Fact]
    public void Bottom_Should_Place_Below_Target()
    {
        var target = new Rect(100, 100, 50, 30);

        var position = _positioner.CalculatePosition(target, TooltipSize, ContainerBounds, ShowcaseTooltipPosition.Bottom);

        Assert.True(position.Y > target.Bottom);
    }

    [Fact]
    public void Top_Should_Place_Above_Target()
    {
        var target = new Rect(100, 200, 50, 30);

        var position = _positioner.CalculatePosition(target, TooltipSize, ContainerBounds, ShowcaseTooltipPosition.Top);

        Assert.True(position.Y + TooltipSize.Height < target.Top);
    }

    [Fact]
    public void Right_Should_Place_Right_Of_Target()
    {
        var target = new Rect(100, 200, 50, 30);

        var position = _positioner.CalculatePosition(target, TooltipSize, ContainerBounds, ShowcaseTooltipPosition.Right);

        Assert.True(position.X > target.Right);
    }

    [Fact]
    public void Left_Should_Place_Left_Of_Target()
    {
        var target = new Rect(300, 200, 50, 30);

        var position = _positioner.CalculatePosition(target, TooltipSize, ContainerBounds, ShowcaseTooltipPosition.Left);

        Assert.True(position.X + TooltipSize.Width < target.Left);
    }

    [Fact]
    public void Center_Should_Center_In_Container()
    {
        var target = new Rect(100, 100, 50, 30);
        var tooltipSize = new Size(200, 100);

        var position = _positioner.CalculatePosition(target, tooltipSize, ContainerBounds, ShowcaseTooltipPosition.Center);

        Assert.Equal((ContainerBounds.Width - tooltipSize.Width) / 2, position.X);
        Assert.Equal((ContainerBounds.Height - tooltipSize.Height) / 2, position.Y);
    }

    [Fact]
    public void Center_Should_Respect_Container_Origin()
    {
        var target = new Rect(150, 150, 50, 30);
        var tooltipSize = new Size(200, 100);
        var container = new Rect(100, 75, 800, 600);

        var position = _positioner.CalculatePosition(
            target,
            tooltipSize,
            container,
            ShowcaseTooltipPosition.Center);

        Assert.Equal(400, position.X);
        Assert.Equal(325, position.Y);
    }

    [Fact]
    public void Auto_Should_Prefer_Bottom_When_Space_Available()
    {
        var target = new Rect(300, 200, 120, 40);

        var position = _positioner.CalculatePosition(target, TooltipSize, ContainerBounds, ShowcaseTooltipPosition.Auto);

        Assert.True(position.Y > target.Bottom);
    }

    [Fact]
    public void Top_Should_Fallback_To_Bottom_When_Clipped()
    {
        var target = new Rect(100, 10, 50, 30);
        var tooltipSize = new Size(100, 100);

        var position = _positioner.CalculatePosition(target, tooltipSize, ContainerBounds, ShowcaseTooltipPosition.Top);

        Assert.True(position.Y > target.Bottom);
    }

    [Fact]
    public void Left_Should_Fallback_To_Right_When_Clipped()
    {
        var target = new Rect(10, 200, 50, 30);

        var position = _positioner.CalculatePosition(target, TooltipSize, ContainerBounds, ShowcaseTooltipPosition.Left);

        Assert.True(position.X >= target.Right);
    }

    [Fact]
    public void Right_Should_Fallback_To_Left_When_Clipped()
    {
        var target = new Rect(700, 200, 50, 30);

        var position = _positioner.CalculatePosition(target, TooltipSize, ContainerBounds, ShowcaseTooltipPosition.Right);

        Assert.True(position.X + TooltipSize.Width <= target.Left);
    }

    [Fact]
    public void Should_Center_When_All_Positions_Clip()
    {
        var smallContainer = new Rect(0, 0, 80, 80);
        var target = new Rect(10, 10, 60, 60);
        var tooltipSize = new Size(200, 100);

        var position = _positioner.CalculatePosition(target, tooltipSize, smallContainer, ShowcaseTooltipPosition.Auto);

        Assert.Equal((smallContainer.Width - tooltipSize.Width) / 2, position.X);
        Assert.Equal((smallContainer.Height - tooltipSize.Height) / 2, position.Y);
    }

    [Fact]
    public void Auto_Should_Not_Overflow_Container()
    {
        var target = new Rect(100, 100, 50, 30);

        var position = _positioner.CalculatePosition(target, TooltipSize, ContainerBounds, ShowcaseTooltipPosition.Auto);

        Assert.True(position.X >= 0);
        Assert.True(position.Y >= 0);
        Assert.True(position.X + TooltipSize.Width <= ContainerBounds.Width);
        Assert.True(position.Y + TooltipSize.Height <= ContainerBounds.Height);
    }

    [Fact]
    public void All_Positions_Should_Stay_Within_Container_For_Centered_Target()
    {
        var target = new Rect(300, 250, 120, 40);
        var positions = new[]
        {
            ShowcaseTooltipPosition.Auto,
            ShowcaseTooltipPosition.Top,
            ShowcaseTooltipPosition.Bottom,
            ShowcaseTooltipPosition.Left,
            ShowcaseTooltipPosition.Right,
            ShowcaseTooltipPosition.Center
        };

        foreach (var pref in positions)
        {
            var result = _positioner.CalculatePosition(target, TooltipSize, ContainerBounds, pref);
            var tooltipBounds = new Rect(result, TooltipSize);

            Assert.True(result.X >= 0, $"{pref}: X={result.X} is negative");
            Assert.True(result.Y >= 0, $"{pref}: Y={result.Y} is negative");
            Assert.True(tooltipBounds.Right <= ContainerBounds.Width, $"{pref}: Right={tooltipBounds.Right} exceeds width");
            Assert.True(tooltipBounds.Bottom <= ContainerBounds.Height, $"{pref}: Bottom={tooltipBounds.Bottom} exceeds height");
        }
    }
}
