using Avalonia.Automation.Peers;

namespace Nova.Avalonia.UI.CodeViewer;

/// <summary>
/// Exposes a <see cref="SourceCodeDrawer"/> as an accessible pane.
/// </summary>
public sealed class SourceCodeDrawerAutomationPeer : ControlAutomationPeer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SourceCodeDrawerAutomationPeer"/> class.
    /// </summary>
    /// <param name="owner">The source drawer represented by this peer.</param>
    public SourceCodeDrawerAutomationPeer(SourceCodeDrawer owner) : base(owner)
    {
    }

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Pane;

    protected override string GetClassNameCore() => nameof(SourceCodeDrawer);

    protected override string? GetNameCore()
    {
        var name = base.GetNameCore();
        return string.IsNullOrWhiteSpace(name) ? "Source code" : name;
    }
}
