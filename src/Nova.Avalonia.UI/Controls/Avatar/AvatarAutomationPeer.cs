using Avalonia.Automation.Peers;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Exposes <see cref="Avatar"/> to accessibility APIs.
/// </summary>
public class AvatarAutomationPeer : ControlAutomationPeer
{
    public AvatarAutomationPeer(Avatar owner) : base(owner) { }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.Image;
    }

    protected override string GetClassNameCore()
    {
        return "Avatar";
    }

    protected override string? GetNameCore()
    {
        var avatar = (Avatar)Owner;
        // Announce the display name, or fall back to default if empty
        if (!string.IsNullOrEmpty(avatar.DisplayName))
            return avatar.DisplayName;
            
        return base.GetNameCore();
    }
}
