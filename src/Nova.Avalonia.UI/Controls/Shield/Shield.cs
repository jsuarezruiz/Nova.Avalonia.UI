using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// A control that displays a subject and status in a badge-like format, commonly used
/// to display build status, version numbers, or other metadata in a compact way.
/// </summary>
/// <remarks>
/// The Shield control is designed to mimic the style of shields.io badges.
/// It consists of two parts: a subject (left side) and a status (right side),
/// each with distinct background colors.
/// </remarks>
public class Shield : Button
{
    /// <summary>
    /// Defines the <see cref="Subject"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> SubjectProperty =
        AvaloniaProperty.Register<Shield, object?>(nameof(Subject));

    /// <summary>
    /// Defines the <see cref="Status"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> StatusProperty =
        AvaloniaProperty.Register<Shield, object?>(nameof(Status));

    /// <summary>
    /// Defines the <see cref="SubjectBackground"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> SubjectBackgroundProperty =
        AvaloniaProperty.Register<Shield, IBrush?>(nameof(SubjectBackground));

    /// <summary>
    /// Defines the <see cref="IsReadOnly"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<Shield, bool>(nameof(IsReadOnly));

    /// <summary>
    /// Gets or sets the content displayed on the left side of the shield (the subject/label).
    /// </summary>
    /// <value>
    /// The subject content, typically a string like "build", "version", or "license".
    /// Can also be any UI element for custom rendering.
    /// </value>
    public object? Subject
    {
        get => GetValue(SubjectProperty);
        set => SetValue(SubjectProperty, value);
    }

    /// <summary>
    /// Gets or sets the content displayed on the right side of the shield (the status/value).
    /// </summary>
    /// <value>
    /// The status content, typically a string like "passing", "1.0.0", or "MIT".
    /// Can also be any UI element for custom rendering.
    /// </value>
    public object? Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>
    /// Gets or sets the background color of the subject (left) part of the shield.
    /// </summary>
    /// <value>
    /// An <see cref="IBrush"/> used as the background for the subject section.
    /// If not set, falls back to the theme-defined <c>ShieldSubjectBackgroundBrush</c>.
    /// </value>
    public IBrush? SubjectBackground
    {
        get => GetValue(SubjectBackgroundProperty);
        set => SetValue(SubjectBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the shield is in read-only (display-only) mode.
    /// </summary>
    /// <value>
    /// When <c>true</c>, the shield is non-interactive but retains full opacity,
    /// unlike the disabled state which fades the control. Use this for informational badges.
    /// </value>
    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsReadOnlyProperty)
        {
            PseudoClasses.Set(":readonly", change.GetNewValue<bool>());
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (IsReadOnly)
        {
            e.Handled = true;
            return;
        }

        base.OnPointerPressed(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (IsReadOnly)
        {
            return;
        }

        base.OnKeyDown(e);
    }

    /// <inheritdoc />
    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new ShieldAutomationPeer(this);
    }
}
