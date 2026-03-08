---
title: Showcase
description: Create interactive tutorials and onboarding experiences with the Showcase control.
ms.date: 2026-03-08
---

# Showcase

The `Showcase` control creates interactive tutorials and onboarding experiences by highlighting UI elements step-by-step with customizable tooltips and overlays. It helps users discover features through guided walkthroughs.

## Create a showcase

Add `Showcase.Key` attached properties to target elements, create a `ShowcaseController` with steps, and bind it to the `Showcase` control.

```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:nova="clr-namespace:Nova.Avalonia.UI.Controls;assembly=Nova.Avalonia.UI">
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
// Create controller and add steps
var controller = new ShowcaseController();
controller.Steps.Add(new ShowcaseStep
{
    Key = "StartButton",
    Title = "Get Started",
    Description = "Click here to begin your journey.",
    TooltipPosition = ShowcaseTooltipPosition.Bottom
});
controller.Steps.Add(new ShowcaseStep
{
    Key = "SearchBox",
    Title = "Quick Search",
    Description = "Find what you're looking for instantly.",
    TooltipPosition = ShowcaseTooltipPosition.Top
});

// Bind and start
ShowcaseControl.Controller = controller;
controller.Start();
ShowcaseControl.IsActive = true;
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

Users can also navigate using keyboard:
- **Arrow Right / Space / Enter**: Next step
- **Arrow Left**: Previous step
- **Escape**: Skip tutorial

## Custom tooltip templates

Override the tooltip content per-step or per-element.

**Per-step** — set `CustomTooltipTemplate` on the step:

```csharp
new ShowcaseStep
{
    Key = "ProfileButton",
    Title = "Your Profile",
    CustomTooltipTemplate = new FuncDataTemplate<ShowcaseStep>((step, _) =>
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

Template resolution order: element-level `Showcase.TooltipTemplate` > step-level `CustomTooltipTemplate` > default template.

## Customize appearance

Set the overlay color and animation duration on the `Showcase` control:

```xaml
<nova:Showcase x:Name="ShowcaseControl"
               OverlayBrush="#CC000000"
               AnimationDuration="0:0:0.3" />
```

The `AnimationDuration` controls the fade-in animation when transitioning between steps. Set to `0:0:0` to disable animations.

The tooltip automatically adapts to light and dark themes using system resources.

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

## ShowcaseStep properties

| Property | Type | Description |
|----------|------|-------------|
| `Key` | `string` | Matches the `Showcase.Key` on target element |
| `Title` | `string` | Tooltip title text |
| `Description` | `string` | Tooltip description text |
| `TooltipPosition` | `ShowcaseTooltipPosition` | Preferred tooltip placement |
| `HighlightShape` | `ShowcaseHighlightShape` | Cutout shape around target |
| `HighlightPadding` | `Thickness` | Padding around the highlight |
| `CornerRadius` | `double` | Corner radius for rounded shapes |
| `CustomTooltipTemplate` | `IDataTemplate?` | Custom tooltip template for this step |

## ShowcaseController properties

| Property | Type | Description |
|----------|------|-------------|
| `NextButtonText` | `string` | Text for the Next button (default: "Next") |
| `FinishButtonText` | `string` | Text for the last-step button (default: "Finish") |
| `PreviousButtonText` | `string` | Text for the Previous button (default: "Previous") |
| `SkipButtonText` | `string` | Text for the Skip button (default: "Skip") |
