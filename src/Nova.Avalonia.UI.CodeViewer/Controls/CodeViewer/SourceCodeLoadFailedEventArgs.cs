using System;

namespace Nova.Avalonia.UI.CodeViewer;

/// <summary>
/// Provides details when a source document cannot be loaded.
/// </summary>
public sealed class SourceCodeLoadFailedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SourceCodeLoadFailedEventArgs"/> class.
    /// </summary>
    /// <param name="source">The resource that could not be loaded.</param>
    /// <param name="exception">The resource-loading error.</param>
    public SourceCodeLoadFailedEventArgs(Uri source, Exception exception)
    {
        Source = source;
        Exception = exception;
    }

    /// <summary>
    /// Gets the resource that could not be loaded.
    /// </summary>
    public Uri Source { get; }

    /// <summary>
    /// Gets the resource-loading error.
    /// </summary>
    public Exception Exception { get; }
}
