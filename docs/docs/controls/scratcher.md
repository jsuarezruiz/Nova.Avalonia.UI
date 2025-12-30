---
title: Scratcher
description: An interactive control that hides content beneath a scratchable overlay.
ms.date: 2025-12-29
---

# Scratcher

The `Scratcher` control temporarily hides content beneath an opaque overlay. Users can reveal the hidden content by "scratching" the overlay with pointer input, similar to a physical scratch card.

## Basic Usage

Declare a `Scratcher` and place your hidden content inside it. Set the `OverlayBrush` to determine what covers the content.

```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:nova="clr-namespace:Nova.Avalonia.UI.Controls;assembly=Nova.Avalonia.UI">

    <nova:Scratcher Width="300" 
                    Height="200" 
                    OverlayBrush="Gray">
        <StackPanel VerticalAlignment="Center" HorizontalAlignment="Center">
            <TextBlock Text="Congratulations!" FontSize="24" FontWeight="Bold"/>
            <TextBlock Text="You won a prize!" HorizontalAlignment="Center"/>
        </StackPanel>
    </nova:Scratcher>
</UserControl>
```

## Customizing the Brush and Threshold

You can control the size of the scratching brush using `BrushSize` and determine when the content should be considered "fully revealed" using the `Threshold` property.

```xaml
<nova:Scratcher BrushSize="40" 
                Threshold="70" 
                OverlayBrush="Silver">
    <!-- Content here -->
</nova:Scratcher>
```

- `BrushSize`: The diameter of the scratch tool in pixels. Default is 30.
- `Threshold`: The percentage (0-100) of the overlay that must be removed to trigger the `ThresholdReached` event. Default is 50.

## Interactive States

`Scratcher` provides several events to track user interaction and progress:

- `ProgressChanged`: Fires as the user scratches, providing the current percentage of revealed area.
- `ThresholdReached`: Fires once when the `Threshold` percentage is met.
- `ScratchStarted`, `ScratchUpdated`, `ScratchEnded`: Fire at different stages of the pointer interaction.

```csharp
private void OnProgressChanged(object? sender, ScratchProgressEventArgs e)
{
    Debug.WriteLine($"Current progress: {e.Progress}%");
}
```

## Methods and Animations

The control supports programmatic reset and reveal, with optional timed animations.

- `Reset(TimeSpan? duration)`: Rebuilds the overlay. If a duration is provided, the overlay fades back in.
- `Reveal(TimeSpan? duration)`: Removes the entire overlay. If a duration is provided, the overlay fades out.

```csharp
// Instant reset
myScratcher.Reset();

// Animated reveal over 500ms
myScratcher.Reveal(TimeSpan.FromMilliseconds(500));
```

## Additional Properties

- `CornerRadius`: Applies rounded corners to the overlay clipping.
- `IsEnabled`: When `False`, the control ignores all scratching input.
- `RebuildOnResize`: Determines if the scratch surface should be rebuilt when the control size changes. Default is `True`.
