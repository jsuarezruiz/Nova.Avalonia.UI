# Nova.Avalonia.UI.CodeViewer

Nova.Avalonia.UI.CodeViewer displays read-only source code with syntax highlighting, line numbers, copying, and an optional multi-document drawer. It is packaged separately so applications that do not show source code avoid the AvaloniaEdit and TextMate dependencies.

Requires Avalonia 12.1.1 or later in the Avalonia 12 release line.

![Nova Avalonia code viewer](https://raw.githubusercontent.com/jsuarezruiz/Nova.Avalonia.UI/main/images/novaui_codeviewer_light.png)

## Install

```bash
dotnet add package Nova.Avalonia.UI.CodeViewer
```

Register the Code Viewer styles after your Avalonia base theme:

```xml
<Application.Styles>
    <FluentTheme />
    <StyleInclude Source="avares://Nova.Avalonia.UI.CodeViewer/Themes/Controls.axaml" />
</Application.Styles>
```

Declare the namespace and display source inline:

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:codeViewer="clr-namespace:Nova.Avalonia.UI.CodeViewer;assembly=Nova.Avalonia.UI.CodeViewer">
    <codeViewer:CodeViewer Code="{Binding SampleCode}"
                           Language="XAML"
                           MinHeight="220" />
</UserControl>
```

See the [Code Viewer documentation](https://jsuarezruiz.github.io/Nova.Avalonia.UI/docs/controls/codeviewer.html) for multi-document drawers, resources, accessibility, and localization.
