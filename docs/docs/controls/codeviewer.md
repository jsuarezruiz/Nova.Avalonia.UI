---
title: CodeViewer
description: Show read-only source code inline or open several source files in a drawer.
ms.date: 2026-08-21
---

# CodeViewer

Code Viewer makes it easy to show the source behind a sample without turning the rest of the application into an editor. It can display one read-only document inline, or open a set of documents from a small source button.

The controls live in the optional `Nova.Avalonia.UI.CodeViewer` package. This keeps AvaloniaEdit and the TextMate grammars out of applications that do not need to display source code.

## Install the package

Add the Code Viewer package to your application:

```bash
dotnet add package Nova.Avalonia.UI.CodeViewer
```

Register its styles after your base Avalonia theme:

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="MyApp.App">
    <Application.Styles>
        <FluentTheme />
        <StyleInclude Source="avares://Nova.Avalonia.UI.CodeViewer/Themes/Controls.axaml" />
    </Application.Styles>
</Application>
```

## Show code inline

Use `CodeViewer` when the source should remain visible on the page. The viewer is read-only and includes line numbers and a copy button by default.

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:codeViewer="clr-namespace:Nova.Avalonia.UI.CodeViewer;assembly=Nova.Avalonia.UI.CodeViewer">

    <codeViewer:CodeViewer Code="{Binding SampleCode}"
                           Language="XAML"
                           MinHeight="220" />
</UserControl>
```

Set `ShowLineNumbers` or `ShowCopyButton` to `False` when those parts are not useful. `TextWrapping` controls whether long lines wrap or scroll horizontally.

Syntax highlighting is available for C#, XAML/XML, JSON, CSS, JavaScript, and Markdown. Other language values still display as plain text.

## Open several source files

`SourceCodeButton` opens a window-level drawer containing one tab for each document. Documents can be declared directly in XAML:

```xml
<codeViewer:SourceCodeButton DrawerMaxWidth="720">
    <codeViewer:SourceCodeButton.Documents>
        <codeViewer:SourceCodeDocument Title="XAML"
                                       Language="XAML"
                                       Code="{Binding XamlSource}" />
        <codeViewer:SourceCodeDocument Title="C#"
                                       Language="C#"
                                       Code="{Binding CSharpSource}" />
    </codeViewer:SourceCodeButton.Documents>
</codeViewer:SourceCodeButton>
```

For dynamic content, bind `DocumentsSource` to a collection of `SourceCodeDocument` objects:

```csharp
Sources =
[
    new SourceCodeDocument
    {
        Title = "XAML",
        Language = "XAML",
        Code = xamlSource,
    },
    new SourceCodeDocument
    {
        Title = "C#",
        Language = "C#",
        Code = csharpSource,
    },
];
```

```xml
<codeViewer:SourceCodeButton DocumentsSource="{Binding Sources}" />
```

The drawer closes when the user presses Escape, clicks outside it, or uses its close button. Focus returns to the source button after the closing transition.

## Load source from an application resource

Instead of keeping a large source string in a view model, set `Source` to an `avares:` or `resm:` URI. Inline `Code` takes precedence when both values are present.

```xml
<codeViewer:SourceCodeDocument
    Title="MainView.axaml"
    Language="XAML"
    Source="avares://MyApp/Views/MainView.axaml" />
```

Make sure the file is included as an Avalonia resource by the application project. If the resource cannot be loaded, the viewer displays a short error message in place of the source.

## Main properties

### CodeViewer

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Code` | `string` | `null` | Source text displayed by the viewer. |
| `Language` | `string` | `Text` | Language used for syntax highlighting. |
| `ShowLineNumbers` | `bool` | `true` | Shows or hides line numbers. |
| `ShowCopyButton` | `bool` | `true` | Shows or hides the copy button. |
| `TextWrapping` | `TextWrapping` | `NoWrap` | Controls wrapping for long lines. |

### SourceCodeButton

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Documents` | `AvaloniaList<SourceCodeDocument>` | Empty | Documents declared directly in XAML or code. |
| `DocumentsSource` | `IEnumerable` | `null` | Bindable collection of source documents. |
| `DrawerMaxWidth` | `double` | `720` | Maximum width of the source drawer. |
