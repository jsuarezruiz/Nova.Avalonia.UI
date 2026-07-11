using Avalonia.Automation.Peers;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Exposes <see cref="SocialButton"/> to UI Automation.
/// </summary>
public class SocialButtonAutomationPeer : ButtonAutomationPeer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SocialButtonAutomationPeer"/> class.
    /// </summary>
    /// <param name="owner">The associated <see cref="SocialButton"/>.</param>
    public SocialButtonAutomationPeer(SocialButton owner) : base(owner)
    {
    }

    /// <inheritdoc />
    protected override string GetClassNameCore() => nameof(SocialButton);

    /// <inheritdoc />
    protected override string? GetNameCore()
    {
        var name = base.GetNameCore();
        return string.IsNullOrWhiteSpace(name) ? SocialButtonOwner.DisplayText : name;
    }

    private SocialButton SocialButtonOwner => (SocialButton)Owner;
}
