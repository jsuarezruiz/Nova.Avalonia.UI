---
title: HexPanel
description: A layout panel that arranges items in a honeycomb hexagonal grid.
ms.date: 2026-01-01
---

# HexPanel

The `HexPanel` arranges items in a hexagonal grid pattern (honeycomb). It supports both "Flat Top" and "Pointy Top" hex orientations and uses row/column coordinates for item placement.

## Basic Usage

To position items, wrap them in the `HexPanel` and use the `HexPanel.Row` and `HexPanel.Column` attached properties on the item container.

```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:nova="using:Nova.Avalonia.UI.Controls">

    <nova:HexPanel ColumnCount="3" RowCount="3" Orientation="Horizontal">
        <!-- Item at Row 0, Column 0 -->
        <Ellipse Width="80" Height="80" Fill="Red" 
                 nova:HexPanel.Row="0" nova:HexPanel.Column="0" />
        
        <!-- Item at Row 1, Column 1 (Center) -->
        <Ellipse Width="80" Height="80" Fill="Blue" 
                 nova:HexPanel.Row="1" nova:HexPanel.Column="1" />
    </nova:HexPanel>
</UserControl>
```

> [!IMPORTANT]
> When using `HexPanel` as the `ItemsPanel` of an `ItemsControl`, you must apply the attached properties to the item container (e.g., `ContentPresenter`) using a Style, as the `DataTemplate` content is wrapped inside the container.

```xaml
<ItemsControl ItemsSource="{Binding Items}">
    <ItemsControl.ItemsPanel>
        <ItemsPanelTemplate>
            <nova:HexPanel ColumnCount="3" RowCount="3" />
        </ItemsPanelTemplate>
    </ItemsControl.ItemsPanel>
    <ItemsControl.Styles>
        <Style Selector="ContentPresenter" x:DataType="local:HexItem">
            <Setter Property="nova:HexPanel.Row" Value="{Binding Row}" />
            <Setter Property="nova:HexPanel.Column" Value="{Binding Column}" />
        </Style>
    </ItemsControl.Styles>
    <!-- ... ItemTemplate ... -->
</ItemsControl>
```

## Orientation

The `Orientation` property controls the shape and stacking direction of the hexagons.

- **Horizontal**: Creates a grid of "Pointy Top" hexagons.
- **Vertical**: Creates a grid of "Flat Top" hexagons.

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Orientation` | `Orientation` | `Vertical` | The orientation of the hex grid (`Horizontal` or `Vertical`). |
| `RowCount` | `int` | `0` | The number of rows in the grid (affects size calculation). |
| `ColumnCount` | `int` | `0` | The number of columns in the grid. |

## Attached Properties

| Property | Type | Description |
|----------|------|-------------|
| `Row` | `int` | The zero-based row index for the item. |
| `Column` | `int` | The zero-based column index for the item. |
