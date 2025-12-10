---
title: RatingControl
description: Allow users to view and set ratings with customizable shapes and precision levels.
ms.date: 2025-12-07
---

# RatingControl

The `RatingControl` allows users to view and set ratings using interactive items such as stars, hearts, or custom shapes. It supports multiple precision levels, customizable appearance, and full keyboard and pointer interaction.

## Create a rating control

Declare a `RatingControl` and set the `Value` property. By default, the control displays 5 star-shaped items.

```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:nova="clr-namespace:Nova.Avalonia.UI.Controls;assembly=Nova.Avalonia.UI">

    <nova:RatingControl Value="3" />
</UserControl>
```

## Precision modes

Control how values are selected using the `Precision` property:

- `Full` - Only whole numbers (1, 2, 3, etc.)
- `Half` - Half-step increments (1.5, 2.5, etc.)
- `Exact` - Continuous values based on pointer position

```xaml
<StackPanel Spacing="12">
    <nova:RatingControl Value="3" Precision="Full" />
    <nova:RatingControl Value="3.5" Precision="Half" />
    <nova:RatingControl Value="4.2" Precision="Exact" />
</StackPanel>
```

> [!NOTE]
> Keyboard navigation respects precision: arrow keys increment by 1.0 for `Full`, 0.5 for `Half`, and 0.1 for `Exact`.

## Shapes

Choose from built-in shapes or provide a custom geometry:

- `Star` (default)
- `Heart`
- `Circle`
- `Diamond`
- `Custom` - Uses the `CustomGeometry` property

```xaml
<StackPanel Spacing="12">
    <nova:RatingControl Shape="Star" Value="3" />
    <nova:RatingControl Shape="Heart" Value="4" RatedFill="#E91E63" />
    <nova:RatingControl Shape="Circle" Value="2" />
    <nova:RatingControl Shape="Diamond" Value="5" />
    <nova:RatingControl Shape="Custom"
                        CustomGeometry="M 12,2 L 2,22 L 22,22 Z"
                        Value="3" />
</StackPanel>
```

## Colors and styling

Customize the appearance using fill and stroke properties for both rated and unrated states. A separate set of preview colors can highlight items during hover.

```xaml
<nova:RatingControl Value="4"
                    ItemSize="40"
                    ItemSpacing="10"
                    RatedFill="#00BCD4"
                    UnratedFill="#B2EBF2"
                    PreviewFill="#4DD0E1"
                    StrokeThickness="1"
                    RatedStroke="#0097A7" />
```

| Property | Description |
|----------|-------------|
| `RatedFill` | Fill brush for rated (active) items |
| `UnratedFill` | Fill brush for unrated (inactive) items |
| `RatedStroke` | Stroke brush for rated items |
| `UnratedStroke` | Stroke brush for unrated items |
| `PreviewFill` | Fill brush shown when hovering |
| `PreviewStroke` | Stroke brush shown when hovering |
| `StrokeThickness` | Thickness of the item stroke |

## Size and layout

Control the size and spacing of rating items, and choose between horizontal or vertical orientation.

```xaml
<StackPanel Spacing="12">
    <nova:RatingControl Value="3" ItemSize="24" ItemSpacing="4" />
    <nova:RatingControl Value="3" ItemSize="48" ItemSpacing="12" />
    <nova:RatingControl Value="3" Orientation="Vertical" />
</StackPanel>
```

## Item count

Set the number of rating items with `ItemCount`. The default is 5, but any positive number is supported.

```xaml
<StackPanel Spacing="12">
    <nova:RatingControl Value="2" ItemCount="3" />
    <nova:RatingControl Value="7" ItemCount="10" ItemSize="20" />
</StackPanel>
```

## Read-only mode

Use `IsReadOnly` to display a rating without allowing user interaction. This is useful for showing existing ratings.

```xaml
<nova:RatingControl Value="3.7" IsReadOnly="True" Precision="Exact" />
```

## Handle value changes

Subscribe to the `ValueChanged` event to respond when the user changes the rating.

```xaml
<nova:RatingControl x:Name="MyRating"
                    Value="0"
                    ValueChanged="OnRatingValueChanged" />
```

```csharp
private void OnRatingValueChanged(object? sender, RoutedEventArgs e)
{
    if (sender is RatingControl rating)
    {
        Debug.WriteLine($"New rating: {rating.Value}");
    }
}
```

## Keyboard support

The control is fully accessible via keyboard:

| Key | Action |
|-----|--------|
| `Right` / `Up` | Increase value by step |
| `Left` / `Down` | Decrease value by step |
| `Home` | Set value to 0 |
| `End` | Set value to maximum |

The step size depends on the `Precision` setting (1.0, 0.5, or 0.1).

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Value` | `double` | `0` | Current rating value |
| `ItemCount` | `int` | `5` | Number of rating items |
| `Precision` | `RatingPrecision` | `Full` | Selection precision (Full, Half, Exact) |
| `IsReadOnly` | `bool` | `false` | Whether the control is read-only |
| `Shape` | `RatingShape` | `Star` | Shape of rating items |
| `CustomGeometry` | `Geometry` | `null` | Custom geometry when Shape is Custom |
| `ItemSize` | `double` | `32` | Size of each rating item |
| `ItemSpacing` | `double` | `6` | Spacing between items |
| `Orientation` | `Orientation` | `Horizontal` | Layout orientation |
| `RatedFill` | `IBrush` | `Gold` | Fill for rated items |
| `UnratedFill` | `IBrush` | `LightGray` | Fill for unrated items |
| `RatedStroke` | `IBrush` | `null` | Stroke for rated items |
| `UnratedStroke` | `IBrush` | `null` | Stroke for unrated items |
| `PreviewFill` | `IBrush` | `Orange` | Fill during hover preview |
| `PreviewStroke` | `IBrush` | `null` | Stroke during hover preview |
| `StrokeThickness` | `double` | `0` | Stroke thickness |

## Events

| Event | Description |
|-------|-------------|
| `ValueChanged` | Raised when the `Value` property changes |
