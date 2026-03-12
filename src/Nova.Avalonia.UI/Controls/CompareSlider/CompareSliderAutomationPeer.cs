using Avalonia.Automation.Peers;
using Avalonia.Controls;

namespace Nova.Avalonia.UI.Controls.AutomationPeers;

/// <summary>
/// Automation peer for the <see cref="CompareSlider"/> control.
/// </summary>
public class CompareSliderAutomationPeer : RangeBaseAutomationPeer
{
    public CompareSliderAutomationPeer(CompareSlider owner) : base(owner)
    {
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.Slider;
    }

    protected override string GetClassNameCore()
    {
        return "CompareSlider";
    }

    protected override string? GetNameCore()
    {
        var name = base.GetNameCore();

        if (string.IsNullOrEmpty(name))
        {
            name = GetClassNameCore();
        }

        return name;
    }
}
