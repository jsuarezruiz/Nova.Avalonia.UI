---
title: CircularPanel
description: A layout panel that arranges items in a circular pattern.
ms.date: 2026-01-02
---

# CircularPanel

The `CircularPanel` arranges child elements evenly around a circle. This is perfect for radial menus, clock faces, or any circular arrangement.

## Basic Usage

```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:nova="using:Nova.Avalonia.UI.Controls">

    <nova:CircularPanel Radius="100">
        <Border Width="40" Height="40" Background="#FF6B6B" CornerRadius="20"/>
        <Border Width="40" Height="40" Background="#4ECDC4" CornerRadius="20"/>
        <Border Width="40" Height="40" Background="#45B7D1" CornerRadius="20"/>
        <Border Width="40" Height="40" Background="#FFA07A" CornerRadius="20"/>
        <Border Width="40" Height="40" Background="#98D8C8" CornerRadius="20"/>
        <Border Width="40" Height="40" Background="#F7DC6F" CornerRadius="20"/>
    </nova:CircularPanel>
</UserControl>
```

## Positioning

Items are automatically distributed evenly around the circle. The `StartAngle` property controls where the first item is placed.

- **0°**: 3 o'clock position (right)
- **-90°**: 12 o'clock position (top)
- **90°**: 6 o'clock position (bottom)
- **180°**: 9 o'clock position (left)

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Radius` | `double` | `100` | The radius of the circle. |
| `StartAngle` | `double` | `0` | The angle where the first item is placed (degrees). |
| `AngleStep` | `double` | `0` | Override the angle between items. 0 = auto-calculate. |
| `SweepDirection` | `SweepDirection` | `Clockwise` | Direction of item placement (`Clockwise` or `CounterClockwise`). |

## Examples

### Clock Face

```xaml
<nova:CircularPanel Radius="100" StartAngle="-90">
    <TextBlock Text="12" FontWeight="Bold"/>
    <TextBlock Text="1"/>
    <TextBlock Text="2"/>
    <TextBlock Text="3" FontWeight="Bold"/>
    <TextBlock Text="4"/>
    <TextBlock Text="5"/>
    <TextBlock Text="6" FontWeight="Bold"/>
    <TextBlock Text="7"/>
    <TextBlock Text="8"/>
    <TextBlock Text="9" FontWeight="Bold"/>
    <TextBlock Text="10"/>
    <TextBlock Text="11"/>
</nova:CircularPanel>
```

### Radial Menu

```xaml
<nova:CircularPanel Radius="80" StartAngle="-90">
    <Button Content="🏠" ToolTip.Tip="Home"/>
    <Button Content="⚙️" ToolTip.Tip="Settings"/>
    <Button Content="📁" ToolTip.Tip="Files"/>
    <Button Content="❤️" ToolTip.Tip="Favorites"/>
</nova:CircularPanel>
```
