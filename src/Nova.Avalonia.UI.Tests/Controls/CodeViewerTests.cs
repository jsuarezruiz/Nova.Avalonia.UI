using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using AvaloniaEdit;
using AvaloniaEdit.Editing;
using AvaloniaEdit.TextMate;
using Nova.Avalonia.UI.CodeViewer;
using TextMateSharp.Grammars;
using Xunit;
using CodeViewerControl = Nova.Avalonia.UI.CodeViewer.CodeViewer;

namespace Nova.Avalonia.UI.Tests.Controls;

public class CodeViewerTests
{
    [AvaloniaFact]
    public void CodeViewer_Has_Useful_Defaults()
    {
        var viewer = new CodeViewerControl();

        Assert.Null(viewer.Code);
        Assert.Equal("Text", viewer.Language);
        Assert.True(viewer.ShowLineNumbers);
        Assert.True(viewer.ShowCopyButton);
    }

    [AvaloniaFact]
    public void CodeViewer_Uses_Dark_Highlighting_For_A_Derived_Dark_Theme()
    {
        var customDark = new ThemeVariant("CustomDark", ThemeVariant.Dark);

        Assert.Equal(ThemeName.DarkPlus, CodeViewerControl.GetTextMateTheme(customDark));
        Assert.Equal(ThemeName.LightPlus, CodeViewerControl.GetTextMateTheme(ThemeVariant.Light));
    }

    [AvaloniaFact]
    public void CodeViewer_Automation_Peer_Exposes_A_Document()
    {
        var peer = new CodeViewerAutomationPeer(new CodeViewerControl { Language = "XAML" });

        Assert.Equal(AutomationControlType.Document, peer.GetAutomationControlType());
        Assert.Equal("CodeViewer", peer.GetClassName());
        Assert.Equal("XAML source code", peer.GetName());

        var valueProvider = Assert.IsAssignableFrom<IValueProvider>(peer);
        Assert.True(valueProvider.IsReadOnly);
        Assert.Equal(string.Empty, valueProvider.Value);
        Assert.Throws<InvalidOperationException>(() => valueProvider.SetValue("replacement"));
    }

    [AvaloniaFact]
    public void CodeViewer_Uses_Consistent_Typography_For_Code_And_Line_Numbers()
    {
        var editor = new TextEditor { ShowLineNumbers = true };
        var fontFamily = new FontFamily("Menlo");
        const double fontSize = 13;

        CodeViewerControl.SynchronizeLineNumberTypography(editor, fontFamily, fontSize);

        var lineNumberMargin = editor.TextArea.LeftMargins.OfType<LineNumberMargin>().Single();
        Assert.Equal(fontFamily, lineNumberMargin.GetValue(TemplatedControl.FontFamilyProperty));
        Assert.Equal(fontSize, lineNumberMargin.GetValue(TemplatedControl.FontSizeProperty));
    }

