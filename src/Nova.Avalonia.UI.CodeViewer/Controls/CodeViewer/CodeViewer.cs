using System;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Input.Platform;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;

namespace Nova.Avalonia.UI.CodeViewer;

/// <summary>
/// Displays read-only source text with optional line numbers and copy support.
/// </summary>
public class CodeViewer : TemplatedControl
{
    private static readonly Lazy<RegistryOptions> SharedRegistry =
        new(() => new RegistryOptions(ThemeName.LightPlus));

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

    private Button? _copyButton;
    private TextEditor? _editor;
    private TextView? _highlightingViewAwaitingLayout;
    private RegistryOptions? _registry;
    private TextMate.Installation? _textMate;

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeViewer"/> class.
    /// </summary>
    public CodeViewer()
    {
        ActualThemeVariantChanged += (_, _) => ApplyEditorTheme();
    }

    /// <summary>
    /// Occurs when the source text cannot be copied to the platform clipboard.
    /// </summary>
    public event EventHandler<SourceCodeCopyFailedEventArgs>? CopyFailed;

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
            if (_editor is not null)
            {
                _editor.Text = Code ?? string.Empty;
            }

            if (ControlAutomationPeer.FromElement(this) is CodeViewerAutomationPeer peer)
            {
                peer.NotifyCodeChanged(
                    change.GetOldValue<string?>(),
                    change.GetNewValue<string?>());
            }

            ResetScrollOffset();
        }
        else if (change.Property == LanguageProperty)
        {
            DisposeHighlighting();
            UpdateHighlighting();
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
        UpdateHighlighting();
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
        if (clipboard is null)
        {
            CopyFailed?.Invoke(
                this,
                new SourceCodeCopyFailedEventArgs(
                    new InvalidOperationException("The platform clipboard is unavailable.")));
            return;
        }

        try
        {
            await clipboard.SetTextAsync(Code ?? string.Empty);
        }
        catch (Exception exception)
        {
            CopyFailed?.Invoke(this, new SourceCodeCopyFailedEventArgs(exception));
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
        UpdateHighlighting();
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

    private void UpdateHighlighting()
    {
        var extension = GetLanguageExtension(Language);
        if (extension is null || !IsTextMateSupported())
        {
            DisposeHighlighting();
            return;
        }

        if (_editor is null || !this.IsAttachedToVisualTree())
        {
            return;
        }

        if (_textMate is null)
        {
            _registry = SharedRegistry.Value;
            _textMate = _editor.InstallTextMate(_registry);
            ApplyEditorTheme();
        }

        var language = _registry?.GetLanguageByExtension(extension);
        var scope = language is null ? null : _registry?.GetScopeByLanguageId(language.Id);
        if (!string.IsNullOrEmpty(scope))
        {
            RefreshHighlightingAfterFirstLayout();
            _textMate.SetGrammar(scope);
        }
    }

    private void RefreshHighlightingAfterFirstLayout()
    {
        StopWaitingForHighlightingLayout();

        if (_editor is null)
        {
            return;
        }

        _highlightingViewAwaitingLayout = _editor.TextArea.TextView;
        // TextMate may tokenize before AvaloniaEdit has a visible range to redraw.
        // Rebuild the visual lines once layout is ready so those early tokens are applied.
        _highlightingViewAwaitingLayout.VisualLinesChanged += OnHighlightingVisualLinesChanged;
    }

    private void OnHighlightingVisualLinesChanged(object? sender, EventArgs e)
    {
        if (sender is not TextView textView || !textView.VisualLinesValid)
        {
            return;
        }

        StopWaitingForHighlightingLayout();
        Dispatcher.UIThread.Post(
            () =>
            {
                if (_textMate is not null && ReferenceEquals(_editor?.TextArea.TextView, textView))
                {
                    textView.Redraw();
                }
            },
            DispatcherPriority.Render);
    }

    private void StopWaitingForHighlightingLayout()
    {
        if (_highlightingViewAwaitingLayout is null)
        {
            return;
        }

        _highlightingViewAwaitingLayout.VisualLinesChanged -= OnHighlightingVisualLinesChanged;
        _highlightingViewAwaitingLayout = null;
    }

    private void ApplyEditorTheme()
    {
        if (_registry is null || _textMate is null)
        {
            return;
        }

        var theme = GetTextMateTheme(ActualThemeVariant);
        _textMate.SetTheme(_registry.LoadTheme(theme));
    }

    internal static ThemeName GetTextMateTheme(ThemeVariant? themeVariant)
    {
        for (var current = themeVariant; current is not null; current = current.InheritVariant)
        {
            if (current == ThemeVariant.Dark)
            {
                return ThemeName.DarkPlus;
            }

            if (current == ThemeVariant.Light)
            {
                return ThemeName.LightPlus;
            }
        }

        return ThemeName.LightPlus;
    }

    private void DisposeHighlighting()
    {
        StopWaitingForHighlightingLayout();
        _textMate?.Dispose();
        _textMate = null;
        _registry = null;

        if (_editor is null)
        {
            return;
        }

        var transformers = _editor.TextArea.TextView.LineTransformers;
        for (var index = transformers.Count - 1; index >= 0; index--)
        {
            if (transformers[index] is TextMateColoringTransformer transformer)
            {
                transformer.Dispose();
                transformers.RemoveAt(index);
            }
        }
    }

    private static string? GetLanguageExtension(string? language) => language?.Trim().ToUpperInvariant() switch
    {
        "C#" or "CS" or "CSHARP" => ".cs",
        "XAML" or "XML" => ".xml",
        "JSON" => ".json",
        "CSS" => ".css",
        "JAVASCRIPT" or "JS" => ".js",
        "MARKDOWN" or "MD" => ".md",
        _ => null,
    };

    private static bool IsTextMateSupported() =>
        !OperatingSystem.IsAndroid() &&
        !OperatingSystem.IsIOS() &&
        !OperatingSystem.IsTvOS() &&
        !OperatingSystem.IsMacCatalyst();
}
