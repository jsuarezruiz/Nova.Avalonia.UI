namespace Nova.Avalonia.UI.Controls;

using global::Avalonia.Automation.Peers;



/// <summary>
/// Custom AutomationPeer for the Badge control.
/// </summary>
public class BadgeAutomationPeer : ControlAutomationPeer
{
    private readonly Badge _owner;

    public BadgeAutomationPeer(Badge owner) : base(owner)
    {
        _owner = owner;
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        // If wrapping content, it's a Group. If standalone, it's Text/Label.
        return _owner.Content != null ? AutomationControlType.Group : AutomationControlType.Text;
    }

    protected override string GetClassNameCore()
    {
        return "Badge";
    }

    protected override string? GetNameCore()
    {
        // Check for manually set AutomationProperties.Name
        var baseName = base.GetNameCore();
        if (!string.IsNullOrEmpty(baseName)) 
            return baseName;

        // 2. Generate name from badge content for screen readers
        if (_owner.IsBadgeVisible)
        {
            var display = _owner.DisplayContent?.ToString() ?? _owner.BadgeContent?.ToString();
            if (!string.IsNullOrEmpty(display))
            {
                return $"Badge: {display}";
            }
            
            if (_owner.Kind == BadgeKind.Dot)
            {
                return "Notification indicator";
            }
        }

        return string.Empty;
    }

    protected override string? GetHelpTextCore()
    {
        // Check for manually set HelpText
        var baseHelp = base.GetHelpTextCore();
        if (!string.IsNullOrEmpty(baseHelp)) return baseHelp;

        // 2. Generate dynamic HelpText with more context
        if (_owner.IsBadgeVisible)
        {
            var display = _owner.DisplayContent?.ToString() ?? _owner.BadgeContent?.ToString();
            if (!string.IsNullOrEmpty(display))
            {
                return $"This item has {display} notifications";
            }
            
            if (_owner.Kind == BadgeKind.Dot)
            {
                return "This item has new activity";
            }
        }

        return string.Empty;
    }
}