using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.VisualTree;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class ShowcaseIntegrationTests
{
    [AvaloniaFact]
    public void Controller_Start_Should_Activate_Attached_Showcase()
    {
        var host = CreateHost();

        host.Controller.Start();
        host.Window.UpdateLayout();

        Assert.True(host.Showcase.IsActive);
        Assert.True(host.Controller.IsActive);
        Assert.NotNull(GetOverlay(host.Showcase).TargetBounds);
    }

    [AvaloniaFact]
    public void Attached_Tooltip_Template_Should_Override_Step_Template_And_Receive_Current_Step()
    {
        var host = CreateHost();
        host.Controller.Steps[0].TooltipTemplate =
            new FuncDataTemplate<ShowcaseStep>((step, _) => new TextBlock { Text = $"step:{step.Key}" });

        Showcase.SetTooltipTemplate(
            host.Target,
            new FuncDataTemplate<ShowcaseStep>((step, _) => new TextBlock { Text = $"element:{step.Key}" }));

        host.Controller.Start();
        host.Window.UpdateLayout();

        var tooltip = GetTooltip(host.Showcase);
        var texts = tooltip
            .GetVisualDescendants()
            .OfType<TextBlock>()
            .Where(x => x.IsEffectivelyVisible)
            .Select(x => x.Text)
            .ToArray();

        Assert.Contains("element:Target", texts);
        Assert.DoesNotContain("step:Target", texts);
        Assert.DoesNotContain("Step Title", texts);
    }

    [AvaloniaFact]
    public void Missing_Target_Should_Keep_Tooltip_Visible_And_Centered()
    {
        var host = CreateHost(includeTarget: false, stepKey: "Missing");

        host.Controller.Start();
        host.Window.UpdateLayout();

        var overlay = GetOverlay(host.Showcase);
        var tooltip = GetTooltip(host.Showcase);

        Assert.True(host.Showcase.IsActive);
        Assert.Null(overlay.TargetBounds);
        Assert.True(tooltip.IsVisible);
        Assert.True(Canvas.GetLeft(tooltip) >= 0);
        Assert.True(Canvas.GetTop(tooltip) >= 0);
    }

    [AvaloniaFact]
    public void Moving_Target_Should_Reposition_Overlay()
    {
        var host = CreateHost();

        host.Controller.Start();
        host.Window.UpdateLayout();

        var overlay = GetOverlay(host.Showcase);
        Assert.True(overlay.TargetBounds.HasValue);
        var initialBounds = overlay.TargetBounds.Value;

        Canvas.SetLeft(host.Target, 220);
        host.Window.UpdateLayout();

        Assert.True(overlay.TargetBounds.HasValue);
        var updatedBounds = overlay.TargetBounds.Value;
        Assert.True(updatedBounds.X > initialBounds.X);
    }

    [AvaloniaFact]
    public void AutoScrollIntoView_Should_Bring_Target_Into_View()
    {
        var host = CreateScrollHost();
        var requestCount = 0;
        host.Target.AddHandler(Control.RequestBringIntoViewEvent, (_, _) => requestCount++);

        host.Controller.Start();
        host.Window.UpdateLayout();
        host.Window.UpdateLayout();

        Assert.Equal(1, requestCount);
        Assert.True(host.Showcase.IsActive);
        Assert.True(GetOverlay(host.Showcase).TargetBounds.HasValue);
    }

    [AvaloniaFact]
    public void AutoScrollIntoView_Can_Be_Disabled()
    {
        var host = CreateScrollHost();
        var requestCount = 0;
        host.Showcase.AutoScrollIntoView = false;
        host.Target.AddHandler(Control.RequestBringIntoViewEvent, (_, _) => requestCount++);

        host.Controller.Start();
        host.Window.UpdateLayout();
        host.Window.UpdateLayout();

        Assert.Equal(0, requestCount);
    }

    [AvaloniaFact]
    public void Validate_Should_Report_Valid_Config()
    {
        var host = CreateHost();

        var result = host.Showcase.Validate();

        Assert.True(result.IsValid);
        Assert.False(result.HasWarnings);
        Assert.Empty(result.Issues);
    }

    [AvaloniaFact]
    public void Validate_Should_Report_Missing_Target()
    {
        var host = CreateHost(includeTarget: false, stepKey: "Missing");

        var result = host.Showcase.Validate();

        Assert.False(result.IsValid);
        var issue = Assert.Single(result.Issues, x => x.Code == ShowcaseValidationIssueCode.MissingTarget);
        Assert.Equal("Missing", issue.Key);
        Assert.Equal(0, issue.StepIndex);
    }

    [AvaloniaFact]
    public void Validate_Should_Report_Duplicate_Target_Keys()
    {
        var host = CreateHost(includeSecondDuplicate: true);

        var result = host.Showcase.Validate();

        Assert.False(result.IsValid);
        var issue = Assert.Single(result.Issues, x => x.Code == ShowcaseValidationIssueCode.DuplicateTargetKey);
        Assert.Equal("Target", issue.Key);
    }

    [AvaloniaFact]
    public void Validate_Should_Report_No_Steps()
    {
        var host = CreateHost();
        host.Controller.Steps.Clear();

        var result = host.Showcase.Validate();

        Assert.False(result.IsValid);
        Assert.Single(result.Issues, x => x.Code == ShowcaseValidationIssueCode.NoSteps);
    }

    [AvaloniaFact]
    public void Validate_Should_Report_Empty_Step_Content()
    {
        var host = CreateHost();
        host.Controller.Steps.Add(new ShowcaseStep { Key = "Other" });
        Showcase.SetKey(host.OtherButton, "Other");

        var result = host.Showcase.Validate();

        Assert.True(result.HasWarnings);
        Assert.Single(result.Issues, x => x.Code == ShowcaseValidationIssueCode.EmptyStepContent);
    }

    [AvaloniaFact]
    public void Validate_Should_Report_No_Controller()
    {
        var showcase = new Showcase();

        var result = showcase.Validate();

        Assert.False(result.IsValid);
        Assert.Single(result.Issues, x => x.Code == ShowcaseValidationIssueCode.NoController);
    }

    [AvaloniaFact]
    public void Validate_Should_Report_Empty_Step_Key()
    {
        var host = CreateHost();
        host.Controller.Steps.Add(new ShowcaseStep { Key = " ", Title = "Bad Step" });

        var result = host.Showcase.Validate();

        Assert.False(result.IsValid);
        Assert.Single(result.Issues, x => x.Code == ShowcaseValidationIssueCode.EmptyStepKey);
    }

    [AvaloniaFact]
    public void EnsureStarted_Should_Throw_When_Not_Started()
    {
        var result = new ShowcaseStartResult(false, new ShowcaseValidationResult(
        [
            new ShowcaseValidationIssue(
                ShowcaseValidationIssueCode.MissingTarget,
                ShowcaseValidationSeverity.Error,
                "Target not found.")
        ]));

        var ex = Assert.Throws<InvalidOperationException>(() => result.EnsureStarted());
        Assert.Contains("Target not found", ex.Message);
    }

    [AvaloniaFact]
    public void Step_TooltipTemplate_Should_Override_Default_Body()
    {
        var host = CreateHost();
        host.Controller.Steps[0].TooltipTemplate =
            new FuncDataTemplate<ShowcaseStep>((step, _) => new TextBlock { Text = $"custom:{step.Key}" });

        host.Controller.Start();
        host.Window.UpdateLayout();

        var tooltip = GetTooltip(host.Showcase);
        var texts = tooltip
            .GetVisualDescendants()
            .OfType<TextBlock>()
            .Where(x => x.IsEffectivelyVisible)
            .Select(x => x.Text)
            .ToArray();

        Assert.Contains("custom:Target", texts);
        Assert.DoesNotContain("Step Title", texts);
    }

    [AvaloniaFact]
    public void Start_Should_Throw_When_Validation_Fails()
    {
        var host = CreateHost(includeTarget: false, stepKey: "Missing");

        var exception = Assert.Throws<InvalidOperationException>(() => host.Showcase.Start());

        Assert.Contains("no matching control was found", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(host.Controller.IsActive);
    }

    [AvaloniaFact]
    public void TryStart_Should_Return_NotStarted_When_Validation_Fails()
    {
        var host = CreateHost(includeTarget: false, stepKey: "Missing");

        var result = host.Showcase.TryStart();

        Assert.False(result.Started);
        Assert.False(result.ValidationResult.IsValid);
        Assert.False(host.Showcase.IsActive);
        Assert.False(host.Controller.IsActive);
    }

    [AvaloniaFact]
    public async Task TryStartAsync_Should_Return_NotStarted_When_Validation_Fails()
    {
        var host = CreateHost(includeTarget: false, stepKey: "Missing");

        var result = await host.Showcase.TryStartAsync();

        Assert.False(result.Started);
        Assert.False(result.ValidationResult.IsValid);
        Assert.False(host.Controller.IsActive);
    }

    [AvaloniaFact]
    public async Task StartAsync_Should_Start_When_Validation_Succeeds()
    {
        var host = CreateHost();

        await host.Showcase.StartAsync();
        host.Window.UpdateLayout();

        Assert.True(host.Showcase.IsActive);
        Assert.True(host.Controller.IsActive);
        Assert.Equal(0, host.Controller.CurrentIndex);
    }

    [AvaloniaFact]
    public void TryStart_Should_Not_Activate_When_Validation_Fails()
    {
        var host = CreateHost(includeTarget: false, stepKey: "Missing");

        var result = host.Showcase.TryStart();
        host.Window.UpdateLayout();

        Assert.False(result.Started);
        Assert.False(host.Showcase.IsActive);
        Assert.False(host.Controller.IsActive);
    }

    [AvaloniaFact]
    public void Modal_InteractionMode_Should_Block_Target_HitTesting()
    {
        var host = CreateHost();
        host.Showcase.InteractionMode = ShowcaseInteractionMode.Modal;

        host.Controller.Start();
        host.Window.UpdateLayout();

        var modalBlocker = GetBlocker(host.Showcase, "PART_ModalBlocker");

        Assert.True(modalBlocker.IsVisible);
        Assert.True(ContainsPoint(modalBlocker, GetTargetCenter(host.Target, host.Root), host.Root));
    }

    [AvaloniaFact]
    public void Passthrough_InteractionMode_Should_Allow_Target_HitTesting()
    {
        var host = CreateHost();
        host.Showcase.InteractionMode = ShowcaseInteractionMode.Passthrough;

        host.Controller.Start();
        host.Window.UpdateLayout();

        Assert.DoesNotContain(GetBlockers(host.Showcase), x => x.IsVisible);
    }

    [AvaloniaFact]
    public void TargetOnly_InteractionMode_Should_Allow_Target_But_Block_Other_Controls()
    {
        var host = CreateHost();
        host.Showcase.InteractionMode = ShowcaseInteractionMode.TargetOnly;

        host.Controller.Start();
        host.Window.UpdateLayout();

        var targetCenter = GetTargetCenter(host.Target, host.Root);
        var otherCenter = GetTargetCenter(host.OtherButton, host.Root);
        var visibleBlockers = GetBlockers(host.Showcase).Where(x => x.IsVisible).ToArray();
        var modalBlocker = GetBlocker(host.Showcase, "PART_ModalBlocker");

        Assert.False(modalBlocker.IsVisible);
        Assert.DoesNotContain(visibleBlockers, x => ContainsPoint(x, targetCenter, host.Root));
        Assert.Contains(visibleBlockers, x => ContainsPoint(x, otherCenter, host.Root));
    }

    [AvaloniaFact]
    public void Step_InteractionMode_Should_Override_Control_Default()
    {
        var host = CreateHost();
        host.Showcase.InteractionMode = ShowcaseInteractionMode.Modal;
        host.Controller.Steps[0].InteractionMode = ShowcaseInteractionMode.Passthrough;

        host.Controller.Start();
        host.Window.UpdateLayout();

        Assert.DoesNotContain(GetBlockers(host.Showcase), x => x.IsVisible);
    }

    [AvaloniaFact]
    public async Task Step_InteractionMode_Should_Update_Across_Step_Transitions()
    {
        var host = CreateHost();
        Showcase.SetKey(host.OtherButton, "Other");
        host.Showcase.InteractionMode = ShowcaseInteractionMode.Modal;
        host.Controller.Steps.Clear();
        host.Controller.Steps.Add(new ShowcaseStep
        {
            Key = "Target",
            Title = "First",
            Description = "First step",
            InteractionMode = ShowcaseInteractionMode.Passthrough
        });
        host.Controller.Steps.Add(new ShowcaseStep
        {
            Key = "Other",
            Title = "Second",
            Description = "Second step",
            InteractionMode = ShowcaseInteractionMode.TargetOnly
        });

        await host.Showcase.StartAsync();
        host.Window.UpdateLayout();

        Assert.DoesNotContain(GetBlockers(host.Showcase), x => x.IsVisible);

        await host.Controller.NextAsync();
        host.Window.UpdateLayout();

        var otherCenter = GetTargetCenter(host.OtherButton, host.Root);
        var originalTargetCenter = GetTargetCenter(host.Target, host.Root);
        var visibleBlockers = GetBlockers(host.Showcase).Where(x => x.IsVisible).ToArray();

        Assert.False(GetBlocker(host.Showcase, "PART_ModalBlocker").IsVisible);
        Assert.DoesNotContain(visibleBlockers, x => ContainsPoint(x, otherCenter, host.Root));
        Assert.Contains(visibleBlockers, x => ContainsPoint(x, originalTargetCenter, host.Root));
    }

    [AvaloniaFact]
    public void KeyDown_Right_Should_Advance_Step()
    {
        var host = CreateHost();
        Showcase.SetKey(host.OtherButton, "Other");
        host.Controller.Steps.Add(new ShowcaseStep
        {
            Key = "Other",
            Title = "Second",
            Description = "Second step"
        });

        host.Controller.Start();
        host.Window.UpdateLayout();

        Assert.Equal(0, host.Controller.CurrentIndex);

        host.Showcase.RaiseEvent(new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Key = Key.Right
        });

        Assert.Equal(1, host.Controller.CurrentIndex);
    }

    [AvaloniaFact]
    public void KeyDown_Left_Should_Go_Back()
    {
        var host = CreateHost();
        Showcase.SetKey(host.OtherButton, "Other");
        host.Controller.Steps.Add(new ShowcaseStep
        {
            Key = "Other",
            Title = "Second",
            Description = "Second step"
        });

        host.Controller.Start();
        host.Controller.Next();
        host.Window.UpdateLayout();

        Assert.Equal(1, host.Controller.CurrentIndex);

        host.Showcase.RaiseEvent(new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Key = Key.Left
        });

        Assert.Equal(0, host.Controller.CurrentIndex);
    }

    [AvaloniaFact]
    public void KeyDown_Escape_Should_Skip()
    {
        var host = CreateHost();

        host.Controller.Start();
        host.Window.UpdateLayout();

        Assert.True(host.Controller.IsActive);

        host.Showcase.RaiseEvent(new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Key = Key.Escape
        });

        Assert.False(host.Controller.IsActive);
    }

    [AvaloniaFact]
    public void KeyDown_Space_Should_Advance_Step()
    {
        var host = CreateHost();
        Showcase.SetKey(host.OtherButton, "Other");
        host.Controller.Steps.Add(new ShowcaseStep
        {
            Key = "Other",
            Title = "Second",
            Description = "Second step"
        });

        host.Controller.Start();
        host.Window.UpdateLayout();

        host.Showcase.RaiseEvent(new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Key = Key.Space
        });

        Assert.Equal(1, host.Controller.CurrentIndex);
    }

    [AvaloniaFact]
    public void KeyDown_Enter_Should_Advance_Step()
    {
        var host = CreateHost();
        Showcase.SetKey(host.OtherButton, "Other");
        host.Controller.Steps.Add(new ShowcaseStep
        {
            Key = "Other",
            Title = "Second",
            Description = "Second step"
        });

        host.Controller.Start();
        host.Window.UpdateLayout();

        host.Showcase.RaiseEvent(new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Key = Key.Enter
        });

        Assert.Equal(1, host.Controller.CurrentIndex);
    }

    [AvaloniaFact]
    public void Duplicate_Key_At_Runtime_Should_Not_Crash()
    {
        var host = CreateHost();
        Showcase.SetKey(host.OtherButton, "Other");
        host.Controller.Steps.Add(new ShowcaseStep
        {
            Key = "Other",
            Title = "Second",
            Description = "Second step"
        });

        host.Controller.Start();
        host.Controller.Next();
        host.Window.UpdateLayout();
        Assert.True(host.Showcase.IsActive);

        // Add a duplicate key while on the second step
        var duplicate = new Button
        {
            Content = "Duplicate",
            Width = 120,
            Height = 40
        };
        Showcase.SetKey(duplicate, "Target");
        Canvas.SetLeft(duplicate, 400);
        Canvas.SetTop(duplicate, 80);
        ((Canvas)host.Root.Children[0]).Children.Add(duplicate);
        host.Window.UpdateLayout();

        // Navigate back — forces target re-resolution with duplicate present
        host.Controller.Previous();
        host.Window.UpdateLayout();

        Assert.True(host.Showcase.IsActive);
        Assert.Null(GetOverlay(host.Showcase).TargetBounds);
    }

    [AvaloniaFact]
    public void Changing_InteractionMode_While_Active_Should_Update_Blockers()
    {
        var host = CreateHost();
        host.Showcase.InteractionMode = ShowcaseInteractionMode.Modal;

        host.Controller.Start();
        host.Window.UpdateLayout();

        var modalBlocker = GetBlocker(host.Showcase, "PART_ModalBlocker");
        Assert.True(modalBlocker.IsVisible);

        host.Showcase.InteractionMode = ShowcaseInteractionMode.Passthrough;
        host.Window.UpdateLayout();

        Assert.DoesNotContain(GetBlockers(host.Showcase), x => x.IsVisible);
    }

    [AvaloniaFact]
    public void TargetOnly_With_Missing_Target_Should_Fallback_To_Modal()
    {
        var host = CreateHost(includeTarget: false, stepKey: "Missing");
        host.Showcase.InteractionMode = ShowcaseInteractionMode.TargetOnly;

        host.Controller.Start();
        host.Window.UpdateLayout();

        var modalBlocker = GetBlocker(host.Showcase, "PART_ModalBlocker");

        Assert.True(host.Showcase.IsActive);
        Assert.True(modalBlocker.IsVisible);
    }

    [AvaloniaFact]
    public void Changing_Modal_To_TargetOnly_Should_Show_Edge_Blockers()
    {
        var host = CreateHost();
        host.Showcase.InteractionMode = ShowcaseInteractionMode.Modal;

        host.Controller.Start();
        host.Window.UpdateLayout();

        var modalBlocker = GetBlocker(host.Showcase, "PART_ModalBlocker");
        Assert.True(modalBlocker.IsVisible);

        host.Showcase.InteractionMode = ShowcaseInteractionMode.TargetOnly;
        host.Window.UpdateLayout();

        Assert.False(modalBlocker.IsVisible);
        var visibleBlockers = GetBlockers(host.Showcase).Where(x => x.IsVisible).ToArray();
        Assert.NotEmpty(visibleBlockers);

        var targetCenter = GetTargetCenter(host.Target, host.Root);
        Assert.DoesNotContain(visibleBlockers, x => ContainsPoint(x, targetCenter, host.Root));
    }

    [AvaloniaFact]
    public void Changing_TargetOnly_To_Modal_Should_Hide_Edge_Blockers()
    {
        var host = CreateHost();
        host.Showcase.InteractionMode = ShowcaseInteractionMode.TargetOnly;

        host.Controller.Start();
        host.Window.UpdateLayout();

        var edgeBlockers = new[]
        {
            GetBlocker(host.Showcase, "PART_TopBlocker"),
            GetBlocker(host.Showcase, "PART_LeftBlocker"),
            GetBlocker(host.Showcase, "PART_RightBlocker"),
            GetBlocker(host.Showcase, "PART_BottomBlocker")
        };
        Assert.Contains(edgeBlockers, x => x.IsVisible);

        host.Showcase.InteractionMode = ShowcaseInteractionMode.Modal;
        host.Window.UpdateLayout();

        var modalBlocker = GetBlocker(host.Showcase, "PART_ModalBlocker");
        Assert.True(modalBlocker.IsVisible);
        Assert.DoesNotContain(edgeBlockers, x => x.IsVisible);
    }

    [AvaloniaFact]
    public void AutomationProperties_Name_Should_Update_On_Step_Change()
    {
        var host = CreateHost();
        Showcase.SetKey(host.OtherButton, "Other");
        host.Controller.Steps.Add(new ShowcaseStep
        {
            Key = "Other",
            Title = "Second Step",
            Description = "Second"
        });

        host.Controller.Start();
        host.Window.UpdateLayout();

        var name = AutomationProperties.GetName(host.Showcase);
        Assert.Contains("Step Title", name);
        Assert.Contains("1 of 2", name);

        host.Controller.Next();
        host.Window.UpdateLayout();

        name = AutomationProperties.GetName(host.Showcase);
        Assert.Contains("Second Step", name);
        Assert.Contains("2 of 2", name);
    }

    [AvaloniaFact]
    public void AutomationProperties_Name_Should_Reset_On_Deactivation()
    {
        var host = CreateHost();

        host.Controller.Start();
        host.Window.UpdateLayout();

        Assert.Contains("Step Title", AutomationProperties.GetName(host.Showcase));

        host.Controller.Skip();
        host.Window.UpdateLayout();

        Assert.Equal("Interactive Tutorial", AutomationProperties.GetName(host.Showcase));
    }

    [AvaloniaFact]
    public void Tab_Should_Cycle_Through_Buttons_In_Modal_Mode()
    {
        var host = CreateHost();
        Showcase.SetKey(host.OtherButton, "Other");
        host.Controller.Steps.Add(new ShowcaseStep
        {
            Key = "Other",
            Title = "Second",
            Description = "Second step"
        });

        host.Controller.Start();
        host.Controller.Next();
        host.Window.UpdateLayout();

        var skipButton = host.Showcase.GetVisualDescendants().OfType<Button>().Single(x => x.Name == "PART_SkipButton");
        var previousButton = host.Showcase.GetVisualDescendants().OfType<Button>().Single(x => x.Name == "PART_PreviousButton");
        var nextButton = host.Showcase.GetVisualDescendants().OfType<Button>().Single(x => x.Name == "PART_NextButton");

        host.Showcase.Focus();

        host.Showcase.RaiseEvent(new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Key = Key.Tab
        });

        var focused = TopLevel.GetTopLevel(host.Showcase)?.FocusManager?.GetFocusedElement();
        Assert.Same(skipButton, focused);

        host.Showcase.RaiseEvent(new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Key = Key.Tab
        });

        focused = TopLevel.GetTopLevel(host.Showcase)?.FocusManager?.GetFocusedElement();
        Assert.Same(previousButton, focused);

        host.Showcase.RaiseEvent(new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Key = Key.Tab
        });

        focused = TopLevel.GetTopLevel(host.Showcase)?.FocusManager?.GetFocusedElement();
        Assert.Same(nextButton, focused);

        host.Showcase.RaiseEvent(new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Key = Key.Tab
        });

        focused = TopLevel.GetTopLevel(host.Showcase)?.FocusManager?.GetFocusedElement();
        Assert.Same(skipButton, focused);
    }

    [AvaloniaFact]
    public void Tab_Should_Not_Trap_In_Passthrough_Mode()
    {
        var host = CreateHost();
        host.Controller.Steps[0].InteractionMode = ShowcaseInteractionMode.Passthrough;

        host.Controller.Start();
        host.Window.UpdateLayout();

        var skipButton = host.Showcase.GetVisualDescendants().OfType<Button>().Single(x => x.Name == "PART_SkipButton");
        skipButton.Focus();

        host.Showcase.RaiseEvent(new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Key = Key.Tab
        });

        var focused = TopLevel.GetTopLevel(host.Showcase)?.FocusManager?.GetFocusedElement();
        Assert.NotSame(skipButton, focused);
    }

    private static (Window Window, Grid Root, Showcase Showcase, ShowcaseController Controller, Button Target, Button OtherButton) CreateHost(
        bool includeTarget = true,
        bool includeSecondDuplicate = false,
        string stepKey = "Target")
    {
        var controller = new ShowcaseController();
        controller.Steps.Add(new ShowcaseStep
        {
            Key = stepKey,
            Title = "Step Title",
            Description = "Step Description"
        });

        var canvas = new Canvas();
        var layoutRoot = new Grid
        {
            Width = 640,
            Height = 480
        };

        layoutRoot.Children.Add(canvas);

        var target = new Button
        {
            Content = "Target",
            Width = 120,
            Height = 40
        };

        if (includeTarget)
        {
            Showcase.SetKey(target, stepKey);
            Canvas.SetLeft(target, 60);
            Canvas.SetTop(target, 80);
            canvas.Children.Add(target);
        }

        var otherButton = new Button
        {
            Content = "Other",
            Width = 120,
            Height = 40
        };
        Canvas.SetLeft(otherButton, 260);
        Canvas.SetTop(otherButton, 80);
        canvas.Children.Add(otherButton);

        if (includeSecondDuplicate)
        {
            var duplicate = new Button
            {
                Content = "Duplicate",
                Width = 120,
                Height = 40
            };

            Showcase.SetKey(duplicate, stepKey);
            Canvas.SetLeft(duplicate, 240);
            Canvas.SetTop(duplicate, 80);
            canvas.Children.Add(duplicate);
        }

        var showcase = new Showcase
        {
            Controller = controller
        };

        layoutRoot.Children.Add(showcase);

        var window = new Window
        {
            Width = 640,
            Height = 480,
            Content = layoutRoot
        };

        window.Show();
        window.UpdateLayout();
        showcase.ApplyTemplate();
        showcase.UpdateLayout();

        return (window, layoutRoot, showcase, controller, target, otherButton);
    }

    private static (Window Window, Showcase Showcase, ShowcaseController Controller, ScrollViewer ScrollViewer, Button Target) CreateScrollHost()
    {
        var controller = new ShowcaseController();
        controller.Steps.Add(new ShowcaseStep
        {
            Key = "DeepTarget",
            Title = "Deep Target",
            Description = "Scroll to reveal this element."
        });

        var target = new Button
        {
            Content = "Deep Target",
            Width = 120,
            Height = 40,
            Margin = new Thickness(24, 0, 24, 0)
        };

        Showcase.SetKey(target, "DeepTarget");

        var stackPanel = new StackPanel
        {
            Width = 420,
            Spacing = 16
        };

        for (var i = 0; i < 10; i++)
        {
            stackPanel.Children.Add(new Border
            {
                Height = 120,
                Margin = new Thickness(24, 0, 24, 0)
            });
        }

        stackPanel.Children.Add(target);
        stackPanel.Children.Add(new Border
        {
            Height = 120,
            Margin = new Thickness(24, 0, 24, 24)
        });

        var scrollViewer = new ScrollViewer
        {
            Width = 420,
            Height = 320,
            Content = stackPanel
        };

        var layoutRoot = new Grid
        {
            Width = 420,
            Height = 320
        };

        var showcase = new Showcase
        {
            Controller = controller
        };

        layoutRoot.Children.Add(scrollViewer);
        layoutRoot.Children.Add(showcase);

        var window = new Window
        {
            Width = 420,
            Height = 320,
            Content = layoutRoot
        };

        window.Show();
        window.UpdateLayout();
        showcase.ApplyTemplate();
        showcase.UpdateLayout();

        return (window, showcase, controller, scrollViewer, target);
    }

    private static ShowcaseOverlay GetOverlay(Showcase showcase) =>
        showcase.GetVisualDescendants().OfType<ShowcaseOverlay>().Single();

    private static ShowcaseTooltip GetTooltip(Showcase showcase) =>
        showcase.GetVisualDescendants().OfType<ShowcaseTooltip>().Single();

    private static Point GetTargetCenter(Control control, Visual relativeTo)
    {
        var topLeft = control.TranslatePoint(new Point(0, 0), relativeTo);
        Assert.True(topLeft.HasValue);

        return new Point(
            topLeft.Value.X + (control.Bounds.Width / 2),
            topLeft.Value.Y + (control.Bounds.Height / 2));
    }

    private static Border GetBlocker(Showcase showcase, string name) =>
        showcase.GetVisualDescendants().OfType<Border>().Single(x => x.Name == name);

    private static Border[] GetBlockers(Showcase showcase) =>
        showcase.GetVisualDescendants()
            .OfType<Border>()
            .Where(x => x.Name is "PART_ModalBlocker" or "PART_TopBlocker" or "PART_LeftBlocker" or "PART_RightBlocker" or "PART_BottomBlocker")
            .ToArray();

    private static bool ContainsPoint(Control control, Point point, Visual relativeTo)
    {
        var topLeft = control.TranslatePoint(new Point(0, 0), relativeTo);
        Assert.True(topLeft.HasValue);

        var bounds = new Rect(topLeft.Value, control.Bounds.Size);
        return bounds.Contains(point);
    }
}
