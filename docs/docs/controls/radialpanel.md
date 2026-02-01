---
title: RadialPanel
description: A layout panel that positions items in a circular or spiral arrangement.
ms.date: 2026-01-01
---

# RadialPanel

The `RadialPanel` arranges its children in a circular fan or ring pattern. It is useful for creating menus, dials, or decorative layouts where items surround a central point.

## Basic Usage

The `RadialPanel` calculates positions based on a central point and a specified radius.

```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:nova="using:Nova.Avalonia.UI.Controls">

    <nova:RadialPanel Radius="150" StartAngle="0" SweepAngle="360" RotateItems="True">
        <Button Content="Item 1" />
        <Button Content="Item 2" />
        <Button Content="Item 3" />
        <Button Content="Item 4" />
        <Button Content="Item 5" />
    </nova:RadialPanel>
</UserControl>
```

## Layout Configuration

You can control the arc of the layout using `StartAngle` and `SweepAngle`.

- **Radius**: The distance from the center of the panel to the center of the items.
- **StartAngle**: The angle (in degrees) where the first item is placed. 0 is exactly to the right (3 o'clock).
- **SweepAngle**: The total angle covered by the layout. 360 creates a full circle; 180 creates a semi-circle.
- **RotateItems**: If true, rotates the items so their top points outward from the center.

```xaml
<nova:RadialPanel Radius="100" 
                  StartAngle="-90" 
                  SweepAngle="180" 
                  RotateItems="False">
    <!-- Items arranged in a semi-circle starting from the top -->
</nova:RadialPanel>
```

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Radius` | `double` | `100` | The radius of the circle layout. |
| `StartAngle` | `double` | `0` | The starting angle in degrees. |
| `SweepAngle` | `double` | `360` | The total angle range in degrees. |
| `ItemAngle` | `double` | `0` | An additional rotation angle applied to each item. |
| `RotateItems` | `bool` | `True` | Whether to rotate items to face outward from the center. |
