using Avalonia.Automation.Peers;

namespace Nova.Avalonia.UI.BarcodeGenerator;

/// <summary>
/// Exposes the <see cref="BarcodeGenerator"/> to UI automation (accessibility).
/// Defines the control as an "Image" type so screen readers treat it appropriately.
/// </summary>
public class BarcodeGeneratorAutomationPeer : ControlAutomationPeer
{
    public BarcodeGeneratorAutomationPeer(BarcodeGenerator owner) : base(owner) { }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        // Inform screen readers that this is an image/graphic
        return AutomationControlType.Image;
    }

    protected override string GetClassNameCore()
    {
        return "BarcodeGenerator";
    }
}