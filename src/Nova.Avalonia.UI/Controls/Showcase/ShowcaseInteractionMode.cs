namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Specifies how the underlying UI remains interactive while the showcase is active.
/// </summary>
public enum ShowcaseInteractionMode
{
    /// <summary>
    /// Block the underlying UI and keep only showcase chrome interactive.
    /// </summary>
    Modal,

    /// <summary>
    /// Allow the highlighted target while blocking the rest of the UI.
    /// </summary>
    TargetOnly,

    /// <summary>
    /// Keep the underlying UI interactive while the showcase acts as a visual guide.
    /// </summary>
    Passthrough
}
