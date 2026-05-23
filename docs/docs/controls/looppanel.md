---
title: LoopPanel
description: An infinite scrolling panel with looping, inertia, and snap-to-item behavior.
ms.date: 2026-01-02
---

# LoopPanel

The `LoopPanel` creates an infinite/looping scrolling experience where children wrap seamlessly. It supports drag gestures, mouse wheel scrolling, momentum (inertia), and snap-to-item behavior.

## Basic Usage

```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:nova="using:Nova.Avalonia.UI.Controls">

    <nova:LoopPanel Height="120" Spacing="10">
        <Border Width="100" Height="100" Background="Red" CornerRadius="8"/>
        <Border Width="100" Height="100" Background="Green" CornerRadius="8"/>
        <Border Width="100" Height="100" Background="Blue" CornerRadius="8"/>
        <Border Width="100" Height="100" Background="Orange" CornerRadius="8"/>
    </nova:LoopPanel>
</UserControl>
```

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Orientation` | `Orientation` | `Horizontal` | Layout direction (Horizontal or Vertical). |
| `Offset` | `double` | `0.0` | Current scroll position. 1.0 = one full item. |
| `AnchorPosition` | `double` | `0.5` | Viewport anchor point (0.0=start, 0.5=center, 1.0=end). |
| `Spacing` | `double` | `0.0` | Gap between items in pixels. |
| `IsInertiaEnabled` | `bool` | `True` | Enable momentum scrolling after drag release. |
| `SnapToItems` | `bool` | `True` | Snap to nearest item when scrolling ends. |
| `ScrollFactor` | `double` | `1.0` | Multiplier for drag/wheel sensitivity. |

## Methods

| Method | Description |
|--------|-------------|
| `ScrollBy(double units)` | Scroll by specified pixel units. |
| `ScrollToIndex(int index, bool animate)` | Scroll to bring a specific child to the anchor point. |

## Events

| Event | Description |
|-------|-------------|
| `CurrentIndexChanged` | Raised when the pivotal (anchored) item index changes. |

## Vertical Orientation

```xaml
<nova:LoopPanel Orientation="Vertical" Height="300" Width="150" Spacing="10">
    <Border Height="80" Background="Red"/>
    <Border Height="80" Background="Green"/>
    <Border Height="80" Background="Blue"/>
</nova:LoopPanel>
```

## Anchor Positioning

Control where items align in the viewport:

```xaml
<!-- Items start at left edge -->
<nova:LoopPanel AnchorPosition="0.0" />

<!-- Items center in viewport (default) -->
<nova:LoopPanel AnchorPosition="0.5" />

<!-- Items align to right edge -->
<nova:LoopPanel AnchorPosition="1.0" />
```

## Programmatic Scrolling

```csharp
// Scroll by 100 pixels
myLoopPanel.ScrollBy(100);

// Jump to item at index 3
myLoopPanel.ScrollToIndex(3, animate: false);

// Animate to item at index 5
myLoopPanel.ScrollToIndex(5, animate: true);
```

## Handling Index Changes

```csharp
myLoopPanel.CurrentIndexChanged += (sender, index) =>
{
    Debug.WriteLine($"Now showing item {index}");
};
```

## Disabling Inertia

For precise control without momentum:

```xaml
<nova:LoopPanel IsInertiaEnabled="False" SnapToItems="True" />
```

## Dynamic Items

Items can be added or removed at runtime:

```csharp
myLoopPanel.Children.Add(new Border { Width = 100, Height = 100, Background = Brushes.Purple });
myLoopPanel.Children.RemoveAt(myLoopPanel.Children.Count - 1);
```

## Notes

- Each child control can only appear once in the viewport at any time.
- For very wide viewports, ensure you have enough items to fill the space.
- `ClipToBounds` is enabled by default to prevent overflow.
