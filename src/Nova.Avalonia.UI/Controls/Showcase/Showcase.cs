using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// An interactive tutorial/onboarding control that highlights UI elements step-by-step
/// with customizable tooltips and overlays.
/// </summary>
public class Showcase : TemplatedControl
{
    private static readonly TimeSpan TargetResolutionRetryInterval = TimeSpan.FromMilliseconds(250);
    private static readonly TimeSpan AutomationIsolationRefreshInterval = TimeSpan.FromMilliseconds(100);
    private const double TooltipViewportMargin = 16;

    private ShowcaseOverlay? _overlay;
    private ShowcaseTooltip? _tooltip;
    private Border? _modalBlocker;
    private Border? _topBlocker;
    private Border? _leftBlocker;
    private Border? _rightBlocker;
    private Border? _bottomBlocker;
    private Button? _skipButton;
    private Button? _previousButton;
    private Button? _nextButton;
    private readonly ShowcaseTooltipPositioner _positioner = new();
    private readonly ShowcaseAutomationIsolation _automationIsolation = new();
    private readonly ShowcaseTransitionScheduler _transitionScheduler = new();
    private CancellationTokenSource? _animationCts;
    private Animation? _tooltipAnimation;
    private IDisposable? _targetResolutionRetry;
    private IDisposable? _automationIsolationRefresh;
    private TopLevel? _keyboardRoot;
    private bool _isActive;
    private bool _isTrackingLayout;
    private int? _autoScrolledStepIndex;
    private Control? _resolvedTarget;
    private Control? _focusBeforeStart;
    private string? _resolvedTargetKey;
    private Rect? _lastTargetBounds;
    private bool _visualTransitionForward = true;
    private ShowcaseNavigationAction _visualTransitionAction = ShowcaseNavigationAction.Start;

    /// <summary>
    /// Defines the Showcase.Key attached property.
    /// </summary>
    public static readonly AttachedProperty<string?> KeyProperty =
        AvaloniaProperty.RegisterAttached<Showcase, Control, string?>("Key");

    /// <summary>
    /// Defines the Showcase.TooltipTemplate attached property.
    /// Set on a target element to override the tooltip body template when that element is highlighted.
    /// </summary>
    public static readonly AttachedProperty<IDataTemplate?> TooltipTemplateProperty =
        AvaloniaProperty.RegisterAttached<Showcase, Control, IDataTemplate?>("TooltipTemplate");

    /// <summary>
    /// Defines the <see cref="IsActive"/> property.
    /// </summary>
    public static readonly DirectProperty<Showcase, bool> IsActiveProperty =
        AvaloniaProperty.RegisterDirect<Showcase, bool>(nameof(IsActive), o => o.IsActive);

    /// <summary>
    /// Defines the <see cref="OverlayBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> OverlayBrushProperty =
        AvaloniaProperty.Register<Showcase, IBrush?>(
            nameof(OverlayBrush),
            new ImmutableSolidColorBrush(0xB3000000));

    /// <summary>
    /// Defines the <see cref="AnimationDuration"/> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> AnimationDurationProperty =
        AvaloniaProperty.Register<Showcase, TimeSpan>(
            nameof(AnimationDuration),
            TimeSpan.FromMilliseconds(300));

    /// <summary>
    /// Defines the <see cref="Transition"/> property.
    /// </summary>
    public static readonly StyledProperty<IPageTransition?> TransitionProperty =
        AvaloniaProperty.Register<Showcase, IPageTransition?>(nameof(Transition));

    /// <summary>
    /// Defines the <see cref="AutoScrollIntoView"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> AutoScrollIntoViewProperty =
        AvaloniaProperty.Register<Showcase, bool>(nameof(AutoScrollIntoView), true);

    /// <summary>
    /// Defines the <see cref="InteractionMode"/> property.
    /// </summary>
    public static readonly StyledProperty<ShowcaseInteractionMode> InteractionModeProperty =
        AvaloniaProperty.Register<Showcase, ShowcaseInteractionMode>(
            nameof(InteractionMode),
            ShowcaseInteractionMode.Modal);

    static Showcase()
    {
        InteractionModeProperty.Changed.AddClassHandler<Showcase>((x, e) => x.OnInteractionModeChanged(e));
        NextButtonTextProperty.Changed.AddClassHandler<Showcase>((x, _) => x.OnButtonTextChanged());
        FinishButtonTextProperty.Changed.AddClassHandler<Showcase>((x, _) => x.OnButtonTextChanged());
        PreviousButtonTextProperty.Changed.AddClassHandler<Showcase>((x, _) => x.OnButtonTextChanged());
        SkipButtonTextProperty.Changed.AddClassHandler<Showcase>((x, _) => x.OnButtonTextChanged());
    }

    /// <summary>
    /// Gets the Key attached property value.
    /// </summary>
    public static string? GetKey(Control element) => element.GetValue(KeyProperty);

    /// <summary>
    /// Sets the Key attached property value.
    /// </summary>
    public static void SetKey(Control element, string? value) => element.SetValue(KeyProperty, value);

    /// <summary>
    /// Gets the TooltipTemplate attached property value.
    /// </summary>
    public static IDataTemplate? GetTooltipTemplate(Control element) => element.GetValue(TooltipTemplateProperty);

    /// <summary>
    /// Sets the TooltipTemplate attached property value.
    /// </summary>
    public static void SetTooltipTemplate(Control element, IDataTemplate? value) => element.SetValue(TooltipTemplateProperty, value);

