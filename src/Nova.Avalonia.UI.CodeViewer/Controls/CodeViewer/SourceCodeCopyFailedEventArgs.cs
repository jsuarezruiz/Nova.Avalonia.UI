using System;

namespace Nova.Avalonia.UI.CodeViewer;

/// <summary>
/// Provides details when source code cannot be copied to the clipboard.
/// </summary>
public sealed class SourceCodeCopyFailedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SourceCodeCopyFailedEventArgs"/> class.
    /// </summary>
    /// <param name="exception">The clipboard error.</param>
    public SourceCodeCopyFailedEventArgs(Exception exception)
    {
        Exception = exception;
    }

    /// <summary>
    /// Gets the clipboard error.
    /// </summary>
    public Exception Exception { get; }
}
