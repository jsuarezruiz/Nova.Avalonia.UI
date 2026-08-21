using System;
using System.IO;
using Avalonia;
using Avalonia.Platform;

namespace Nova.Avalonia.UI.CodeViewer;

/// <summary>
/// Describes one source document displayed by a <see cref="SourceCodeViewer"/>.
/// </summary>
public sealed class SourceCodeDocument : AvaloniaObject
{
    /// <summary>
    /// Defines the <see cref="Title"/> property.
    /// </summary>
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<SourceCodeDocument, string>(nameof(Title), "Code");

    /// <summary>
    /// Defines the <see cref="Language"/> property.
    /// </summary>
    public static readonly StyledProperty<string> LanguageProperty =
        AvaloniaProperty.Register<SourceCodeDocument, string>(nameof(Language), "Text");

    /// <summary>
    /// Defines the <see cref="Code"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> CodeProperty =
        AvaloniaProperty.Register<SourceCodeDocument, string?>(nameof(Code));

    /// <summary>
    /// Defines the <see cref="Source"/> property.
    /// </summary>
    public static readonly StyledProperty<Uri?> SourceProperty =
        AvaloniaProperty.Register<SourceCodeDocument, Uri?>(nameof(Source));

    /// <summary>
    /// Defines the <see cref="ResolvedCode"/> property.
    /// </summary>
    public static readonly DirectProperty<SourceCodeDocument, string> ResolvedCodeProperty =
        AvaloniaProperty.RegisterDirect<SourceCodeDocument, string>(
            nameof(ResolvedCode),
            document => document.ResolvedCode);

    /// <summary>
    /// Defines the <see cref="LoadError"/> property.
    /// </summary>
    public static readonly DirectProperty<SourceCodeDocument, string?> LoadErrorProperty =
        AvaloniaProperty.RegisterDirect<SourceCodeDocument, string?>(
            nameof(LoadError),
            document => document.LoadError);

    private string _resolvedCode = string.Empty;
    private string? _loadError;

    /// <summary>
    /// Gets or sets the document title displayed by the source viewer.
    /// </summary>
    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
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
    /// Gets or sets inline source text. When set, this takes precedence over <see cref="Source"/>.
    /// </summary>
    public string? Code
    {
        get => GetValue(CodeProperty);
        set => SetValue(CodeProperty, value);
    }

    /// <summary>
    /// Gets or sets an <c>avares:</c> or <c>resm:</c> URI containing source text.
    /// </summary>
    public Uri? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>
    /// Gets the inline or loaded source text.
    /// </summary>
    public string ResolvedCode
    {
        get => _resolvedCode;
        private set => SetAndRaise(ResolvedCodeProperty, ref _resolvedCode, value);
    }

    /// <summary>
    /// Gets a user-facing message when <see cref="Source"/> cannot be loaded.
    /// </summary>
    public string? LoadError
    {
        get => _loadError;
        private set => SetAndRaise(LoadErrorProperty, ref _loadError, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == CodeProperty || change.Property == SourceProperty)
        {
            ResolveCode();
        }
    }

    private void ResolveCode()
    {
        if (Code is { } code)
        {
            LoadError = null;
            ResolvedCode = code;
            return;
        }

        if (Source is not { } source)
        {
            LoadError = null;
            ResolvedCode = string.Empty;
            return;
        }

        try
        {
            using var stream = AssetLoader.Open(source);
            using var reader = new StreamReader(stream);
            ResolvedCode = reader.ReadToEnd();
            LoadError = null;
        }
        catch (Exception)
        {
            LoadError = $"Unable to load source '{source}'.";
            ResolvedCode = LoadError;
        }
    }
}
