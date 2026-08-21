namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Identifies a showcase navigation operation.
/// </summary>
public enum ShowcaseNavigationAction
{
    /// <summary>
    /// Start the showcase from the beginning.
    /// </summary>
    Start,

    /// <summary>
    /// Resume the showcase from persisted progress.
    /// </summary>
    Resume,

    /// <summary>
    /// Advance to the next step.
    /// </summary>
    Next,

    /// <summary>
    /// Go back to the previous step.
    /// </summary>
    Previous,

    /// <summary>
    /// Skip the showcase.
    /// </summary>
    Skip,

    /// <summary>
    /// Reset the showcase to its initial state.
    /// </summary>
    Reset,

    /// <summary>
    /// Update persisted state after the steps collection changes.
    /// </summary>
    StepsChanged
}
