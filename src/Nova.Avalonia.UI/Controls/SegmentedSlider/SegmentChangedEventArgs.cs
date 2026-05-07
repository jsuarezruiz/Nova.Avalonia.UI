using System;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Provides data for the <see cref="SegmentedSlider.SegmentChanged"/> event.
/// </summary>
public class SegmentChangedEventArgs(int oldIndex, int newIndex, SegmentedSliderSegment? segment) : EventArgs
{
    /// <summary>
    /// Gets the previously active segment index.
    /// </summary>
    public int OldIndex { get; } = oldIndex;

    /// <summary>
    /// Gets the newly active segment index.
    /// </summary>
    public int NewIndex { get; } = newIndex;

    /// <summary>
    /// Gets the newly active segment, or null when the slider uses generated equal segments.
    /// </summary>
    public SegmentedSliderSegment? Segment { get; } = segment;
}
