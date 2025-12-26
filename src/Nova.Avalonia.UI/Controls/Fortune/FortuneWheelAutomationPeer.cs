using Avalonia.Automation.Peers;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Provides accessibility support for the <see cref="FortuneWheel"/> control.
/// </summary>
public class FortuneWheelAutomationPeer : ControlAutomationPeer
{
    private readonly FortuneWheel _owner;

    /// <summary>
    /// Initializes a new instance of the <see cref="FortuneWheelAutomationPeer"/> class.
    /// </summary>
    /// <param name="owner">The FortuneWheel control.</param>
    public FortuneWheelAutomationPeer(FortuneWheel owner) : base(owner)
    {
        _owner = owner;
    }

    /// <inheritdoc/>
    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.Custom;
    }

    /// <inheritdoc/>
    protected override string GetClassNameCore()
    {
        return "FortuneWheel";
    }

    /// <inheritdoc/>
    protected override string? GetNameCore()
    {
        if (_owner.Items == null || _owner.Items.Count == 0)
            return "Fortune Wheel (empty)";

        var count = _owner.Items.Count;
        var current = _owner.SelectedIndex >= 0 && _owner.SelectedIndex < count
            ? _owner.Items[_owner.SelectedIndex].Content?.ToString() ?? "Unknown"
            : "None";

        return $"Fortune Wheel with {count} items. Currently selected: {current}";
    }

    /// <inheritdoc/>
    protected override string GetHelpTextCore()
    {
        return _owner.IsSpinning
            ? "Wheel is spinning..."
            : "Press to spin the wheel and select a random item.";
    }
}
