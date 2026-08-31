# Nova.Avalonia.UI.BarcodeGenerator

Nova.Avalonia.UI.BarcodeGenerator adds QR code, linear barcode, and matrix barcode generation to Avalonia applications. It is powered by ZXing.Net and supports custom colors, quiet zones, captions, error correction, and logo overlays.

Requires Avalonia 12.1.1 or later in the Avalonia 12 release line.

![Nova Avalonia barcode generator](https://raw.githubusercontent.com/jsuarezruiz/Nova.Avalonia.UI/main/images/novaui_barcodegenerator_light.gif)

## Install

```bash
dotnet add package Nova.Avalonia.UI.BarcodeGenerator
```

Add the barcode theme dictionary to your application resources:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceInclude Source="avares://Nova.Avalonia.UI.BarcodeGenerator/Themes/BarcodeGenerator.axaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

Declare the namespace and create a barcode:

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:barcode="clr-namespace:Nova.Avalonia.UI.BarcodeGenerator;assembly=Nova.Avalonia.UI.BarcodeGenerator">
    <barcode:BarcodeGenerator Value="https://avaloniaui.net"
                              Symbology="QRCode"
                              Width="200"
                              Height="200" />
</UserControl>
```

See the [barcode documentation](https://jsuarezruiz.github.io/Nova.Avalonia.UI/docs/controls/barcodegenerator.html) for supported formats and customization options.
