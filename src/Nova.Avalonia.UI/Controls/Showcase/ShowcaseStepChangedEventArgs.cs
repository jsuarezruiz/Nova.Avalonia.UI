using System;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Provides data for showcase step changes.
/// </summary>
public class ShowcaseStepChangedEventArgs : EventArgs
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public ShowcaseStepChangedEventArgs(ShowcaseStep? previous, ShowcaseStep current, int index)
    {
        PreviousStep = previous;
        CurrentStep = current;
        CurrentIndex = index;
    }

    /// <summary>
    /// Gets the previous step, or null if the showcase is starting.
    /// </summary>
    public ShowcaseStep? PreviousStep { get; }

    /// <summary>
    /// Gets the current step.
    /// </summary>
    public ShowcaseStep CurrentStep { get; }

    /// <summary>
    /// Gets the index of the current step.
    /// </summary>
    public int CurrentIndex { get; }
}
