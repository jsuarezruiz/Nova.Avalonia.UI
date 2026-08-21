using System;
using System.Text;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using AvaloniaEdit.Editing;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;

namespace Nova.Avalonia.UI.CodeViewer;

/// <summary>
/// Displays read-only source text with optional line numbers and copy support.
/// </summary>
public class CodeViewer : TemplatedControl
{
    /// <summary>
    /// Defines the <see cref="Code"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> CodeProperty =
        AvaloniaProperty.Register<CodeViewer, string?>(nameof(Code));

    /// <summary>
    /// Defines the <see cref="Language"/> property.
    /// </summary>
    public static readonly StyledProperty<string> LanguageProperty =
        AvaloniaProperty.Register<CodeViewer, string>(nameof(Language), "Text");

    /// <summary>
    /// Defines the <see cref="ShowLineNumbers"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowLineNumbersProperty =
        AvaloniaProperty.Register<CodeViewer, bool>(nameof(ShowLineNumbers), true);

    /// <summary>
    /// Defines the <see cref="ShowCopyButton"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowCopyButtonProperty =
        AvaloniaProperty.Register<CodeViewer, bool>(nameof(ShowCopyButton), true);

    /// <summary>
    /// Defines the <see cref="TextWrapping"/> property.
    /// </summary>
    public static readonly StyledProperty<TextWrapping> TextWrappingProperty =
        AvaloniaProperty.Register<CodeViewer, TextWrapping>(nameof(TextWrapping), TextWrapping.NoWrap);

    /// <summary>
    /// Defines the <see cref="LineNumbers"/> property.
    /// </summary>
    public static readonly DirectProperty<CodeViewer, string> LineNumbersProperty =
        AvaloniaProperty.RegisterDirect<CodeViewer, string>(
            nameof(LineNumbers),
            viewer => viewer.LineNumbers);

    private Button? _copyButton;
    private TextEditor? _editor;
    private RegistryOptions? _registry;
    private TextMate.Installation? _textMate;
    private string _lineNumbers = "1";

    public CodeViewer()
    {
        ActualThemeVariantChanged += (_, _) => ApplyEditorTheme();
    }

    /// <summary>
    /// Gets or sets the source text to display.
    /// </summary>
    public string? Code
    {
        get => GetValue(CodeProperty);
        set => SetValue(CodeProperty, value);
    }

