using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
    /// Defines the <see cref="LoadErrorMessage"/> property.
    /// </summary>
    public static readonly StyledProperty<string> LoadErrorMessageProperty =
        AvaloniaProperty.Register<SourceCodeDocument, string>(
            nameof(LoadErrorMessage),
            "Unable to load source.");

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
    private CancellationTokenSource? _resolutionCancellation;
    private Task? _resolutionTask;
    private int _resolutionRequestCount;
    private int _resolutionVersion;
    private bool _isResolved = true;

    /// <summary>
    /// Occurs when the source resource cannot be loaded.
    /// </summary>
    public event EventHandler<SourceCodeLoadFailedEventArgs>? LoadFailed;

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
    /// Gets or sets the user-facing message displayed when <see cref="Source"/> cannot be loaded.
    /// </summary>
    public string LoadErrorMessage
    {
        get => GetValue(LoadErrorMessageProperty);
        set => SetValue(LoadErrorMessageProperty, value);
    }

    /// <summary>
    /// Gets the inline source text, or the resource text after the document is first selected.
    /// </summary>
    public string ResolvedCode
    {
        get => _resolvedCode;
        private set => SetAndRaise(ResolvedCodeProperty, ref _resolvedCode, value);
    }

    /// <summary>
    /// Gets a user-facing message when a selected <see cref="Source"/> cannot be loaded.
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
            InvalidateResolvedCode();
        }
        else if (change.Property == LoadErrorMessageProperty && LoadError is not null)
        {
            LoadError = LoadErrorMessage;
            ResolvedCode = LoadErrorMessage;
        }
    }

    internal Task EnsureCodeResolvedAsync()
    {
        if (_isResolved)
        {
            return Task.CompletedTask;
        }

        if (_resolutionTask is not null)
        {
            return _resolutionTask;
        }

        var source = Source;
        if (source is null)
        {
            _isResolved = true;
            return Task.CompletedTask;
        }

        var cancellation = new CancellationTokenSource();
        _resolutionCancellation = cancellation;
        var resolutionTask = ResolveCodeAsync(source, _resolutionVersion, cancellation);
        if (ReferenceEquals(_resolutionCancellation, cancellation))
        {
            _resolutionTask = resolutionTask;
        }

        return resolutionTask;
    }

    internal void RequestCodeResolution()
    {
        _resolutionRequestCount++;
        _ = EnsureCodeResolvedAsync();
    }

    internal void ReleaseCodeResolution()
    {
        if (_resolutionRequestCount == 0)
        {
            return;
        }

        _resolutionRequestCount--;
        if (_resolutionRequestCount == 0)
        {
            CancelCodeResolution();
        }
    }

    private void CancelCodeResolution()
    {
        var cancellation = _resolutionCancellation;
        if (cancellation is null)
        {
            return;
        }

        _resolutionCancellation = null;
        _resolutionTask = null;
        cancellation.Cancel();
        cancellation.Dispose();
    }

    private void InvalidateResolvedCode()
    {
        _resolutionVersion++;
        CancelCodeResolution();
        _isResolved = false;
        LoadError = null;

        if (Code is { } code)
        {
            ResolvedCode = code;
            _isResolved = true;
            return;
        }

        ResolvedCode = string.Empty;
        if (Source is null)
        {
            _isResolved = true;
        }

        if (!_isResolved && _resolutionRequestCount > 0)
        {
            _ = EnsureCodeResolvedAsync();
        }
    }

    private async Task ResolveCodeAsync(
        Uri source,
        int version,
        CancellationTokenSource cancellation)
    {
        try
        {
            using var stream = AssetLoader.Open(source);
            using var reader = new StreamReader(stream);
            var resolvedCode = await reader.ReadToEndAsync(cancellation.Token);
            if (cancellation.IsCancellationRequested || version != _resolutionVersion)
            {
                return;
            }

            ResolvedCode = resolvedCode;
            LoadError = null;
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            return;
        }
        catch (Exception exception)
        {
            if (cancellation.IsCancellationRequested || version != _resolutionVersion)
            {
                return;
            }

            LoadError = LoadErrorMessage;
            ResolvedCode = LoadError;
            LoadFailed?.Invoke(this, new SourceCodeLoadFailedEventArgs(source, exception));
        }
        finally
        {
            if (ReferenceEquals(_resolutionCancellation, cancellation))
            {
                _resolutionCancellation = null;
                _resolutionTask = null;
                cancellation.Dispose();
                if (version == _resolutionVersion)
                {
                    _isResolved = true;
                }
            }
        }
    }
}
