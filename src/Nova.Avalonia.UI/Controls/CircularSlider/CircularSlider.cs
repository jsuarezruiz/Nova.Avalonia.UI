using System;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Avalonia.Media;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// A circular slider control that allows users to select a value by dragging around a circular arc.
/// Supports customizable angles, themes, and value formatting.
/// </summary>
[PseudoClasses(":minimum", ":maximum")]
public class CircularSlider : RangeBase
{
    private const double DefaultThumbDiameter = 20.0;
    private const double DefaultTrackThickness = 12.0;
    private const double TouchThumbHitTargetPadding = 12.0;
    private const double ArcHitTestPadding = 6.0;
    private const string TrackThicknessResourceKey = "CircularSliderTrackThickness";

    private Border? _thumbContainer;
    private ContentPresenter? _centerContent;
    private TextBlock? _defaultCenterText;
    private IPointer? _dragPointer;
    private bool _isDragging;
    
    private StreamGeometry? _inactiveGeometryCache;
    private IBrush? _foregroundBrushCache;
    private double _trackThickness = DefaultTrackThickness;

    /// <summary>
    /// Defines the <see cref="TickFrequency"/> property.
    /// </summary>
    public static readonly StyledProperty<double> TickFrequencyProperty =
        AvaloniaProperty.Register<CircularSlider, double>(nameof(TickFrequency), 1.0);

    /// <summary>
    /// Defines the <see cref="IsSnapToTickEnabled"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsSnapToTickEnabledProperty =
        AvaloniaProperty.Register<CircularSlider, bool>(nameof(IsSnapToTickEnabled), false);

    /// <summary>
    /// Defines the <see cref="ValueStringFormat"/> property.
    /// </summary>
    public static readonly StyledProperty<string> ValueStringFormatProperty =
        AvaloniaProperty.Register<CircularSlider, string>(nameof(ValueStringFormat), "F0");

    /// <summary>
    /// Defines the <see cref="StartAngle"/> property.
    /// </summary>
    public static readonly StyledProperty<double> StartAngleProperty = 
        AvaloniaProperty.Register<CircularSlider, double>(nameof(StartAngle), -135.0);

    /// <summary>
    /// Defines the <see cref="EndAngle"/> property.
    /// </summary>
    public static readonly StyledProperty<double> EndAngleProperty = 
        AvaloniaProperty.Register<CircularSlider, double>(nameof(EndAngle), 135.0);

    /// <summary>
    /// Defines the <see cref="Content"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<CircularSlider, object?>(nameof(Content));

    /// <summary>
    /// Defines the <see cref="ContentTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        AvaloniaProperty.Register<CircularSlider, IDataTemplate?>(nameof(ContentTemplate));

    /// <summary>
    /// Gets or sets the interval between tick marks used for snapping.
    /// </summary>
    public double TickFrequency { get => GetValue(TickFrequencyProperty); set => SetValue(TickFrequencyProperty, value); }

    /// <summary>
    /// Gets or sets a value indicating whether user interaction snaps the thumb to the closest tick.
    /// </summary>
    public bool IsSnapToTickEnabled { get => GetValue(IsSnapToTickEnabledProperty); set => SetValue(IsSnapToTickEnabledProperty, value); }

    /// <summary>
    /// Gets or sets the format string used to display the default center value.
    /// </summary>
    public string ValueStringFormat { get => GetValue(ValueStringFormatProperty); set => SetValue(ValueStringFormatProperty, value); }

    /// <summary>
    /// Gets or sets the start angle of the arc in degrees.
    /// </summary>
    public double StartAngle { get => GetValue(StartAngleProperty); set => SetValue(StartAngleProperty, value); }

    /// <summary>
    /// Gets or sets the end angle of the arc in degrees.
    /// </summary>
    public double EndAngle { get => GetValue(EndAngleProperty); set => SetValue(EndAngleProperty, value); }

    /// <summary>
    /// Gets or sets the content to display in the center of the slider.
    /// </summary>
    [Content]
    public object? Content { get => GetValue(ContentProperty); set => SetValue(ContentProperty, value); }

    /// <summary>
    /// Gets or sets the template for the center content.
    /// </summary>
    public IDataTemplate? ContentTemplate { get => GetValue(ContentTemplateProperty); set => SetValue(ContentTemplateProperty, value); }

