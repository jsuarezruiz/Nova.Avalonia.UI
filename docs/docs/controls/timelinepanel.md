---
title: TimelinePanel
description: A layout panel for timeline-style step/process flows.
ms.date: 2026-01-02
---

# TimelinePanel

The `TimelinePanel` arranges child elements in a timeline or step-by-step flow, with connecting lines between items. It's perfect for wizards, process flows, order tracking, or historical timelines.

## Basic Usage

```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:nova="using:Nova.Avalonia.UI.Controls">

    <nova:TimelinePanel Orientation="Vertical" Spacing="20">
        <Border Background="#3498DB" CornerRadius="20" Width="40" Height="40">
            <TextBlock Text="1" Foreground="White" HorizontalAlignment="Center" VerticalAlignment="Center"/>
        </Border>
        <Border Background="#2ECC71" CornerRadius="20" Width="40" Height="40">
            <TextBlock Text="2" Foreground="White" HorizontalAlignment="Center" VerticalAlignment="Center"/>
        </Border>
        <Border Background="#E74C3C" CornerRadius="20" Width="40" Height="40">
            <TextBlock Text="3" Foreground="White" HorizontalAlignment="Center" VerticalAlignment="Center"/>
        </Border>
    </nova:TimelinePanel>
</UserControl>
```

## Orientation

The panel supports both vertical and horizontal layouts:

- **Vertical**: Steps flow from top to bottom (default)
- **Horizontal**: Steps flow from left to right

```xaml
<nova:TimelinePanel Orientation="Horizontal" Spacing="30">
    <!-- Steps arranged horizontally -->
</nova:TimelinePanel>
```

## Connector Lines

The panel automatically draws connector lines between items. Customize their appearance with:

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ConnectorBrush` | `IBrush` | `Gray` | Color of the connecting lines. |
| `ConnectorThickness` | `double` | `2` | Thickness of the connecting lines. |

```xaml
<nova:TimelinePanel ConnectorBrush="#3498DB" ConnectorThickness="3">
    <!-- Steps with blue connectors -->
</nova:TimelinePanel>
```

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Orientation` | `Orientation` | `Vertical` | Layout direction (Vertical or Horizontal). |
| `Spacing` | `double` | `10` | Space between timeline items. |
| `ConnectorBrush` | `IBrush` | `Gray` | Brush for connector lines. |
| `ConnectorThickness` | `double` | `2` | Thickness of connector lines. |

## Example: Order Tracking

```xaml
<nova:TimelinePanel Orientation="Vertical" Spacing="15" ConnectorBrush="#27AE60">
    <StackPanel Orientation="Horizontal" Spacing="10">
        <Ellipse Width="20" Height="20" Fill="#27AE60"/>
        <TextBlock Text="Order Placed" VerticalAlignment="Center"/>
    </StackPanel>
    <StackPanel Orientation="Horizontal" Spacing="10">
        <Ellipse Width="20" Height="20" Fill="#27AE60"/>
        <TextBlock Text="Processing" VerticalAlignment="Center"/>
    </StackPanel>
    <StackPanel Orientation="Horizontal" Spacing="10">
        <Ellipse Width="20" Height="20" Fill="#BDC3C7"/>
        <TextBlock Text="Shipped" VerticalAlignment="Center" Opacity="0.5"/>
    </StackPanel>
    <StackPanel Orientation="Horizontal" Spacing="10">
        <Ellipse Width="20" Height="20" Fill="#BDC3C7"/>
        <TextBlock Text="Delivered" VerticalAlignment="Center" Opacity="0.5"/>
    </StackPanel>
</nova:TimelinePanel>
```
