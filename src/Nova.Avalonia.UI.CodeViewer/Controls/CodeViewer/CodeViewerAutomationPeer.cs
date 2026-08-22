using System;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;

namespace Nova.Avalonia.UI.CodeViewer;

/// <summary>
/// Exposes a <see cref="CodeViewer"/> as a read-only document.
/// </summary>
public sealed class CodeViewerAutomationPeer : ControlAutomationPeer, IValueProvider
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

    bool IValueProvider.IsReadOnly => true;

    string? IValueProvider.Value => _owner.Code ?? string.Empty;

    void IValueProvider.SetValue(string? value) =>
        throw new InvalidOperationException("The source code viewer is read-only.");

    internal void NotifyCodeChanged(string? oldValue, string? newValue)
    {
        RaisePropertyChangedEvent(
            ValuePatternIdentifiers.ValueProperty,
            oldValue ?? string.Empty,
            newValue ?? string.Empty);
    }
}
