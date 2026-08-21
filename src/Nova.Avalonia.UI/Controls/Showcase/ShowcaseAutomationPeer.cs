using Avalonia.Automation.Peers;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Exposes the <see cref="Showcase"/> control to UI automation.
/// </summary>
public sealed class ShowcaseAutomationPeer : ControlAutomationPeer
{
    /// <summary>
    /// Creates a new automation peer.
    /// </summary>
    public ShowcaseAutomationPeer(Showcase owner) : base(owner)
    {
    }

    /// <inheritdoc />
    protected override string GetClassNameCore() => nameof(Showcase);

    /// <inheritdoc />
    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Pane;
}