    static CircularSlider()
    {
        AffectsRender<CircularSlider>(
            ValueProperty, MinimumProperty, MaximumProperty,
            StartAngleProperty, EndAngleProperty,
            BackgroundProperty, ForegroundProperty);

        MinimumProperty.Changed.AddClassHandler<CircularSlider>((o, _) => o.CoerceValueToRange());
        MaximumProperty.Changed.AddClassHandler<CircularSlider>((o, _) => o.CoerceValueToRange());

        FocusableProperty.OverrideDefaultValue<CircularSlider>(true);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CircularSlider"/> class.
    /// </summary>
    public CircularSlider()
    {
        ResourcesChanged += OnResourcesChanged;
        ActualThemeVariantChanged += OnActualThemeVariantChanged;
        UpdatePseudoClasses();
    }

    /// <inheritdoc/>
    protected override AutomationPeer OnCreateAutomationPeer() => new CircularSliderAutomationPeer(this);

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_thumbContainer != null)
            _thumbContainer.PropertyChanged -= OnThumbContainerPropertyChanged;

        _thumbContainer = e.NameScope.Find<Border>("PART_Thumb");
        _centerContent = e.NameScope.Find<ContentPresenter>("PART_CenterContent");
        _defaultCenterText = e.NameScope.Find<TextBlock>("PART_DefaultCenterText");
        if (_thumbContainer != null)
            _thumbContainer.PropertyChanged += OnThumbContainerPropertyChanged;

