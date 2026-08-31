# Nova.Avalonia.UI

Nova.Avalonia.UI provides accessible, themeable controls and layout panels for Avalonia applications. The package includes identity and status controls, interactive inputs, loading effects, comparison and game controls, and responsive or virtualized layouts.

Requires Avalonia 12.1.1 or later in the Avalonia 12 release line.

![Nova.Avalonia.UI](https://raw.githubusercontent.com/jsuarezruiz/Nova.Avalonia.UI/main/images/banner.png)

## Install

```bash
dotnet add package Nova.Avalonia.UI
```

Register the control themes after your Avalonia base theme:

```xml
<Application.Styles>
    <FluentTheme />
    <StyleInclude Source="avares://Nova.Avalonia.UI/Themes/Controls.axaml" />
</Application.Styles>
```

Then declare the control namespace in a view:

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:nova="clr-namespace:Nova.Avalonia.UI.Controls;assembly=Nova.Avalonia.UI">
    <nova:Avatar DisplayName="Avery Patel" Status="Online" />
</UserControl>
```

The barcode generator and source-code viewer are separate packages, so applications only take those dependencies when they need them.

See the [documentation](https://jsuarezruiz.github.io/Nova.Avalonia.UI/) and [source repository](https://github.com/jsuarezruiz/Nova.Avalonia.UI) for the complete control catalog and examples.
