using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Automation.Peers;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// A circular slider control that allows users to select a value by dragging around a circular arc.
/// Supports customizable angles, themes, and value formatting.
/// </summary>
[PseudoClasses(":minimum", ":maximum")]
public class CircularSlider : TemplatedControl
{
    private Border? _thumbContainer;
    private ContentPresenter? _centerContent;
    private bool _isDragging;
    
    private StreamGeometry? _inactiveGeometryCache;
    private IBrush? _activeBrushCache;

    /// <summary>
    /// Defines the <see cref="MinValue"/> property.
    /// </summary>
    public static readonly StyledProperty<double> MinValueProperty = 
        AvaloniaProperty.Register<CircularSlider, double>(nameof(MinValue), 0.0);

    /// <summary>
    /// Defines the <see cref="MaxValue"/> property.
    /// </summary>
    public static readonly StyledProperty<double> MaxValueProperty = 
        AvaloniaProperty.Register<CircularSlider, double>(nameof(MaxValue), 100.0);

    /// <summary>
    /// Defines the <see cref="Value"/> property.
    /// </summary>
    public static readonly StyledProperty<double> ValueProperty = 
        AvaloniaProperty.Register<CircularSlider, double>(nameof(Value), 0.0, coerce: CoerceValue);

    /// <summary>
    /// Defines the <see cref="StepFrequency"/> property.
    /// </summary>
    public static readonly StyledProperty<double> StepFrequencyProperty = 
        AvaloniaProperty.Register<CircularSlider, double>(nameof(StepFrequency), 0.0);

    /// <summary>
    /// Defines the <see cref="ValueFormat"/> property.
    /// </summary>
    public static readonly StyledProperty<string> ValueFormatProperty = 
        AvaloniaProperty.Register<CircularSlider, string>(nameof(ValueFormat), "F0");

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
    /// Defines the <see cref="TrackBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush> TrackBrushProperty = 
        AvaloniaProperty.Register<CircularSlider, IBrush>(nameof(TrackBrush), Brushes.Transparent);

    /// <summary>
    /// Defines the <see cref="InactiveBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush> InactiveBrushProperty = 
        AvaloniaProperty.Register<CircularSlider, IBrush>(nameof(InactiveBrush), new SolidColorBrush(Color.Parse("#E0E0E0")));

    /// <summary>
    /// Defines the <see cref="ActiveBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush> ActiveBrushProperty = 
        AvaloniaProperty.Register<CircularSlider, IBrush>(nameof(ActiveBrush), new SolidColorBrush(Color.Parse("#2196F3")));

    /// <summary>
    /// Defines the <see cref="ThumbBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush> ThumbBrushProperty = 
        AvaloniaProperty.Register<CircularSlider, IBrush>(nameof(ThumbBrush), new SolidColorBrush(Color.Parse("#1976D2")));

    /// <summary>
    /// Defines the <see cref="InnerBackground"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush> InnerBackgroundProperty = 
        AvaloniaProperty.Register<CircularSlider, IBrush>(nameof(InnerBackground), Brushes.White);

    /// <summary>
    /// Defines the <see cref="InactiveThickness"/> property.
    /// </summary>
    public static readonly StyledProperty<double> InactiveThicknessProperty = 
        AvaloniaProperty.Register<CircularSlider, double>(nameof(InactiveThickness), 12.0);

    /// <summary>
    /// Defines the <see cref="ActiveThickness"/> property.
    /// </summary>
    public static readonly StyledProperty<double> ActiveThicknessProperty = 
        AvaloniaProperty.Register<CircularSlider, double>(nameof(ActiveThickness), 12.0);

    /// <summary>
    /// Defines the <see cref="InactiveStrokeLineCap"/> property.
    /// </summary>
    public static readonly StyledProperty<PenLineCap> InactiveStrokeLineCapProperty = 
        AvaloniaProperty.Register<CircularSlider, PenLineCap>(nameof(InactiveStrokeLineCap), PenLineCap.Round);

    /// <summary>
    /// Defines the <see cref="ActiveStrokeLineCap"/> property.
    /// </summary>
    public static readonly StyledProperty<PenLineCap> ActiveStrokeLineCapProperty = 
        AvaloniaProperty.Register<CircularSlider, PenLineCap>(nameof(ActiveStrokeLineCap), PenLineCap.Round);