    /// <summary>
    /// Gets whether the showcase is currently active.
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        private set => SetAndRaise(IsActiveProperty, ref _isActive, value);
    }

    /// <summary>
    /// Gets or sets the brush used for the overlay background.
    /// </summary>
    public IBrush? OverlayBrush
    {
        get => GetValue(OverlayBrushProperty);
        set => SetValue(OverlayBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the duration of tooltip transition animations.
    /// Set to <see cref="TimeSpan.Zero"/> to disable animations.
    /// </summary>
    public TimeSpan AnimationDuration
    {
        get => GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    /// <summary>
    /// Gets or sets a custom Avalonia page transition for tooltip entrances.
    /// When unset, Showcase uses its built-in fade transition.
    /// </summary>
    public IPageTransition? Transition
    {
        get => GetValue(TransitionProperty);
        set => SetValue(TransitionProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the active step target should be brought into view when the step becomes active.
    /// </summary>
    public bool AutoScrollIntoView
    {
        get => GetValue(AutoScrollIntoViewProperty);
        set => SetValue(AutoScrollIntoViewProperty, value);
    }

    /// <summary>
    /// Gets or sets how the underlying UI remains interactive while the showcase is active.
    /// </summary>
    public ShowcaseInteractionMode InteractionMode
    {
        get => GetValue(InteractionModeProperty);
        set => SetValue(InteractionModeProperty, value);
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        UnsubscribeButtons();

        _overlay = e.NameScope.Find<ShowcaseOverlay>("PART_Overlay");
        _tooltip = e.NameScope.Find<ShowcaseTooltip>("PART_Tooltip");
        _modalBlocker = e.NameScope.Find<Border>("PART_ModalBlocker");
        _topBlocker = e.NameScope.Find<Border>("PART_TopBlocker");
        _leftBlocker = e.NameScope.Find<Border>("PART_LeftBlocker");
        _rightBlocker = e.NameScope.Find<Border>("PART_RightBlocker");
        _bottomBlocker = e.NameScope.Find<Border>("PART_BottomBlocker");

        _skipButton = e.NameScope.Find<Button>("PART_SkipButton");
        _previousButton = e.NameScope.Find<Button>("PART_PreviousButton");
        _nextButton = e.NameScope.Find<Button>("PART_NextButton");

        if (_skipButton != null)
        {
            _skipButton.Click += OnSkipButtonClick;
        }

        if (_previousButton != null)
        {
            _previousButton.Click += OnPreviousButtonClick;
        }

        if (_nextButton != null)
        {
            _nextButton.Click += OnNextButtonClick;
        }

        UpdateVisualState();
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateKeyboardHandling();
        UpdateLayoutTracking();
        UpdateVisualState();
    }

    private void OnInteractionModeChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (IsActive)
        {
            UpdateVisualState();
        }
        else
        {
            ResetInteractionBlockers();
        }
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        CancelAnimation();
        _transitionScheduler.Cancel();
        StopTargetResolutionRetry();
        StopAutomationIsolationRefresh();
        _automationIsolation.Clear();
        UpdateKeyboardHandling(forceDisable: true);
        UpdateLayoutTracking(forceDisable: true);
        ClearResolvedTarget();
        base.OnDetachedFromVisualTree(e);
    }

    /// <inheritdoc />
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        if (IsActive)
        {
            UpdateActiveLayout();
        }
    }

    private void OnTopLevelKeyDown(object? sender, KeyEventArgs e)
    {
        if (!IsActive)
        {
            return;
        }

        switch (e.Key)
        {
            case Key.Right:
                if (ShouldHandleDirectionalShortcut(e.Source))
                {
                    Next();
                    e.Handled = true;
                }

                break;

            case Key.Left:
                if (ShouldHandleDirectionalShortcut(e.Source))
                {
                    Previous();
                    e.Handled = true;
                }

                break;

            case Key.Space:
            case Key.Enter:
                if (ShouldHandleActivationShortcut(e.Source))
                {
                    Next();
                    e.Handled = true;
                }

                break;

            case Key.Escape:
                Skip();
                e.Handled = true;
                break;

            case Key.Tab:
                HandleTabFocusTrap(e);
                break;
        }
    }

    private bool ShouldHandleDirectionalShortcut(object? source) =>
        source is Showcase ||
        ReferenceEquals(source, _tooltip) ||
        ReferenceEquals(source, _skipButton) ||
        ReferenceEquals(source, _previousButton) ||
        ReferenceEquals(source, _nextButton);

    private bool ShouldHandleActivationShortcut(object? source) =>
        source is Showcase || ReferenceEquals(source, _tooltip);

    private void HandleTabFocusTrap(KeyEventArgs e)
    {
        var effectiveMode = CurrentStep?.InteractionMode ?? InteractionMode;
        if (effectiveMode == ShowcaseInteractionMode.Passthrough || _tooltip == null)
        {
            return;
        }

        var focusableControls = GetFocusableControls(effectiveMode);

        if (focusableControls.Count == 0)
        {
            Focus();
            e.Handled = true;
            return;
        }

        var current = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() as Control;
        var currentIndex = current == null ? -1 : focusableControls.IndexOf(current);
        var backward = (e.KeyModifiers & KeyModifiers.Shift) != 0;

        int nextIndex;
        if (currentIndex < 0)
        {
            nextIndex = backward ? focusableControls.Count - 1 : 0;
        }
        else
        {
            nextIndex = backward ? currentIndex - 1 : currentIndex + 1;
            if (nextIndex < 0)
            {
                nextIndex = focusableControls.Count - 1;
            }
            else if (nextIndex >= focusableControls.Count)
            {
                nextIndex = 0;
            }
        }

        focusableControls[nextIndex].Focus(NavigationMethod.Tab, e.KeyModifiers);
        e.Handled = true;
    }

    private List<Control> GetFocusableControls(ShowcaseInteractionMode interactionMode)
    {
        var controls = new List<Control>();
        AddFocusableControls(_tooltip, controls);

        if (interactionMode == ShowcaseInteractionMode.TargetOnly)
        {
            AddFocusableControls(_resolvedTarget, controls);
        }

        return controls
            .Distinct()
            .OrderBy(x => x.TabIndex)
            .ToList();
    }

    private static void AddFocusableControls(Control? root, ICollection<Control> controls)
    {
        if (root == null)
        {
            return;
        }

        if (IsFocusableControl(root))
        {
            controls.Add(root);
        }

        foreach (var control in root.GetVisualDescendants().OfType<Control>())
        {
            if (IsFocusableControl(control))
            {
                controls.Add(control);
            }
        }
    }

    private static bool IsFocusableControl(Control control) =>
        CanReceiveFocus(control) && control.IsTabStop;

    private static bool CanReceiveFocus(Control control) =>
        control.Focusable &&
        control.IsEffectivelyEnabled &&
        control.IsEffectivelyVisible &&
        control.IsAttachedToVisualTree();

    private void UpdateKeyboardHandling(bool forceDisable = false)
    {
        var keyboardRoot = !forceDisable && IsActive ? TopLevel.GetTopLevel(this) : null;
        if (ReferenceEquals(_keyboardRoot, keyboardRoot))
        {
            return;
        }

        _keyboardRoot?.RemoveHandler(InputElement.KeyDownEvent, OnTopLevelKeyDown);
        _keyboardRoot = keyboardRoot;
        _keyboardRoot?.AddHandler(
            InputElement.KeyDownEvent,
            OnTopLevelKeyDown,
            RoutingStrategies.Tunnel,
            handledEventsToo: true);
    }

    /// <inheritdoc />
    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new ShowcaseAutomationPeer(this);
    }

    private void OnSkipButtonClick(object? sender, RoutedEventArgs e) => Skip();
    private void OnPreviousButtonClick(object? sender, RoutedEventArgs e) => Previous();
    private void OnNextButtonClick(object? sender, RoutedEventArgs e) => Next();

    private void UnsubscribeButtons()
    {
        if (_skipButton != null)
        {
            _skipButton.Click -= OnSkipButtonClick;
        }

        if (_previousButton != null)
        {
            _previousButton.Click -= OnPreviousButtonClick;
        }

        if (_nextButton != null)
        {
            _nextButton.Click -= OnNextButtonClick;
        }
    }

    private void OnVisualStepChanged(bool forward, ShowcaseNavigationAction action)
    {
        _autoScrolledStepIndex = null;
        StopTargetResolutionRetry();
        ClearResolvedTarget();
        UpdateVisualState(animate: true, forward: forward, action: action);
    }

    private void OnFlowActiveStateChanged()
    {
        if (IsActive)
        {
            _focusBeforeStart = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() as Control;
            Focusable = true;
        }
        else
        {
            _autoScrolledStepIndex = null;
            StopTargetResolutionRetry();
            StopAutomationIsolationRefresh();
            ClearResolvedTarget();
        }

        UpdateKeyboardHandling();
        UpdateLayoutTracking();
        UpdateVisualState(
            animate: IsActive,
            forward: _visualTransitionForward,
            action: _visualTransitionAction);

        if (IsActive)
        {
            (_nextButton as Control ?? this).Focus();
        }
        else
        {
            if (_focusBeforeStart is { } previousFocus && CanReceiveFocus(previousFocus))
            {
                previousFocus.Focus();
            }
            else
            {
                FindFallbackFocusTarget()?.Focus();
            }

            _focusBeforeStart = null;
        }
    }

    private Control? FindFallbackFocusTarget()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        return topLevel?
            .GetVisualDescendants()
            .OfType<Control>()
            .FirstOrDefault(control =>
                !ReferenceEquals(control, this) &&
                !control.GetVisualAncestors().Contains(this) &&
                IsFocusableControl(control));
    }

    private void OnStepsChanged()
    {
        _autoScrolledStepIndex = null;
        StopTargetResolutionRetry();
        ClearResolvedTarget();

        if (IsActive)
        {
            UpdateVisualState();
        }
    }

    private void OnLayoutUpdated(object? sender, EventArgs e)
    {
        if (!IsActive)
        {
            return;
        }

        ScheduleAutomationIsolationRefresh();

        // Avoid a full visual state rebuild when the target bounds haven't changed.
        var currentStep = CurrentStep;
        if (currentStep != null && _resolvedTarget != null)
        {
            if (TryGetBoundsRelativeToThis(_resolvedTarget, out var currentBounds))
            {
                if (_lastTargetBounds.HasValue && _lastTargetBounds.Value == currentBounds)
                {
                    return;
                }

                UpdateTargetLayout(currentStep, currentBounds);
                return;
            }

            // Clear stale highlight/input geometry immediately when an existing
            // target leaves the tree. Subsequent retries are throttled below.
            UpdateVisualState();
            return;
        }

        ScheduleTargetResolutionRetry();
    }

    private Control? ResolveTarget(string key)
    {
        var root = this.GetVisualRoot() as Visual;
        if (root == null)
        {
            ClearResolvedTarget();
            return null;
        }

        if (_resolvedTarget != null &&
            _resolvedTargetKey == key &&
            ReferenceEquals(_resolvedTarget.GetVisualRoot(), root) &&
            GetKey(_resolvedTarget) == key)
        {
            return _resolvedTarget;
        }

        Control? match = null;
        var duplicate = false;

        foreach (var visual in root.GetSelfAndVisualDescendants().OfType<Control>())
        {
            var k = GetKey(visual);
            if (k != key)
            {
                continue;
            }

            if (match != null)
            {
                duplicate = true;
                break;
            }

            match = visual;
        }

        if (duplicate)
        {
            ClearResolvedTarget();
            return null;
        }

        _resolvedTarget = match;
        _resolvedTargetKey = match != null ? key : null;

        if (match != null)
        {
            StopTargetResolutionRetry();
        }

        return match;
    }

    /// <summary>
    /// Validates the current showcase configuration and target resolution state.
    /// </summary>
    public ShowcaseValidationResult Validate()
    {
        var issues = new List<ShowcaseValidationIssue>();
        if (Steps.Count == 0)
        {
            issues.Add(new ShowcaseValidationIssue(
                ShowcaseValidationIssueCode.NoSteps,
                ShowcaseValidationSeverity.Error,
                "The showcase does not define any steps."));
        }

        for (var i = 0; i < Steps.Count; i++)
        {
            var step = Steps[i];
            if (string.IsNullOrWhiteSpace(step.Key))
            {
                issues.Add(new ShowcaseValidationIssue(
                    ShowcaseValidationIssueCode.EmptyStepKey,
                    ShowcaseValidationSeverity.Error,
                    $"Step {i + 1} is missing a Showcase.Key target.",
                    i));
            }

            if (string.IsNullOrWhiteSpace(step.Title) &&
                string.IsNullOrWhiteSpace(step.Description) &&
                step.TooltipTemplate == null)
            {
                issues.Add(new ShowcaseValidationIssue(
                    ShowcaseValidationIssueCode.EmptyStepContent,
                    ShowcaseValidationSeverity.Warning,
                    $"Step {i + 1} has no title, description, or tooltip template.",
                    i,
                    string.IsNullOrWhiteSpace(step.Key) ? null : step.Key));
            }
        }

        if (CanPersistProgress())
        {
            var ambiguousIdentities = Steps
                .Select((step, index) => new { Identity = GetStepIdentity(step), Index = index })
                .Where(x => !string.IsNullOrWhiteSpace(x.Identity))
                .GroupBy(x => x.Identity!, StringComparer.Ordinal)
                .Where(x => x.Count() > 1);

            foreach (var group in ambiguousIdentities)
            {
                issues.Add(new ShowcaseValidationIssue(
                    ShowcaseValidationIssueCode.AmbiguousStepIdentity,
                    ShowcaseValidationSeverity.Warning,
                    $"Multiple steps use the persisted identity '{group.Key}'. Assign a unique ShowcaseStep.Id to each repeated target.",
                    group.First().Index,
                    group.Key));
            }
        }

        var root = this.GetVisualRoot() as Visual;
        if (root == null)
        {
            issues.Add(new ShowcaseValidationIssue(
                ShowcaseValidationIssueCode.VisualRootUnavailable,
                ShowcaseValidationSeverity.Warning,
                "Target resolution checks were skipped because the showcase is not attached to a visual root."));
            return new ShowcaseValidationResult(issues);
        }

        var targetsByKey = root
            .GetSelfAndVisualDescendants()
            .OfType<Control>()
            .Select(x => new { Control = x, Key = GetKey(x) })
            .Where(x => !string.IsNullOrWhiteSpace(x.Key))
            .GroupBy(x => x.Key!)
            .ToDictionary(x => x.Key, x => x.Select(y => y.Control).ToList());

        foreach (var pair in targetsByKey.Where(x => x.Value.Count > 1))
        {
            issues.Add(new ShowcaseValidationIssue(
                ShowcaseValidationIssueCode.DuplicateTargetKey,
                ShowcaseValidationSeverity.Error,
                $"Multiple controls with Showcase.Key '{pair.Key}' were found in the current visual root.",
                key: pair.Key));
        }

        for (var i = 0; i < Steps.Count; i++)
        {
            var step = Steps[i];
            if (string.IsNullOrWhiteSpace(step.Key))
            {
                continue;
            }

            if (!targetsByKey.TryGetValue(step.Key, out var matches))
            {
                issues.Add(new ShowcaseValidationIssue(
                    ShowcaseValidationIssueCode.MissingTarget,
                    ShowcaseValidationSeverity.Warning,
                    $"Step {i + 1} targets Showcase.Key '{step.Key}', but no matching control is currently available.",
                    i,
                    step.Key));
                continue;
            }

            if (matches.Count != 1)
            {
                continue;
            }

            if (!TryGetBoundsRelativeToThis(matches[0], out _))
            {
                issues.Add(new ShowcaseValidationIssue(
                    ShowcaseValidationIssueCode.TargetUnavailable,
                    ShowcaseValidationSeverity.Warning,
                    $"Step {i + 1} targets Showcase.Key '{step.Key}', but the control is not currently visible or laid out.",
                    i,
                    step.Key));
            }
        }

        return new ShowcaseValidationResult(issues);
    }

    private void UpdateVisualState(
        bool animate = false,
        bool forward = true,
        ShowcaseNavigationAction action = ShowcaseNavigationAction.Next)
    {
        if (_overlay == null || _tooltip == null)
        {
            return;
        }

        var currentStep = CurrentStep;
        if (!IsActive || currentStep == null)
        {
            _autoScrolledStepIndex = null;
            _lastTargetBounds = null;
            StopTargetResolutionRetry();
            ClearResolvedTarget();
            _overlay.TargetBounds = null;
            ResetInteractionBlockers();
            _tooltip.IsVisible = false;
            _tooltip.Content = null;
            _tooltip.ContentTemplate = null;
            CancelAnimation();
            _automationIsolation.Clear();
            AutomationProperties.SetName(this, "Interactive Tutorial");
            AutomationProperties.SetName(_tooltip, "Interactive Tutorial");
            AutomationProperties.SetHelpText(_tooltip, string.Empty);
            return;
        }

        var automationName = BuildAutomationName(currentStep);
        AutomationProperties.SetName(this, automationName);
        AutomationProperties.SetName(_tooltip, automationName);
        AutomationProperties.SetHelpText(_tooltip, currentStep.Description);

        _overlay.HighlightShape = currentStep.HighlightShape;
        _overlay.HighlightPadding = currentStep.HighlightPadding;
        _overlay.HighlightCornerRadius = currentStep.HighlightCornerRadius;

        _tooltip.Title = currentStep.Title;
        _tooltip.Description = currentStep.Description;
        _tooltip.IsVisible = true;
        var effectiveInteractionMode = currentStep.InteractionMode ?? InteractionMode;

        if (_skipButton != null)
        {
            _skipButton.Content = SkipButtonText;
        }

        if (_previousButton != null)
        {
            _previousButton.IsVisible = CanGoPrevious;
            _previousButton.Content = PreviousButtonText;
        }

        if (_nextButton != null)
        {
            _nextButton.Content = CurrentButtonText;
        }

        var target = ResolveTarget(currentStep.Key);
        StopAutomationIsolationRefresh();
        _automationIsolation.Update(this, effectiveInteractionMode, target);
        if (target != null &&
            AutoScrollIntoView &&
            _autoScrolledStepIndex != CurrentIndex)
        {
            target.BringIntoView();
            _autoScrolledStepIndex = CurrentIndex;
        }

        ConfigureTooltipTemplate(currentStep, target);

        if (target == null || !TryGetBoundsRelativeToThis(target, out var targetBounds))
        {
            ScheduleTargetResolutionRetry();
            _lastTargetBounds = null;
            _overlay.TargetBounds = null;
            UpdateInteractionBlockers(effectiveInteractionMode, null);
            PositionTooltipCentered();

            if (animate)
            {
                AnimateTooltipIn(forward, action);
            }

            EnsureFocusWithinActiveScope(effectiveInteractionMode);
            return;
        }

        UpdateTargetLayout(currentStep, targetBounds);

        if (animate)
        {
            AnimateTooltipIn(forward, action);
        }

        EnsureFocusWithinActiveScope(effectiveInteractionMode);
    }

    private void UpdateActiveLayout()
    {
        if (_overlay == null || _tooltip == null || CurrentStep == null)
        {
            return;
        }

        if (_resolvedTarget != null && TryGetBoundsRelativeToThis(_resolvedTarget, out var targetBounds))
        {
            UpdateTargetLayout(CurrentStep, targetBounds);
            return;
        }

        _lastTargetBounds = null;
        _overlay.TargetBounds = null;
        UpdateInteractionBlockers(CurrentStep.InteractionMode ?? InteractionMode, null);
        PositionTooltipCentered();
    }

    private void EnsureFocusWithinActiveScope(ShowcaseInteractionMode interactionMode)
    {
        if (!IsActive || interactionMode == ShowcaseInteractionMode.Passthrough)
        {
            return;
        }

        var focusableControls = GetFocusableControls(interactionMode);
        var current = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() as Control;
        if (current != null && focusableControls.Contains(current))
        {
            return;
        }

        var preferred = _nextButton != null && IsFocusableControl(_nextButton)
            ? _nextButton
            : focusableControls.FirstOrDefault();

        (preferred as Control ?? this).Focus();
    }

    private string BuildAutomationName(ShowcaseStep step)
    {
        var title = string.IsNullOrWhiteSpace(step.Title) ? "Interactive Tutorial" : step.Title.Trim();
        return $"{title}. Step {CurrentIndex + 1} of {Steps.Count}";
    }

    private void UpdateTargetLayout(ShowcaseStep step, Rect targetBounds)
    {
        if (_overlay == null)
        {
            return;
        }

        _lastTargetBounds = targetBounds;
        _overlay.TargetBounds = targetBounds;

        var highlightBounds = targetBounds.Inflate(SanitizeHighlightPadding(step.HighlightPadding));
        var interactionMode = step.InteractionMode ?? InteractionMode;
        UpdateInteractionBlockers(interactionMode, targetBounds);
        PositionTooltip(highlightBounds, step.TooltipPosition);
    }

    private bool TryGetBoundsRelativeToThis(Control target, out Rect bounds)
    {
        bounds = default;

        if (!target.IsEffectivelyVisible)
        {
            return false;
        }

        var transform = target.TransformToVisual(this);
        if (transform is null)
        {
            return false;
        }

        var size = target.Bounds.Size;
        if (size.Width <= 0 || size.Height <= 0)
        {
            return false;
        }

        var topLeft = transform.Value.Transform(default);
        var topRight = transform.Value.Transform(new Point(size.Width, 0));
        var bottomLeft = transform.Value.Transform(new Point(0, size.Height));
        var bottomRight = transform.Value.Transform(new Point(size.Width, size.Height));
        var left = Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X));
        var top = Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y));
        var right = Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X));
        var bottom = Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y));

        bounds = new Rect(left, top, right - left, bottom - top);
        return true;
    }

    private void PositionTooltip(Rect targetBounds, ShowcaseTooltipPosition preferredPosition)
    {
        if (_tooltip == null)
        {
            return;
        }

        ConstrainTooltipToViewport();
        _tooltip.Measure(Bounds.Size);
        var tooltipSize = _tooltip.DesiredSize;
        var containerBounds = new Rect(0, 0, Bounds.Width, Bounds.Height);

        var position = _positioner.CalculatePosition(
            targetBounds,
            tooltipSize,
            containerBounds,
            preferredPosition);

        var clampedX = Math.Clamp(position.X, 0, Math.Max(0, containerBounds.Width - tooltipSize.Width));
        var clampedY = Math.Clamp(position.Y, 0, Math.Max(0, containerBounds.Height - tooltipSize.Height));

        Canvas.SetLeft(_tooltip, clampedX);
        Canvas.SetTop(_tooltip, clampedY);
    }

    private void PositionTooltipCentered()
    {
        if (_tooltip == null)
        {
            return;
        }

        ConstrainTooltipToViewport();
        _tooltip.Measure(Bounds.Size);
        var tooltipSize = _tooltip.DesiredSize;
        var position = new Point(
            Math.Max(0, (Bounds.Width - tooltipSize.Width) / 2),
            Math.Max(0, (Bounds.Height - tooltipSize.Height) / 2));

        Canvas.SetLeft(_tooltip, position.X);
        Canvas.SetTop(_tooltip, position.Y);
    }

    private void ConstrainTooltipToViewport()
    {
        if (_tooltip == null)
        {
            return;
        }

        _tooltip.MaxHeight = Math.Max(0, Bounds.Height - (TooltipViewportMargin * 2));
    }

    private void ConfigureTooltipTemplate(ShowcaseStep currentStep, Control? target)
    {
        if (_tooltip == null)
        {
            return;
        }

        var tooltipTemplate = target != null
            ? GetTooltipTemplate(target) ?? currentStep.TooltipTemplate
            : currentStep.TooltipTemplate;

        if (tooltipTemplate != null)
        {
            _tooltip.Content = currentStep;
            _tooltip.ContentTemplate = tooltipTemplate;
            return;
        }

        _tooltip.Content = null;
        _tooltip.ContentTemplate = null;
    }

    private void UpdateInteractionBlockers(ShowcaseInteractionMode interactionMode, Rect? highlightBounds)
    {
        if (_modalBlocker == null ||
            _topBlocker == null ||
            _leftBlocker == null ||
            _rightBlocker == null ||
            _bottomBlocker == null)
        {
            return;
        }

        if (Bounds.Width <= 0 || Bounds.Height <= 0)
        {
            ResetInteractionBlockers();
            return;
        }

        switch (interactionMode)
        {
            case ShowcaseInteractionMode.Passthrough:
                ResetInteractionBlockers();
                return;

            case ShowcaseInteractionMode.Modal:
                ShowModalBlocker();
                return;

            case ShowcaseInteractionMode.TargetOnly:
                if (highlightBounds is null)
                {
                    ShowModalBlocker();
                    return;
                }

                ShowTargetOnlyBlockers(highlightBounds.Value);
                return;
        }
    }

    private void ShowModalBlocker()
    {
        if (_modalBlocker == null)
        {
            return;
        }

        _modalBlocker.IsVisible = true;
        _modalBlocker.Width = Bounds.Width;
        _modalBlocker.Height = Bounds.Height;
        Canvas.SetLeft(_modalBlocker, 0);
        Canvas.SetTop(_modalBlocker, 0);

        SetBlockerBounds(_topBlocker, default);
        SetBlockerBounds(_leftBlocker, default);
        SetBlockerBounds(_rightBlocker, default);
        SetBlockerBounds(_bottomBlocker, default);
    }

    private void ShowTargetOnlyBlockers(Rect highlightBounds)
    {
        if (_modalBlocker == null)
        {
            return;
        }

        var left = Math.Clamp(highlightBounds.Left, 0, Bounds.Width);
        var top = Math.Clamp(highlightBounds.Top, 0, Bounds.Height);
        var right = Math.Clamp(highlightBounds.Right, 0, Bounds.Width);
        var bottom = Math.Clamp(highlightBounds.Bottom, 0, Bounds.Height);

        if (right <= left || bottom <= top)
        {
            ShowModalBlocker();
            return;
        }

        _modalBlocker.IsVisible = false;
        SetBlockerBounds(_topBlocker, new Rect(0, 0, Bounds.Width, top));
        SetBlockerBounds(_leftBlocker, new Rect(0, top, left, bottom - top));
        SetBlockerBounds(_rightBlocker, new Rect(right, top, Bounds.Width - right, bottom - top));
        SetBlockerBounds(_bottomBlocker, new Rect(0, bottom, Bounds.Width, Bounds.Height - bottom));
    }

    private static void SetBlockerBounds(Border? blocker, Rect rect)
    {
        if (blocker == null)
        {
            return;
        }

        var width = Math.Max(0, rect.Width);
        var height = Math.Max(0, rect.Height);
        blocker.IsVisible = width > 0 && height > 0;
        blocker.Width = width;
        blocker.Height = height;
        Canvas.SetLeft(blocker, rect.X);
        Canvas.SetTop(blocker, rect.Y);
    }

    private static Thickness SanitizeHighlightPadding(Thickness padding) =>
        new(
            SanitizeLength(padding.Left),
            SanitizeLength(padding.Top),
            SanitizeLength(padding.Right),
            SanitizeLength(padding.Bottom));

    private static double SanitizeLength(double value) =>
        double.IsFinite(value) ? Math.Max(0, value) : 0;

    private void ResetInteractionBlockers()
    {
        if (_modalBlocker != null)
        {
            _modalBlocker.IsVisible = false;
        }

        SetBlockerBounds(_topBlocker, default);
        SetBlockerBounds(_leftBlocker, default);
        SetBlockerBounds(_rightBlocker, default);
        SetBlockerBounds(_bottomBlocker, default);
    }

    private void UpdateLayoutTracking(bool forceDisable = false)
    {
        var shouldTrack = !forceDisable && this.GetVisualRoot() != null && IsActive;
        if (_isTrackingLayout == shouldTrack)
        {
            return;
        }

        if (shouldTrack)
        {
            LayoutUpdated += OnLayoutUpdated;
        }
        else
        {
            LayoutUpdated -= OnLayoutUpdated;
        }

        _isTrackingLayout = shouldTrack;
    }

    private void ClearResolvedTarget()
    {
        _resolvedTarget = null;
        _resolvedTargetKey = null;
    }

    private void ScheduleTargetResolutionRetry()
    {
        if (_targetResolutionRetry != null || !IsActive || !this.IsAttachedToVisualTree())
        {
            return;
        }

        _targetResolutionRetry = DispatcherTimer.RunOnce(
            () =>
            {
                _targetResolutionRetry = null;

                if (IsActive && this.IsAttachedToVisualTree())
                {
                    UpdateVisualState();
                }
            },
            TargetResolutionRetryInterval,
            DispatcherPriority.Background);
    }

    private void StopTargetResolutionRetry()
    {
        _targetResolutionRetry?.Dispose();
        _targetResolutionRetry = null;
    }

    private void ScheduleAutomationIsolationRefresh()
    {
        var interactionMode = CurrentStep?.InteractionMode ?? InteractionMode;
        if (_automationIsolationRefresh != null ||
            !IsActive ||
            interactionMode == ShowcaseInteractionMode.Passthrough ||
            !this.IsAttachedToVisualTree())
        {
            return;
        }

        _automationIsolationRefresh = DispatcherTimer.RunOnce(
            () =>
            {
                _automationIsolationRefresh = null;

                if (IsActive && this.IsAttachedToVisualTree())
                {
                    _automationIsolation.Refresh();
                }
            },
            AutomationIsolationRefreshInterval,
            DispatcherPriority.Background);
    }

    private void StopAutomationIsolationRefresh()
    {
        _automationIsolationRefresh?.Dispose();
        _automationIsolationRefresh = null;
    }

    private void CancelAnimation()
    {
        _animationCts?.Cancel();
        _animationCts?.Dispose();
        _animationCts = null;
    }

    private Animation GetOrCreateTooltipAnimation(TimeSpan duration)
    {
        if (_tooltipAnimation == null)
        {
            _tooltipAnimation = new Animation
            {
                Easing = new CubicEaseOut(),
                Children =
                {
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter(OpacityProperty, 0d)
                        },
                        Cue = new Cue(0)
                    },
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter(OpacityProperty, 1d)
                        },
                        Cue = new Cue(1)
                    }
                }
            };
        }

        _tooltipAnimation.Duration = duration;
        return _tooltipAnimation;
    }

    private async void AnimateTooltipIn(bool forward, ShowcaseNavigationAction action)
    {
        if (_tooltip == null)
        {
            return;
        }

        var duration = AnimationDuration;
        if (duration <= TimeSpan.Zero)
        {
            CancelAnimation();
            _tooltip.Opacity = 1;
            return;
        }

        CancelAnimation();
        var cts = new CancellationTokenSource();
        _animationCts = cts;

        try
        {
            if (Transition is { } transition)
            {
                await transition.Start(null, _tooltip, forward, cts.Token);
            }
            else
            {
                await GetOrCreateTooltipAnimation(duration).RunAsync(_tooltip, cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during rapid step changes
        }
        catch (Exception ex)
        {
            if (ReferenceEquals(_animationCts, cts))
            {
                _tooltip.IsVisible = true;
                _tooltip.Opacity = 1;
                TransitionFailed?.Invoke(this, new ShowcaseTransitionFailedEventArgs(action, ex));
            }
        }
        finally
        {
            if (ReferenceEquals(_animationCts, cts))
            {
                _tooltip.IsVisible = IsActive;
                _tooltip.Opacity = 1;
                _animationCts = null;
                cts.Dispose();
            }
        }
    }
    private int _currentIndex = -1;
    private ShowcaseStep? _currentStep;
    private bool _canGoPrevious;
    private bool _canGoNext;
    private string _currentButtonText = "Finish";

    /// <summary>
    /// Defines the <see cref="CurrentStep"/> property.
    /// </summary>
    public static readonly DirectProperty<Showcase, ShowcaseStep?> CurrentStepProperty =
        AvaloniaProperty.RegisterDirect<Showcase, ShowcaseStep?>(nameof(CurrentStep), o => o.CurrentStep);

    /// <summary>
    /// Defines the <see cref="CurrentIndex"/> property.
    /// </summary>
    public static readonly DirectProperty<Showcase, int> CurrentIndexProperty =
        AvaloniaProperty.RegisterDirect<Showcase, int>(nameof(CurrentIndex), o => o.CurrentIndex);

    /// <summary>
    /// Defines the <see cref="CanGoPrevious"/> property.
    /// </summary>
    public static readonly DirectProperty<Showcase, bool> CanGoPreviousProperty =
        AvaloniaProperty.RegisterDirect<Showcase, bool>(nameof(CanGoPrevious), o => o.CanGoPrevious);

    /// <summary>
    /// Defines the <see cref="CanGoNext"/> property.
    /// </summary>
    public static readonly DirectProperty<Showcase, bool> CanGoNextProperty =
        AvaloniaProperty.RegisterDirect<Showcase, bool>(nameof(CanGoNext), o => o.CanGoNext);

    /// <summary>
    /// Defines the <see cref="CurrentButtonText"/> property.
    /// </summary>
    public static readonly DirectProperty<Showcase, string> CurrentButtonTextProperty =
        AvaloniaProperty.RegisterDirect<Showcase, string>(nameof(CurrentButtonText), o => o.CurrentButtonText);

    /// <summary>
    /// Defines the <see cref="NextButtonText"/> property.
    /// </summary>
    public static readonly StyledProperty<string> NextButtonTextProperty =
        AvaloniaProperty.Register<Showcase, string>(nameof(NextButtonText), "Next");

    /// <summary>
    /// Defines the <see cref="FinishButtonText"/> property.
    /// </summary>
    public static readonly StyledProperty<string> FinishButtonTextProperty =
        AvaloniaProperty.Register<Showcase, string>(nameof(FinishButtonText), "Finish");

    /// <summary>
    /// Defines the <see cref="PreviousButtonText"/> property.
    /// </summary>
    public static readonly StyledProperty<string> PreviousButtonTextProperty =
        AvaloniaProperty.Register<Showcase, string>(nameof(PreviousButtonText), "Previous");

    /// <summary>
    /// Defines the <see cref="SkipButtonText"/> property.
    /// </summary>
    public static readonly StyledProperty<string> SkipButtonTextProperty =
        AvaloniaProperty.Register<Showcase, string>(nameof(SkipButtonText), "Skip");

    /// <summary>
    /// Defines the <see cref="ProgressStore"/> property.
    /// </summary>
    public static readonly StyledProperty<IShowcaseProgressStore?> ProgressStoreProperty =
        AvaloniaProperty.Register<Showcase, IShowcaseProgressStore?>(nameof(ProgressStore));

    /// <summary>
    /// Defines the <see cref="PersistenceKey"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> PersistenceKeyProperty =
        AvaloniaProperty.Register<Showcase, string?>(nameof(PersistenceKey));

    /// <summary>
    /// Gets the collection of showcase steps.
    /// </summary>
    public ObservableCollection<ShowcaseStep> Steps { get; } = new();

    /// <summary>
    /// Gets the current step being displayed, or null if not active.
    /// </summary>
    public ShowcaseStep? CurrentStep => _currentStep;

    /// <summary>
    /// Gets the index of the current step.
    /// </summary>
    public int CurrentIndex => _currentIndex;

    /// <summary>
    /// Gets whether the previous step is available.
    /// </summary>
    public bool CanGoPrevious => _canGoPrevious;

    /// <summary>
    /// Gets whether the next step is available.
    /// </summary>
    public bool CanGoNext => _canGoNext;

    /// <summary>
    /// Gets the text for the next/finish button based on step position.
    /// </summary>
    public string CurrentButtonText => _currentButtonText;

    /// <summary>
    /// Gets or sets the text for the Next button.
    /// </summary>
    public string NextButtonText
    {
        get => GetValue(NextButtonTextProperty);
        set => SetValue(NextButtonTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the text for the Finish button (shown on the last step).
    /// </summary>
    public string FinishButtonText
    {
        get => GetValue(FinishButtonTextProperty);
        set => SetValue(FinishButtonTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the text for the Previous button.
    /// </summary>
    public string PreviousButtonText
    {
        get => GetValue(PreviousButtonTextProperty);
        set => SetValue(PreviousButtonTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the text for the Skip button.
    /// </summary>
    public string SkipButtonText
    {
        get => GetValue(SkipButtonTextProperty);
        set => SetValue(SkipButtonTextProperty, value);
    }

    /// <summary>
    /// Gets or sets an async hook that runs before a new step becomes active.
    /// </summary>
    public Func<ShowcaseStepTransitionContext, CancellationToken, Task>? BeforeStepAsync { get; set; }

    /// <summary>
    /// Gets or sets an async hook that runs after a new step becomes active.
    /// </summary>
    public Func<ShowcaseStepTransitionContext, CancellationToken, Task>? AfterStepAsync { get; set; }

    /// <summary>
    /// Gets or sets the store used to persist showcase progress.
    /// </summary>
    public IShowcaseProgressStore? ProgressStore
    {
        get => GetValue(ProgressStoreProperty);
        set => SetValue(ProgressStoreProperty, value);
    }

    /// <summary>
    /// Gets or sets the persistence key used with <see cref="ProgressStore"/>.
    /// </summary>
    public string? PersistenceKey
    {
        get => GetValue(PersistenceKeyProperty);
        set => SetValue(PersistenceKeyProperty, value);
    }

    private readonly ShowcaseRelayCommand _nextCommand;
    private readonly ShowcaseRelayCommand _previousCommand;
    private readonly ShowcaseRelayCommand _skipCommand;

    /// <summary>
    /// Command to advance to the next step.
    /// </summary>
    public ICommand NextCommand => _nextCommand;

    /// <summary>
    /// Command to go back to the previous step.
    /// </summary>
    public ICommand PreviousCommand => _previousCommand;

    /// <summary>
    /// Command to skip/cancel the showcase.
    /// </summary>
    public ICommand SkipCommand => _skipCommand;

    /// <summary>
    /// Raised when the showcase starts.
    /// </summary>
    public event EventHandler? Started;

    /// <summary>
    /// Raised when the showcase resumes from persisted progress.
    /// </summary>
    public event EventHandler? Resumed;

    /// <summary>
    /// Raised when the showcase completes (all steps finished).
    /// </summary>
    public event EventHandler? Completed;

    /// <summary>
    /// Raised when the showcase is skipped/cancelled.
    /// </summary>
    public event EventHandler? Skipped;

    /// <summary>
    /// Raised when the current step changes.
    /// </summary>
    public event EventHandler<ShowcaseStepChangedEventArgs>? StepChanged;

    /// <summary>
    /// Raised when a synchronous navigation wrapper or visual transition fails.
    /// </summary>
    public event EventHandler<ShowcaseTransitionFailedEventArgs>? TransitionFailed;

    /// <summary>
    /// Creates a new showcase.
    /// </summary>
    public Showcase()
    {
        _nextCommand = new ShowcaseRelayCommand(Next, () => IsActive);
        _previousCommand = new ShowcaseRelayCommand(Previous, () => CanGoPrevious && IsActive);
        _skipCommand = new ShowcaseRelayCommand(Skip, () => IsActive);
        Steps.CollectionChanged += OnStepsCollectionChanged;
    }

    /// <summary>
    /// Starts the showcase from the first step.
    /// If already active, resets and restarts from the beginning.
    /// </summary>
    public void Start() => TryStart().EnsureStarted();

    /// <summary>
    /// Attempts to validate and start the showcase.
    /// </summary>
    public ShowcaseStartResult TryStart()
    {
        var validation = Validate();
        if (!validation.IsValid)
        {
            return new ShowcaseStartResult(false, validation);
        }

        FireAndForget(StartFlowAsync(), ShowcaseNavigationAction.Start);
        return new ShowcaseStartResult(true, validation);
    }

    /// <summary>
    /// Starts the showcase from the first step.
    /// If already active, resets and restarts from the beginning.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        var result = await TryStartAsync(cancellationToken);
        result.EnsureStarted();
    }

    /// <summary>
    /// Attempts to validate and start the showcase asynchronously.
    /// </summary>
    public async Task<ShowcaseStartResult> TryStartAsync(CancellationToken cancellationToken = default)
    {
        var validation = Validate();
        if (!validation.IsValid)
        {
            return new ShowcaseStartResult(false, validation);
        }

        await StartFlowAsync(cancellationToken);
        return new ShowcaseStartResult(IsActive, validation);
    }

    private Task StartFlowAsync(CancellationToken cancellationToken = default) =>
        _transitionScheduler.RunAsync(StartCoreAsync, cancellationToken);

    /// <summary>
    /// Resumes the showcase from persisted progress when available.
    /// </summary>
    public void Resume() => FireAndForget(ResumeAsync(), ShowcaseNavigationAction.Resume);

    /// <summary>
    /// Resumes the showcase from persisted progress when available.
    /// </summary>
    public Task<bool> ResumeAsync(CancellationToken cancellationToken = default) =>
        _transitionScheduler.RunAsync(ResumeCoreAsync, cancellationToken);

    /// <summary>
    /// Advances to the next step, or completes if on the last step.
    /// </summary>
    public void Next() => FireAndForget(NextAsync(), ShowcaseNavigationAction.Next);

    /// <summary>
    /// Advances to the next step, or completes if on the last step.
    /// </summary>
    public Task NextAsync(CancellationToken cancellationToken = default) =>
        _transitionScheduler.RunAsync(NextCoreAsync, cancellationToken);

    /// <summary>
    /// Goes back to the previous step.
    /// </summary>
    public void Previous() => FireAndForget(PreviousAsync(), ShowcaseNavigationAction.Previous);

    /// <summary>
    /// Goes back to the previous step.
    /// </summary>
    public Task PreviousAsync(CancellationToken cancellationToken = default) =>
        _transitionScheduler.RunAsync(PreviousCoreAsync, cancellationToken);

    /// <summary>
    /// Skips/cancels the showcase.
    /// </summary>
    public void Skip() => FireAndForget(SkipAsync(), ShowcaseNavigationAction.Skip);

    /// <summary>
    /// Skips/cancels the showcase.
    /// </summary>
    public Task SkipAsync(CancellationToken cancellationToken = default) =>
        _transitionScheduler.RunAsync(SkipCoreAsync, cancellationToken);

    /// <summary>
    /// Resets the showcase to its initial state.
    /// </summary>
    public void Reset() => FireAndForget(ResetAsync(), ShowcaseNavigationAction.Reset);

    /// <summary>
    /// Resets the showcase to its initial state.
    /// </summary>
    public Task ResetAsync(CancellationToken cancellationToken = default) =>
        _transitionScheduler.RunAsync(ResetCoreAsync, cancellationToken);

    private async Task StartCoreAsync(CancellationToken cancellationToken)
    {
        if (Steps.Count == 0)
        {
            return;
        }

        await ActivateStepAsync(0, ShowcaseStepTransitionReason.Start, cancellationToken);
        Started?.Invoke(this, EventArgs.Empty);
    }

    private async Task<bool> ResumeCoreAsync(CancellationToken cancellationToken)
    {
        if (Steps.Count == 0)
        {
            return false;
        }

        var state = await LoadProgressAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        if (state == null || !state.IsActive)
        {
            return false;
        }

        var persistedIndex = ResolvePersistedIndex(state);
        if (persistedIndex < 0 || persistedIndex >= Steps.Count)
        {
            await ClearProgressAsync(cancellationToken);
            return false;
        }

        await ActivateStepAsync(persistedIndex, ShowcaseStepTransitionReason.Resume, cancellationToken);
        Resumed?.Invoke(this, EventArgs.Empty);
        return true;
    }

    private async Task NextCoreAsync(CancellationToken cancellationToken)
    {
        if (!IsActive)
        {
            return;
        }

        if (_currentIndex < Steps.Count - 1)
        {
            await ActivateStepAsync(_currentIndex + 1, ShowcaseStepTransitionReason.Next, cancellationToken);
            return;
        }

        await CompleteAsync(cancellationToken);
    }

    private Task PreviousCoreAsync(CancellationToken cancellationToken)
    {
        if (!IsActive || _currentIndex <= 0)
        {
            return Task.CompletedTask;
        }

        return ActivateStepAsync(_currentIndex - 1, ShowcaseStepTransitionReason.Previous, cancellationToken);
    }

    private async Task SkipCoreAsync(CancellationToken cancellationToken)
    {
        if (!IsActive)
        {
            return;
        }

        await ClearProgressAsync(cancellationToken);
        Deactivate();
        Skipped?.Invoke(this, EventArgs.Empty);
    }

    private async Task ResetCoreAsync(CancellationToken cancellationToken)
    {
        await ClearProgressAsync(cancellationToken);
        Deactivate();
    }

    private async Task CompleteAsync(CancellationToken cancellationToken)
    {
        await ClearProgressAsync(cancellationToken);
        Deactivate();
        Completed?.Invoke(this, EventArgs.Empty);
    }

    private void Deactivate()
    {
        SetAndRaise(CurrentIndexProperty, ref _currentIndex, -1);
        SetAndRaise(CurrentStepProperty, ref _currentStep, null);
        UpdateDerivedState();
        SetActiveState(false);
    }

    private async Task ActivateStepAsync(
        int targetIndex,
        ShowcaseStepTransitionReason reason,
        CancellationToken cancellationToken)
    {
        if (targetIndex < 0 || targetIndex >= Steps.Count)
        {
            return;
        }

        var nextStep = Steps[targetIndex];
        var context = new ShowcaseStepTransitionContext(
            this,
            CurrentStep,
            nextStep,
            _currentIndex >= 0 ? _currentIndex : null,
            targetIndex,
            reason);

        if (BeforeStepAsync != null)
        {
            await BeforeStepAsync(context, cancellationToken);
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (targetIndex >= Steps.Count || !ReferenceEquals(Steps[targetIndex], nextStep))
        {
            throw new OperationCanceledException(
                "The showcase steps changed while navigation was in progress.",
                cancellationToken);
        }

        await SaveProgressAsync(
            targetIndex,
            isActive: true,
            GetStepIdentity(nextStep),
            cancellationToken);

        if (targetIndex >= Steps.Count || !ReferenceEquals(Steps[targetIndex], nextStep))
        {
            throw new OperationCanceledException(
                "The showcase steps changed while progress was being saved.",
                cancellationToken);
        }

        SetActiveStep(targetIndex, reason);

        if (AfterStepAsync != null)
        {
            await AfterStepAsync(context, cancellationToken);
        }

        cancellationToken.ThrowIfCancellationRequested();
    }

    private void SetActiveStep(int targetIndex, ShowcaseStepTransitionReason reason)
    {
        var nextStep = Steps[targetIndex];
        var previousStep = CurrentStep;
        var becameActive = !IsActive;
        var forward = reason != ShowcaseStepTransitionReason.Previous;
        var action = ToNavigationAction(reason);

        _visualTransitionForward = forward;
        _visualTransitionAction = action;

        SetAndRaise(CurrentIndexProperty, ref _currentIndex, targetIndex);
        SetAndRaise(CurrentStepProperty, ref _currentStep, nextStep);
        UpdateDerivedState();

        if (becameActive)
        {
            SetActiveState(true);
        }

        StepChanged?.Invoke(this, new ShowcaseStepChangedEventArgs(previousStep, nextStep, targetIndex));

        if (!becameActive)
        {
            OnVisualStepChanged(forward, action);
        }
    }

    private static ShowcaseNavigationAction ToNavigationAction(ShowcaseStepTransitionReason reason) =>
        reason switch
        {
            ShowcaseStepTransitionReason.Start => ShowcaseNavigationAction.Start,
            ShowcaseStepTransitionReason.Resume => ShowcaseNavigationAction.Resume,
            ShowcaseStepTransitionReason.Previous => ShowcaseNavigationAction.Previous,
            _ => ShowcaseNavigationAction.Next
        };

    private void SetActiveState(bool value)
    {
        if (IsActive == value)
        {
            return;
        }

        IsActive = value;
        UpdateDerivedState();
        RaiseCommandCanExecuteChanged();
        OnFlowActiveStateChanged();
    }

    private void UpdateDerivedState()
    {
        SetAndRaise(CanGoPreviousProperty, ref _canGoPrevious, IsActive && _currentIndex > 0);
        SetAndRaise(CanGoNextProperty, ref _canGoNext, IsActive && _currentIndex >= 0 && _currentIndex < Steps.Count - 1);

        var buttonText = _currentIndex >= 0 && _currentIndex < Steps.Count - 1
            ? NextButtonText
            : FinishButtonText;
        SetAndRaise(CurrentButtonTextProperty, ref _currentButtonText, buttonText);
        RaiseCommandCanExecuteChanged();
    }

    private void RaiseCommandCanExecuteChanged()
    {
        _nextCommand.RaiseCanExecuteChanged();
        _previousCommand.RaiseCanExecuteChanged();
        _skipCommand.RaiseCanExecuteChanged();
    }

    private void OnButtonTextChanged()
    {
        UpdateDerivedState();

        if (IsActive)
        {
            UpdateVisualState();
        }
    }

    private void OnStepsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var wasActive = IsActive;
        _transitionScheduler.Cancel();

        if (IsActive)
        {
            var currentIndex = CurrentStep == null ? -1 : Steps.IndexOf(CurrentStep);
            if (currentIndex >= 0)
            {
                SetAndRaise(CurrentIndexProperty, ref _currentIndex, currentIndex);
                UpdateDerivedState();
            }
            else if (Steps.Count == 0)
            {
                Deactivate();
            }
            else
            {
                var replacementIndex = Math.Clamp(_currentIndex, 0, Steps.Count - 1);
                var reason = replacementIndex < _currentIndex
                    ? ShowcaseStepTransitionReason.Previous
                    : ShowcaseStepTransitionReason.Next;
                SetActiveStep(replacementIndex, reason);
            }
        }
        else
        {
            UpdateDerivedState();
        }

        OnStepsChanged();

        if (wasActive && CanPersistProgress())
        {
            FireAndForget(
                _transitionScheduler.RunAsync(PersistStepsChangeAsync, CancellationToken.None),
                ShowcaseNavigationAction.StepsChanged);
        }
    }

    private bool CanPersistProgress() =>
        ProgressStore != null && !string.IsNullOrWhiteSpace(PersistenceKey);

    private Task<ShowcaseProgressState?> LoadProgressAsync(CancellationToken cancellationToken)
    {
        if (!CanPersistProgress())
        {
            return Task.FromResult<ShowcaseProgressState?>(null);
        }

        return ProgressStore!.LoadAsync(PersistenceKey!, cancellationToken);
    }

    private Task SaveProgressAsync(CancellationToken cancellationToken)
    {
        return SaveProgressAsync(
            CurrentIndex,
            IsActive,
            GetStepIdentity(CurrentStep),
            cancellationToken);
    }

    private Task SaveProgressAsync(
        int currentIndex,
        bool isActive,
        string? stepIdentity,
        CancellationToken cancellationToken)
    {
        if (!CanPersistProgress())
        {
            return Task.CompletedTask;
        }

        return ProgressStore!.SaveAsync(
            PersistenceKey!,
            new ShowcaseProgressState(currentIndex, isActive, stepIdentity),
            cancellationToken);
    }

    private Task PersistStepsChangeAsync(CancellationToken cancellationToken) =>
        IsActive
            ? SaveProgressAsync(cancellationToken)
            : ClearProgressAsync(cancellationToken);

    private int ResolvePersistedIndex(ShowcaseProgressState state)
    {
        if (string.IsNullOrWhiteSpace(state.StepKey))
        {
            return state.CurrentIndex;
        }

        var matchingIndex = -1;
        for (var i = 0; i < Steps.Count; i++)
        {
            if (!string.Equals(GetStepIdentity(Steps[i]), state.StepKey, StringComparison.Ordinal))
            {
                continue;
            }

            if (matchingIndex >= 0)
            {
                return state.CurrentIndex >= 0 &&
                       state.CurrentIndex < Steps.Count &&
                       string.Equals(
                           GetStepIdentity(Steps[state.CurrentIndex]),
                           state.StepKey,
                           StringComparison.Ordinal)
                    ? state.CurrentIndex
                    : -1;
            }

            matchingIndex = i;
        }

        return matchingIndex;
    }

    private static string? GetStepIdentity(ShowcaseStep? step) =>
        step == null
            ? null
            : string.IsNullOrWhiteSpace(step.Id) ? step.Key : step.Id;

    private Task ClearProgressAsync(CancellationToken cancellationToken)
    {
        if (!CanPersistProgress())
        {
            return Task.CompletedTask;
        }

        return ProgressStore!.ClearAsync(PersistenceKey!, cancellationToken);
    }

    private void FireAndForget(Task task, ShowcaseNavigationAction action)
    {
        _ = ObserveTransitionAsync(task, action);
    }

    private async Task ObserveTransitionAsync(Task task, ShowcaseNavigationAction action)
    {
        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
            // Expected when a newer navigation operation supersedes the current one.
        }
        catch (Exception ex)
        {
            TransitionFailed?.Invoke(this, new ShowcaseTransitionFailedEventArgs(action, ex));
        }
    }

}
