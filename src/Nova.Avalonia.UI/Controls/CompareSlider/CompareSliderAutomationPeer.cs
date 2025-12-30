using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Nova.Avalonia.UI.Controls;

namespace Nova.Avalonia.UI.Controls.AutomationPeers;

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
        return nameof(CompareSlider);
    }
}
