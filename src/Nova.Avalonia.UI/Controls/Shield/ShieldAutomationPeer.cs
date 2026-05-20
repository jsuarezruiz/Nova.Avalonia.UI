using System;
using System.Linq;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Exposes <see cref="Shield"/> types to UI Automation.
/// </summary>
public class ShieldAutomationPeer : ButtonAutomationPeer, IInvokeProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShieldAutomationPeer"/> class.
    /// </summary>
    /// <param name="owner">The <see cref="Shield"/> that is associated with this peer.</param>
    public ShieldAutomationPeer(Shield owner) : base(owner)
    {
    }

    /// <inheritdoc />
    public new void Invoke()
    {
        ((IInvokeProvider)this).Invoke();
    }

    /// <inheritdoc />
    protected override string GetClassNameCore()
    {
        return "Shield";
    }

    /// <inheritdoc />
    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return ShieldOwner.IsReadOnly ? AutomationControlType.Text : AutomationControlType.Button;
    }

    /// <inheritdoc />
    protected override string? GetNameCore()
    {
        var name = base.GetNameCore();
        if (!string.IsNullOrWhiteSpace(name))
        {
            return name;
        }

        var subject = GetAccessibleText(ShieldOwner.Subject);
        var status = GetAccessibleText(ShieldOwner.Status);

        if (!string.IsNullOrEmpty(subject) && !string.IsNullOrEmpty(status))
        {
            return $"{subject}: {status}";
        }

        return !string.IsNullOrEmpty(subject) ? subject : status;
    }

    /// <inheritdoc />
    protected override bool IsKeyboardFocusableCore()
    {
        return !ShieldOwner.IsReadOnly && base.IsKeyboardFocusableCore();
    }

    /// <inheritdoc />
    protected override object? GetProviderCore(Type providerType)
    {
        if (ShieldOwner.IsReadOnly && providerType == typeof(IInvokeProvider))
        {
            return null;
        }

        return base.GetProviderCore(providerType);
    }

    private Shield ShieldOwner => (Shield)Owner;

    private static string GetAccessibleText(object? content)
    {
        switch (content)
        {
            case null:
                return string.Empty;
            case string text:
                return text;
            case TextBlock textBlock:
                return textBlock.Text ?? string.Empty;
            case ContentControl contentControl:
                return GetAccessibleText(contentControl.Content);
            case Panel panel:
                return string.Join(" ", panel.Children
                    .Select(GetAccessibleText)
                    .Where(text => !string.IsNullOrWhiteSpace(text)));
            case Control control:
                return AutomationProperties.GetName(control) ?? string.Empty;
            default:
                return content.ToString() ?? string.Empty;
        }
    }

    void IInvokeProvider.Invoke()
    {
        if (ShieldOwner.IsReadOnly)
        {
            return;
        }

        base.Invoke();
    }
}
