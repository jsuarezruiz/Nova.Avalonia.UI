using Avalonia.Automation;
using Avalonia.Automation.Peers;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Provides accessibility support for the <see cref="FortuneBar"/> control.
/// </summary>
public class FortuneBarAutomationPeer : ControlAutomationPeer
{
    private readonly FortuneBar _owner;

    /// <summary>
    /// Initializes a new instance of the <see cref="FortuneBarAutomationPeer"/> class.
    /// </summary>
    /// <param name="owner">The FortuneBar control.</param>
    public FortuneBarAutomationPeer(FortuneBar owner) : base(owner)
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
        return "FortuneBar";
    }

    /// <inheritdoc/>
    protected override string GetHelpTextCore()
    {
        return _owner.IsSpinning
            ? "Bar is spinning..."
            : "Press to spin the bar and select a random item.";
    }
}
