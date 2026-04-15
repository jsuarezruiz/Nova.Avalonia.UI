---
title: Showcase
description: Create interactive tutorials and onboarding experiences with the Showcase control.
ms.date: 2026-03-23
---

# Showcase

The `Showcase` control creates interactive tutorials and onboarding experiences by highlighting UI elements step-by-step with customizable tooltips and overlays. It helps users discover features through guided walkthroughs.

## Create a showcase

Add unique `Showcase.Key` attached properties to target elements, create a `ShowcaseController` with steps, and bind it to the `Showcase` control.

```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:nova="using:Nova.Avalonia.UI.Controls">
    <Panel>
        <!-- Your UI with showcase keys -->
        <Button nova:Showcase.Key="StartButton" Content="Get Started" />
        <TextBox nova:Showcase.Key="SearchBox" Watermark="Search..." />

        <!-- Showcase control -->
        <nova:Showcase x:Name="ShowcaseControl" />
    </Panel>
</UserControl>
```

```csharp
var controller = new ShowcaseController(
[
    new ShowcaseStep
    {
        Key = "StartButton",
        Title = "Get Started",
        Description = "Click here to begin your journey.",
        TooltipPosition = ShowcaseTooltipPosition.Bottom
    },
    new ShowcaseStep
    {
        Key = "SearchBox",
        Title = "Quick Search",
        Description = "Find what you're looking for instantly.",
        TooltipPosition = ShowcaseTooltipPosition.Top
    }
]);

ShowcaseControl.Controller = controller;
ShowcaseControl.Start();
```

Use `ShowcaseControl.Start()` or `await ShowcaseControl.StartAsync()` when starting from the view. Those entrypoints validate the visual tree before they activate the controller. `ShowcaseController.Start()` is still available when you intentionally want low-level controller-only flow control.

## Validation

Call `Validate()` to inspect configuration issues before starting a showcase, or use `TryStart()` / `TryStartAsync()` to validate and start in one step.

```csharp
var result = await ShowcaseControl.TryStartAsync();

if (!result.Started)
{
    foreach (var issue in result.ValidationResult.Issues)
        Console.WriteLine($"{issue.Severity}: {issue.Message}");
}
```

`IsValid` only fails on errors. Warnings report degraded experiences, such as targets that exist but are not currently visible or laid out.

## Interaction modes

Use `InteractionMode` to control how much of the underlying UI remains interactive while the showcase is active.

| Mode | Description |
|------|-------------|
| `Modal` | Blocks the underlying UI and keeps only the showcase chrome interactive |
| `TargetOnly` | Keeps the highlighted target interactive while blocking the rest of the UI |
| `Passthrough` | Leaves the underlying UI interactive and uses the showcase as a visual guide |

```xaml
<nova:Showcase x:Name="ShowcaseControl"
               InteractionMode="TargetOnly" />
```

`Modal` is the default.

You can also override the mode per step:

```csharp
new ShowcaseStep
{
    Key = "DangerousButton",
    Title = "Confirm this action",
    InteractionMode = ShowcaseInteractionMode.TargetOnly
}
```

## Tooltip positions

Control where tooltips appear relative to highlighted elements with `TooltipPosition`. The control automatically falls back to alternative positions if the preferred position would clip.

| Position | Description |
|----------|-------------|
| `Auto` | Automatically choose the best position |
| `Top` | Above the target element |
| `Bottom` | Below the target element |
| `Left` | To the left of the target |
| `Right` | To the right of the target |
| `Center` | Centered on screen |

```csharp
new ShowcaseStep
{
    Key = "SidePanel",
    Title = "Navigation",
    Description = "Access all sections from here.",
    TooltipPosition = ShowcaseTooltipPosition.Right
}
```

## Highlight shapes

Customize the cutout shape around highlighted elements using `HighlightShape`.

```csharp
new ShowcaseStep
{
    Key = "ProfileButton",
    Title = "Your Profile",
    Description = "View and edit your settings.",
    HighlightShape = ShowcaseHighlightShape.Circle,
    HighlightPadding = new Thickness(16)
}
```

Available shapes: `RoundedRectangle` (default), `Rectangle`, `Circle`.

