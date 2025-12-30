---
title: CompareSlider
description: A control that allows side-by-side comparison of two pieces of content with a draggable divider.
ms.date: 2025-12-30
---

# CompareSlider

The `CompareSlider` control enables users to compare two pieces of content (e.g., images, text, or complex layouts) side-by-side. A draggable divider allows the user to reveal more of the "before" or "after" content interactively.

## Create a compare slider

To use the `CompareSlider`, specify the `BeforeContent` and `AfterContent`.

```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:controls="clr-namespace:Nova.Avalonia.UI.Controls;assembly=Nova.Avalonia.UI">

    <controls:CompareSlider Height="300">
        <controls:CompareSlider.BeforeContent>
            <Image Source="before.jpg" Stretch="UniformToFill" />
        </controls:CompareSlider.BeforeContent>
        <controls:CompareSlider.AfterContent>
            <Image Source="after.jpg" Stretch="UniformToFill" />
        </controls:CompareSlider.AfterContent>
    </controls:CompareSlider>
</UserControl>
```

## Orientation

The slider supports both `Horizontal` (default) and `Vertical` orientations.

```xaml
<controls:CompareSlider Orientation="Vertical" Height="400">
    <controls:CompareSlider.BeforeContent>
        <TextBlock Text="Top Content" FontSize="24" HorizontalAlignment="Center" VerticalAlignment="Center"/>
    </controls:CompareSlider.BeforeContent>
    <controls:CompareSlider.AfterContent>
        <TextBlock Text="Bottom Content" FontSize="24" HorizontalAlignment="Center" VerticalAlignment="Center"/>
    </controls:CompareSlider.AfterContent>
</controls:CompareSlider>
```

## Interaction

### Move to Point
By default, clicking anywhere on the slider track will move the thumb to that position. This can be disabled using `IsMoveToPointEnabled`.

```xaml
<controls:CompareSlider IsMoveToPointEnabled="False" ... />
```

### Direction Reversal
You can reverse the direction of the value increase using `IsDirectionReversed`.

```xaml
<controls:CompareSlider IsDirectionReversed="True" ... />
```

### Programmatic Control
You can control the slider programmatically using the `AnimateTo` and `Reset` methods.

```csharp
// Animate to 75%
await MySlider.AnimateTo(0.75, TimeSpan.FromMilliseconds(500));

// Reset to center (0.5)
MySlider.Reset(animate: true);
```

## Customizing Templates

You can use `BeforeContentTemplate` and `AfterContentTemplate` to define how the data should be displayed if you are binding to non-visual objects.

## Styling

The control consists of a divider (`Line`) and a thumb (`Thumb`). You can customize these using standard styles.

```xaml
<controls:CompareSlider.Styles>
    <Style Selector="controls|CompareSlider /template/ Line#PART_Divider">
        <Setter Property="Stroke" Value="Orange" />
        <Setter Property="StrokeThickness" Value="4" />
    </Style>
    <Style Selector="controls|CompareSlider /template/ Thumb#PART_Thumb">
        <Setter Property="Background" Value="Orange" />
        <Setter Property="Width" Value="30" />
        <Setter Property="Height" Value="60" />
        <Setter Property="CornerRadius" Value="4" />
    </Style>
</controls:CompareSlider.Styles>
```

### Pseudo Classes
- `:dragging`: Applied when the user is actively dragging the thumb.

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Value` | `double` | `0.5` | The current position of the divider (0.0 to 1.0). |
| `Orientation` | `Orientation` | `Horizontal` | The orientation of the slider. |
| `BeforeContent` | `object` | `null` | Content displayed before/above the divider. |
| `AfterContent` | `object` | `null` | Content displayed after/below the divider. |
| `IsMoveToPointEnabled` | `bool` | `true` | Whether clicking the track moves the thumb. |
| `IsDirectionReversed` | `bool` | `false` | Reverses the direction of value increase. |
| `SmallChange` | `double` | `0.01` | Value change for arrow keys. |
| `LargeChange` | `double` | `0.1` | Value change for Page keys. |
| `IsDragging` | `bool` | `false` | (Read-only) Whether the thumb is being dragged. |

## Events

| Event | Description |
|-------|-------------|
| `DragStarted` | Occurs when the user starts dragging the thumb. |
| `DragDelta` | Occurs as the user moves the thumb. |
| `DragCompleted` | Occurs when the user stops dragging the thumb. |
| `ValueChanged` | Inherited from `RangeBase`, occurs when `Value` changes. |
