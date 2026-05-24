using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Exposes <see cref="PinBox"/> to accessibility APIs.
/// </summary>
public class PinBoxAutomationPeer : ControlAutomationPeer, IValueProvider
{
    private readonly PinBox _owner;

    public PinBoxAutomationPeer(PinBox owner) : base(owner)
    {
        _owner = owner;
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.Edit;
    }

    protected override string GetClassNameCore()
    {
        return "PinBox";
    }

    protected override string GetNameCore()
    {
        var text = _owner.Text ?? string.Empty;

        if (_owner.HasError && !string.IsNullOrEmpty(_owner.ErrorText))
        {
            return $"PIN entry, error: {_owner.ErrorText}";
        }

        if (_owner.IsPassword)
        {
            return $"PIN entry, {text.Length} of {_owner.Length} characters entered";
        }

        return $"PIN entry, {text.Length} of {_owner.Length} characters";
    }

    protected override bool IsContentElementCore() => true;

    protected override bool IsControlElementCore() => true;

    bool IValueProvider.IsReadOnly => !_owner.IsEffectivelyEnabled || _owner.IsReadOnly;

    string? IValueProvider.Value
    {
        get
        {
            var text = _owner.Text ?? string.Empty;
            return _owner.IsPassword ? new string(_owner.PasswordChar, text.Length) : text;
        }
    }

    void IValueProvider.SetValue(string? value)
    {
        if (!_owner.IsEffectivelyEnabled)
        {
            throw new ElementNotEnabledException();
        }

        if (!_owner.IsReadOnly)
        {
            _owner.Text = value ?? string.Empty;
        }
    }
}