    /// <summary>
    /// Defines the <see cref="ActiveRadiusDelta"/> property.
    /// </summary>
    public static readonly StyledProperty<double?> ActiveRadiusDeltaProperty = 
        AvaloniaProperty.Register<CircularSlider, double?>(nameof(ActiveRadiusDelta));

    /// <summary>
    /// Defines the <see cref="ThumbSize"/> property.
    /// </summary>
    public static readonly StyledProperty<double> ThumbSizeProperty = 
        AvaloniaProperty.Register<CircularSlider, double>(nameof(ThumbSize), 20.0);
    
    /// <summary>
    /// Defines the <see cref="TextBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush> TextBrushProperty = 
        AvaloniaProperty.Register<CircularSlider, IBrush>(nameof(TextBrush), Brushes.Black);

    /// <summary>
    /// Defines the <see cref="TextFontSize"/> property.
    /// </summary>
    public static readonly StyledProperty<double> TextFontSizeProperty = 
        AvaloniaProperty.Register<CircularSlider, double>(nameof(TextFontSize), 24.0);

    /// <summary>
    /// Defines the <see cref="TextFontWeight"/> property.
    /// </summary>
    public static readonly StyledProperty<FontWeight> TextFontWeightProperty = 
        AvaloniaProperty.Register<CircularSlider, FontWeight>(nameof(TextFontWeight), FontWeight.Normal);

    /// <summary>
    /// Defines the <see cref="ThumbContent"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> ThumbContentProperty = 
        AvaloniaProperty.Register<CircularSlider, object?>(nameof(ThumbContent));

    /// <summary>
    /// Defines the <see cref="ThumbContentTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ThumbContentTemplateProperty = 
        AvaloniaProperty.Register<CircularSlider, IDataTemplate?>(nameof(ThumbContentTemplate));

    /// <summary>
    /// Defines the <see cref="CenterContent"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> CenterContentProperty = 
        AvaloniaProperty.Register<CircularSlider, object?>(nameof(CenterContent));

    /// <summary>
    /// Defines the <see cref="CenterContentTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> CenterContentTemplateProperty = 
        AvaloniaProperty.Register<CircularSlider, IDataTemplate?>(nameof(CenterContentTemplate));

    /// <summary>
    /// Defines the <see cref="DragStartedCommand"/> property.
    /// </summary>
    public static readonly StyledProperty<ICommand?> DragStartedCommandProperty = 
        AvaloniaProperty.Register<CircularSlider, ICommand?>(nameof(DragStartedCommand));

    /// <summary>
    /// Defines the <see cref="DragCompletedCommand"/> property.
    /// </summary>
    public static readonly StyledProperty<ICommand?> DragCompletedCommandProperty = 
        AvaloniaProperty.Register<CircularSlider, ICommand?>(nameof(DragCompletedCommand));

    /// <summary>
    /// Defines the <see cref="ValueChangedCommand"/> property.
    /// </summary>
    public static readonly StyledProperty<ICommand?> ValueChangedCommandProperty = 
        AvaloniaProperty.Register<CircularSlider, ICommand?>(nameof(ValueChangedCommand));

    /// <summary>
    /// Defines the <see cref="DragStartedCommandParameter"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> DragStartedCommandParameterProperty = 
        AvaloniaProperty.Register<CircularSlider, object?>(nameof(DragStartedCommandParameter));

    /// <summary>
    /// Defines the <see cref="DragCompletedCommandParameter"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> DragCompletedCommandParameterProperty = 
        AvaloniaProperty.Register<CircularSlider, object?>(nameof(DragCompletedCommandParameter));

    /// <summary>
    /// Defines the <see cref="ValueChangedCommandParameter"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> ValueChangedCommandParameterProperty = 
        AvaloniaProperty.Register<CircularSlider, object?>(nameof(ValueChangedCommandParameter));

    /// <summary>
    /// Gets or sets the minimum value of the slider.
    /// </summary>
    public double MinValue { get => GetValue(MinValueProperty); set => SetValue(MinValueProperty, value); }

    /// <summary>
    /// Gets or sets the maximum value of the slider.
    /// </summary>
    public double MaxValue { get => GetValue(MaxValueProperty); set => SetValue(MaxValueProperty, value); }

