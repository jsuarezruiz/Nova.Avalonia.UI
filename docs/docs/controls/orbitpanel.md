---
title: OrbitPanel
description: A panel that arranges child elements in concentric orbit rings around a center point.
ms.date: 2026-01-01
---

# OrbitPanel

The `OrbitPanel` places items in concentric rings (orbits) around a central point. You assign items to specific orbits using the `OrbitPanel.Orbit` attached property.

## Basic Usage

Items with `Orbit="0"` are placed in the center. Items with `Orbit="1"` go to the first ring, and so on.

```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:nova="using:Nova.Avalonia.UI.Controls">

    <nova:OrbitPanel InnerRadius="50" OrbitSpacing="60" StartAngle="0">
        
        <!-- Center Item -->
        <Ellipse Width="50" Height="50" Fill="Red" nova:OrbitPanel.Orbit="0" />
        
        <!-- Orbit 1 Items -->
        <Ellipse Width="30" Height="30" Fill="Blue" nova:OrbitPanel.Orbit="1" />
        <Ellipse Width="30" Height="30" Fill="Blue" nova:OrbitPanel.Orbit="1" />
        
        <!-- Orbit 2 Items -->
        <Ellipse Width="40" Height="40" Fill="Green" nova:OrbitPanel.Orbit="2" />
        <Ellipse Width="40" Height="40" Fill="Green" nova:OrbitPanel.Orbit="2" />
        <Ellipse Width="40" Height="40" Fill="Green" nova:OrbitPanel.Orbit="2" />
        
    </nova:OrbitPanel>
</UserControl>
```

## Layout Logic

- **Orbit 0**: Items are stacked at the center coordinates (useful if you only have one center item, or want them to overlap at the center).
- **Orbit 1+**: Items are distributed evenly along the circumference of the ring.

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `InnerRadius` | `double` | `50` | The radius of the first orbit ring (Orbit 1). |
| `OrbitSpacing` | `double` | `60` | The distance between consecutive rings. |
| `StartAngle` | `double` | `0` | The starting angle for item distribution in degrees. |

## Attached Properties

| Property | Type | Description |
|----------|------|-------------|
| `Orbit` | `int` | The zero-based index of the orbit ring to place the item in. 0 is center. |
