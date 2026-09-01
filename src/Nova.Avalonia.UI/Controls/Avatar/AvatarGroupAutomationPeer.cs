using System.Collections.Generic;
using Avalonia.Automation.Peers;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Exposes <see cref="AvatarGroup"/> to accessibility APIs.
/// </summary>
public class AvatarGroupAutomationPeer : ControlAutomationPeer
{
    public AvatarGroupAutomationPeer(AvatarGroup owner) : base(owner) { }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        // Reports as a Group or List for screen readers
        return AutomationControlType.Group;
    }

    protected override string GetClassNameCore()
    {
        return "AvatarGroup";
    }

    protected override IReadOnlyList<AutomationPeer>? GetChildrenCore()
    {
        // This calls the base implementation which traverses visual children.
        // Since our Avatars are inside the AvatarStackPanel (a visual child), 
        // the automation tree should find them automatically.
        return base.GetChildrenCore();
    }
}
