namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Defines how segment titles are displayed in a <see cref="SegmentedSlider"/>.
/// </summary>
public enum SegmentTitleVisibility
{
    /// <summary>
    /// Segment titles are hidden.
    /// </summary>
    Collapsed,

    /// <summary>
    /// Segment titles are always visible.
    /// </summary>
    AlwaysVisible,

    /// <summary>
    /// Only the active segment title is visible.
    /// </summary>
    ActiveSegmentOnly,

    /// <summary>
    /// The active segment title and all preceding segment titles are visible.
    /// </summary>
    ActiveAndPrevious
}
