---
title: CircularSlider
description: A circular arc slider control for selecting numeric values.
ms.date: 2026-01-04
---

# CircularSlider

The `CircularSlider` control allows users to select a value by dragging a thumb around a circular arc. It supports customizable angles, step frequencies, color themes, and center content for displaying the current value or custom UI.

## Create a circular slider

```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:controls="using:Nova.Avalonia.UI.Controls">

    <controls:CircularSlider Width="200" Height="200"
                              MinValue="0"
                              MaxValue="100"
                              Value="50"/>
</UserControl>
```

## Arc configurations

Control the arc shape using `StartAngle` and `EndAngle` properties:

| Configuration | StartAngle | EndAngle | Description |
|---------------|------------|----------|-------------|
| Default (270°) | -135 | 135 | Gap at bottom |
| Bottom semicircle | -180 | 0 | 180° arc on top |
| Top semicircle | 90 | 270 | 180° arc on bottom |
| Full circle | 0 | 359 | Complete circle |

```xaml
<!-- Bottom semicircle -->
<controls:CircularSlider StartAngle="-180" EndAngle="0" Value="50"/>

<!-- Full circle -->
<controls:CircularSlider StartAngle="0" EndAngle="359" Value="75"/>
```

## Custom styling

Customize colors, thicknesses, and line caps:

```xaml
<controls:CircularSlider Width="180" Height="180"
                          MinValue="0" MaxValue="100"
                          Value="75"
                          ActiveBrush="#FF5722"
                          InactiveBrush="#FFCCBC"
                          ThumbBrush="#D32F2F"
                          ActiveThickness="16"
                          InactiveThickness="14"
                          ThumbSize="24"/>
```

## Center content

Display custom content in the center of the slider:

```xaml
<controls:CircularSlider x:Name="slider"
                          Width="180" Height="180"
                          MinValue="0" MaxValue="100"
                          Value="65">
    <controls:CircularSlider.CenterContent>
        <StackPanel Spacing="2" HorizontalAlignment="Center">
            <TextBlock Text="{Binding #slider.Value, StringFormat='{}{0:F0}%'}"
                       FontSize="28" FontWeight="Bold"/>
            <TextBlock Text="Progress" FontSize="12" Foreground="Gray"/>
        </StackPanel>
    </controls:CircularSlider.CenterContent>
</controls:CircularSlider>
```

## Step frequency

Snap values to defined intervals:

```xaml
<!-- Value snaps to multiples of 5 -->
<controls:CircularSlider MinValue="0" MaxValue="100"
                          StepFrequency="5"/>

<!-- Value snaps to multiples of 0.5 -->
<controls:CircularSlider MinValue="16" MaxValue="30"
                          StepFrequency="0.5"/>
```

## Keyboard navigation

| Key | Action |
|-----|--------|
| `←` / `↓` | Decrease value by step |
| `→` / `↑` | Increase value by step |
| `Home` | Set to minimum value |
| `End` | Set to maximum value |
| `PageUp` | Increase by 10 steps |
| `PageDown` | Decrease by 10 steps |

Scroll wheel also adjusts the value.

## Properties reference

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `MinValue` | `double` | 0 | Minimum slider value |
| `MaxValue` | `double` | 100 | Maximum slider value |
| `Value` | `double` | 0 | Current slider value |
| `StepFrequency` | `double` | 0 | Step interval (0 = continuous) |
| `ValueFormat` | `string` | "F0" | Format string for default center display |
| `StartAngle` | `double` | -135 | Arc start angle in degrees |
| `EndAngle` | `double` | 135 | Arc end angle in degrees |
| `ActiveBrush` | `IBrush` | Blue | Filled arc color |
| `InactiveBrush` | `IBrush` | Gray | Unfilled arc color |
| `ThumbBrush` | `IBrush` | Blue | Thumb color |
| `InnerBackground` | `IBrush` | White | Center circle background |
| `InactiveThickness` | `double` | 12 | Unfilled arc stroke thickness |
| `ActiveThickness` | `double` | 12 | Filled arc stroke thickness |
| `ThumbSize` | `double` | 20 | Thumb diameter |
| `ActiveRadiusDelta` | `double?` | null | Offset for active arc radius |
| `InactiveStrokeLineCap` | `PenLineCap` | Round | Line cap for inactive arc |
| `ActiveStrokeLineCap` | `PenLineCap` | Round | Line cap for active arc |
| `CenterContent` | `object` | null | Custom center content |
| `CenterContentTemplate` | `IDataTemplate` | null | Template for center content |
| `TextBrush` | `IBrush` | Black | Default center text color |
| `TextFontSize` | `double` | 24 | Default center text size |
| `TextFontWeight` | `FontWeight` | Normal | Default center text weight |

## Commands and events

| Member | Type | Description |
|--------|------|-------------|
| `ValueChanged` | `event` | Fired when value changes |
| `DragStarted` | `event` | Fired when dragging begins |
| `DragCompleted` | `event` | Fired when dragging ends |
| `ValueChangedCommand` | `ICommand` | Command on value change |
| `DragStartedCommand` | `ICommand` | Command on drag start |
| `DragCompletedCommand` | `ICommand` | Command on drag complete |

## Pseudo-classes

| Class | Condition |
|-------|-----------|
| `:minimum` | Value equals MinValue |
| `:maximum` | Value equals MaxValue |
| `:disabled` | Control is disabled |
| `:focus-visible` | Control has keyboard focus |