    /// <summary>
    /// Gets or sets the language used for syntax highlighting.
    /// </summary>
    public string Language
    {
        get => GetValue(LanguageProperty);
        set => SetValue(LanguageProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether line numbers are displayed.
    /// </summary>
    public bool ShowLineNumbers
    {
        get => GetValue(ShowLineNumbersProperty);
        set => SetValue(ShowLineNumbersProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the copy button is displayed.
    /// </summary>
    public bool ShowCopyButton
    {
        get => GetValue(ShowCopyButtonProperty);
        set => SetValue(ShowCopyButtonProperty, value);
    }

    /// <summary>
    /// Gets or sets the wrapping behavior for the source text.
    /// </summary>
    public TextWrapping TextWrapping
    {
        get => GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }

    /// <summary>
    /// Gets the formatted line numbers for the current source text.
    /// </summary>
    public string LineNumbers
    {
        get => _lineNumbers;
        private set => SetAndRaise(LineNumbersProperty, ref _lineNumbers, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_copyButton is not null)
        {
            _copyButton.Click -= OnCopyClicked;
        }

        base.OnApplyTemplate(e);

        _copyButton = e.NameScope.Find<Button>("PART_CopyButton");
        DisposeHighlighting();
        _editor = e.NameScope.Find<TextEditor>("PART_Editor");
        if (_copyButton is not null)
        {
            _copyButton.Click += OnCopyClicked;
        }

        ConfigureEditor();
        ResetScrollOffset();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == CodeProperty)
        {
            LineNumbers = CreateLineNumbers(Code);
            if (_editor is not null)
            {
                _editor.Text = Code ?? string.Empty;
            }

            ResetScrollOffset();
        }
        else if (change.Property == LanguageProperty)
        {
            ApplyGrammar();
        }
        else if (change.Property == ShowLineNumbersProperty && _editor is not null)
        {
            _editor.ShowLineNumbers = ShowLineNumbers;
            SynchronizeLineNumberTypography(_editor, FontFamily, FontSize);
        }
        else if (change.Property == TextWrappingProperty && _editor is not null)
        {
            _editor.WordWrap = TextWrapping != TextWrapping.NoWrap;
        }
        else if ((change.Property == TemplatedControl.FontFamilyProperty ||
                  change.Property == TemplatedControl.FontSizeProperty) &&
                 _editor is not null)
        {
            SynchronizeLineNumberTypography(_editor, FontFamily, FontSize);
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        InstallHighlighting();
        ResetScrollOffset();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        DisposeHighlighting();
        base.OnDetachedFromVisualTree(e);
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new CodeViewerAutomationPeer(this);

    private async void OnCopyClicked(object? sender, RoutedEventArgs e)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is not null)
        {
            await clipboard.SetTextAsync(Code ?? string.Empty);
        }
    }

    private void ResetScrollOffset()
    {
        Dispatcher.UIThread.Post(
            () =>
            {
                if (_editor is not null)
                {
                    _editor.CaretOffset = 0;
                    _editor.ScrollToHome();
                }
            },
            DispatcherPriority.Background);
    }

    private void ConfigureEditor()
    {
        if (_editor is null)
        {
            return;
        }

        _editor.Text = Code ?? string.Empty;
        _editor.ShowLineNumbers = ShowLineNumbers;
        _editor.WordWrap = TextWrapping != TextWrapping.NoWrap;
        _editor.Options.AllowScrollBelowDocument = false;
        _editor.Options.EnableHyperlinks = false;
        _editor.Options.EnableEmailHyperlinks = false;
        SynchronizeLineNumberTypography(_editor, FontFamily, FontSize);
        InstallHighlighting();
    }

    internal static void SynchronizeLineNumberTypography(
        TextEditor editor,
        FontFamily fontFamily,
        double fontSize)
    {
        foreach (var margin in editor.TextArea.LeftMargins)
        {
            if (margin is not LineNumberMargin lineNumberMargin)
            {
                continue;
            }

            lineNumberMargin.SetValue(TemplatedControl.FontFamilyProperty, fontFamily);
            lineNumberMargin.SetValue(TemplatedControl.FontSizeProperty, fontSize);
            break;
        }
    }

    private void InstallHighlighting()
    {
        if (_editor is null || _textMate is not null || !this.IsAttachedToVisualTree())
        {
            return;
        }

        _registry = new RegistryOptions(
            ActualThemeVariant == global::Avalonia.Styling.ThemeVariant.Dark
                ? ThemeName.DarkPlus
                : ThemeName.LightPlus);
        _textMate = _editor.InstallTextMate(_registry);
        ApplyGrammar();
    }

    private void ApplyGrammar()
    {
        if (_registry is null || _textMate is null)
        {
            return;
        }

        var extension = Language?.Trim().ToUpperInvariant() switch
        {
            "C#" or "CS" or "CSHARP" => ".cs",
            "XAML" or "XML" => ".xml",
            "JSON" => ".json",
            "CSS" => ".css",
            "JAVASCRIPT" or "JS" => ".js",
            "MARKDOWN" or "MD" => ".md",
            _ => null,
        };
        if (extension is null)
        {
            return;
        }

        var language = _registry.GetLanguageByExtension(extension);
        if (language is not null)
        {
            _textMate.SetGrammar(_registry.GetScopeByLanguageId(language.Id));
        }
    }

    private void ApplyEditorTheme()
    {
        if (_registry is null || _textMate is null)
        {
            return;
        }

        var theme = ActualThemeVariant == global::Avalonia.Styling.ThemeVariant.Dark
            ? ThemeName.DarkPlus
            : ThemeName.LightPlus;
        _textMate.SetTheme(_registry.LoadTheme(theme));
    }

    private void DisposeHighlighting()
    {
        _textMate?.Dispose();
        _textMate = null;
        _registry = null;
    }

    private static string CreateLineNumbers(string? code)
    {
        var lineCount = 1;
        for (var index = 0; index < code?.Length; index++)
        {
            if (code[index] == '\n' ||
                code[index] == '\r' && (index + 1 == code.Length || code[index + 1] != '\n'))
            {
                lineCount++;
            }
        }

        var result = new StringBuilder(lineCount * 3);
        for (var line = 1; line <= lineCount; line++)
        {
            if (line > 1)
            {
                result.Append(Environment.NewLine);
            }

            result.Append(line);
        }

        return result.ToString();
    }
}
