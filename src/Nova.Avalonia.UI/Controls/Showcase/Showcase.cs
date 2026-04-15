using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// An interactive tutorial/onboarding control that highlights UI elements step-by-step
/// with customizable tooltips and overlays.
/// </summary>
public class Showcase : TemplatedControl
{
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
    private CancellationTokenSource? _animationCts;
    private Animation? _tooltipAnimation;
    private bool _isActive;
    private bool _isTrackingLayout;
    private int? _autoScrolledStepIndex;
    private Control? _resolvedTarget;
    private string? _resolvedTargetKey;
    private Rect? _lastTargetBounds;

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
    /// Defines the <see cref="Controller"/> property.
    /// </summary>
    public static readonly StyledProperty<ShowcaseController?> ControllerProperty =
        AvaloniaProperty.Register<Showcase, ShowcaseController?>(nameof(Controller));

    /// <summary>
    /// Defines the <see cref="OverlayBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> OverlayBrushProperty =
        AvaloniaProperty.Register<Showcase, IBrush?>(
            nameof(OverlayBrush),
            new SolidColorBrush(Colors.Black, 0.7));

    /// <summary>
    /// Defines the <see cref="AnimationDuration"/> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> AnimationDurationProperty =
        AvaloniaProperty.Register<Showcase, TimeSpan>(
            nameof(AnimationDuration),
            TimeSpan.FromMilliseconds(300));

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
        ControllerProperty.Changed.AddClassHandler<Showcase>((x, e) => x.OnControllerChanged(e));
        InteractionModeProperty.Changed.AddClassHandler<Showcase>((x, e) => x.OnInteractionModeChanged(e));
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
    /// Gets whether the showcase is currently active. This is read-only and derived from the <see cref="Controller"/>.
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        private set => SetAndRaise(IsActiveProperty, ref _isActive, value);
    }

    /// <summary>
    /// Gets or sets the controller managing the showcase flow.
    /// </summary>
    public ShowcaseController? Controller
    {
        get => GetValue(ControllerProperty);
        set => SetValue(ControllerProperty, value);
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

    /// <summary>
    /// Validates and starts the showcase, throwing if validation fails.
    /// </summary>
    public void Start() => TryStart().EnsureStarted();

    /// <summary>
    /// Validates and starts the showcase asynchronously, throwing if validation fails.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        var result = await TryStartAsync(cancellationToken);
        result.EnsureStarted();
    }

    /// <summary>
    /// Attempts to validate and start the showcase.
    /// </summary>
    public ShowcaseStartResult TryStart()
    {
        var validation = Validate();
        if (!validation.IsValid || Controller == null)
        {
            return new ShowcaseStartResult(false, validation);
        }

        Controller.Start();
        return new ShowcaseStartResult(true, validation);
    }

    /// <summary>
    /// Attempts to validate and start the showcase asynchronously.
    /// </summary>
    public async Task<ShowcaseStartResult> TryStartAsync(CancellationToken cancellationToken = default)
    {
        var validation = Validate();
        if (!validation.IsValid || Controller == null)
        {
            return new ShowcaseStartResult(false, validation);
        }

        await Controller.StartAsync(cancellationToken);
        return new ShowcaseStartResult(true, validation);
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
        SubscribeController(Controller);
        SyncIsActiveFromController();
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
        UpdateLayoutTracking(forceDisable: true);
        UnsubscribeController(Controller);
        UnsubscribeButtons();
        ClearResolvedTarget();
        base.OnDetachedFromVisualTree(e);
    }

    /// <inheritdoc />
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        if (IsActive)
        {
            UpdateVisualState();
        }
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (!IsActive || Controller == null)
        {
            return;
        }

        switch (e.Key)
        {
            case Key.Right:
            case Key.Space:
            case Key.Enter:
                Controller.Next();
                e.Handled = true;
                break;

            case Key.Left:
                Controller.Previous();
                e.Handled = true;
                break;

            case Key.Escape:
                Controller.Skip();
                e.Handled = true;
                break;

            case Key.Tab:
                HandleTabFocusTrap(e);
                break;
        }
    }

    private void HandleTabFocusTrap(KeyEventArgs e)
    {
        var effectiveMode = Controller?.CurrentStep?.InteractionMode ?? InteractionMode;
        if (effectiveMode == ShowcaseInteractionMode.Passthrough)
        {
            return;
        }

        var focusableButtons = new List<Button>(3);
        if (_skipButton is { IsVisible: true })
        {
            focusableButtons.Add(_skipButton);
        }

        if (_previousButton is { IsVisible: true })
        {
            focusableButtons.Add(_previousButton);
        }

        if (_nextButton is { IsVisible: true })
        {
            focusableButtons.Add(_nextButton);
        }

        if (focusableButtons.Count == 0)
        {
            e.Handled = true;
            return;
        }

        var current = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() as Control;
        var currentIndex = current is Button button ? focusableButtons.IndexOf(button) : -1;
        var backward = (e.KeyModifiers & KeyModifiers.Shift) != 0;

        int nextIndex;
        if (currentIndex < 0)
        {
            nextIndex = backward ? focusableButtons.Count - 1 : 0;
        }
        else
        {
            nextIndex = backward ? currentIndex - 1 : currentIndex + 1;
            if (nextIndex < 0)
            {
                nextIndex = focusableButtons.Count - 1;
            }
            else if (nextIndex >= focusableButtons.Count)
            {
                nextIndex = 0;
            }
        }

        focusableButtons[nextIndex].Focus();
        e.Handled = true;
    }

    /// <inheritdoc />
    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new ShowcaseAutomationPeer(this);
    }

    private void OnSkipButtonClick(object? sender, RoutedEventArgs e) => Controller?.Skip();
    private void OnPreviousButtonClick(object? sender, RoutedEventArgs e) => Controller?.Previous();
    private void OnNextButtonClick(object? sender, RoutedEventArgs e) => Controller?.Next();

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

    private void OnControllerChanged(AvaloniaPropertyChangedEventArgs e)
    {
        UnsubscribeController(e.OldValue as ShowcaseController);
        ClearResolvedTarget();

        if (this.GetVisualRoot() != null)
        {
            SubscribeController(e.NewValue as ShowcaseController);
        }

        SyncIsActiveFromController();
        UpdateLayoutTracking();
        UpdateVisualState();
    }

    private void SubscribeController(ShowcaseController? controller)
    {
        if (controller == null)
        {
            return;
        }

        controller.StepChanged += OnStepChanged;
        controller.PropertyChanged += OnControllerPropertyChanged;
        controller.Steps.CollectionChanged += OnStepsCollectionChanged;
    }

    private void UnsubscribeController(ShowcaseController? controller)
    {
        if (controller == null)
        {
            return;
        }

        controller.StepChanged -= OnStepChanged;
        controller.PropertyChanged -= OnControllerPropertyChanged;
        controller.Steps.CollectionChanged -= OnStepsCollectionChanged;
    }

    private void OnStepChanged(object? sender, ShowcaseStepChangedEventArgs e)
    {
        _autoScrolledStepIndex = null;
        ClearResolvedTarget();
        UpdateVisualState(animate: true);
    }

    private void OnControllerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not ShowcaseController controller)
        {
            return;
        }

        switch (e.PropertyName)
        {
            case nameof(ShowcaseController.IsActive):
                if (!controller.IsActive)
                {
                    _autoScrolledStepIndex = null;
                    ClearResolvedTarget();
                }

                SyncIsActiveFromController();
                UpdateLayoutTracking();
                UpdateVisualState(animate: controller.IsActive);
                break;

            case nameof(ShowcaseController.NextButtonText):
            case nameof(ShowcaseController.FinishButtonText):
            case nameof(ShowcaseController.PreviousButtonText):
            case nameof(ShowcaseController.SkipButtonText):
                if (controller.IsActive)
                {
                    UpdateVisualState();
                }

                break;
        }
    }

    private void OnStepsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _autoScrolledStepIndex = null;
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

        // Avoid a full visual state rebuild when the target bounds haven't changed.
        var currentStep = Controller?.CurrentStep;
        if (currentStep != null && _resolvedTarget != null)
        {
            if (TryGetBoundsRelativeToThis(_resolvedTarget, out var currentBounds))
            {
                if (_lastTargetBounds.HasValue && _lastTargetBounds.Value == currentBounds)
                {
                    return;
                }
            }
        }

        UpdateVisualState();
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
        return match;
    }

    /// <summary>
    /// Validates the current showcase configuration and target resolution state.
    /// </summary>
    public ShowcaseValidationResult Validate()
    {
        var issues = new List<ShowcaseValidationIssue>();
        var controller = Controller;
        if (controller == null)
        {
            issues.Add(new ShowcaseValidationIssue(
                ShowcaseValidationIssueCode.NoController,
                ShowcaseValidationSeverity.Error,
                "The showcase has no controller assigned."));
            return new ShowcaseValidationResult(issues);
        }

        if (controller.Steps.Count == 0)
        {
            issues.Add(new ShowcaseValidationIssue(
                ShowcaseValidationIssueCode.NoSteps,
                ShowcaseValidationSeverity.Error,
                "The showcase controller does not define any steps."));
        }

        for (var i = 0; i < controller.Steps.Count; i++)
        {
            var step = controller.Steps[i];
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

        for (var i = 0; i < controller.Steps.Count; i++)
        {
            var step = controller.Steps[i];
            if (string.IsNullOrWhiteSpace(step.Key))
            {
                continue;
            }

            if (!targetsByKey.TryGetValue(step.Key, out var matches))
            {
                issues.Add(new ShowcaseValidationIssue(
                    ShowcaseValidationIssueCode.MissingTarget,
                    ShowcaseValidationSeverity.Error,
                    $"Step {i + 1} targets Showcase.Key '{step.Key}', but no matching control was found.",
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

    private void UpdateVisualState(bool animate = false)
    {
        if (_overlay == null || _tooltip == null)
        {
            return;
        }

        var currentStep = Controller?.CurrentStep;
        if (!IsActive || currentStep == null)
        {
            _autoScrolledStepIndex = null;
            _lastTargetBounds = null;
            ClearResolvedTarget();
            _overlay.TargetBounds = null;
            ResetInteractionBlockers();
            _tooltip.IsVisible = false;
            _tooltip.Content = null;
            _tooltip.ContentTemplate = null;
            CancelAnimation();
            AutomationProperties.SetName(this, "Interactive Tutorial");
            return;
        }

        AutomationProperties.SetName(this,
            $"{currentStep.Title}. Step {Controller!.CurrentIndex + 1} of {Controller.Steps.Count}");

        _overlay.HighlightShape = currentStep.HighlightShape;
        _overlay.HighlightPadding = currentStep.HighlightPadding;
        _overlay.HighlightCornerRadius = currentStep.HighlightCornerRadius;

        _tooltip.Title = currentStep.Title;
        _tooltip.Description = currentStep.Description;
        _tooltip.IsVisible = true;
        var effectiveInteractionMode = currentStep.InteractionMode ?? InteractionMode;

        if (_skipButton != null && Controller != null)
        {
            _skipButton.Content = Controller.SkipButtonText;
        }

        if (_previousButton != null)
        {
            _previousButton.IsVisible = Controller?.CanGoPrevious ?? false;
            if (Controller != null)
            {
                _previousButton.Content = Controller.PreviousButtonText;
            }
        }

        if (_nextButton != null && Controller != null)
        {
            _nextButton.Content = Controller.CurrentButtonText;
        }

        var target = ResolveTarget(currentStep.Key);
        if (target != null &&
            AutoScrollIntoView &&
            Controller?.CurrentIndex is int currentIndex &&
            _autoScrolledStepIndex != currentIndex)
        {
            target.BringIntoView();
            _autoScrolledStepIndex = currentIndex;
        }

        ConfigureTooltipTemplate(currentStep, target);

        if (target == null || !TryGetBoundsRelativeToThis(target, out var targetBounds))
        {
            _lastTargetBounds = null;
            _overlay.TargetBounds = null;
            UpdateInteractionBlockers(effectiveInteractionMode, null);
            PositionTooltipCentered();

            if (animate)
            {
                AnimateTooltipIn();
            }

            return;
        }

        _lastTargetBounds = targetBounds;
        _overlay.TargetBounds = targetBounds;
        UpdateInteractionBlockers(effectiveInteractionMode, targetBounds.Inflate(currentStep.HighlightPadding));
        PositionTooltip(targetBounds, currentStep.TooltipPosition);

        if (animate)
        {
            AnimateTooltipIn();
        }
    }

    private bool TryGetBoundsRelativeToThis(Control target, out Rect bounds)
    {
        bounds = default;

        if (!target.IsVisible)
        {
            return false;
        }

        var topLeft = target.TranslatePoint(new Point(0, 0), this);
        if (topLeft is null)
        {
            return false;
        }

        var size = target.Bounds.Size;
        if (size.Width <= 0 || size.Height <= 0)
        {
            return false;
        }

        bounds = new Rect(topLeft.Value, size);
        return true;
    }

    private void PositionTooltip(Rect targetBounds, ShowcaseTooltipPosition preferredPosition)
    {
        if (_tooltip == null)
        {
            return;
        }

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

        _tooltip.Measure(Bounds.Size);
        var tooltipSize = _tooltip.DesiredSize;
        var position = new Point(
            Math.Max(0, (Bounds.Width - tooltipSize.Width) / 2),
            Math.Max(0, (Bounds.Height - tooltipSize.Height) / 2));

        Canvas.SetLeft(_tooltip, position.X);
        Canvas.SetTop(_tooltip, position.Y);
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

    private void SyncIsActiveFromController()
    {
        var controllerActive = Controller?.IsActive ?? false;
        if (IsActive == controllerActive)
        {
            return;
        }

        IsActive = controllerActive;

        if (IsActive)
        {
            Focus();
        }
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
                            new Setter(OpacityProperty, 0d),
                            new Setter(ScaleTransform.ScaleXProperty, 0.95d),
                            new Setter(ScaleTransform.ScaleYProperty, 0.95d)
                        },
                        Cue = new Cue(0)
                    },
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter(OpacityProperty, 1d),
                            new Setter(ScaleTransform.ScaleXProperty, 1d),
                            new Setter(ScaleTransform.ScaleYProperty, 1d)
                        },
                        Cue = new Cue(1)
                    }
                }
            };
        }

        _tooltipAnimation.Duration = duration;
        return _tooltipAnimation;
    }

    private async void AnimateTooltipIn()
    {
        if (_tooltip == null)
        {
            return;
        }

        var duration = AnimationDuration;
        if (duration <= TimeSpan.Zero)
        {
            return;
        }

        CancelAnimation();
        var cts = new CancellationTokenSource();
        _animationCts = cts;

        try
        {
            await GetOrCreateTooltipAnimation(duration).RunAsync(_tooltip, cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected during rapid step changes
        }
        catch (Exception)
        {
            // Guard against unexpected animation failures to avoid crashing the app
        }
    }
}

/// <summary>
/// Automation peer for <see cref="Showcase"/>.
/// </summary>
public class ShowcaseAutomationPeer : ControlAutomationPeer
{
    /// <summary>
    /// Creates a new <see cref="ShowcaseAutomationPeer"/>.
    /// </summary>
    public ShowcaseAutomationPeer(Showcase owner) : base(owner) { }

    /// <inheritdoc />
    protected override string GetClassNameCore() => nameof(Showcase);

    /// <inheritdoc />
    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Group;
}
