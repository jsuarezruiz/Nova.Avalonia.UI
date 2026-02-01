---
title: Watermark
description: A control that renders tiled text or image watermarks as an overlay.
ms.date: 2026-02-01
---

# Watermark

The `Watermark` control renders repeating text or image patterns as a tiled overlay. Useful for marking documents as confidential, draft, or adding branding.

## Basic Usage

```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:nova="using:Nova.Avalonia.UI.Controls">

    <nova:Watermark Text="CONFIDENTIAL">
        <Border Background="White" Padding="20">
            <TextBlock Text="Document content here" />
        </Border>
    </nova:Watermark>
</UserControl>
```

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Text` | `string` | `null` | Watermark text to display. |
| `Source` | `IImage` | `null` | Image source for image watermarks. |
| `Angle` | `double` | `-30` | Rotation angle in degrees. |
| `HorizontalSpacing` | `double` | `50` | Horizontal gap between tiles. |
| `VerticalSpacing` | `double` | `30` | Vertical gap between tiles. |
| `WatermarkOpacity` | `double` | `0.15` | Watermark transparency (0.0-1.0). |
| `WatermarkFontSize` | `double` | `14` | Font size for text watermarks. |
| `WatermarkFontFamily` | `FontFamily` | `Default` | Font family for text watermarks. |
| `WatermarkFontWeight` | `FontWeight` | `Normal` | Font weight for text watermarks (e.g. Bold). |
| `WatermarkFontStyle` | `FontStyle` | `Normal` | Font style for text watermarks (e.g. Italic). |
| `WatermarkForeground` | `IBrush` | `Gray` | Text color brush. |
| `WatermarkFlowDirection` | `FlowDirection` | `LeftToRight` | Text direction for RTL languages. Supporting complex script shaping (CTL). |

## Custom Angle and Spacing

```xaml
<nova:Watermark Text="DRAFT" 
                Angle="-45" 
                HorizontalSpacing="100" 
                VerticalSpacing="50"
                WatermarkOpacity="0.2"
                WatermarkFontSize="18">
    <Border Background="White" Padding="20">
        <TextBlock Text="Draft document content" />
    </Border>
</nova:Watermark>
```

## RTL Text Direction

```xaml
<nova:Watermark Text="سري للغاية" 
                WatermarkFlowDirection="RightToLeft"
                Angle="-30"
                WatermarkOpacity="0.2">
    <Border Background="White" Padding="20">
        <TextBlock Text="Arabic RTL watermark" />
    </Border>
</nova:Watermark>
```

## Horizontal Watermark

```xaml
<nova:Watermark Text="SAMPLE" Angle="0" WatermarkOpacity="0.3">
    <Image Source="document.png" />
</nova:Watermark>
```

## Custom Foreground Color

```xaml
<nova:Watermark Text="PROTECTED" 
                WatermarkForeground="Red"
                WatermarkOpacity="0.2">
    <Border Background="White" Padding="20">
        <TextBlock Text="Protected content" />
    </Border>
</nova:Watermark>
```

## Accessibility

The `Watermark` control implements `WatermarkAutomationPeer`, allowing screen readers to perceive the watermark content. This is essential for documents where "Confidential" or "Draft" status is critical information.

## Notes

- The control uses `ContentControl` with a `ContentPresenter` template.
- Watermark is rendered behind the content using `Render` override.
- `ClipToBounds` is enabled by default to prevent overflow.
- Pure Avalonia rendering works on all backends.

