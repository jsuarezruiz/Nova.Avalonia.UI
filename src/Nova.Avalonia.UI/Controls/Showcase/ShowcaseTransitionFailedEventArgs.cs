using System;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Provides data for asynchronous showcase transition failures.
/// </summary>
public sealed class ShowcaseTransitionFailedEventArgs : EventArgs
{
    /// <summary>
    /// Creates new transition failure arguments.
    /// </summary>
    public ShowcaseTransitionFailedEventArgs(ShowcaseNavigationAction action, Exception exception)
    {
        Action = action;
        Exception = exception;
    }

    /// <summary>
    /// Gets the navigation action that failed.
    /// </summary>
    public ShowcaseNavigationAction Action { get; }

    /// <summary>
    /// Gets the underlying exception.
    /// </summary>
    public Exception Exception { get; }
}
