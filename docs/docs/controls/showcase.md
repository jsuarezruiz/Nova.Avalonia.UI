---
title: Showcase
description: Create interactive tutorials and onboarding experiences with the Showcase control.
ms.date: 2026-08-21
---

# Showcase

The `Showcase` control creates interactive tutorials and onboarding experiences by highlighting UI elements step-by-step with customizable tooltips and overlays. It helps users discover features through guided walkthroughs.

## Create a showcase

Add unique `Showcase.Key` attached properties to the elements you want to highlight, then add the tour steps to the `Showcase` control.

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
ShowcaseStep[] steps =
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
];

foreach (var step in steps)
    ShowcaseControl.Steps.Add(step);

ShowcaseControl.Start();
```

`Showcase` owns its steps, navigation state, commands, events, and persistence settings. Use `ShowcaseControl.Start()` for event-handler code or `await ShowcaseControl.StartAsync()` when the caller needs to observe asynchronous hooks and failures. Both entry points validate the visual tree before starting.

### Migrating from ShowcaseController

`Showcase` now owns the tour state directly. Move steps, navigation calls, commands, hooks, events, and persistence settings from `ShowcaseController` to the `Showcase` control, then remove the old `Controller` assignment. In step hooks, replace `context.Controller` with `context.Showcase`. This is a breaking API change from the controller-based version.

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

`IsValid` only fails on errors. Missing, hidden, or not-yet-laid-out targets are warnings because a `BeforeStepAsync` hook can create or reveal them before the step becomes active. Duplicate target keys remain errors because target resolution would be ambiguous.

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

The `Showcase` control provides methods, commands, and events for navigation:

```csharp
// Navigate programmatically
ShowcaseControl.Next();
ShowcaseControl.Previous();
ShowcaseControl.Skip();

// Handle events
ShowcaseControl.StepChanged += (s, e) => Console.WriteLine($"Step: {e.CurrentStep.Title}");
ShowcaseControl.Completed += (s, e) => Console.WriteLine("Tutorial finished!");
ShowcaseControl.Skipped += (s, e) => Console.WriteLine("User skipped tutorial");
```

For MVVM scenarios, bind to the built-in commands:

```xaml
<Button Command="{Binding NextCommand, ElementName=ShowcaseControl}" Content="Next" />
<Button Command="{Binding PreviousCommand, ElementName=ShowcaseControl}" Content="Back" />
<Button Command="{Binding SkipCommand, ElementName=ShowcaseControl}" Content="Skip" />
```

For asynchronous setup, use the async navigation methods and step hooks:

```csharp
ShowcaseControl.BeforeStepAsync = async (context, cancellationToken) =>
{
    if (context.NextStep.Key == "AdvancedPanel")
        await ExpandPanelAsync(cancellationToken);
};

await ShowcaseControl.StartAsync();
await ShowcaseControl.NextAsync();
```

The hook runs before Showcase resolves the next target, so it can safely load, expand, or add controls on demand.

To persist progress across sessions, assign a store and persistence key, then call `ResumeAsync()`:

```csharp
ShowcaseControl.ProgressStore = new InMemoryShowcaseProgressStore();
ShowcaseControl.PersistenceKey = "main-tour";

var resumed = await ShowcaseControl.ResumeAsync();
if (!resumed)
    await ShowcaseControl.StartAsync();
```

Progress snapshots include a stable step identity, so reordering the steps does not resume the wrong item. By default the target `Key` is used. Give each step a unique `Id` when multiple steps target the same control:

```csharp
new ShowcaseStep
{
    Id = "search-basics",
    Key = "SearchBox",
    Title = "Search"
}
```

Existing stores that only contain an index remain supported. Navigation is transactional when persistence is enabled: a step becomes active only after its progress is saved, and completing, skipping, or resetting deactivates the showcase only after its saved progress is cleared. The async methods surface store failures to the caller; the synchronous wrappers report them through `TransitionFailed`.

Users can also navigate using keyboard:

- **Arrow Right**: Move to the next step while focus is in the tutorial controls
- **Arrow Left**: Move to the previous step while focus is in the tutorial controls
- **Space / Enter**: Activate the focused tutorial button
- **Escape**: Skip the tutorial from anywhere in the active window

In `Modal` mode, Tab stays within the complete tooltip content, including controls supplied by a custom template, and underlying controls are removed from the control view of the automation tree. In `TargetOnly` mode, the highlighted control and its focusable children remain available to keyboard and assistive-technology users. `Passthrough` mode keeps the application's normal focus navigation and automation exposure.

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

Give interactive controls in a custom template clear accessible names. The tooltip announces the current title, description, and step number through UI automation when the active step changes.

## Customize appearance

Set the overlay color and animation duration on the `Showcase` control:

```xaml
<nova:Showcase x:Name="ShowcaseControl"
               OverlayBrush="#CC000000"
               AnimationDuration="0:0:0.3"
               AutoScrollIntoView="True"
               InteractionMode="Modal" />
```

`AnimationDuration` controls the built-in fade when `Transition` is `null`. A custom transition owns its duration; setting `AnimationDuration` to `0:0:0` disables either transition mode.

Set `Transition` to any Avalonia `IPageTransition` to replace the built-in fade. Showcase passes `forward: true` for Start, Resume, and Next, and `forward: false` for Previous, so direction-aware transitions work automatically.

```xaml
<nova:Showcase x:Name="ShowcaseControl"
               AnimationDuration="0:0:0.2">
    <nova:Showcase.Transition>
        <CrossFade Duration="0:0:0.2" />
    </nova:Showcase.Transition>
