using System.Linq;
using Avalonia.Automation.Peers;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Editing;
using Nova.Avalonia.UI.CodeViewer;
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
        Assert.Equal("1", viewer.LineNumbers);
    }

    [AvaloniaFact]
    public void CodeViewer_Updates_Line_Numbers_For_Common_Line_Endings()
    {
        var viewer = new CodeViewerControl
        {
            Code = "one\r\ntwo\nthree\rfour",
        };

        Assert.Equal($"1{System.Environment.NewLine}2{System.Environment.NewLine}3{System.Environment.NewLine}4", viewer.LineNumbers);
    }

    [AvaloniaFact]
    public void CodeViewer_Automation_Peer_Exposes_A_Document()
    {
        var peer = new CodeViewerAutomationPeer(new CodeViewerControl { Language = "XAML" });

        Assert.Equal(AutomationControlType.Document, peer.GetAutomationControlType());
        Assert.Equal("CodeViewer", peer.GetClassName());
        Assert.Equal("XAML source code", peer.GetName());
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
    public void SourceCodeButton_Exposes_Drawer_Width_Limit()
    {
        var button = new SourceCodeButton
        {
            DrawerMaxWidth = 640,
        };

        Assert.Equal(640, button.DrawerMaxWidth);
    }
}
