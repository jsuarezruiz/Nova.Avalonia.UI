using System.Collections.ObjectModel;
using Nova.Avalonia.UI.CodeViewer;

namespace Nova.Avalonia.UI.Gallery.ViewModels;

public sealed class CodeViewerViewModel : PageViewModel
{
    private const string XamlSample = """
        <UserControl xmlns="https://github.com/avaloniaui"
                     xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                     xmlns:codeViewer="using:Nova.Avalonia.UI.CodeViewer">
            <codeViewer:SourceCodeButton DocumentsSource="{Binding Sources}" />
        </UserControl>
        """;

    private const string CSharpSample = """
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
            ];
        }
        """;

    public CodeViewerViewModel() : base("Code Viewer")
    {
        Sources =
        [
            new SourceCodeDocument
            {
                Title = "XAML",
                Language = "XAML",
                Code = XamlSample,
            },
            new SourceCodeDocument
            {
                Title = "C#",
                Language = "C#",
                Code = CSharpSample,
            },
        ];
    }

    public string Description =>
        "Display selectable source with line numbers, or open several source files from a compact button.";

    public string InlineSample => XamlSample;

    public ObservableCollection<SourceCodeDocument> Sources { get; }
}
