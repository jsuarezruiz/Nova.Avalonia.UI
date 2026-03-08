---
title: Scratcher
description: An interactive control that hides content beneath a scratchable overlay.
ms.date: 2026-02-01
---

# Scratcher

The `Scratcher` control temporarily hides content beneath an opaque overlay. Users can reveal the hidden content by "scratching" the overlay with pointer input, similar to a physical scratch card.

## Basic Usage

Declare a `Scratcher` and place your hidden content inside it. Set the `OverlayBrush` to determine what covers the content. The `OverlayBrush` supports any `IBrush`, including solid colors and gradients.

```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:nova="clr-namespace:Nova.Avalonia.UI.Controls;assembly=Nova.Avalonia.UI">

    <nova:Scratcher Width="300" 
                    Height="200">
        <nova:Scratcher.OverlayBrush>
            <LinearGradientBrush StartPoint="0%,0%" EndPoint="100%,100%">
                <GradientStop Color="Gray" Offset="0" />
                <GradientStop Color="Silver" Offset="1" />
            </LinearGradientBrush>
        </nova:Scratcher.OverlayBrush>
        <StackPanel VerticalAlignment="Center" HorizontalAlignment="Center">
            <TextBlock Text="Congratulations!" FontSize="24" FontWeight="Bold"/>
            <TextBlock Text="You won a prize!" HorizontalAlignment="Center"/>
        </StackPanel>
    </nova:Scratcher>
</UserControl>
```

## Customizing the Brush and Threshold

You can control the size of the scratching brush using `BrushSize` and determine when the content should be considered "fully revealed" using the `Threshold` property.

- `BrushSize`: The diameter of the scratch tool in pixels. Default is 30.
- `Threshold`: The percentage (0 to 100) of the overlay that must be removed to trigger the `ThresholdReached` event. Default is 50.

## Interactive States and Properties

`Scratcher` provides several events and properties to track progress:

- `ScratchProgress`: Read only property returning the current percentage (0 to 100) of revealed area.
- `IsThresholdReached`: Read only property that becomes true once the `Threshold` is met.
- `ProgressChanged`: Fires as the user scratches, providing detailed progress data.
- `ThresholdReached`: Fires once when the `Threshold` percentage is met.
- `ScratchStarted`, `ScratchUpdated`, `ScratchEnded`: Fire at different stages of the pointer interaction.

## Methods and Animations

The control supports programmatic reset and reveal, with optional timed animations.

- `Reset(TimeSpan? duration)`: Rebuilds the overlay. If a duration is provided, the overlay fades back in.
- `Reveal(TimeSpan? duration)`: Removes the entire overlay. If a duration is provided, the overlay fades out.

## Mask Management

The mask methods let you save and restore the scratch state, for example to resume progress after navigating away:

- `GetScratchMask()`: Returns a `WriteableBitmap` snapshot of the current scratch state.
- `SetScratchMask(WriteableBitmap mask)`: Applies a previously saved mask, restoring the scratch state exactly.

```csharp
// Save
_savedMask = scratcher.GetScratchMask();

// Restore
if (_savedMask != null)
    scratcher.SetScratchMask(_savedMask);
```

## Accessibility

The `Scratcher` control is designed for accessibility:

- **Keyboard navigation**: The control is focusable. Users can press `Space` or `Enter` to instantly reveal the content.
- **Screen readers**: The control uses a custom `AutomationPeer` to report its state and instructions. Screen readers will announce the revealed state or prompt the user to interact.

## Additional Properties

- `IsEnabled`: When `False`, the control ignores all scratching input.
- `RebuildOnResize`: Determines if the scratch surface should be rebuilt when the control size changes. Default is `True`.
