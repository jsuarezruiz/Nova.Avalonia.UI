using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Nova.Avalonia.UI.CodeViewer;

/// <summary>
/// Hosts source documents in a window-level drawer with light-dismiss support.
/// </summary>
public class SourceCodeDrawer : ContentControl
{
    /// <summary>
    /// Defines the <see cref="DrawerWidth"/> property.
    /// </summary>
    public static readonly StyledProperty<double> DrawerWidthProperty =
        AvaloniaProperty.Register<SourceCodeDrawer, double>(
            nameof(DrawerWidth),
            640,
            validate: static value => double.IsFinite(value) && value >= 0);

    private static readonly TimeSpan TransitionDuration = TimeSpan.FromMilliseconds(180);

    private readonly Dictionary<Control, IDisposable> _backgroundAutomationOverrides = [];
    private Border? _backdrop;
    private Border? _drawerPanel;
    private Button? _closeButton;
    private TranslateTransform? _translation;
    private IDisposable? _visualParentSubscription;
    private IDisposable? _closeTimer;
    private TopLevel? _topLevel;
    private bool _isOpen;
    private bool _isClosing;
    private bool _isRedirectingFocus;

    /// <summary>
    /// Initializes a new instance of the <see cref="SourceCodeDrawer"/> class.
    /// </summary>
    public SourceCodeDrawer()
    {
        IsHitTestVisible = false;
    }

    /// <summary>
    /// Occurs after the drawer exit transition completes.
    /// </summary>
    public event EventHandler? Closed;