    [AvaloniaFact]
    public async Task CodeViewer_Highlights_Multiline_Xaml_After_Initial_Layout()
    {
        var viewer = new CodeViewerControl
        {
            Code = """
                <UserControl xmlns="https://github.com/avaloniaui"
                             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                             xmlns:codeViewer="using:Nova.Avalonia.UI.CodeViewer">
                    <codeViewer:SourceCodeButton />
                </UserControl>
                """,
            Language = "XAML",
        };
        var window = new Window
        {
            Width = 900,
            Height = 360,
            Content = viewer,
        };

        try
        {
            window.Show();
            viewer.ApplyTemplate();
            window.UpdateLayout();

            var editor = viewer.GetVisualDescendants().OfType<TextEditor>().Single();
            await AssertLinesHighlightedAsync(window, editor, 2, 3);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task CodeViewer_Highlights_Multiline_Xaml_After_Language_Changes()
    {
        const string xaml = """
            <UserControl xmlns="https://github.com/avaloniaui"
                         xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                         xmlns:codeViewer="using:Nova.Avalonia.UI.CodeViewer">
                <codeViewer:SourceCodeButton />
            </UserControl>
            """;
        var viewer = new CodeViewerControl
        {
            Code = xaml,
            Language = "XAML",
        };
        var window = new Window
        {
            Width = 900,
            Height = 360,
            Content = viewer,
        };

        try
        {
            window.Show();
            viewer.ApplyTemplate();
            window.UpdateLayout();

            var editor = viewer.GetVisualDescendants().OfType<TextEditor>().Single();
            await AssertLinesHighlightedAsync(window, editor, 2, 3);

            viewer.Code = """
                public sealed class Sample
                {
                    public string Name { get; } = "Nova";
                }
                """;
            viewer.Language = "C#";
            window.UpdateLayout();

            viewer.Code = xaml;
            viewer.Language = "XAML";
            window.UpdateLayout();

            await AssertLinesHighlightedAsync(window, editor, 2, 3);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void SourceCodeDocument_Uses_Inline_Code()
    {
        var document = new SourceCodeDocument
        {
            Title = "Sample",
            Language = "C#",
            Code = "var value = 42;",
        };

        Assert.Equal("Sample", document.Title);
        Assert.Equal("C#", document.Language);
        Assert.Equal("var value = 42;", document.ResolvedCode);
        Assert.Null(document.LoadError);
    }

    [AvaloniaFact]
    public void SourceCodeButton_Uses_Direct_Documents_By_Default()
    {
        var button = new SourceCodeButton();
        var document = new SourceCodeDocument { Code = "<Button />" };

        button.Documents.Add(document);

        Assert.Same(button.Documents, button.Viewer.ItemsSource);
        Assert.Single(button.Documents);
        Assert.Equal(0, button.Viewer.SelectedIndex);
        Assert.Same(button.Viewer, button.Drawer.Content);
    }

    [AvaloniaFact]
    public void SourceCodeButton_Accepts_A_Bindable_Document_Source()
    {
        var source = new[]
        {
            new SourceCodeDocument { Title = "XAML" },
            new SourceCodeDocument { Title = "C#" },
        };
        var button = new SourceCodeButton { DocumentsSource = source };

        Assert.Same(source, button.Viewer.ItemsSource);
    }

    [AvaloniaFact]
    public void SourceCodeViewer_Selects_The_First_Document()
    {
        var viewer = new SourceCodeViewer
        {
            ItemsSource = new[] { new SourceCodeDocument(), new SourceCodeDocument() },
        };

        Assert.Equal(0, viewer.SelectedIndex);
    }

    [AvaloniaFact]
    public void SourceCodeViewer_Selects_First_Document_Added_To_Observable_Source()
    {
        var source = new ObservableCollection<SourceCodeDocument>();
        var viewer = new SourceCodeViewer
        {
            ItemsSource = source,
        };
        var window = new Window { Content = viewer };

        try
        {
            window.Show();
            source.Add(new SourceCodeDocument { Title = "XAML" });

            Assert.Equal(0, viewer.SelectedIndex);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async System.Threading.Tasks.Task SourceCodeDocument_Loads_Resources_Only_When_Selected()
    {
        var document = new SourceCodeDocument
        {
            Source = new Uri("avares://Missing.Assembly/missing.axaml"),
        };

        Assert.Equal(string.Empty, document.ResolvedCode);
        Assert.Null(document.LoadError);

        await document.EnsureCodeResolvedAsync();

        Assert.NotNull(document.LoadError);
        Assert.Equal(document.LoadError, document.ResolvedCode);
    }

    [AvaloniaFact]
    public async Task SourceCodeViewer_Does_Not_Load_A_Selected_Document_Until_Attached()
    {
        var document = new SourceCodeDocument
        {
            Source = new Uri("avares://Missing.Assembly/missing.axaml"),
        };
        var viewer = new SourceCodeViewer
        {
            ItemsSource = new[] { document },
        };

        await Task.Yield();

        Assert.Null(document.LoadError);
        Assert.Equal(string.Empty, document.ResolvedCode);

        var window = new Window { Content = viewer };
        try
        {
            window.Show();
            await document.EnsureCodeResolvedAsync();

            Assert.NotNull(document.LoadError);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task SourceCodeDocument_Reloads_Changed_Source_While_Shared_By_Another_Viewer()
    {
        var document = new SourceCodeDocument
        {
            Source = new Uri("avares://Missing.Assembly/first.axaml"),
            LoadErrorMessage = "The first source could not be loaded.",
        };
        var firstViewer = new SourceCodeViewer
        {
            ItemsSource = new[] { document },
        };
        var secondViewer = new SourceCodeViewer
        {
            ItemsSource = new[] { document },
        };
        var root = new Grid();
        root.Children.Add(firstViewer);
        root.Children.Add(secondViewer);
        var window = new Window { Content = root };

        try
        {
            window.Show();
            await document.EnsureCodeResolvedAsync();
            Assert.Equal("The first source could not be loaded.", document.LoadError);

            root.Children.Remove(firstViewer);
            document.LoadErrorMessage = "The second source could not be loaded.";
            document.Source = new Uri("avares://Missing.Assembly/second.axaml");
            await Task.Yield();

            Assert.Equal(document.LoadErrorMessage, document.LoadError);
            Assert.Equal(document.LoadError, document.ResolvedCode);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task SourceCodeDocument_Refreshes_An_Active_Load_Error_Message()
    {
        var document = new SourceCodeDocument
        {
            Source = new Uri("avares://Missing.Assembly/missing.axaml"),
        };

        await document.EnsureCodeResolvedAsync();
        document.LoadErrorMessage = "The source is unavailable.";

        Assert.Equal("The source is unavailable.", document.LoadError);
        Assert.Equal(document.LoadError, document.ResolvedCode);
    }

    [AvaloniaFact]
    public void SourceCodeButton_Exposes_Drawer_Width_Limit()
    {
        var button = new SourceCodeButton
        {
            DrawerMaxWidth = 640,
        };

        Assert.Equal(640, button.DrawerMaxWidth);
    }

    [AvaloniaFact]
    public void SourceCodeButton_Rejects_Invalid_Drawer_Width_Limit()
    {
        var button = new SourceCodeButton();

        Assert.Throws<ArgumentException>(() => button.DrawerMaxWidth = double.NaN);
        Assert.Throws<ArgumentException>(() => button.DrawerMaxWidth = 0);
    }

    [AvaloniaFact]
    public void SourceCodeDrawer_Rejects_Invalid_Widths()
    {
        var drawer = new SourceCodeDrawer();

        Assert.Throws<ArgumentException>(() => drawer.DrawerWidth = double.NaN);
        Assert.Throws<ArgumentException>(() => drawer.DrawerWidth = double.PositiveInfinity);
        Assert.Throws<ArgumentException>(() => drawer.DrawerWidth = -1);

        drawer.DrawerWidth = 0;
        Assert.Equal(0, drawer.DrawerWidth);
    }

    [AvaloniaFact]
    public void CodeViewer_Reinstalls_Highlighting_After_Reattachment()
    {
        var viewer = new CodeViewerControl
        {
            Code = "<Button />",
            Language = "XAML",
        };
        var host = new ContentControl { Content = viewer };
        var window = new Window { Content = host };

        try
        {
            window.Show();
            viewer.ApplyTemplate();
            window.UpdateLayout();

            var editor = viewer.GetVisualDescendants().OfType<TextEditor>().Single();
            Assert.Single(editor.TextArea.TextView.LineTransformers.OfType<TextMateColoringTransformer>());

            host.Content = null;
            window.UpdateLayout();
            Assert.Empty(editor.TextArea.TextView.LineTransformers.OfType<TextMateColoringTransformer>());

            host.Content = viewer;
            viewer.ApplyTemplate();
            window.UpdateLayout();
            Assert.Single(editor.TextArea.TextView.LineTransformers.OfType<TextMateColoringTransformer>());

            viewer.Language = "Text";
            Assert.Empty(editor.TextArea.TextView.LineTransformers.OfType<TextMateColoringTransformer>());

            viewer.Language = "XAML";
            Assert.Single(editor.TextArea.TextView.LineTransformers.OfType<TextMateColoringTransformer>());
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task SourceCodeDrawer_Cancels_A_Pending_Close_When_Reopened()
    {
        var drawer = new SourceCodeDrawer();
        var closedCount = 0;
        drawer.Closed += (_, _) => closedCount++;

        drawer.Open();
        drawer.Close();
        drawer.Open();
        await Task.Delay(250);

        Assert.Equal(0, closedCount);
    }

    [AvaloniaFact]
    public async Task SourceCodeDrawer_Remains_Modal_Until_The_Close_Transition_Completes()
    {
        var drawer = new SourceCodeDrawer();

        drawer.Open();
        drawer.Close();

        Assert.True(drawer.IsHitTestVisible);

        await Task.Delay(250);

        Assert.False(drawer.IsHitTestVisible);
    }

    [AvaloniaFact]
    public async Task SourceCodeDrawer_Temporarily_Hides_Background_Controls_From_Automation()
    {
        var backgroundButton = new Button { Content = "Background" };
        var addedWhileOpen = new Button { Content = "Added while open" };
        var sourceButton = new TestSourceCodeButton();
        var root = new Grid();
        root.Children.Add(backgroundButton);
        root.Children.Add(sourceButton);
        var window = new Window { Content = root };
        var originalView = AutomationProperties.GetAccessibilityView(backgroundButton);

        try
        {
            window.Show();
            sourceButton.ApplyTemplate();
            window.UpdateLayout();

            sourceButton.InvokeClick();

            Assert.Equal(AccessibilityView.Raw, AutomationProperties.GetAccessibilityView(backgroundButton));

            root.Children.Add(addedWhileOpen);
            window.UpdateLayout();
            await Task.Delay(50);

            Assert.Equal(AccessibilityView.Raw, AutomationProperties.GetAccessibilityView(addedWhileOpen));

            sourceButton.Drawer.Close();
            await Task.Delay(250);

            Assert.Equal(originalView, AutomationProperties.GetAccessibilityView(backgroundButton));
            Assert.Equal(AccessibilityView.Default, AutomationProperties.GetAccessibilityView(addedWhileOpen));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void SourceCodeButton_Removes_Open_Drawer_When_Detached()
    {
        var button = new TestSourceCodeButton();
        var root = new Grid();
        root.Children.Add(button);
        var window = new Window { Content = root };

        try
        {
            window.Show();
            button.ApplyTemplate();
            window.UpdateLayout();
            button.InvokeClick();

            var overlay = OverlayLayer.GetOverlayLayer(button);
            Assert.NotNull(overlay);
            Assert.Contains(button.Drawer, overlay!.Children);

            root.Children.Remove(button);

            Assert.DoesNotContain(button.Drawer, overlay.Children);
        }
        finally
        {
            window.Close();
        }
    }

    private sealed class TestSourceCodeButton : SourceCodeButton
    {
        public void InvokeClick() => base.OnClick();
    }

    private static async Task AssertLinesHighlightedAsync(
        Window window,
        TextEditor editor,
        params int[] lineNumbers)
    {
        var linesHighlighted = false;
        for (var attempt = 0; attempt < 40 && !linesHighlighted; attempt++)
        {
            await Task.Delay(25);
            window.UpdateLayout();
            linesHighlighted = lineNumbers.All(
                lineNumber => editor.TextArea.TextView.GetVisualLine(lineNumber)?.Elements.Count > 1);
        }

        Assert.True(linesHighlighted);
    }
}
