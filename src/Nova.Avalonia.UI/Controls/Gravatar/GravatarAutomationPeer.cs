using Avalonia.Automation.Peers;
using Avalonia.Controls;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Automation peer for the <see cref="Gravatar"/> control.
/// </summary>
public class GravatarAutomationPeer : ControlAutomationPeer
{
    public GravatarAutomationPeer(Gravatar owner) : base(owner)
    {
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.Image;
    }

    protected override string GetClassNameCore()
    {
        return "Gravatar";
    }

    protected override string? GetNameCore()
    {
        var name = base.GetNameCore();
        if (!string.IsNullOrEmpty(name))
        {
            return name;
        }

        if (Owner is Gravatar gravatar)
        {
            return gravatar.Id ?? "Avatar";
        }

        return "Avatar";
    }
}
