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

Syntax highlighting is available for C#, XAML/XML, JSON, CSS, JavaScript, and Markdown. Other language values display as plain text. Highlighting uses TextMate on desktop and browser targets; Android, iOS, tvOS, and Mac Catalyst fall back to plain text because the native tokenizer is not available on those platforms.

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
using System.Collections.ObjectModel;
using Nova.Avalonia.UI.CodeViewer;

public sealed class SampleViewModel
{
    public ObservableCollection<SourceCodeDocument> Sources { get; } =
    [
        new SourceCodeDocument
        {
            Title = "XAML",
            Language = "XAML",
            Code = "<Button Content=\"Save\" />",
        },
        new SourceCodeDocument
        {
            Title = "C#",
            Language = "C#",
            Code = "var answer = 42;",
        },
    ];
}
```

```xml
<codeViewer:SourceCodeButton DocumentsSource="{Binding Sources}" />
```

When `DocumentsSource` is set, it takes precedence over documents added directly to `Documents`. Set it to `null` to use the direct collection again. Observable collections are supported, including collections that receive their first item after binding.

The drawer closes when the user presses Escape, clicks outside it, or uses its close button. Focus returns to the source button after the closing transition.

## Load source from an application resource

Instead of keeping a large source string in a view model, set `Source` to an `avares:` or `resm:` URI. Inline `Code` takes precedence when both values are present.

```xml
<codeViewer:SourceCodeDocument
    Title="MainView.axaml"
    Language="XAML"
    Source="avares://MyApp/Views/MainView.axaml" />
```

Make sure the file is included as an Avalonia resource by the application project. Resources are loaded asynchronously when their document is first selected. If a resource cannot be loaded, the viewer displays a short error message in place of the source. Set `LoadErrorMessage` on the document when that message needs to be localized.

Handle `SourceCodeDocument.LoadFailed` when the application needs to log or inspect the underlying resource-loading exception.

`CodeViewer` exposes its source as a read-only automation value, and the drawer keeps keyboard focus inside itself while open. Handle the `CopyFailed` event when the application needs to report clipboard errors to the user.

Visible and accessible labels are stored in the `CodeViewerCopyText`, `CodeViewerCopyAutomationName`, `SourceCodeButtonText`, `SourceCodeDrawerTitle`, `SourceCodeDrawerDescription`, and `SourceCodeDrawerCloseText` resources. Override these keys after including the Code Viewer styles to localize the controls.

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
| `DocumentsSource` | `IEnumerable<SourceCodeDocument>` | `null` | Bindable collection of source documents. |
| `DrawerMaxWidth` | `double` | `720` | Maximum width of the source drawer. |

### SourceCodeDocument

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Title` | `string` | `Code` | Label displayed by the document tab. |
| `Language` | `string` | `Text` | Language used for syntax highlighting. |
| `Code` | `string` | `null` | Inline source text, which takes precedence over `Source`. |
| `Source` | `Uri` | `null` | Avalonia or embedded resource containing the source text. |
| `LoadErrorMessage` | `string` | `Unable to load source.` | Localizable message displayed when `Source` cannot be loaded. |
| `ResolvedCode` | `string` | Empty | Read-only inline or loaded source displayed by the viewer. |
| `LoadError` | `string` | `null` | Read-only error message from the latest load attempt. |
