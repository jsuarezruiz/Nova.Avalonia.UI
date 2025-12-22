---
title: Badge
description: Display notifications, counts, or status indicators with the Badge control.
ms.date: 2025-12-16
---

# Badge

The `Badge` control displays notifications, status indicators, or short information (like counts) attached to another element or as a standalone indicator. It supports different placements, themes (colors), shapes, and overflow handling for large numbers.

## Create a badge

Wrap any content (like a `Button` or `Icon`) with the `Badge` control. Set the `BadgeContent` to define what is displayed inside the badge.

```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:controls="using:Nova.Avalonia.UI.Controls">

    <controls:Badge BadgeContent="5">
        <Button Content="Notifications" />
    </controls:Badge>
</UserControl>
```

## Placements

Control where the badge appears relative to its content using the `BadgePlacement` property. Supported values are:
- `TopLeft`, `Top`, `TopRight` (Default)
- `Left`, `Right`
- `BottomLeft`, `Bottom`, `BottomRight`

```xaml
<StackPanel Spacing="20" Orientation="Horizontal">
    <!-- Default is TopRight -->
    <controls:Badge BadgeContent="1">
        <Border Width="40" Height="40" Background="LightGray" CornerRadius="4"/>
    </controls:Badge>

    <controls:Badge BadgeContent="2" BadgePlacement="BottomRight">
        <Border Width="40" Height="40" Background="LightGray" CornerRadius="4"/>
    </controls:Badge>
    
    <controls:Badge BadgeContent="3" BadgePlacement="TopLeft">
        <Border Width="40" Height="40" Background="LightGray" CornerRadius="4"/>
    </controls:Badge>
</StackPanel>
```

## Themes and Colors

The control includes several built-in themes for common semantic colors. You can apply them using the `Theme` property with a `StaticResource`.

**Solid Themes:**
- `PrimaryBadge`, `SecondaryBadge`
- `SuccessBadge`, `WarningBadge`, `DangerBadge`, `InfoBadge`
- `LightBadge`, `DarkBadge`

**Outline Themes:**
- `PrimaryOutlineBadge`, `SuccessOutlineBadge`, etc.

```xaml
<WrapPanel>
    <controls:Badge BadgeContent="Success" Theme="{StaticResource SuccessBadge}" Margin="5" />
    <controls:Badge BadgeContent="Warning" Theme="{StaticResource WarningBadge}" Margin="5" />
    <controls:Badge BadgeContent="Danger" Theme="{StaticResource DangerBadge}" Margin="5" />
    <controls:Badge BadgeContent="Outline" Theme="{StaticResource PrimaryOutlineBadge}" Margin="5" />
</WrapPanel>
```

## Shapes and Sizes

You can customize the shape and size using themes or properties.
- **Shapes**: `SquareBadge`, `PillBadge`
- **Sizes**: `SmallBadge`, `LargeBadge`

```xaml
<StackPanel Spacing="10" Orientation="Horizontal">
    <controls:Badge BadgeContent="Pill" Theme="{StaticResource PillBadge}" />
    <controls:Badge BadgeContent="Square" Theme="{StaticResource SquareBadge}" />
    <controls:Badge BadgeContent="Small" Theme="{StaticResource SmallBadge}" />
</StackPanel>
```

## Dot Badges

If you only need a simple indicator without text, use `Kind="Dot"`. You can combined this with size themes like `SmallDotBadge` or `LargeDotBadge`.

```xaml
<controls:Badge Kind="Dot" Theme="{StaticResource SuccessBadge}">
    <Button Content="Status" />
</controls:Badge>
```

## Overflow Handling (MaxCount)

When displaying numbers, you can limit the maximum value shown using `MaxCount`. If the `BadgeContent` exceeds this limit, it will display the limit followed by a `+` (e.g., "99+").

- The default `MaxCount` is 99. If the content is numeric and exceeds this value, it will be truncated with a `+` suffix logic.

```xaml
<!-- Displays "99+" if content is > 99 -->
<controls:Badge BadgeContent="150" Theme="{StaticResource DangerBadge}">
    <Button Content="Inbox" />
</controls:Badge>

<!-- Custom limit Example (Conceptual) -->
<controls:Badge BadgeContent="10" MaxCount="9" Theme="{StaticResource WarningBadge}">
    <Button Content="Messages" />
</controls:Badge>
```

## Standalone Usage

The `Badge` control can also be used without wrapping content, acting as a standalone tag or label.

```xaml
<StackPanel Orientation="Horizontal" Spacing="5">
    <TextBlock Text="Status:" VerticalAlignment="Center"/>
    <controls:Badge BadgeContent="New" Theme="{StaticResource InfoBadge}" />
</StackPanel>
```

## Properties reference

| Property | Type | Description |
|----------|------|-------------|
| `BadgeContent` | `object` | The content to display inside the badge (text, number, etc.). |
| `BadgePlacement` | `BadgePlacement` | The position of the badge relative to the content. Default is `TopRight`. |
| `Kind` | `BadgeKind` | The visual style of the badge content. Values: `Content` (default), `Dot`. |
| `MaxCount` | `int` | The maximum numeric value to display before showing a `+` suffix. Default is 99. |
| `IsBadgeVisible` | `bool` | Controls the visibility of the badge itself. Default is `True`. |
| `BadgeOffset` | `double` | Additional X/Y offset to fine-tune the badge position. |
