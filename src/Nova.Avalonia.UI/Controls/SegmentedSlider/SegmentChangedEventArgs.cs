using System;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Provides data for the <see cref="SegmentedSlider.SegmentChanged"/> event.
/// </summary>
public class SegmentChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SegmentChangedEventArgs"/> class.
    /// </summary>
    /// <param name="oldIndex">The previously active segment index.</param>
    /// <param name="newIndex">The newly active segment index.</param>
    /// <param name="segment">The newly active segment, or null when the slider uses generated equal segments.</param>
    public SegmentChangedEventArgs(int oldIndex, int newIndex, SegmentedSliderSegment? segment)
    {
        OldIndex = oldIndex;
        NewIndex = newIndex;
        Segment = segment;
    }

    /// <summary>
    /// Gets the previously active segment index.
    /// </summary>
    public int OldIndex { get; }

    /// <summary>
    /// Gets the newly active segment index.
    /// </summary>
    public int NewIndex { get; }

    /// <summary>
    /// Gets the newly active segment, or null when the slider uses generated equal segments.
    /// </summary>
    public SegmentedSliderSegment? Segment { get; }
}