        RefreshTrackThicknessResource();
        UpdateThumbPosition();
        UpdateCenterContent();
        UpdateForegroundBrushCache();
    }

    private void UpdateForegroundBrushCache()
    {
        if (Foreground is ConicGradientBrush conic)
        {
            var rotation = StartAngle - 90;
            var newBrush = new ConicGradientBrush
            {
                GradientStops = conic.GradientStops,
                Center = conic.Center,
                Opacity = conic.Opacity,
                Transform = new RotateTransform(rotation)
            };
            _foregroundBrushCache = newBrush;
        }
        else
        {
            _foregroundBrushCache = Foreground;
        }
    }

    private double CalculateRadius(Size availableSize)
    {
        var maxElement = Math.Max(GetTrackThickness(), GetThumbDiameter());
        return (Math.Min(availableSize.Width, availableSize.Height) - maxElement) / 2;
    }

    private double GetThumbDiameter()
    {
        if (_thumbContainer is { Bounds.Width: > 0, Bounds.Height: > 0 })
            return Math.Max(_thumbContainer.Bounds.Width, _thumbContainer.Bounds.Height);

        return DefaultThumbDiameter;
    }

    private void CoerceValueToRange()
    {
        Value = Maximum < Minimum ? Minimum : Math.Clamp(Value, Minimum, Maximum);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == BoundsProperty ||
            change.Property == StartAngleProperty ||
            change.Property == EndAngleProperty)
        {
            _inactiveGeometryCache = null;
        }

        if (change.Property == ForegroundProperty || change.Property == StartAngleProperty)
        {
            UpdateForegroundBrushCache();
        }

        if (change.Property == ValueProperty)
        {
            UpdateThumbPosition();
            UpdateCenterContent();
            UpdatePseudoClasses();
        }
        else if (change.Property == BoundsProperty || 
                 change.Property == StartAngleProperty || 
                 change.Property == EndAngleProperty ||
                 change.Property == MinimumProperty ||
                 change.Property == MaximumProperty)
        {
            UpdateThumbPosition();
            if (change.Property == MinimumProperty || change.Property == MaximumProperty)
                UpdatePseudoClasses();
        }
        else if (change.Property == ContentProperty || change.Property == ContentTemplateProperty)
        {
            UpdateCenterContent();
        }
        else if (change.Property == ValueStringFormatProperty)
        {
            UpdateCenterContent();
        }
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!IsEnabled)
            return;
        
        var point = e.GetCurrentPoint(this);
        var position = e.GetPosition(this);

        if (point.Properties.IsLeftButtonPressed)
        {
            if (!IsInteractivePress(position, e.Pointer.Type))
                return;

            BeginDrag(e.Pointer, position);
            e.Handled = true;
        }
    }

    /// <inheritdoc/>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_isDragging && e.Pointer == _dragPointer)
        {
            UpdateValueFromPoint(e.GetPosition(this));
            e.Handled = true;
        }
    }

    /// <inheritdoc/>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (_isDragging && e.Pointer == _dragPointer)
        {
            CompleteDrag(e.Pointer);
            e.Handled = true;
        }
    }

    /// <inheritdoc/>
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        CompleteDrag(null);
    }

    private void BeginDrag(IPointer pointer, Point position)
    {
        _dragPointer = pointer;
        _isDragging = true;
        pointer.Capture(this);
        UpdateValueFromPoint(position);
        Focus();
    }

    private void CompleteDrag(IPointer? pointer)
    {
        if (!_isDragging) return;

        var pointerToRelease = _dragPointer ?? pointer;
        _isDragging = false;
        _dragPointer = null;
        pointerToRelease?.Capture(null);
    }

    private bool IsTouchPressOnThumb(Point position)
    {
        if (_thumbContainer is null || !_thumbContainer.IsVisible)
            return false;

        var bounds = _thumbContainer.Bounds;
        var hitBounds = new Rect(
            bounds.X - TouchThumbHitTargetPadding,
            bounds.Y - TouchThumbHitTargetPadding,
            bounds.Width + TouchThumbHitTargetPadding * 2,
            bounds.Height + TouchThumbHitTargetPadding * 2);

        return hitBounds.Contains(position);
    }

    private bool IsInteractivePress(Point position, PointerType pointerType)
    {
        if (pointerType == PointerType.Touch)
            return IsTouchPressOnThumb(position);

        return IsTouchPressOnThumb(position) || IsPointOnArcBand(position);
    }

    private bool IsPointOnArcBand(Point position)
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
            return false;

        var radius = CalculateRadius(Bounds.Size);
        if (radius <= 0)
            return false;

        var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
        var dx = position.X - center.X;
        var dy = position.Y - center.Y;
        var distance = Math.Sqrt(dx * dx + dy * dy);
        var halfBand = Math.Max(GetTrackThickness(), GetThumbDiameter()) / 2 + ArcHitTestPadding;

        if (Math.Abs(distance - radius) > halfBand)
            return false;

        var angleRange = GetAngleRange();
        if (angleRange >= 359.99)
            return true;

        var pointAngle = NormalizeAngle(Math.Atan2(dy, dx) * 180.0 / Math.PI + 90);
        var start = NormalizeAngle(StartAngle);
        var relativeAngle = pointAngle - start;
        if (relativeAngle < 0)
            relativeAngle += 360;

        return relativeAngle <= angleRange;
    }

    /// <inheritdoc/>
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        if (_isDragging)
            CompleteDrag(e.Pointer);
    }

    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!IsEnabled) return;
        
        var range = Maximum - Minimum;
        if (range <= 0) return;
        
        var smallChange = GetSmallInteractionChange(range);
        var largeChange = GetLargeInteractionChange(range);
        if (smallChange <= 0 || largeChange <= 0) return;

        switch (e.Key)
        {
            case Key.Left:
            case Key.Down:
                Value = SnapValueToTick(Value - smallChange);
                e.Handled = true;
                break;
            case Key.Right:
            case Key.Up:
                Value = SnapValueToTick(Value + smallChange);
                e.Handled = true;
                break;
            case Key.Home:
                Value = Minimum;
                e.Handled = true;
                break;
            case Key.End:
                Value = Maximum;
                e.Handled = true;
                break;
            case Key.PageDown:
                Value = SnapValueToTick(Value - largeChange);
                e.Handled = true;
                break;
            case Key.PageUp:
                Value = SnapValueToTick(Value + largeChange);
                e.Handled = true;
                break;
        }
    }

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        var width = Bounds.Width;
        var height = Bounds.Height;
        if (width <= 0 || height <= 0) return;

        var radius = CalculateRadius(Bounds.Size);
        if (radius <= 0) return;

        var center = new Point(width / 2, height / 2);

        if (_inactiveGeometryCache == null)
        {
            _inactiveGeometryCache = new StreamGeometry();
            using var ctx = _inactiveGeometryCache.Open();
            DrawArcStream(ctx, center, radius, StartAngle, EndAngle);
        }
        var trackThickness = GetTrackThickness();
        context.DrawGeometry(null, new Pen(Background, trackThickness, lineCap: PenLineCap.Round), _inactiveGeometryCache);

        var range = Maximum - Minimum;
        if (range <= 0) return;

        var normalizedValue = (Value - Minimum) / range;
        if (normalizedValue > 0.001)
        {
            var angleRange = GetAngleRange();
            var valueAngle = StartAngle + normalizedValue * angleRange;

            var activeGeo = new StreamGeometry();
            using (var ctx = activeGeo.Open())
            {
                DrawArcStream(ctx, center, radius, StartAngle, valueAngle);
            }
            
            var brushToUse = _foregroundBrushCache ?? Foreground;
            context.DrawGeometry(null, new Pen(brushToUse, trackThickness, lineCap: PenLineCap.Round), activeGeo);
        }
    }

    private void DrawArcStream(StreamGeometryContext ctx, Point center, double radius, double startAngle, double endAngle)
    {
        var start = NormalizeAngle(startAngle);
        var end = NormalizeAngle(endAngle);
        if (end <= start) end += 360;

        var angleDiff = end - start;
        if (Math.Abs(angleDiff - 360) < 0.01)
        {
            ctx.BeginFigure(new Point(center.X + radius, center.Y), false);
            ctx.ArcTo(new Point(center.X + radius - 0.01, center.Y), new Size(radius, radius), 359, true, SweepDirection.Clockwise);
            return;
        }

        var startRad = (start - 90) * Math.PI / 180.0;
        var endRad = (end - 90) * Math.PI / 180.0;

        var startPoint = new Point(center.X + radius * Math.Cos(startRad), center.Y + radius * Math.Sin(startRad));
        var endPoint = new Point(center.X + radius * Math.Cos(endRad), center.Y + radius * Math.Sin(endRad));

        var isLargeArc = angleDiff > 180;

        ctx.BeginFigure(startPoint, false);
        ctx.ArcTo(endPoint, new Size(radius, radius), 0, isLargeArc, SweepDirection.Clockwise);
    }

    private void UpdateValueFromPoint(Point point)
    {
        var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
        var dx = point.X - center.X;
        var dy = point.Y - center.Y;

        var angleRad = Math.Atan2(dy, dx);
        var angleDeg = angleRad * 180.0 / Math.PI + 90;
        angleDeg = NormalizeAngle(angleDeg);

        var startNorm = NormalizeAngle(StartAngle);
        var relativeAngle = angleDeg - startNorm;
        if (relativeAngle < 0) relativeAngle += 360;

        var angleRange = GetAngleRange();
        if (angleRange <= 0) return;

        if (relativeAngle > angleRange)
            relativeAngle = relativeAngle > angleRange + (360 - angleRange) / 2 ? 0 : angleRange;

        var normalizedValue = relativeAngle / angleRange;
        var range = Maximum - Minimum;
        if (range <= 0) return;
        
        var rawValue = Minimum + normalizedValue * range;

        Value = SnapValueToTick(rawValue);
    }


    private void UpdateThumbPosition()
    {
        if (_thumbContainer == null || Bounds.Width == 0 || Bounds.Height == 0) return;

        var range = Maximum - Minimum;
        var normalizedValue = range > 0 ? (Value - Minimum) / range : 0;

        var angleRange = GetAngleRange();
        var valueAngle = StartAngle + normalizedValue * angleRange;

        var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
        var radius = CalculateRadius(Bounds.Size);
        var thumbDiameter = GetThumbDiameter();

        var angleRad = (valueAngle - 90) * Math.PI / 180.0;
        var thumbX = center.X + radius * Math.Cos(angleRad) - thumbDiameter / 2;
        var thumbY = center.Y + radius * Math.Sin(angleRad) - thumbDiameter / 2;

        Canvas.SetLeft(_thumbContainer, thumbX);
        Canvas.SetTop(_thumbContainer, thumbY);

        _thumbContainer.IsVisible = true;
    }

    private void UpdateCenterContent()
    {
        var hasCustomContent = Content is not null;

        if (_centerContent != null)
            _centerContent.IsVisible = hasCustomContent;

        if (_defaultCenterText != null)
        {
            _defaultCenterText.Text = Value.ToString(ValueStringFormat);
            _defaultCenterText.IsVisible = !hasCustomContent;
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":minimum", Value <= Minimum);
        PseudoClasses.Set(":maximum", Value >= Maximum);
    }

    private double NormalizeAngle(double angle)
    {
        while (angle < 0) angle += 360;
        while (angle >= 360) angle -= 360;
        return angle;
    }

    private double GetAngleRange()
    {
        var start = NormalizeAngle(StartAngle);
        var end = NormalizeAngle(EndAngle);
        var range = end - start;
        if (range <= 0) range += 360;
        return range;
    }

    private double GetSmallInteractionChange(double range)
    {
        if (range <= 0) return 0;
        return SmallChange > 0 ? SmallChange : range / 100.0;
    }

    internal double GetSmallInteractionChange()
    {
        var range = Maximum - Minimum;
        return range > 0 ? GetSmallInteractionChange(range) : 0;
    }

    private double GetLargeInteractionChange(double range)
    {
        if (range <= 0) return 0;
        return LargeChange > 0 ? LargeChange : GetSmallInteractionChange(range) * 10.0;
    }

    internal double GetLargeInteractionChange()
    {
        var range = Maximum - Minimum;
        return range > 0 ? GetLargeInteractionChange(range) : 0;
    }

    private double GetTrackThickness()
        => _trackThickness;

    private void OnResourcesChanged(object? sender, ResourcesChangedEventArgs e)
    {
        RefreshTrackThicknessResource();
        InvalidateThumbLayout();
    }

    private void OnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        RefreshTrackThicknessResource();
        InvalidateThumbLayout();
    }

    private void OnThumbContainerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == BoundsProperty)
        {
            var oldBounds = change.GetOldValue<Rect>();
            var newBounds = change.GetNewValue<Rect>();
            if (oldBounds.Size == newBounds.Size)
                return;
        }
        else if (change.Property != WidthProperty && change.Property != HeightProperty)
        {
            return;
        }

        InvalidateThumbLayout();
    }

    private void InvalidateThumbLayout()
    {
        _inactiveGeometryCache = null;
        UpdateThumbPosition();
        InvalidateVisual();
    }

    private void RefreshTrackThicknessResource()
    {
        if (!ApplyTrackThicknessResource())
            return;

        InvalidateThumbLayout();
    }

    private bool ApplyTrackThicknessResource()
    {
        var trackThickness = GetTrackThicknessResource();
        if (Math.Abs(_trackThickness - trackThickness) < 0.001)
            return false;

        _trackThickness = trackThickness;
        _inactiveGeometryCache = null;
        return true;
    }

    private double GetTrackThicknessResource()
    {
        if (Resources.ContainsKey(TrackThicknessResourceKey))
            return CoerceTrackThicknessResource(Resources[TrackThicknessResourceKey]);

        if (Resources.TryGetResource(TrackThicknessResourceKey, ActualThemeVariant, out var resource) ||
            Resources.TryGetResource(TrackThicknessResourceKey, null, out resource) ||
            TryGetResource(TrackThicknessResourceKey, null, out resource) ||
            TryGetResource(TrackThicknessResourceKey, ActualThemeVariant, out resource))
        {
            return CoerceTrackThicknessResource(resource);
        }

        return DefaultTrackThickness;
    }

    private static double CoerceTrackThicknessResource(object? resource)
        => resource is double thickness && double.IsFinite(thickness) ? Math.Max(0, thickness) : DefaultTrackThickness;

    private double SnapValueToTick(double value)
    {
        var range = Maximum - Minimum;
        if (range <= 0)
            return Minimum;

        var clamped = Math.Clamp(value, Minimum, Maximum);
        if (!IsSnapToTickEnabled || TickFrequency <= 0)
            return clamped;

        var steps = Math.Round((clamped - Minimum) / TickFrequency, MidpointRounding.AwayFromZero);
        var snapped = Minimum + steps * TickFrequency;
        return Math.Clamp(snapped, Minimum, Maximum);
    }
}

