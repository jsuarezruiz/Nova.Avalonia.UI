using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Defines an optional segment for a <see cref="SegmentedSlider"/>, including title, proportional width, and per-segment brushes.
/// </summary>
public class SegmentedSliderSegment : INotifyPropertyChanged
{
    private string? _title;
    private double _widthRatio = 1.0;
    private IBrush? _fillBrush;
    private IBrush? _trackBrush;

    /// <summary>
    /// Gets or sets the title displayed for the segment.
    /// </summary>
    public string? Title
    {
        get => _title;
        set => SetField(ref _title, value);
    }

    /// <summary>
    /// Gets or sets the proportional segment width.
    /// </summary>
    public double WidthRatio
    {
        get => _widthRatio;
        set => SetField(ref _widthRatio, SegmentedSlider.CoerceSegmentRatio(value));
    }

    /// <summary>
    /// Gets or sets the filled portion brush for this segment.
    /// </summary>
    public IBrush? FillBrush
    {
        get => _fillBrush;
        set => SetField(ref _fillBrush, value);
    }

    /// <summary>
    /// Gets or sets the unfilled track brush for this segment.
    /// </summary>
    public IBrush? TrackBrush
    {
        get => _trackBrush;
        set => SetField(ref _trackBrush, value);
    }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
