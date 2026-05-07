using System;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Automation peer for the <see cref="SegmentedSlider"/> control.
/// </summary>
public class SegmentedSliderAutomationPeer : ControlAutomationPeer, IRangeValueProvider
{
    private readonly SegmentedSlider _owner;

    /// <summary>
    /// Initializes a new instance of the <see cref="SegmentedSliderAutomationPeer"/> class.
    /// </summary>
    public SegmentedSliderAutomationPeer(SegmentedSlider owner) : base(owner)
    {
        _owner = owner;
    }

    /// <inheritdoc/>
    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Slider;

    /// <inheritdoc/>
    protected override string GetClassNameCore() => nameof(SegmentedSlider);

    /// <inheritdoc/>
    protected override string? GetNameCore()
    {
        var name = base.GetNameCore();
        if (!string.IsNullOrWhiteSpace(name))
            return name;

        var index = _owner.CalculateSegmentIndex(_owner.Value);
        var segment = _owner.Segments is { Count: > 0 } && index < _owner.Segments.Count
            ? _owner.Segments[index]
            : null;
        var segmentName = string.IsNullOrWhiteSpace(segment?.Title) ? $"Segment {index + 1}" : segment.Title;

        var maximum = Math.Max(_owner.Minimum, _owner.Maximum);
        return $"{segmentName} ({_owner.Value:G} from {_owner.Minimum:G} to {maximum:G})";
    }

    bool IRangeValueProvider.IsReadOnly => !_owner.IsEnabled || _owner.IsReadOnly;

    double IRangeValueProvider.Minimum => _owner.Minimum;

    double IRangeValueProvider.Maximum => Math.Max(_owner.Minimum, _owner.Maximum);

    double IRangeValueProvider.Value => _owner.Value;

    double IRangeValueProvider.SmallChange => _owner.GetSmallInteractionChange();

    double IRangeValueProvider.LargeChange => _owner.GetLargeInteractionChange();

    void IRangeValueProvider.SetValue(double value)
    {
        if (!_owner.IsEnabled)
            throw new ElementNotEnabledException();

        if (!_owner.IsReadOnly)
            _owner.Value = value;
    }
}