/// <summary>
/// Automation peer for the <see cref="CircularSlider"/> control.
/// </summary>
public class CircularSliderAutomationPeer : ControlAutomationPeer, IRangeValueProvider
{
    private readonly CircularSlider _owner;

    /// <summary>
    /// Initializes a new instance of the <see cref="CircularSliderAutomationPeer"/> class.
    /// </summary>
    public CircularSliderAutomationPeer(CircularSlider owner) : base(owner)
    {
        _owner = owner;
    }

    /// <inheritdoc/>
    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Slider;

    /// <inheritdoc/>
    protected override string GetClassNameCore() => nameof(CircularSlider);

    /// <inheritdoc/>
    protected override string GetNameCore()
    {
        var name = base.GetNameCore();
        if (!string.IsNullOrWhiteSpace(name))
            return name;

        var format = _owner.ValueStringFormat;
        var minimum = _owner.Minimum;
        var maximum = Math.Max(_owner.Minimum, _owner.Maximum);
        return $"Value {_owner.Value.ToString(format)} ({minimum.ToString(format)} to {maximum.ToString(format)})";
    }

    bool IRangeValueProvider.IsReadOnly => !_owner.IsEnabled;

    double IRangeValueProvider.Minimum => _owner.Minimum;

    double IRangeValueProvider.Maximum => Math.Max(_owner.Minimum, _owner.Maximum);

    double IRangeValueProvider.Value => _owner.Value;

    double IRangeValueProvider.LargeChange => _owner.GetLargeInteractionChange();

    double IRangeValueProvider.SmallChange => _owner.GetSmallInteractionChange();

    void IRangeValueProvider.SetValue(double value)
    {
        if (!_owner.IsEnabled)
            throw new ElementNotEnabledException();

        _owner.Value = value;
    }

}