    /// <summary>
    /// Gets or sets the width of the drawer panel.
    /// </summary>
    public double DrawerWidth
    {
        get => GetValue(DrawerWidthProperty);
        set => SetValue(DrawerWidthProperty, value);
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new SourceCodeDrawerAutomationPeer(this);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        DetachTemplateHandlers();
        base.OnApplyTemplate(e);

        _backdrop = e.NameScope.Find<Border>("PART_Backdrop");
        _drawerPanel = e.NameScope.Find<Border>("PART_DrawerPanel");
        _closeButton = e.NameScope.Find<Button>("PART_CloseButton");

        if (_backdrop is not null)
        {
            _backdrop.PointerPressed += OnBackdropPressed;
            _backdrop.Opacity = 0;
            _backdrop.Transitions =
            [
                new DoubleTransition
                {
                    Property = OpacityProperty,
                    Duration = TransitionDuration,
                    Easing = new CubicEaseOut(),
                },
            ];
        }

        if (_drawerPanel is not null)
        {
            _translation = new TranslateTransform(DrawerWidth, 0)
            {
                Transitions =
                [
                    new DoubleTransition
                    {
                        Property = TranslateTransform.XProperty,
                        Duration = TransitionDuration,
                        Easing = new CubicEaseOut(),
                    },
                ],
            };
            _drawerPanel.RenderTransform = _translation;
        }

        if (_closeButton is not null)
        {
            _closeButton.Click += OnCloseClicked;
        }

        if (_isOpen)
        {
            Dispatcher.UIThread.Post(BeginOpen, DispatcherPriority.Render);
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _topLevel = TopLevel.GetTopLevel(this);
        if (_topLevel is not null)
        {
            _topLevel.AddHandler(KeyDownEvent, OnTopLevelKeyDown, RoutingStrategies.Tunnel, true);
            _topLevel.AddHandler(GotFocusEvent, OnTopLevelGotFocus, RoutingStrategies.Tunnel, true);
        }

        if (_isOpen)
        {
            StartBackgroundAutomationIsolation();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        RestoreBackgroundAutomation();

        if (_topLevel is not null)
        {
            _topLevel.RemoveHandler(KeyDownEvent, OnTopLevelKeyDown);
            _topLevel.RemoveHandler(GotFocusEvent, OnTopLevelGotFocus);
            _topLevel = null;
        }

        _closeTimer?.Dispose();
        _closeTimer = null;
        _isOpen = false;
        _isClosing = false;
        _isRedirectingFocus = false;
        IsHitTestVisible = false;
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Opens the drawer and restores pointer and keyboard interaction.
    /// </summary>
    public void Open()
    {
        _closeTimer?.Dispose();
        _closeTimer = null;
        _isOpen = true;
        _isClosing = false;
        IsHitTestVisible = true;
        StartBackgroundAutomationIsolation();

        if (_backdrop is not null)
        {
            _backdrop.Opacity = 0;
        }

        if (_translation is not null)
        {
            _translation.X = DrawerWidth;
        }

        Dispatcher.UIThread.Post(BeginOpen, DispatcherPriority.Render);
    }

    /// <summary>
    /// Closes the drawer after its exit transition completes.
    /// </summary>
    public void Close()
    {
        if (!_isOpen || _isClosing)
        {
            return;
        }

        _isClosing = true;

        if (_backdrop is not null)
        {
            _backdrop.Opacity = 0;
        }

        if (_translation is not null)
        {
            _translation.X = DrawerWidth;
        }

        _closeTimer = DispatcherTimer.RunOnce(
            CompleteClose,
            TransitionDuration);
    }

    internal void Reset()
    {
        _closeTimer?.Dispose();
        _closeTimer = null;
        _isOpen = false;
        _isClosing = false;
        IsHitTestVisible = false;
        RestoreBackgroundAutomation();
    }

    private void BeginOpen()
    {
        if (!_isOpen || _isClosing)
        {
            return;
        }

        if (_backdrop is not null)
        {
            _backdrop.Opacity = 1;
        }

        if (_translation is not null)
        {
            _translation.X = 0;
        }

        FocusInitialElement();
    }

    private void CompleteClose()
    {
        _closeTimer = null;
        _isOpen = false;
        _isClosing = false;
        IsHitTestVisible = false;
        RestoreBackgroundAutomation();
        Closed?.Invoke(this, EventArgs.Empty);
    }

    private void FocusInitialElement()
    {
        if (_closeButton is not null)
        {
            _closeButton.Focus();
        }
        else
        {
            Focus();
        }
    }

    private void OnTopLevelKeyDown(object? sender, KeyEventArgs e)
    {
        if (_isOpen && e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
    }

    private void OnTopLevelGotFocus(object? sender, GotFocusEventArgs e)
    {
        if (!_isOpen || _isRedirectingFocus || e.Source is not Visual focusedVisual || Contains(focusedVisual))
        {
            return;
        }

        _isRedirectingFocus = true;
        Dispatcher.UIThread.Post(
            () =>
            {
                if (_isOpen)
                {
                    if (_isClosing)
                    {
                        Focus();
                    }
                    else
                    {
                        FocusInitialElement();
                    }
                }

                _isRedirectingFocus = false;
            },
            DispatcherPriority.Input);
    }

    private bool Contains(Visual visual)
    {
        if (ReferenceEquals(visual, this))
        {
            return true;
        }

        foreach (var ancestor in visual.GetVisualAncestors())
        {
            if (ReferenceEquals(ancestor, this))
            {
                return true;
            }
        }

        return false;
    }

    private void StartBackgroundAutomationIsolation()
    {
        if (_topLevel is null)
        {
            return;
        }

        _visualParentSubscription ??= Visual.VisualParentProperty.Changed.Subscribe(
            new AvaloniaPropertyChangedObserver(OnVisualParentChanged));
        SynchronizeBackgroundAutomationIsolation();
    }

    private void SynchronizeBackgroundAutomationIsolation()
    {
        if (_topLevel is null || !_isOpen)
        {
            return;
        }

        var drawerAncestors = new HashSet<Visual>(this.GetVisualAncestors())
        {
            this,
        };
        foreach (var visual in _topLevel.GetVisualDescendants())
        {
            if (visual is not Control control ||
                drawerAncestors.Contains(control) ||
                Contains(control))
            {
                continue;
            }

            IsolateBackgroundControl(control);
        }
    }

    private void OnVisualParentChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (!_isOpen || change.Sender is not Visual visual)
        {
            return;
        }

        if (IsDrawerVisualOrAncestor(visual))
        {
            RestoreBackgroundSubtree(visual);
            return;
        }

        if (ReferenceEquals(TopLevel.GetTopLevel(visual), _topLevel))
        {
            IsolateBackgroundSubtree(visual);
        }
        else
        {
            RestoreBackgroundSubtree(visual);
        }
    }

    private void IsolateBackgroundSubtree(Visual visual)
    {
        if (visual is Control control)
        {
            IsolateBackgroundControl(control);
        }

        foreach (var descendant in visual.GetVisualDescendants())
        {
            if (descendant is Control descendantControl && !IsDrawerVisualOrAncestor(descendantControl))
            {
                IsolateBackgroundControl(descendantControl);
            }
        }
    }

    private void IsolateBackgroundControl(Control control)
    {
        if (_backgroundAutomationOverrides.ContainsKey(control) ||
            AutomationProperties.GetAccessibilityView(control) == AccessibilityView.Raw)
        {
            return;
        }

        var temporaryOverride = control.SetValue(
            AutomationProperties.AccessibilityViewProperty,
            AccessibilityView.Raw,
            BindingPriority.Animation);
        if (temporaryOverride is not null)
        {
            _backgroundAutomationOverrides.Add(control, temporaryOverride);
        }
    }

    private void RestoreBackgroundSubtree(Visual visual)
    {
        if (visual is Control control)
        {
            RestoreBackgroundControl(control);
        }

        foreach (var descendant in visual.GetVisualDescendants())
        {
            if (descendant is Control descendantControl)
            {
                RestoreBackgroundControl(descendantControl);
            }
        }
    }

    private void RestoreBackgroundControl(Control control)
    {
        if (_backgroundAutomationOverrides.Remove(control, out var temporaryOverride))
        {
            temporaryOverride.Dispose();
        }
    }

    private bool IsDrawerVisualOrAncestor(Visual visual)
    {
        if (Contains(visual))
        {
            return true;
        }

        foreach (var ancestor in this.GetVisualAncestors())
        {
            if (ReferenceEquals(ancestor, visual))
            {
                return true;
            }
        }

        return false;
    }

    private void RestoreBackgroundAutomation()
    {
        _visualParentSubscription?.Dispose();
        _visualParentSubscription = null;
        foreach (var temporaryOverride in _backgroundAutomationOverrides.Values)
        {
            temporaryOverride.Dispose();
        }

        _backgroundAutomationOverrides.Clear();
    }

    private void OnBackdropPressed(object? sender, PointerPressedEventArgs e)
    {
        e.Handled = true;
        Close();
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e) => Close();

    private void DetachTemplateHandlers()
    {
        if (_backdrop is not null)
        {
            _backdrop.PointerPressed -= OnBackdropPressed;
        }

        if (_closeButton is not null)
        {
            _closeButton.Click -= OnCloseClicked;
        }
    }
}