## Navigation and events

The `ShowcaseController` provides commands and events for navigation:

```csharp
// Navigate programmatically
controller.Next();
controller.Previous();
controller.Skip();

// Handle events
controller.StepChanged += (s, e) => Console.WriteLine($"Step: {e.CurrentStep?.Title}");
controller.Completed += (s, e) => Console.WriteLine("Tutorial finished!");
controller.Skipped += (s, e) => Console.WriteLine("User skipped tutorial");
```

For MVVM scenarios, bind to the built-in commands:

```xaml
<Button Command="{Binding Controller.NextCommand}" Content="Next" />
<Button Command="{Binding Controller.PreviousCommand}" Content="Back" />
<Button Command="{Binding Controller.SkipCommand}" Content="Skip" />
```

For asynchronous setup, use the async navigation methods and step hooks:

```csharp
controller.BeforeStepAsync = async (context, cancellationToken) =>
{
    if (context.NextStep.Key == "AdvancedPanel")
        await ExpandPanelAsync(cancellationToken);
};

await controller.StartAsync();
await controller.NextAsync();
```

To persist progress across sessions, assign a store and persistence key, then call `ResumeAsync()`:

```csharp
controller.ProgressStore = new InMemoryShowcaseProgressStore();
controller.PersistenceKey = "main-tour";

var resumed = await controller.ResumeAsync();
if (!resumed)
    await controller.StartAsync();
```

Users can also navigate using keyboard:
- **Arrow Right / Space / Enter**: Next step
- **Arrow Left**: Previous step
- **Escape**: Skip tutorial

## Custom tooltip templates

Replace the default tooltip body per-step or per-element. The footer buttons remain built in.

**Per-step** — set `TooltipTemplate` on the step:

```csharp
new ShowcaseStep
{
    Key = "ProfileButton",
    Title = "Your Profile",
    TooltipTemplate = new FuncDataTemplate<ShowcaseStep>((step, _) =>
        new StackPanel
        {
            Children = { new TextBlock { Text = step.Title, FontWeight = FontWeight.Bold } }
        })
}
```

**Per-element** — set the `Showcase.TooltipTemplate` attached property on the target control. This takes priority over the step-level template:

```xaml
<Button nova:Showcase.Key="HelpButton" Content="Help">
    <nova:Showcase.TooltipTemplate>
        <DataTemplate>
            <StackPanel>
                <TextBlock Text="Need Help?" FontWeight="Bold" />
                <TextBlock Text="Click here for support." />
            </StackPanel>
        </DataTemplate>
    </nova:Showcase.TooltipTemplate>
</Button>
```

Template resolution order: element-level `Showcase.TooltipTemplate` > step-level `TooltipTemplate` > default title/description body.

## Customize appearance

Set the overlay color and animation duration on the `Showcase` control:

```xaml
<nova:Showcase x:Name="ShowcaseControl"
               OverlayBrush="#CC000000"
               AnimationDuration="0:0:0.3"
               AutoScrollIntoView="True"
               InteractionMode="Modal" />
```

The `AnimationDuration` controls the fade-in animation when transitioning between steps. Set to `0:0:0` to disable animations.

`AutoScrollIntoView` is enabled by default. When a step becomes active, the target control is brought into view once before the tooltip is positioned. Set it to `False` if your application manages scrolling separately.

The tooltip automatically adapts to light and dark themes using system resources.

## Target resolution

`Showcase.Key` values must be unique within the current visual root. If a target is temporarily unavailable because the layout has not finished yet, the tooltip stays visible and the control retries the target lookup during layout updates. If duplicate keys are detected at runtime, the highlight is skipped for that step and the tooltip is centered instead.

## Localization

Customize the button texts via `ShowcaseController` properties:

```csharp
var controller = new ShowcaseController
{
    NextButtonText = "Siguiente",
    FinishButtonText = "Finalizar",
    PreviousButtonText = "Anterior",
    SkipButtonText = "Omitir"
};
```

## API reference

### Attached properties

