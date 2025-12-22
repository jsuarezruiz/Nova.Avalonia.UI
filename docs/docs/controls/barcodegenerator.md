---
title: BarcodeGenerator
description: Generate and display QR codes, 1D barcodes, and 2D matrix codes with customizable styling.
ms.date: 2025-12-18
---

# BarcodeGenerator

The `BarcodeGenerator` control generates and renders various barcode symbologies including QR codes, Data Matrix, Code 128, and more. It uses the ZXing library for encoding and supports customizable colors, error correction, and logo overlays.

## Create a barcode

Declare a `BarcodeGenerator` and set the `Value` and `Symbology` properties.

```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:bc="clr-namespace:Nova.Avalonia.UI.BarcodeGenerator;assembly=Nova.Avalonia.UI.BarcodeGenerator">

    <bc:BarcodeGenerator Value="https://avaloniaui.net"
                         Symbology="QRCode"
                         Width="200" Height="200" />
</UserControl>
```

## Supported symbologies

The control supports multiple barcode formats via the `Symbology` property:

| Symbology | Type | Use Case |
|-----------|------|----------|
| `QRCode` | 2D | URLs, contact info, general data |
| `DataMatrix` | 2D | Small items, electronics |
| `Aztec` | 2D | Transport tickets, boarding passes |
| `PDF417` | 2D | ID cards, shipping labels |
| `Code128` | 1D | Shipping, logistics |
| `Code39` | 1D | Automotive, defense |
| `Code93` | 1D | Postal services |
| `EAN13` | 1D | Retail products (Europe) |
| `EAN8` | 1D | Small retail products |
| `UPCA` | 1D | Retail products (North America) |
| `UPCE` | 1D | Small packages |
| `Codabar` | 1D | Libraries, blood banks |
| `ITF` | 1D | Logistics, shipping |

```xaml
<StackPanel Spacing="12">
    <bc:BarcodeGenerator Value="PRODUCT123" Symbology="Code128" Width="250" Height="80" />
    <bc:BarcodeGenerator Value="5901234123457" Symbology="EAN13" Width="200" Height="70" />
    <bc:BarcodeGenerator Value="DATA-MATRIX" Symbology="DataMatrix" Width="150" Height="150" />
</StackPanel>
```

## Custom colors

Customize the barcode appearance using `BarBrush` and `BackgroundBrush`.

```xaml
<bc:BarcodeGenerator Value="Styled QR"
                     Symbology="QRCode"
                     BarBrush="#1565C0"
                     BackgroundBrush="#E3F2FD"
                     Width="200" Height="200" />
```

> [!TIP]
> Use `DynamicResource` bindings for theme-aware colors that adapt to light and dark modes.

## Display text

Show the encoded value below the barcode with `ShowText`. Customize the text appearance with related properties.

```xaml
<bc:BarcodeGenerator Value="AVALONIA2025"
                     Symbology="Code128"
                     ShowText="True"
                     TextFontSize="14"
                     TextAlignment="Center"
                     TextMargin="0,8,0,0"
                     Width="300" Height="100" />
```

## Error correction (QR and Aztec)

For QR codes and Aztec codes, set the `ErrorCorrectionLevel` to control damage recovery:

| Level | Recovery | Description |
|-------|----------|-------------|
| `L` | ~7% | Low - smallest code size |
| `M` | ~15% | Medium (default) |
| `Q` | ~25% | Quartile |
| `H` | ~30% | High - best for logos |

```xaml
<bc:BarcodeGenerator Value="https://avaloniaui.net"
                     Symbology="QRCode"
                     ErrorCorrectionLevel="H"
                     Width="200" Height="200" />
```

## QR code with logo

Overlay a logo in the center of QR or Aztec codes. Use `ErrorCorrectionLevel="H"` for best results.

```xaml
<bc:BarcodeGenerator Value="https://avaloniaui.net"
                     Symbology="QRCode"
                     ErrorCorrectionLevel="H"
                     LogoSizePercent="0.25"
                     Width="200" Height="200">
    <bc:BarcodeGenerator.Logo>
        <Bitmap>avares://MyApp/Assets/logo.png</Bitmap>
    </bc:BarcodeGenerator.Logo>
</bc:BarcodeGenerator>
```

The `LogoSizePercent` property controls the logo size relative to the barcode (0.1 to 0.4).

## Quiet zone

The `QuietZone` property sets the margin around the barcode in modules. The default is 2.

```xaml
<bc:BarcodeGenerator Value="QR" Symbology="QRCode" QuietZone="4" />
```

## Handle events

Subscribe to `BarcodeGenerated` and `BarcodeError` events for generation feedback.

```csharp
barcode.BarcodeGenerated += (s, e) => 
{
    Console.WriteLine($"Generated {e.Matrix.Width}x{e.Matrix.Height} matrix");
};

barcode.BarcodeError += (s, e) => 
{
    Console.WriteLine($"Error: {e.Exception.Message}");
};
```

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Value` | `string` | `""` | Data to encode |
| `Symbology` | `BarcodeSymbology` | `QRCode` | Barcode format |
| `BarBrush` | `IBrush` | `Black` | Fill for barcode bars |
| `BackgroundBrush` | `IBrush` | `White` | Background fill |
| `QuietZone` | `int` | `2` | Margin in modules |
| `ShowText` | `bool` | `false` | Show encoded value as text |
| `TextFontSize` | `double` | `14` | Text font size |
| `TextForeground` | `IBrush` | `Black` | Text color |
| `TextMargin` | `Thickness` | `0,8,0,0` | Text margin |
| `TextAlignment` | `TextAlignment` | `Center` | Text alignment |
| `ErrorCorrectionLevel` | `QRErrorCorrectionLevel` | `M` | Error recovery level |
| `Logo` | `IImage` | `null` | Center logo image |
| `LogoSizePercent` | `double` | `0.25` | Logo size (0.1-0.4) |
| `ErrorMessage` | `string` | `null` | Last error message |

## Events

| Event | Description |
|-------|-------------|
| `BarcodeGenerated` | Raised when barcode is successfully generated |
| `BarcodeError` | Raised when generation fails |
