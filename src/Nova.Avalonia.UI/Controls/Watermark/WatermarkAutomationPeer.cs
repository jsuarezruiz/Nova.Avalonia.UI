using Avalonia.Automation;
using Avalonia.Automation.Peers;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Custom AutomationPeer for the Watermark control.
/// </summary>
public class WatermarkAutomationPeer : ControlAutomationPeer
{
    private readonly Watermark _owner;

    /// <summary>
    /// Initializes a new instance of the <see cref="WatermarkAutomationPeer"/> class.
    /// </summary>
    /// <param name="owner">The <see cref="Watermark"/> control that owns this peer.</param>
    public WatermarkAutomationPeer(Watermark owner) : base(owner)
    {
        _owner = owner;
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return _owner.Source != null ? AutomationControlType.Image : AutomationControlType.Text;
    }

    protected override string GetClassNameCore()
    {
        return "Watermark";
    }

    protected override string? GetNameCore()
    {
        var name = base.GetNameCore();
        if (!string.IsNullOrEmpty(name))
            return name;

        if (!string.IsNullOrEmpty(_owner.Text))
            return _owner.Text;

        if (_owner.Source != null)
            return "Watermark image";

        return string.Empty;
    }

    protected override bool IsContentElementCore()
    {
        return true;
    }

    protected override bool IsControlElementCore()
    {
        return true;
    }
}
