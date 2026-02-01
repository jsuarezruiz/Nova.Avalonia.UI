---
title: ArcPanel
description: A layout panel that arranges items along an arc (partial circle).
ms.date: 2026-01-02
---

# ArcPanel

The `ArcPanel` arranges child elements along an arc, which is a portion of a circle. This is useful for creating semi-circular menus, dial interfaces, or decorative layouts.

## Basic Usage

```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:nova="using:Nova.Avalonia.UI.Controls">

    <nova:ArcPanel Radius="100" StartAngle="-90" SweepAngle="180">
        <Border Width="40" Height="40" Background="Red" CornerRadius="20"/>
        <Border Width="40" Height="40" Background="Green" CornerRadius="20"/>
        <Border Width="40" Height="40" Background="Blue" CornerRadius="20"/>
        <Border Width="40" Height="40" Background="Orange" CornerRadius="20"/>
        <Border Width="40" Height="40" Background="Purple" CornerRadius="20"/>
    </nova:ArcPanel>
</UserControl>
```

## Positioning

- **StartAngle**: The angle (in degrees) where the arc begins. 0° is at the 3 o'clock position, and angles increase clockwise.
- **SweepAngle**: The total arc length in degrees. Use 180 for a semi-circle, 90 for a quarter-circle.

Items are distributed evenly along the arc based on the number of children.

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Radius` | `double` | `100` | The radius of the arc from center to item centers. |
| `StartAngle` | `double` | `0` | The starting angle in degrees (0° = 3 o'clock). |
| `SweepAngle` | `double` | `180` | The arc span in degrees. |
| `RotateItems` | `bool` | `false` | Whether items rotate to follow the arc tangent. |

## Examples

### Semi-Circle Menu (Top)

```xaml
<nova:ArcPanel Radius="120" StartAngle="-90" SweepAngle="180" 
               HorizontalAlignment="Center" VerticalAlignment="Center">
    <Button Content="1"/>
    <Button Content="2"/>
    <Button Content="3"/>
    <Button Content="4"/>
    <Button Content="5"/>
</nova:ArcPanel>
```

### Quarter Arc

```xaml
<nova:ArcPanel Radius="80" StartAngle="0" SweepAngle="90">
    <Ellipse Width="30" Height="30" Fill="Coral"/>
    <Ellipse Width="30" Height="30" Fill="Teal"/>
    <Ellipse Width="30" Height="30" Fill="Gold"/>
</nova:ArcPanel>
```
