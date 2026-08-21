using System.Collections.ObjectModel;
using Nova.Avalonia.UI.CodeViewer;

namespace Nova.Avalonia.UI.Gallery.ViewModels;

public sealed class CodeViewerViewModel : PageViewModel
{
    private const string XamlSample = """
        <ui:SourceCodeButton DocumentsSource="{Binding Sources}" />
        """;

    private const string CSharpSample = """
        Sources =
        [
            new SourceCodeDocument
            {
                Title = "XAML",
                Language = "XAML",
                Code = xaml
            }
        ];
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
