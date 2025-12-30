using Avalonia.Automation.Peers;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Exposes <see cref="Scratcher"/> to accessibility APIs.
/// </summary>
public class ScratcherAutomationPeer : ControlAutomationPeer
{
    public ScratcherAutomationPeer(Scratcher owner) : base(owner) { }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.Custom;
    }

    protected override string GetClassNameCore()
    {
        return "Scratcher";
    }

    protected override string GetNameCore()
    {
        var scratcher = (Scratcher)Owner;
        
        if (scratcher.IsThresholdReached)
            return "Scratcher - Content revealed";
        
        return $"Scratcher - {scratcher.ScratchProgress:F0}% scratched. Press Space or Enter to reveal content.";
    }

    protected override string? GetHelpTextCore()
    {
        return "Interactive scratch card. Scratch to reveal hidden content, or press Space/Enter to reveal instantly.";
    }

    protected override bool IsContentElementCore() => true;

    protected override bool IsControlElementCore() => true;
}
