using Avalonia.Automation.Peers;

namespace Nova.Avalonia.UI.CodeViewer;

/// <summary>
/// Exposes a <see cref="CodeViewer"/> as a read-only document.
/// </summary>
public sealed class CodeViewerAutomationPeer : ControlAutomationPeer
{
    private readonly CodeViewer _owner;

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeViewerAutomationPeer"/> class.
    /// </summary>
    /// <param name="owner">The code viewer represented by the automation peer.</param>
    public CodeViewerAutomationPeer(CodeViewer owner) : base(owner)
    {
        _owner = owner;
    }

    protected override AutomationControlType GetAutomationControlTypeCore() =>
        AutomationControlType.Document;

    protected override string GetClassNameCore() => nameof(CodeViewer);

    protected override string? GetNameCore()
    {
        var name = base.GetNameCore();
        return string.IsNullOrWhiteSpace(name) ? $"{_owner.Language} source code" : name;
    }
}