</nova:Showcase>
```

You can also implement `IPageTransition` for application-specific motion. The transition receives the tooltip as the incoming visual, participates in Showcase cancellation, and is replaced by the built-in fade when `Transition` is `null`.

`AutoScrollIntoView` is enabled by default. When a step becomes active, the target control is brought into view once before the tooltip is positioned. Set it to `False` if your application manages scrolling separately.

The tooltip automatically adapts to light and dark themes using Nova-owned resources, so it does not require FluentTheme. Applications can override `ShowcaseOverlayBrush`, `ShowcaseTooltipBackgroundBrush`, `ShowcaseTooltipForegroundBrush`, `ShowcaseTooltipSecondaryForegroundBrush`, `ShowcaseTooltipBorderBrush`, and `ShowcaseTooltipShadow` to match a custom theme.

## Target resolution

`Showcase.Key` values must be unique within the current visual root. If a target is temporarily unavailable because the layout has not finished yet, the tooltip stays visible and the control retries the target lookup at a bounded interval. If duplicate keys are detected at runtime, the highlight is skipped for that step and the tooltip is centered instead.

## Localization

Customize the button texts directly on the `Showcase` control:

```csharp
ShowcaseControl.NextButtonText = "Siguiente";
ShowcaseControl.FinishButtonText = "Finalizar";
ShowcaseControl.PreviousButtonText = "Anterior";
ShowcaseControl.SkipButtonText = "Omitir";
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
| `Steps` | `ObservableCollection<ShowcaseStep>` | empty | The ordered collection of tour steps |
| `CurrentStep` | `ShowcaseStep?` | `null` | The active step, or `null` when inactive |
| `CurrentIndex` | `int` | `-1` | Index of the active step |
| `IsActive` | `bool` | `false` | Whether the showcase is currently active |
| `CanGoPrevious` | `bool` | `false` | Whether a previous step is available |
| `CanGoNext` | `bool` | `false` | Whether another step is available |
| `CurrentButtonText` | `string` | `"Finish"` | The next or finish text for the current position |
| `NextButtonText` | `string` | `"Next"` | Text for the Next button |
| `FinishButtonText` | `string` | `"Finish"` | Text for the last-step button |
| `PreviousButtonText` | `string` | `"Previous"` | Text for the Previous button |
| `SkipButtonText` | `string` | `"Skip"` | Text for the Skip button |
| `OverlayBrush` | `IBrush?` | Black 70% | Brush for the dimmed overlay |
| `AnimationDuration` | `TimeSpan` | 300ms | Built-in fade duration; `0` disables built-in and custom transitions |
| `Transition` | `IPageTransition?` | `null` | Custom tooltip transition; `null` uses the built-in fade |
| `AutoScrollIntoView` | `bool` | `true` | Bring the active target into view on step change |
| `InteractionMode` | `ShowcaseInteractionMode` | `Modal` | Default interaction mode for all steps |
| `BeforeStepAsync` | `Func<..., Task>?` | `null` | Async hook before a step becomes active |
| `AfterStepAsync` | `Func<..., Task>?` | `null` | Async hook after a step becomes active |
| `ProgressStore` | `IShowcaseProgressStore?` | `null` | Store for persisting progress |
| `PersistenceKey` | `string?` | `null` | Key used with `ProgressStore` |

### ShowcaseStep properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Id` | `string?` | `null` | Stable persisted identity; set a unique value when steps share a target |
| `Key` | `required string` | | Matches a unique `Showcase.Key` on the target element |
| `Title` | `string` | `""` | Tooltip title text |
| `Description` | `string` | `""` | Tooltip description text |
| `TooltipPosition` | `ShowcaseTooltipPosition` | `Auto` | Preferred tooltip placement |
| `InteractionMode` | `ShowcaseInteractionMode?` | `null` | Per-step interaction override |
| `HighlightShape` | `ShowcaseHighlightShape` | `RoundedRectangle` | Cutout shape around target |
| `HighlightPadding` | `Thickness` | `8` | Padding around the highlight |
| `HighlightCornerRadius` | `double` | `8` | Corner radius for rounded shapes |
| `TooltipTemplate` | `IDataTemplate?` | `null` | Tooltip body template (element-level takes priority) |

### Showcase events

| Event | Args | Description |
|-------|------|-------------|
| `Started` | `EventArgs` | Raised when the showcase starts |
| `Completed` | `EventArgs` | Raised when all steps finish |
| `Skipped` | `EventArgs` | Raised when the user skips |
| `Resumed` | `EventArgs` | Raised when resuming from persisted progress |
| `StepChanged` | `ShowcaseStepChangedEventArgs` | Raised when the active step changes |
| `TransitionFailed` | `ShowcaseTransitionFailedEventArgs` | Raised when a synchronous navigation wrapper or visual transition fails |

### Showcase commands

| Command | Description |
|---------|-------------|
| `NextCommand` | Advances to the next step (enabled when active) |
| `PreviousCommand` | Goes back to the previous step (enabled when active and not on first step) |
| `SkipCommand` | Skips or cancels the showcase (enabled when active) |

Call `Reset()` or `ResetAsync()` to deactivate the showcase and clear persisted progress. Pending transitions are cancelled automatically when the control leaves the visual tree.
