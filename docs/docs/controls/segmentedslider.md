---
title: SegmentedSlider
description: Display and edit a range value across equal or custom weighted segments.
ms.date: 2026-05-07
---

# SegmentedSlider

The `SegmentedSlider` control displays a horizontal range value across equal or custom segments. Use it for stepped workflows, labeled progress, weighted ranges, and compact segmented selection.

`SegmentedSlider` derives from Avalonia `RangeBase`, so it uses the standard `Minimum`, `Maximum`, `Value`, `SmallChange`, `LargeChange`, and `ValueChanged` members.

## Create a segmented slider

Use `SegmentCount` when each segment has the same width.

```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:nova="clr-namespace:Nova.Avalonia.UI.Controls;assembly=Nova.Avalonia.UI">

    <nova:SegmentedSlider Minimum="0"
                          Maximum="100"
                          Value="40"
                          SegmentCount="5" />
</UserControl>
```

## Custom segments

Add `SegmentedSliderSegment` items when segments need titles, proportional widths, or per-segment brushes. `Segments` is the XAML content property, so direct child syntax is supported.

```xaml
<nova:SegmentedSlider Minimum="0"
                      Maximum="100"
                      Value="55">
    <nova:SegmentedSliderSegment Title="Low"
                                 TrackBrush="#E3F2FD"
                                 FillBrush="#1976D2" />
    <nova:SegmentedSliderSegment Title="Medium"
                                 WidthRatio="2"
                                 TrackBrush="#E8F5E9"
                                 FillBrush="#2E7D32" />
    <nova:SegmentedSliderSegment Title="High"
                                 TrackBrush="#FFF3E0"
                                 FillBrush="#EF6C00" />
</nova:SegmentedSlider>
```

`WidthRatio` is proportional. In the example above, the middle segment receives twice as much track width as either side segment.

## Title visibility

Use `TitleVisibility` to control how segment titles are displayed.

| Value | Description |
|-------|-------------|
| `Collapsed` | Hide all segment titles. |
| `AlwaysVisible` | Show all segment titles. |
| `ActiveSegmentOnly` | Show only the active segment title. |
| `ActiveAndPrevious` | Show the active segment title and all previous titles. |

```xaml
<nova:SegmentedSlider Value="65"
                      TitleVisibility="ActiveAndPrevious">
    <nova:SegmentedSliderSegment Title="Draft" />
    <nova:SegmentedSliderSegment Title="Review" />
    <nova:SegmentedSliderSegment Title="Approved" />
</nova:SegmentedSlider>
```

## Snapping and read-only state

Set `IsSnapToSegmentEnabled` to snap drag completion to the nearest segment center. Set `IsReadOnly` to display the current range value without allowing pointer or keyboard edits.

```xaml
<StackPanel Spacing="12">
    <nova:SegmentedSlider Value="34"
                          IsSnapToSegmentEnabled="True" />

    <nova:SegmentedSlider Value="65"
                          IsReadOnly="True"
                          TitleVisibility="Collapsed" />
</StackPanel>
```

## Styling

The default theme follows Avalonia styling conventions:

- `Background` sets the unfilled track brush.
- `Foreground` sets the filled track brush and thumb fill.
- `CornerRadius` sets the track corner radius.
- `FontSize` sets the segment title font size.

```xaml
<nova:SegmentedSlider Value="75"
                      SegmentCount="4"
                      Background="#C8E6C9"
                      Foreground="#2E7D32"
                      CornerRadius="5"
                      FontSize="12" />
```

Use theme resources for template metrics and thumb details.

```xaml
<nova:SegmentedSlider Value="75"
                      SegmentCount="4"
                      Background="#C8E6C9"
                      Foreground="#2E7D32">
    <nova:SegmentedSlider.Resources>
        <x:Double x:Key="SegmentedSliderTrackHeight">10</x:Double>
        <x:Double x:Key="SegmentedSliderThumbSize">22</x:Double>
        <Thickness x:Key="SegmentedSliderTrackMargin">10,4</Thickness>
    </nova:SegmentedSlider.Resources>
</nova:SegmentedSlider>
```

| Resource | Description |
|----------|-------------|
| `SegmentedSliderTrackBrush` | Default unfilled track brush. |
| `SegmentedSliderFillBrush` | Default filled track and thumb brush. |
| `SegmentedSliderTrackHeight` | Height of generated track segments. |
| `SegmentedSliderTrackCornerRadius` | Default track corner radius. |
| `SegmentedSliderTrackMargin` | Margin around the generated track. |
| `SegmentedSliderTitleFontSize` | Default segment title font size. |
| `SegmentedSliderThumbSize` | Thumb width and height. |
| `SegmentedSliderThumbBorderBrush` | Thumb border brush. |
| `SegmentedSliderThumbBorderThickness` | Thumb border thickness. |
| `SegmentedSliderThumbBoxShadow` | Thumb shadow. |
| `SegmentedSliderReadOnlyThumbBrush` | Thumb brush when `IsReadOnly` is true. |
| `SegmentedSliderReadOnlyThumbBorderBrush` | Thumb border brush when `IsReadOnly` is true. |
| `SegmentedSliderReadOnlyThumbBoxShadow` | Thumb shadow when `IsReadOnly` is true. |

## Keyboard and accessibility

The control is focusable and exposes the range value automation pattern.

| Key | Action |
|-----|--------|
| `Right` / `Up` | Increase by `SmallChange`. |
| `Left` / `Down` | Decrease by `SmallChange`. |
| `PageUp` | Increase by `LargeChange`. |
| `PageDown` | Decrease by `LargeChange`. |
| `Home` | Set `Value` to `Minimum`. |
| `End` | Set `Value` to `Maximum`. |

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Minimum` | `double` | `0` | Minimum range value inherited from `RangeBase`. |
| `Maximum` | `double` | `100` | Maximum range value inherited from `RangeBase`. |
| `Value` | `double` | `0` | Current range value inherited from `RangeBase`. |
| `SmallChange` | `double` | inherited | Keyboard increment inherited from `RangeBase`. |
| `LargeChange` | `double` | inherited | Page-key increment inherited from `RangeBase`. |
| `SegmentCount` | `int` | `5` | Number of equal-width segments when `Segments` is empty. |
| `Segments` | `IList<SegmentedSliderSegment>` | empty | Custom segment definitions. |
| `Spacing` | `double` | `4` | Space between segment track pieces. |
| `TitleVisibility` | `SegmentTitleVisibility` | `AlwaysVisible` | Controls segment title visibility. |
| `IsReadOnly` | `bool` | `false` | Prevents user interaction from changing `Value`. |
| `IsSnapToSegmentEnabled` | `bool` | `false` | Snaps drag completion to the nearest segment center. |

## Segment properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Title` | `string` | `null` | Text displayed below the segment. |
| `WidthRatio` | `double` | `1` | Proportional segment width. |
| `FillBrush` | `IBrush` | `null` | Filled brush for this segment. Uses control `Foreground` when unset. |
| `TrackBrush` | `IBrush` | `null` | Unfilled brush for this segment. Uses control `Background` when unset. |

## Events

| Event | Description |
|-------|-------------|
| `ValueChanged` | Inherited from `RangeBase`; raised when `Value` changes. |
| `SegmentChanged` | Raised when `Value` moves into a different active segment. |
