---
title: StaggeredPanel
description: A layout panel that positions items in a staggered grid, creating a masonry-like effect.
ms.date: 2026-01-01
---

# StaggeredPanel

The `StaggeredPanel` arranges items into columns based on their height, filling the shortest column first. This creates a staggered "masonry" layout that is efficient for displaying items with varying heights, such as cards or images.

## Basic Usage

The `StaggeredPanel` is typically used as the `ItemsPanel` of an `ItemsControl` or `ListBox`. Set the `DesiredColumnWidth` to control how many columns are created based on the available space.

```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:nova="using:Nova.Avalonia.UI.Controls">

    <ScrollViewer>
        <ItemsControl ItemsSource="{Binding Items}">
            <ItemsControl.ItemsPanel>
                <ItemsPanelTemplate>
                    <nova:StaggeredPanel DesiredColumnWidth="200"
                                         ColumnSpacing="10" 
                                         RowSpacing="10" />
                </ItemsPanelTemplate>
            </ItemsControl.ItemsPanel>
            
            <ItemsControl.ItemTemplate>
                <DataTemplate>
                    <Border Height="{Binding Height}" Background="{Binding Brush}" CornerRadius="4">
                        <TextBlock Text="{Binding Title}" Margin="10" />
                    </Border>
                </DataTemplate>
            </ItemsControl.ItemTemplate>
        </ItemsControl>
    </ScrollViewer>
</UserControl>
```

## Configure Columns

The control automatically determines the number of columns by dividing the available width by the `DesiredColumnWidth` (plus spacing). It ensures at least one column is always present.

- **DesiredColumnWidth**: The target width for each column. The actual width may be slightly larger to fill the available space.
- **ColumnSpacing**: The horizontal space between columns.
- **RowSpacing**: The vertical space between items in the same column.

```xaml
<nova:StaggeredPanel DesiredColumnWidth="150"
                     ColumnSpacing="20"
                     RowSpacing="20"
                     Padding="10">
    <!-- Items... -->
</nova:StaggeredPanel>
```

## Layout Logic

Items are added sequentially to the column with the **minimum total height**. This prevents large gaps and maintains a balanced visual weight across the panel.

> [!NOTE]
> If the available width is infinite (e.g., inside a horizontal `StackPanel` or `ScrollViewer`), the panel will default to using the `DesiredColumnWidth` for every item, potentially creating a single horizontal row or one column per item depending on exact constraints. It is best used within a container that provides a constrained width (like a vertical `ScrollViewer`).

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `DesiredColumnWidth` | `double` | `250` | The desired width for each column. |
| `ColumnSpacing` | `double` | `0` | The distance between columns. |
| `RowSpacing` | `double` | `0` | The distance between items in the same column. |
| `Padding` | `Thickness` | `0` | The padding inside the panel border. |

## Examples

### Dynamic Resizing

The `StaggeredPanel` responds to window resizing by recalculating the number of columns.

```xaml
<nova:StaggeredPanel DesiredColumnWidth="300" HorizontalAlignment="Stretch">
    <!-- As the window widens, more columns will appear. -->
</nova:StaggeredPanel>
```

### Direct Children

You can also use `StaggeredPanel` directly with children defined in XAML:

```xaml
<nova:StaggeredPanel DesiredColumnWidth="120" ColumnSpacing="5" RowSpacing="5">
    <Border Height="50" Background="Red" />
    <Border Height="100" Background="Green" />
    <Border Height="75" Background="Blue" />
    <Border Height="120" Background="Orange" />
</nova:StaggeredPanel>
```

## Virtualized Version

For large datasets (100+ items), use `VirtualizedStaggeredLayout` with `ItemsRepeater` for efficient scrolling:

```xaml
<ScrollViewer>
    <ItemsControl ItemsSource="{Binding LargeCollection}">
        <ItemsControl.ItemsPanel>
            <ItemsPanelTemplate>
                <nova:VirtualizingStaggeredPanel 
                    DesiredColumnWidth="200"
                    ColumnSpacing="10" 
                    RowSpacing="10" />
            </ItemsPanelTemplate>
        </ItemsControl.ItemsPanel>
        <ItemsControl.ItemTemplate>
            <DataTemplate>
                <Border Height="{Binding Height}" Background="{Binding Brush}" CornerRadius="4">
                    <TextBlock Text="{Binding Title}" Margin="10" />
                </Border>
            </DataTemplate>
        </ItemsControl.ItemTemplate>
    </ItemsControl>
</ScrollViewer>
```

### VirtualizingStaggeredPanel Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `DesiredColumnWidth` | `double` | `250` | The desired width for each column. |
| `ColumnSpacing` | `double` | `0` | The distance between columns. |
| `RowSpacing` | `double` | `0` | The distance between items in the same column. |

### Performance Features

The `VirtualizingStaggeredPanel` is optimized for large datasets:

- **Container Recycling**: Off-screen items are hidden and reused, not destroyed
- **Minimal Allocations**: Reusable collections avoid per-frame allocations
- **2x Viewport Buffer**: Items above and below the viewport are pre-realized for smooth scrolling
- **Pool Size Limit**: Maximum 20 recycled containers per type to prevent memory bloat

> [!TIP]
> For best performance with 500+ items, ensure your `ItemTemplate` is lightweight and avoid expensive bindings or effects on each item.
