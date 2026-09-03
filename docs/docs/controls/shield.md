---
title: Shield
description: Display build status, versions, licenses, and other compact metadata with the Shield control.
ms.date: 2026-04-16
---

# Shield

The `Shield` control displays a subject and status with distinct background colors for each part. It is commonly used for build status, package versions, licenses, and other compact metadata.

<p>
  <img src="https://raw.githubusercontent.com/jsuarezruiz/Nova.Avalonia.UI/main/images/novaui_shield_light.png" alt="Shield control in light theme" width="300" />
  <img src="https://raw.githubusercontent.com/jsuarezruiz/Nova.Avalonia.UI/main/images/novaui_shield_dark.png" alt="Shield control in dark theme" width="300" />
</p>

## Usage

```xaml
<controls:Shield Subject="Build" Status="passing" Background="Green" />
```

## Properties

| Property | Type | Description |
| -- | -- | -- |
| Subject | `object` | The content displayed on the left side (subject). |
| Status | `object` | The content displayed on the right side (status). |
| Background | `IBrush` | The background color of the status part. |
| SubjectBackground | `IBrush` | The background color of the subject part. |
| IsReadOnly | `bool` | When true, the shield is display-only with no interaction but full opacity. |
| CornerRadius | `CornerRadius` | The corner radius of the control. Defaults to the theme's default. |

## Examples

### Basic Usage

```xaml
<StackPanel Spacing="10" Orientation="Horizontal">
    <controls:Shield Subject="Build" Status="passing" Background="Green" />
    <controls:Shield Subject="Version" Status="1.0.0" Background="Blue" />
    <controls:Shield Subject="License" Status="MIT" Background="Orange" />
</StackPanel>
```

### Colors

You can customize the color of the status part using the inherited `Background` property.

```xaml
<controls:Shield Subject="Red" Status="Failed" Background="Red" />
<controls:Shield Subject="Blue" Status="Info" Background="DodgerBlue" />
```

### Custom Content

Since `Subject` and `Status` are of type `object`, you can put any content inside them.

```xaml
<controls:Shield Subject="Users" Background="Teal">
    <controls:Shield.Status>
        <StackPanel Orientation="Horizontal" Spacing="4">
             <PathIcon Data="{StaticResource UserIcon}" />
             <TextBlock Text="1.2k" />
        </StackPanel>
    </controls:Shield.Status>
</controls:Shield>
```

### Read-Only Mode

Use `IsReadOnly` for display-only badges that maintain full opacity without interaction.

```xaml
<controls:Shield Subject="build" Status="passing" Background="Green" IsReadOnly="True" />
```
