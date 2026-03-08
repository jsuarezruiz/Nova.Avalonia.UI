using System;
using System.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
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
    private readonly ShowcaseTooltipPositioner _positioner = new();
    private CancellationTokenSource? _animationCts;

    /// <summary>
    /// Defines the Showcase.Key attached property.
    /// </summary>
    public static readonly AttachedProperty<string?> KeyProperty =
        AvaloniaProperty.RegisterAttached<Showcase, Control, string?>("Key");

    /// <summary>
    /// Defines the Showcase.TooltipTemplate attached property.
    /// Set on a target element to override the tooltip template when that element is highlighted.
    /// </summary>
    public static readonly AttachedProperty<IDataTemplate?> TooltipTemplateProperty =
        AvaloniaProperty.RegisterAttached<Showcase, Control, IDataTemplate?>("TooltipTemplate");

    /// <summary>
    /// Defines the <see cref="IsActive"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<Showcase, bool>(nameof(IsActive));

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

    static Showcase()
    {
        IsActiveProperty.Changed.AddClassHandler<Showcase>((x, e) => x.OnIsActiveChanged(e));
        ControllerProperty.Changed.AddClassHandler<Showcase>((x, e) => x.OnControllerChanged(e));
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
    /// Gets or sets whether the showcase is currently active.
    /// </summary>
    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
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

    private Button? _skipButton;
    private Button? _previousButton;
    private Button? _nextButton;

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        UnsubscribeButtons();

        _overlay = e.NameScope.Find<ShowcaseOverlay>("PART_Overlay");
        _tooltip = e.NameScope.Find<ShowcaseTooltip>("PART_Tooltip");

        _skipButton = e.NameScope.Find<Button>("PART_SkipButton");
        _previousButton = e.NameScope.Find<Button>("PART_PreviousButton");
        _nextButton = e.NameScope.Find<Button>("PART_NextButton");

        if (_skipButton != null)
            _skipButton.Click += OnSkipButtonClick;
        if (_previousButton != null)
            _previousButton.Click += OnPreviousButtonClick;
        if (_nextButton != null)
            _nextButton.Click += OnNextButtonClick;

        UpdateVisualState();
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SubscribeController(Controller);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        CancelAnimation();
        UnsubscribeController(Controller);
        UnsubscribeButtons();
        base.OnDetachedFromVisualTree(e);
    }

    /// <inheritdoc />
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        if (IsActive)
            UpdateVisualState();
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (!IsActive || Controller == null) return;

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
        }
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
            _skipButton.Click -= OnSkipButtonClick;
        if (_previousButton != null)
            _previousButton.Click -= OnPreviousButtonClick;
        if (_nextButton != null)
            _nextButton.Click -= OnNextButtonClick;
    }

    private void OnIsActiveChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var isActive = (bool)e.NewValue!;

        if (isActive)
            Focus();

        UpdateVisualState(animate: true);
    }

    private void OnControllerChanged(AvaloniaPropertyChangedEventArgs e)
    {
        UnsubscribeController(e.OldValue as ShowcaseController);

        if (this.GetVisualRoot() != null)
            SubscribeController(e.NewValue as ShowcaseController);
    }

    private void SubscribeController(ShowcaseController? controller)
    {
        if (controller == null) return;

        controller.StepChanged += OnStepChanged;
        controller.Completed += OnShowcaseEnded;
        controller.Skipped += OnShowcaseEnded;
    }

    private void UnsubscribeController(ShowcaseController? controller)
    {
        if (controller == null) return;

        controller.StepChanged -= OnStepChanged;
        controller.Completed -= OnShowcaseEnded;
        controller.Skipped -= OnShowcaseEnded;
    }

    private void OnStepChanged(object? sender, ShowcaseStepChangedEventArgs e)
    {
        UpdateVisualState(animate: true);
    }

    private void OnShowcaseEnded(object? sender, EventArgs e)
    {
        IsActive = false;
    }

    private Control? ResolveTarget(string key)
    {
        var root = this.GetVisualRoot() as Visual;
        if (root == null) return null;

        foreach (var visual in root.GetSelfAndVisualDescendants().OfType<Control>())
        {
            var k = GetKey(visual);
            if (k == key)
                return visual;
        }

        return null;
    }

    private void UpdateVisualState(bool animate = false)
    {
        if (_overlay == null || _tooltip == null) return;

        var currentStep = Controller?.CurrentStep;
        if (!IsActive || currentStep == null)
        {
            _overlay.TargetBounds = null;
            _tooltip.IsVisible = false;
            CancelAnimation();
            return;
        }

        var target = ResolveTarget(currentStep.Key);
        if (target == null)
        {
            _overlay.TargetBounds = null;
            _tooltip.IsVisible = false;
            return;
        }

        var targetBounds = GetBoundsRelativeToThis(target);
        _overlay.TargetBounds = targetBounds;
        _overlay.HighlightShape = currentStep.HighlightShape;
        _overlay.HighlightPadding = currentStep.HighlightPadding;
        _overlay.HighlightCornerRadius = currentStep.CornerRadius;
        _overlay.OverlayBrush = OverlayBrush;

        _tooltip.Title = currentStep.Title;
        _tooltip.Description = currentStep.Description;
        _tooltip.DataContext = currentStep;

        var tooltipTemplate = GetTooltipTemplate(target) ?? currentStep.CustomTooltipTemplate;
        _tooltip.ContentTemplate = tooltipTemplate;

        _tooltip.IsVisible = true;

        if (_skipButton != null && Controller != null)
            _skipButton.Content = Controller.SkipButtonText;
        if (_previousButton != null)
        {
            _previousButton.IsVisible = Controller?.CanGoPrevious ?? false;
            if (Controller != null)
                _previousButton.Content = Controller.PreviousButtonText;
        }
        if (_nextButton != null && Controller != null)
            _nextButton.Content = Controller.CurrentButtonText;

        PositionTooltip(targetBounds, currentStep.TooltipPosition);

        if (animate)
            AnimateTooltipIn();
    }

    private Rect GetBoundsRelativeToThis(Control target)
    {
        var topLeft = target.TranslatePoint(new Point(0, 0), this) ?? new Point(0, 0);
        return new Rect(topLeft, target.Bounds.Size);
    }

    private void PositionTooltip(Rect targetBounds, ShowcaseTooltipPosition preferredPosition)
    {
        if (_tooltip == null) return;

        _tooltip.Measure(Bounds.Size);
        var tooltipSize = _tooltip.DesiredSize;

        var inflatedTargetBounds = targetBounds.Inflate(new Thickness(8));
        var position = _positioner.CalculatePosition(
            inflatedTargetBounds,
            tooltipSize,
            new Rect(0, 0, Bounds.Width, Bounds.Height),
            preferredPosition);

        Canvas.SetLeft(_tooltip, position.X);
        Canvas.SetTop(_tooltip, position.Y);
    }

    private void CancelAnimation()
    {
        _animationCts?.Cancel();
        _animationCts?.Dispose();
        _animationCts = null;
    }

    private async void AnimateTooltipIn()
    {
        if (_tooltip == null) return;

        var duration = AnimationDuration;
        if (duration <= TimeSpan.Zero) return;

        CancelAnimation();
        var cts = new CancellationTokenSource();
        _animationCts = cts;

        var animation = new Animation
        {
            Duration = duration,
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

        try
        {
            await animation.RunAsync(_tooltip, cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected during rapid step changes
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

    /// <inheritdoc />
    protected override string GetNameCore()
    {
        var showcase = (Showcase)Owner;
        var controller = showcase.Controller;

        if (controller?.CurrentStep == null)
            return "Interactive Tutorial";

        var step = controller.CurrentStep;
        return $"{step.Title}. Step {controller.CurrentIndex + 1} of {controller.Steps.Count}";
    }
}
