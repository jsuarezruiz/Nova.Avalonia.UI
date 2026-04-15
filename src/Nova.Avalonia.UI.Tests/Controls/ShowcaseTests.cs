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
        Assert.Equal(ShowcaseInteractionMode.Modal, showcase.InteractionMode);
    }

    [AvaloniaFact]
    public void ShowcaseStep_Should_Have_Default_Values()
    {
        var step = new ShowcaseStep { Key = "Defaults" };

        Assert.Equal(string.Empty, step.Title);
        Assert.Equal(string.Empty, step.Description);
        Assert.Equal(ShowcaseTooltipPosition.Auto, step.TooltipPosition);
        Assert.Null(step.InteractionMode);
        Assert.Equal(ShowcaseHighlightShape.RoundedRectangle, step.HighlightShape);
        Assert.Equal(new Thickness(8), step.HighlightPadding);
        Assert.Equal(8, step.HighlightCornerRadius);
        Assert.Null(step.TooltipTemplate);
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
    public void ShowcaseTooltip_ShowDefaultBody_Should_Be_True_By_Default()
    {
        var tooltip = new ShowcaseTooltip();

        Assert.True(tooltip.ShowDefaultBody);
        Assert.False(tooltip.ShowCustomBody);
    }

    [AvaloniaFact]
    public void ShowcaseTooltip_Setting_ContentTemplate_Should_Toggle_CustomTemplate_PseudoClass()
    {
        var tooltip = new ShowcaseTooltip();
        var template = new FuncDataTemplate<ShowcaseStep>((s, _) => new TextBlock { Text = s.Title });

        tooltip.ContentTemplate = template;

        Assert.True(tooltip.ShowCustomBody);
        Assert.False(tooltip.ShowDefaultBody);
        Assert.Contains(":custom-template", tooltip.Classes);

        tooltip.ContentTemplate = null;

        Assert.False(tooltip.ShowCustomBody);
        Assert.True(tooltip.ShowDefaultBody);
        Assert.DoesNotContain(":custom-template", tooltip.Classes);
    }

    [AvaloniaFact]
    public void ValidationIssue_ToString_Should_Include_Severity_And_Message()
    {
        var issue = new ShowcaseValidationIssue(
            ShowcaseValidationIssueCode.MissingTarget,
            ShowcaseValidationSeverity.Error,
            "Target not found.");

        Assert.Equal("[Error] Target not found.", issue.ToString());
    }

    [AvaloniaFact]
    public void ValidationResult_ToString_Should_Summarize_Issues()
    {
        var valid = new ShowcaseValidationResult([]);
        Assert.Equal("Valid", valid.ToString());

        var withError = new ShowcaseValidationResult(
        [
            new ShowcaseValidationIssue(
                ShowcaseValidationIssueCode.MissingTarget,
                ShowcaseValidationSeverity.Error,
                "Missing target.")
        ]);
        Assert.Contains("1 error", withError.ToString());

        var withWarning = new ShowcaseValidationResult(
        [
            new ShowcaseValidationIssue(
                ShowcaseValidationIssueCode.TargetUnavailable,
                ShowcaseValidationSeverity.Warning,
                "Target not visible.")
        ]);
        Assert.Contains("1 warning", withWarning.ToString());
    }

    [AvaloniaFact]
    public void StartResult_ToString_Should_Indicate_Outcome()
    {
        var started = new ShowcaseStartResult(true, new ShowcaseValidationResult([]));
        Assert.Equal("Started", started.ToString());

        var notStarted = new ShowcaseStartResult(false, new ShowcaseValidationResult(
        [
            new ShowcaseValidationIssue(
                ShowcaseValidationIssueCode.NoSteps,
                ShowcaseValidationSeverity.Error,
                "No steps.")
        ]));
        Assert.StartsWith("Not started", notStarted.ToString());
    }
}
