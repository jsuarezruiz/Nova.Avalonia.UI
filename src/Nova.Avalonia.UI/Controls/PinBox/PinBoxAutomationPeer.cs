using Avalonia.Automation.Peers;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Exposes <see cref="PinBox"/> to accessibility APIs.
/// </summary>
public class PinBoxAutomationPeer : ControlAutomationPeer
{
    public PinBoxAutomationPeer(PinBox owner) : base(owner) { }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.Edit;
    }

    protected override string GetClassNameCore()
    {
        return "PinBox";
    }

    protected override string GetNameCore()
    {
        var pinBox = (PinBox)Owner;
        
        // Announce masked text for password mode, or actual length
        if (pinBox.IsPassword)
        {
            return $"PIN entry, {pinBox.Text.Length} of {pinBox.Length} digits entered";
        }
        
        if (pinBox.HasError && !string.IsNullOrEmpty(pinBox.ErrorText))
        {
            return $"PIN entry, error: {pinBox.ErrorText}";
        }
        
        return $"PIN entry, {pinBox.Text.Length} of {pinBox.Length} digits";
    }

    protected override bool IsContentElementCore() => true;

    protected override bool IsControlElementCore() => true;
}
