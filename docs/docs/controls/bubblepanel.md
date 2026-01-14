---
title: BubblePanel
description: A layout panel that packs circular items using a circle packing algorithm.
ms.date: 2026-01-02
---

# BubblePanel

The `BubblePanel` arranges child elements using a circle packing algorithm, creating a compact bubble-like arrangement. It's ideal for tag clouds, data visualization, or decorative layouts where items should appear densely packed.

## Basic Usage

```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:nova="using:Nova.Avalonia.UI.Controls">

    <nova:BubblePanel>
        <Ellipse Width="60" Height="60" Fill="#FF6B6B"/>
        <Ellipse Width="40" Height="40" Fill="#4ECDC4"/>
        <Ellipse Width="80" Height="80" Fill="#45B7D1"/>
        <Ellipse Width="50" Height="50" Fill="#FFA07A"/>
        <Ellipse Width="70" Height="70" Fill="#98D8C8"/>
    </nova:BubblePanel>
</UserControl>
```

## How It Works

The panel uses a force-directed circle packing algorithm that:

1. Places items starting from the center
2. Pushes items outward to avoid overlap
3. Packs larger items first for optimal density
4. Respects each item's `DesiredSize` as the bubble diameter

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Padding` | `Thickness` | `0` | Space around the packed bubbles. |
| `Spacing` | `double` | `4` | Minimum spacing between bubbles. |

## Tips

> [!TIP]
> For best results, use circular items (equal width/height) like `Ellipse` or `Border` with `CornerRadius` equal to half the size.

## Example with Data Binding

```xaml
<ItemsControl ItemsSource="{Binding Tags}">
    <ItemsControl.ItemsPanel>
        <ItemsPanelTemplate>
            <nova:BubblePanel/>
        </ItemsPanelTemplate>
    </ItemsControl.ItemsPanel>
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <Border Width="{Binding Size}" Height="{Binding Size}" 
                    Background="{Binding Color}" 
                    CornerRadius="{Binding Size, Converter={StaticResource HalfConverter}}">
                <TextBlock Text="{Binding Name}" 
                           HorizontalAlignment="Center" 
                           VerticalAlignment="Center"/>
            </Border>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```
