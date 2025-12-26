using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
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
    private readonly Dictionary<string, Control> _targetCache = new();
    
    /// <summary>
    /// Defines the Showcase.Key attached property.
    /// </summary>
    public static readonly AttachedProperty<string?> KeyProperty =
        AvaloniaProperty.RegisterAttached<Showcase, Control, string?>("Key");
    
    /// <summary>
    /// Defines the Showcase.Order attached property.
    /// </summary>
    public static readonly AttachedProperty<int> OrderProperty =
        AvaloniaProperty.RegisterAttached<Showcase, Control, int>("Order", 0);
    
    /// <summary>
    /// Defines the Showcase.TooltipTemplate attached property.
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
    /// Gets the Order attached property value.
    /// </summary>
    public static int GetOrder(Control element) => element.GetValue(OrderProperty);
    
    /// <summary>
    /// Sets the Order attached property value.
    /// </summary>
    public static void SetOrder(Control element, int value) => element.SetValue(OrderProperty, value);
    
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
    /// Gets or sets the animation duration.
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
        
        _overlay = e.NameScope.Find<ShowcaseOverlay>("PART_Overlay");
        _tooltip = e.NameScope.Find<ShowcaseTooltip>("PART_Tooltip");
        

        _skipButton = e.NameScope.Find<Button>("PART_SkipButton");
        _previousButton = e.NameScope.Find<Button>("PART_PreviousButton");
        _nextButton = e.NameScope.Find<Button>("PART_NextButton");
        
        if (_skipButton != null)
            _skipButton.Click += (s, e) => Controller?.Skip();
        if (_previousButton != null)
            _previousButton.Click += (s, e) => Controller?.Previous();
        if (_nextButton != null)
            _nextButton.Click += (s, e) => Controller?.Next();
        
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
                Controller.NextCommand.Execute(null);
                e.Handled = true;
                break;
                
            case Key.Left:
                Controller.PreviousCommand.Execute(null);
                e.Handled = true;
                break;
                
            case Key.Escape:
                Controller.SkipCommand.Execute(null);
                e.Handled = true;
                break;
        }
    }
    
    /// <inheritdoc />
    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new ShowcaseAutomationPeer(this);
    }
    
    private void OnIsActiveChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var isActive = (bool)e.NewValue!;
        
        if (isActive)
        {
            Focus();
            CacheTargets();
        }
        
        UpdateVisualState();
    }
    
    private void OnControllerChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.OldValue is ShowcaseController oldController)
        {
            oldController.StepChanged -= OnStepChanged;
            oldController.Completed -= OnShowcaseEnded;
            oldController.Skipped -= OnShowcaseEnded;
        }
        
        if (e.NewValue is ShowcaseController newController)
        {
            newController.StepChanged += OnStepChanged;
            newController.Completed += OnShowcaseEnded;
            newController.Skipped += OnShowcaseEnded;
        }
    }
    
    private void OnStepChanged(object? sender, ShowcaseStepChangedEventArgs e)
    {
        UpdateVisualState();
    }
    
    private void OnShowcaseEnded(object? sender, EventArgs e)
    {
        IsActive = false;
    }
    
    private void CacheTargets()
    {
        _targetCache.Clear();
        
        var root = this.GetVisualRoot() as Visual;
        if (root == null) return;
        
        foreach (var visual in root.GetSelfAndVisualDescendants().OfType<Control>())
        {
            var key = GetKey(visual);
            if (!string.IsNullOrEmpty(key))
            {
                _targetCache[key] = visual;
            }
        }
    }
    
    private void UpdateVisualState()
    {
        if (_overlay == null || _tooltip == null) return;
        
        var currentStep = Controller?.CurrentStep;
        if (!IsActive || currentStep == null)
        {
            _overlay.TargetBounds = null;
            _tooltip.IsVisible = false;
            return;
        }
        

        if (!_targetCache.TryGetValue(currentStep.Key, out var target))
        {

            CacheTargets();
            if (!_targetCache.TryGetValue(currentStep.Key, out target))
            {
                _overlay.TargetBounds = null;
                _tooltip.IsVisible = false;
                return;
            }
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
        _tooltip.IsVisible = true;
        

        if (_previousButton != null)
            _previousButton.IsVisible = Controller?.CanGoPrevious ?? false;
        if (_nextButton != null && Controller != null)
            _nextButton.Content = Controller.NextButtonText;
        

        PositionTooltip(targetBounds, currentStep.TooltipPosition);
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