    /// <summary>
    /// Gets or sets the current value of the slider.
    /// </summary>
    public double Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }

    /// <summary>
    /// Gets or sets the step frequency for value changes.
    /// </summary>
    public double StepFrequency { get => GetValue(StepFrequencyProperty); set => SetValue(StepFrequencyProperty, value); }

    /// <summary>
    /// Gets or sets the format string used to display the value.
    /// </summary>
    public string ValueFormat { get => GetValue(ValueFormatProperty); set => SetValue(ValueFormatProperty, value); }

    /// <summary>
    /// Gets or sets the start angle of the arc in degrees.
    /// </summary>
    public double StartAngle { get => GetValue(StartAngleProperty); set => SetValue(StartAngleProperty, value); }

    /// <summary>
    /// Gets or sets the end angle of the arc in degrees.
    /// </summary>
    public double EndAngle { get => GetValue(EndAngleProperty); set => SetValue(EndAngleProperty, value); }

    /// <summary>
    /// Gets or sets the brush for the active (filled) portion of the arc.
    /// </summary>
    public IBrush ActiveBrush { get => GetValue(ActiveBrushProperty); set => SetValue(ActiveBrushProperty, value); }

    /// <summary>
    /// Gets or sets the brush for the inactive (unfilled) portion of the arc.
    /// </summary>
    public IBrush InactiveBrush { get => GetValue(InactiveBrushProperty); set => SetValue(InactiveBrushProperty, value); }

    /// <summary>
    /// Gets or sets the brush for the thumb.
    /// </summary>
    public IBrush ThumbBrush { get => GetValue(ThumbBrushProperty); set => SetValue(ThumbBrushProperty, value); }

    /// <summary>
    /// Gets or sets the brush for the inner background circle.
    /// </summary>
    public IBrush InnerBackground { get => GetValue(InnerBackgroundProperty); set => SetValue(InnerBackgroundProperty, value); }

    /// <summary>
    /// Gets or sets the brush for the track background.
    /// </summary>
    public IBrush TrackBrush { get => GetValue(TrackBrushProperty); set => SetValue(TrackBrushProperty, value); }

    /// <summary>
    /// Gets or sets the thickness of the inactive arc stroke.
    /// </summary>
    public double InactiveThickness { get => GetValue(InactiveThicknessProperty); set => SetValue(InactiveThicknessProperty, value); }

    /// <summary>
    /// Gets or sets the thickness of the active arc stroke.
    /// </summary>
    public double ActiveThickness { get => GetValue(ActiveThicknessProperty); set => SetValue(ActiveThicknessProperty, value); }

    /// <summary>
    /// Gets or sets the line cap style for the inactive arc.
    /// </summary>
    public PenLineCap InactiveStrokeLineCap { get => GetValue(InactiveStrokeLineCapProperty); set => SetValue(InactiveStrokeLineCapProperty, value); }

    /// <summary>
    /// Gets or sets the line cap style for the active arc.
    /// </summary>
    public PenLineCap ActiveStrokeLineCap { get => GetValue(ActiveStrokeLineCapProperty); set => SetValue(ActiveStrokeLineCapProperty, value); }

    /// <summary>
    /// Gets or sets the radius offset for the active arc relative to the inactive arc.
    /// </summary>
    public double? ActiveRadiusDelta { get => GetValue(ActiveRadiusDeltaProperty); set => SetValue(ActiveRadiusDeltaProperty, value); }

    /// <summary>
    /// Gets or sets the size of the thumb.
    /// </summary>
    public double ThumbSize { get => GetValue(ThumbSizeProperty); set => SetValue(ThumbSizeProperty, value); }

    /// <summary>
    /// Gets or sets the content to display inside the thumb.
    /// </summary>
    public object? ThumbContent { get => GetValue(ThumbContentProperty); set => SetValue(ThumbContentProperty, value); }

    /// <summary>
    /// Gets or sets the template for the thumb content.
    /// </summary>
    public IDataTemplate? ThumbContentTemplate { get => GetValue(ThumbContentTemplateProperty); set => SetValue(ThumbContentTemplateProperty, value); }

    /// <summary>
    /// Gets or sets the content to display in the center of the slider.
    /// </summary>
    public object? CenterContent { get => GetValue(CenterContentProperty); set => SetValue(CenterContentProperty, value); }

    /// <summary>
    /// Gets or sets the template for the center content.
    /// </summary>
    public IDataTemplate? CenterContentTemplate { get => GetValue(CenterContentTemplateProperty); set => SetValue(CenterContentTemplateProperty, value); }

    /// <summary>
    /// Gets or sets the brush for the center text.
    /// </summary>
    public IBrush TextBrush { get => GetValue(TextBrushProperty); set => SetValue(TextBrushProperty, value); }

    /// <summary>
    /// Gets or sets the font size for the center text.
    /// </summary>
    public double TextFontSize { get => GetValue(TextFontSizeProperty); set => SetValue(TextFontSizeProperty, value); }

    /// <summary>
    /// Gets or sets the font weight for the center text.
    /// </summary>
    public FontWeight TextFontWeight { get => GetValue(TextFontWeightProperty); set => SetValue(TextFontWeightProperty, value); }

    /// <summary>
    /// Gets or sets the command executed when dragging starts.
    /// </summary>
    public ICommand? DragStartedCommand { get => GetValue(DragStartedCommandProperty); set => SetValue(DragStartedCommandProperty, value); }

    /// <summary>
    /// Gets or sets the command executed when dragging completes.
    /// </summary>
    public ICommand? DragCompletedCommand { get => GetValue(DragCompletedCommandProperty); set => SetValue(DragCompletedCommandProperty, value); }

    /// <summary>
    /// Gets or sets the command executed when the value changes.
    /// </summary>
    public ICommand? ValueChangedCommand { get => GetValue(ValueChangedCommandProperty); set => SetValue(ValueChangedCommandProperty, value); }

    /// <summary>
    /// Gets or sets the parameter for the drag started command.
    /// </summary>
    public object? DragStartedCommandParameter { get => GetValue(DragStartedCommandParameterProperty); set => SetValue(DragStartedCommandParameterProperty, value); }

    /// <summary>
    /// Gets or sets the parameter for the drag completed command.
    /// </summary>
    public object? DragCompletedCommandParameter { get => GetValue(DragCompletedCommandParameterProperty); set => SetValue(DragCompletedCommandParameterProperty, value); }

    /// <summary>
    /// Gets or sets the parameter for the value changed command.
    /// </summary>
    public object? ValueChangedCommandParameter { get => GetValue(ValueChangedCommandParameterProperty); set => SetValue(ValueChangedCommandParameterProperty, value); }

    /// <summary>
    /// Occurs when dragging starts.
    /// </summary>
    public event EventHandler? DragStarted;

    /// <summary>
    /// Occurs when dragging completes.
    /// </summary>
    public event EventHandler? DragCompleted;

    /// <summary>
    /// Occurs when the value changes.
    /// </summary>
    public event EventHandler<ValueChangedEventArgs>? ValueChanged;

    static CircularSlider()
    {
        AffectsRender<CircularSlider>(
            ValueProperty, StartAngleProperty, EndAngleProperty,
            TrackBrushProperty, InactiveBrushProperty, ActiveBrushProperty,
            InnerBackgroundProperty, InactiveThicknessProperty, ActiveThicknessProperty,
            InactiveStrokeLineCapProperty, ActiveStrokeLineCapProperty, ActiveRadiusDeltaProperty,
            ThumbSizeProperty);

        AffectsArrange<CircularSlider>(ThumbSizeProperty, ValueProperty, StartAngleProperty, EndAngleProperty);

        MinValueProperty.Changed.AddClassHandler<CircularSlider>((o, _) => o.CoerceValue(ValueProperty));
        MaxValueProperty.Changed.AddClassHandler<CircularSlider>((o, _) => o.CoerceValue(ValueProperty));

        FocusableProperty.OverrideDefaultValue<CircularSlider>(true);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CircularSlider"/> class.
    /// </summary>
    public CircularSlider()
    {
        UpdatePseudoClasses();
    }

    /// <inheritdoc/>
    protected override AutomationPeer OnCreateAutomationPeer() => new CircularSliderAutomationPeer(this);

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _thumbContainer = e.NameScope.Find<Border>("PART_Thumb");
        _centerContent = e.NameScope.Find<ContentPresenter>("PART_CenterContent");
        UpdateThumbPosition();
        UpdateCenterContent();
        UpdateActiveBrushCache();
    }

    private void UpdateActiveBrushCache()
    {
        if (ActiveBrush is ConicGradientBrush conic)
        {
            var rotation = StartAngle - 90;
            var newBrush = new ConicGradientBrush
            {
                GradientStops = conic.GradientStops,
                Center = conic.Center,
                Opacity = conic.Opacity,
                Transform = new RotateTransform(rotation)
            };
            _activeBrushCache = newBrush;
        }
        else
        {
            _activeBrushCache = ActiveBrush;
        }
    }

    private double CalculateRadius(Size availableSize)
    {
        var maxThickness = Math.Max(InactiveThickness, ActiveThickness);
        var maxElement = Math.Max(maxThickness, ThumbSize);
        if (ActiveRadiusDelta.HasValue && ActiveRadiusDelta.Value > 0)
            maxElement += ActiveRadiusDelta.Value * 2;
        
        return (Math.Min(availableSize.Width, availableSize.Height) - maxElement) / 2;
    }

    private static double CoerceValue(AvaloniaObject sender, double value)
    {
        if (sender is CircularSlider slider)
        {
            if (slider.MaxValue < slider.MinValue) return slider.MinValue;
            return Math.Clamp(value, slider.MinValue, slider.MaxValue);
        }
        return value;
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == BoundsProperty ||
            change.Property == StartAngleProperty ||
            change.Property == EndAngleProperty ||
            change.Property == InactiveThicknessProperty ||
            change.Property == ActiveThicknessProperty || 
            change.Property == ThumbSizeProperty ||
            change.Property == ActiveRadiusDeltaProperty)
        {
            _inactiveGeometryCache = null;
        }

        if (change.Property == ActiveBrushProperty || change.Property == StartAngleProperty)
        {
            UpdateActiveBrushCache();
        }

        if (change.Property == ValueProperty)
        {
            UpdateThumbPosition();
            UpdateCenterContent();
            UpdatePseudoClasses();

            var args = new ValueChangedEventArgs((double)change.OldValue!, (double)change.NewValue!);
            ValueChanged?.Invoke(this, args);

            if (ValueChangedCommand?.CanExecute(ValueChangedCommandParameter ?? args) == true)
                ValueChangedCommand.Execute(ValueChangedCommandParameter ?? args);
        }
        else if (change.Property == BoundsProperty)
        {
            UpdateThumbPosition();
        }
        else if (change.Property == CenterContentProperty || change.Property == CenterContentTemplateProperty)
        {
            UpdateCenterContent();
        }
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!IsEnabled) return;
        
        var point = e.GetCurrentPoint(this);
        if (point.Properties.IsLeftButtonPressed)
        {
            _isDragging = true;
            e.Pointer.Capture(this);
            UpdateValueFromPoint(e.GetPosition(this));
            Focus();
            DragStarted?.Invoke(this, EventArgs.Empty);
            if (DragStartedCommand?.CanExecute(DragStartedCommandParameter) == true)
                DragStartedCommand.Execute(DragStartedCommandParameter);
            e.Handled = true;
        }
    }

    /// <inheritdoc/>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_isDragging)
        {
            UpdateValueFromPoint(e.GetPosition(this));
            e.Handled = true;
        }
    }

    /// <inheritdoc/>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (_isDragging)
        {
            _isDragging = false;
            e.Pointer.Capture(null);
            DragCompleted?.Invoke(this, EventArgs.Empty);
            if (DragCompletedCommand?.CanExecute(DragCompletedCommandParameter) == true)
                DragCompletedCommand.Execute(DragCompletedCommandParameter);
            e.Handled = true;
        }
    }

    /// <inheritdoc/>
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        if (!IsEnabled || e.Handled) return;
        
        var range = MaxValue - MinValue;
        if (range <= 0) return;
        
        var step = StepFrequency > 0 ? StepFrequency : range / 100.0;
        if (step < 0.01) step = 1.0;

        if (e.Delta.Y > 0)
            Value = Math.Min(MaxValue, Value + step);
        else
            Value = Math.Max(MinValue, Value - step);

        e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!IsEnabled) return;
        
        var range = MaxValue - MinValue;
        if (range <= 0) return;
        
        var step = StepFrequency > 0 ? StepFrequency : range / 100.0;
        if (step <= 0) step = 1;

        switch (e.Key)
        {
            case Key.Left:
            case Key.Down:
                Value -= step;
                e.Handled = true;
                break;
            case Key.Right:
            case Key.Up:
                Value += step;
                e.Handled = true;
                break;
            case Key.Home:
                Value = MinValue;
                e.Handled = true;
                break;
            case Key.End:
                Value = MaxValue;
                e.Handled = true;
                break;
            case Key.PageDown:
                Value -= step * 10;
                e.Handled = true;
                break;
            case Key.PageUp:
                Value += step * 10;
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
        var maxThickness = Math.Max(InactiveThickness, ActiveThickness);

        var innerRadius = radius - maxThickness / 2;
        if (innerRadius > 0 && InnerBackground != null)
        {
            context.DrawEllipse(InnerBackground, null, center, innerRadius, innerRadius);
        }

        if (_inactiveGeometryCache == null)
        {
            _inactiveGeometryCache = new StreamGeometry();
            using var ctx = _inactiveGeometryCache.Open();
            DrawArcStream(ctx, center, radius, StartAngle, EndAngle);
        }
        context.DrawGeometry(null, new Pen(InactiveBrush, InactiveThickness, lineCap: InactiveStrokeLineCap), _inactiveGeometryCache);

        var normalizedValue = (Value - MinValue) / (MaxValue - MinValue);
        if (normalizedValue > 0.001)
        {
            var angleRange = GetAngleRange();
            var valueAngle = StartAngle + normalizedValue * angleRange;
            
            var activeRadius = radius;
            if (ActiveRadiusDelta.HasValue) activeRadius += ActiveRadiusDelta.Value;

            var activeGeo = new StreamGeometry();
            using (var ctx = activeGeo.Open())
            {
                DrawArcStream(ctx, center, activeRadius, StartAngle, valueAngle);
            }
            
            var brushToUse = _activeBrushCache ?? ActiveBrush;
            context.DrawGeometry(null, new Pen(brushToUse, ActiveThickness, lineCap: ActiveStrokeLineCap), activeGeo);
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
        var range = MaxValue - MinValue;
        if (range <= 0) return;
        
        var rawValue = MinValue + normalizedValue * range;

        if (StepFrequency > 0)
        {
            var steps = Math.Round((rawValue - MinValue) / StepFrequency);
            rawValue = MinValue + steps * StepFrequency;
        }

        Value = rawValue;
    }


    private void UpdateThumbPosition()
    {
        if (_thumbContainer == null || Bounds.Width == 0 || Bounds.Height == 0) return;

        var normalizedValue = (Value - MinValue) / (MaxValue - MinValue);
        if (double.IsNaN(normalizedValue)) normalizedValue = 0;

        var angleRange = GetAngleRange();
        var valueAngle = StartAngle + normalizedValue * angleRange;

        var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
        var radius = CalculateRadius(Bounds.Size);

        if (ActiveRadiusDelta.HasValue)
            radius += ActiveRadiusDelta.Value;

        var angleRad = (valueAngle - 90) * Math.PI / 180.0;
        var thumbX = center.X + radius * Math.Cos(angleRad) - ThumbSize / 2;
        var thumbY = center.Y + radius * Math.Sin(angleRad) - ThumbSize / 2;

        Canvas.SetLeft(_thumbContainer, thumbX);
        Canvas.SetTop(_thumbContainer, thumbY);

        _thumbContainer.IsVisible = true;
    }

    private void UpdateCenterContent()
    {
        if (_centerContent == null) return;
        if (CenterContent == null && CenterContentTemplate == null)
        {
            _centerContent.Content = Value.ToString(ValueFormat);
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":minimum", Value <= MinValue);
        PseudoClasses.Set(":maximum", Value >= MaxValue);
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
}

/// <summary>
/// Event arguments for the <see cref="CircularSlider.ValueChanged"/> event.
/// </summary>
public class ValueChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the previous value.
    /// </summary>
    public double OldValue { get; }

    /// <summary>
    /// Gets the new value.
    /// </summary>
    public double NewValue { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueChangedEventArgs"/> class.
    /// </summary>
    public ValueChangedEventArgs(double oldValue, double newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }
}

/// <summary>
/// Automation peer for the <see cref="CircularSlider"/> control.
/// </summary>
public class CircularSliderAutomationPeer : ControlAutomationPeer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CircularSliderAutomationPeer"/> class.
    /// </summary>
    public CircularSliderAutomationPeer(CircularSlider owner) : base(owner) { }

    /// <inheritdoc/>
    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Slider;
}