| Property | Type | Description |
|----------|------|-------------|
| `Showcase.Key` | `string?` | Unique key that identifies a target element for a showcase step |
| `Showcase.TooltipTemplate` | `IDataTemplate?` | Element-level tooltip body override (takes priority over step-level `TooltipTemplate`) |

### Showcase control properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `IsActive` | `bool` | `false` | Whether the showcase is currently active (read-only, derived from Controller) |
| `Controller` | `ShowcaseController?` | `null` | The controller managing the showcase flow |
| `OverlayBrush` | `IBrush?` | Black 70% | Brush for the dimmed overlay |
| `AnimationDuration` | `TimeSpan` | 300ms | Tooltip fade-in duration (`0` to disable) |
| `AutoScrollIntoView` | `bool` | `true` | Bring the active target into view on step change |
| `InteractionMode` | `ShowcaseInteractionMode` | `Modal` | Default interaction mode for all steps |

### ShowcaseStep properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Key` | `required string` | | Matches a unique `Showcase.Key` on the target element |
| `Title` | `string` | `""` | Tooltip title text |
| `Description` | `string` | `""` | Tooltip description text |
| `TooltipPosition` | `ShowcaseTooltipPosition` | `Auto` | Preferred tooltip placement |
| `InteractionMode` | `ShowcaseInteractionMode?` | `null` | Per-step interaction override |
| `HighlightShape` | `ShowcaseHighlightShape` | `RoundedRectangle` | Cutout shape around target |
| `HighlightPadding` | `Thickness` | `8` | Padding around the highlight |
| `HighlightCornerRadius` | `double` | `8` | Corner radius for rounded shapes |
| `TooltipTemplate` | `IDataTemplate?` | `null` | Tooltip body template (element-level takes priority) |

### ShowcaseController properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Steps` | `ObservableCollection<ShowcaseStep>` | empty | The collection of showcase steps |
| `CurrentStep` | `ShowcaseStep?` | `null` | The active step, or null when inactive |
| `CurrentIndex` | `int` | `-1` | Index of the active step |
| `IsActive` | `bool` | `false` | Whether the showcase is running |
| `CanGoPrevious` | `bool` | `false` | Whether a previous step is available |
| `CanGoNext` | `bool` | `false` | Whether a next step is available |
| `CurrentButtonText` | `string` | | `NextButtonText` or `FinishButtonText` based on position |
| `NextButtonText` | `string` | `"Next"` | Text for the Next button |
| `FinishButtonText` | `string` | `"Finish"` | Text for the last-step button |
| `PreviousButtonText` | `string` | `"Previous"` | Text for the Previous button |
| `SkipButtonText` | `string` | `"Skip"` | Text for the Skip button |
| `BeforeStepAsync` | `Func<..., Task>?` | `null` | Async hook before a step becomes active |
| `AfterStepAsync` | `Func<..., Task>?` | `null` | Async hook after a step becomes active |
| `ProgressStore` | `IShowcaseProgressStore?` | `null` | Store for persisting progress |
| `PersistenceKey` | `string?` | `null` | Key used with `ProgressStore` |

### ShowcaseController events

| Event | Args | Description |
|-------|------|-------------|
| `Started` | `EventArgs` | Raised when the showcase starts |
| `Completed` | `EventArgs` | Raised when all steps finish |
| `Skipped` | `EventArgs` | Raised when the user skips |
| `Resumed` | `EventArgs` | Raised when resuming from persisted progress |
| `StepChanged` | `ShowcaseStepChangedEventArgs` | Raised when the active step changes |
| `TransitionFailed` | `ShowcaseTransitionFailedEventArgs` | Raised when a fire-and-forget transition throws |

### ShowcaseController commands

| Command | Description |
|---------|-------------|
| `NextCommand` | Advances to the next step (enabled when active) |
| `PreviousCommand` | Goes back to the previous step (enabled when active and not on first step) |
| `SkipCommand` | Skips/cancels the showcase (enabled when active) |

`ShowcaseController` implements `IDisposable`. Disposing a controller deactivates it (sets `IsActive` to `false` and resets `CurrentIndex`), cancels any in-flight transitions, and releases internal synchronization resources. Calling navigation methods after disposal throws `ObjectDisposedException`.
