---
title: OverlapPanel
description: A panel that stacks children with configurable X/Y offsets.
ms.date: 2026-01-02
---

# OverlapPanel

The `OverlapPanel` stacks child elements with configurable horizontal and vertical offsets, creating an overlapping card or pile effect. It's ideal for card stacks, notification badges, or layered UI elements.

## Basic Usage

```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:nova="using:Nova.Avalonia.UI.Controls">

    <nova:OverlapPanel OffsetX="20" OffsetY="15">
        <Border Width="100" Height="70" Background="#E74C3C" CornerRadius="8"/>
        <Border Width="100" Height="70" Background="#3498DB" CornerRadius="8"/>
        <Border Width="100" Height="70" Background="#2ECC71" CornerRadius="8"/>
        <Border Width="100" Height="70" Background="#F39C12" CornerRadius="8"/>
    </nova:OverlapPanel>
</UserControl>
```

## Z-Order

By default, later items appear on top of earlier items. Use `ReverseZIndex` to flip this order.

```xaml
<!-- First item on top -->
<nova:OverlapPanel OffsetX="20" OffsetY="10" ReverseZIndex="True">
    <Border Background="Red" Width="80" Height="60"/>
    <Border Background="Green" Width="80" Height="60"/>
    <Border Background="Blue" Width="80" Height="60"/>
</nova:OverlapPanel>
```

## Negative Offsets

Use negative offsets to stack items in the opposite direction:

```xaml
<!-- Stack from bottom-right to top-left -->
<nova:OverlapPanel OffsetX="-15" OffsetY="-10">
    <Border Background="Purple" Width="80" Height="60"/>
    <Border Background="Teal" Width="80" Height="60"/>
    <Border Background="Orange" Width="80" Height="60"/>
</nova:OverlapPanel>
```

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `OffsetX` | `double` | `10` | Horizontal offset between items (can be negative). |
| `OffsetY` | `double` | `10` | Vertical offset between items (can be negative). |
| `ReverseZIndex` | `bool` | `false` | When true, earlier items appear on top. |

## Use Cases

- **Card stacks**: Playing cards, photo albums
- **Notification stacks**: Multiple alerts piled together
- **Breadcrumb trails**: Overlapping path indicators
- **Avatar groups**: Use with circular items for grouped avatars
