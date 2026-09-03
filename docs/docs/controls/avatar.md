---
title: Avatar
description: Display profile imagery, initials, or icons with the Avatar control.
ms.date: 2025-12-01
---

# Avatar

The `Avatar` control presents a person's identity using an image, initials, icon, or custom content. It includes automatic background generation, size presets, and optional presence status indicators.

## Create an avatar

Declare an `Avatar` and set `DisplayName`. With the default `DisplayMode` of `Auto`, the control will render initials when no image or icon is provided.

```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:nova="clr-namespace:Nova.Avalonia.UI.Controls;assembly=Nova.Avalonia.UI">

    <nova:Avatar DisplayName="Alex Martin" />
</UserControl>
```

## Use images, icons, or custom content

Choose a display mode explicitly when you need to control the visual output:

- `DisplayMode="Image"` uses the `ImageSource` bitmap.
- `DisplayMode="Icon"` shows the provided `Icon` content.
- `DisplayMode="Content"` renders any custom `Content`.

```xaml
<StackPanel Spacing="12">
    <nova:Avatar DisplayName="Jamie Fox" ImageSource="avares://Assets/jamie.png" DisplayMode="Image" />
    <nova:Avatar DisplayName="Operations" DisplayMode="Icon">
        <nova:Avatar.Icon>
            <PathIcon Data="M18,13 L6,13 6,11 18,11z" />
        </nova:Avatar.Icon>
    </nova:Avatar>
    <nova:Avatar DisplayName="Admin" DisplayMode="Content">
        <nova:Avatar.Content>
            <Ellipse Fill="#F59E0B" Width="18" Height="18" />
        </nova:Avatar.Content>
    </nova:Avatar>
</StackPanel>
```

> [!NOTE]
> If `DisplayMode` is left as `Auto`, the control picks an image when available, otherwise initials, then icon, then content.

## Size, shape, and color

`Avatar` supports preset sizes via `Size` (`ExtraSmall` through `ExtraLarge`) and a `Custom` option controlled by `CustomSize`. Use `Shape` to switch between `Circle`, `Square`, and `Rectangle` corners.

The control can auto-generate a background color from the display name when `AutoGenerateBackground` is `True`, or you can set `BackgroundColor` and `ForegroundColor` directly.

```xaml
<UniformGrid Columns="3" Rows="1" Margin="0,12,0,0">
    <nova:Avatar DisplayName="Kim Lee" Size="Small" />
    <nova:Avatar DisplayName="Drew Parker" Size="Large" Shape="Square" BackgroundColor="#111827" />
    <nova:Avatar DisplayName="Avery Patel" Size="Custom" CustomSize="80" Shape="Rectangle" AutoGenerateBackground="False" BackgroundColor="#2563EB" />
</UniformGrid>
```

## Show presence status

Attach a status indicator with the `Status` property. You can override the default color per status with `StatusColor`.

```xaml
<StackPanel Orientation="Horizontal" Spacing="10">
    <nova:Avatar DisplayName="Taylor Reed" Status="Online" />
    <nova:Avatar DisplayName="Morgan" Status="Away" />
    <nova:Avatar DisplayName="Jordan" Status="Busy" StatusColor="#C026D3" />
</StackPanel>
```

Tooltips automatically display the `DisplayName` when `ShowTooltip` is `True`, which helps identify users when only initials or icons are visible.

## Arrange multiple avatars with AvatarGroup

Use `AvatarGroup` to arrange multiple `Avatar` controls with configurable overlap and overflow handling. Set `Overlap` and `MaxDisplayed` to control the layout; when more avatars are supplied, the group can display the remaining count.

```xaml
<StackPanel Spacing="12">
    <nova:AvatarGroup MaxDisplayed="3" Overlap="0.3">
        <nova:AvatarGroup.Avatars>
            <nova:Avatar DisplayName="Taylor Reed" Status="Online" />
            <nova:Avatar DisplayName="Morgan Lee" Status="Away" />
            <nova:Avatar DisplayName="Jamie Fox" Status="Busy" />
            <nova:Avatar DisplayName="Avery Patel" />
        </nova:AvatarGroup.Avatars>
    </nova:AvatarGroup>

    <nova:AvatarGroup Orientation="Vertical" Overlap="0.15">
        <nova:AvatarGroup.Avatars>
            <nova:Avatar DisplayName="Ops" DisplayMode="Icon">
                <nova:Avatar.Icon>
                    <PathIcon Data="M18,13 L6,13 6,11 18,11z" />
                </nova:Avatar.Icon>
            </nova:Avatar>
            <nova:Avatar DisplayName="Engineering" Status="Online" />
        </nova:AvatarGroup.Avatars>
    </nova:AvatarGroup>
</StackPanel>
```

`AvatarGroup` also exposes `BorderBrush` and `BorderThickness` to add a ring around each avatar when you need a stronger visual boundary against busy backgrounds.
