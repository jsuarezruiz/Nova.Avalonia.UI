using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class ShowcaseTests
{
    [AvaloniaFact]
    public void Showcase_Attached_Key_Should_Get_And_Set()
    {
        var button = new Button();

        Showcase.SetKey(button, "TestKey");

        Assert.Equal("TestKey", Showcase.GetKey(button));
    }

    [AvaloniaFact]
    public void Showcase_Should_Have_Default_Values()
    {
        var showcase = new Showcase();

        Assert.False(showcase.IsActive);
        Assert.Null(showcase.Controller);
        Assert.NotNull(showcase.OverlayBrush);
    }

    [AvaloniaFact]
    public void Showcase_IsActive_Can_Be_Set()
    {
        var showcase = new Showcase();
        showcase.Controller = new ShowcaseController();
        showcase.Controller.Steps.Add(new ShowcaseStep { Key = "Test" });

        Assert.False(showcase.IsActive);

        showcase.IsActive = true;

        Assert.True(showcase.IsActive);
    }

    [AvaloniaFact]
    public void ShowcaseStep_Should_Have_Default_Values()
    {
        var step = new ShowcaseStep();

        Assert.Equal(string.Empty, step.Key);
        Assert.Equal(string.Empty, step.Title);
        Assert.Equal(string.Empty, step.Description);
        Assert.Equal(ShowcaseTooltipPosition.Auto, step.TooltipPosition);
        Assert.Equal(ShowcaseHighlightShape.RoundedRectangle, step.HighlightShape);
        Assert.Equal(new Thickness(8), step.HighlightPadding);
        Assert.Equal(8, step.CornerRadius);
        Assert.Null(step.CustomTooltipTemplate);
    }

    [AvaloniaFact]
    public void ShowcaseStep_Properties_Should_Be_Settable()
    {
        var step = new ShowcaseStep
        {
            Key = "MyKey",
            Title = "My Title",
            Description = "My Description",
            TooltipPosition = ShowcaseTooltipPosition.Left,
            HighlightShape = ShowcaseHighlightShape.Circle,
            HighlightPadding = new Thickness(16),
            CornerRadius = 12
        };

        Assert.Equal("MyKey", step.Key);
        Assert.Equal("My Title", step.Title);
        Assert.Equal("My Description", step.Description);
        Assert.Equal(ShowcaseTooltipPosition.Left, step.TooltipPosition);
        Assert.Equal(ShowcaseHighlightShape.Circle, step.HighlightShape);
        Assert.Equal(new Thickness(16), step.HighlightPadding);
        Assert.Equal(12, step.CornerRadius);
    }

    [AvaloniaFact]
    public void ShowcaseStep_CustomTooltipTemplate_Can_Be_Set()
    {
        var template = new FuncDataTemplate<ShowcaseStep>((s, _) => new TextBlock { Text = s.Title });
        var step = new ShowcaseStep
        {
            Key = "Test",
            CustomTooltipTemplate = template
        };

        Assert.Same(template, step.CustomTooltipTemplate);
    }

    [AvaloniaFact]
    public void ShowcaseOverlay_Should_Have_Default_Values()
    {
        var overlay = new ShowcaseOverlay();

        Assert.NotNull(overlay.OverlayBrush);
        Assert.Null(overlay.TargetBounds);
        Assert.Equal(new Thickness(8), overlay.HighlightPadding);
        Assert.Equal(ShowcaseHighlightShape.RoundedRectangle, overlay.HighlightShape);
        Assert.Equal(8, overlay.HighlightCornerRadius);
    }

    [AvaloniaFact]
    public void ShowcaseOverlay_Properties_Should_Be_Settable()
    {
        var overlay = new ShowcaseOverlay
        {
            TargetBounds = new Rect(10, 20, 100, 50),
            HighlightShape = ShowcaseHighlightShape.Circle,
            HighlightPadding = new Thickness(12),
            HighlightCornerRadius = 16
        };

        Assert.Equal(new Rect(10, 20, 100, 50), overlay.TargetBounds);
        Assert.Equal(ShowcaseHighlightShape.Circle, overlay.HighlightShape);
        Assert.Equal(new Thickness(12), overlay.HighlightPadding);
        Assert.Equal(16, overlay.HighlightCornerRadius);
    }

    [AvaloniaFact]
    public void ShowcaseTooltip_Should_Have_Title_And_Description()
    {
        var tooltip = new ShowcaseTooltip
        {
            Title = "Test Title",
            Description = "Test Description"
        };

        Assert.Equal("Test Title", tooltip.Title);
        Assert.Equal("Test Description", tooltip.Description);
    }
}
