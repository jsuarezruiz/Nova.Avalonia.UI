using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// A circular fortune wheel control that can be spun to select random items.
/// Supports keyboard interaction (Space or Enter to spin) and full accessibility.
/// </summary>
public class FortuneWheel : TemplatedControl
{
    private readonly Random _random = new();
    private CancellationTokenSource? _animationCts;

    /// <summary>
    /// Defines the <see cref="Items"/> property.
    /// </summary>
    public static readonly StyledProperty<ObservableCollection<FortuneItem>> ItemsProperty =
        AvaloniaProperty.Register<FortuneWheel, ObservableCollection<FortuneItem>>(nameof(Items));

    /// <summary>
    /// Defines the <see cref="SelectedIndex"/> property.
    /// </summary>
    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<FortuneWheel, int>(
            nameof(SelectedIndex),
            defaultValue: 0,
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Defines the <see cref="RotationAngle"/> property.
    /// </summary>
    public static readonly StyledProperty<double> RotationAngleProperty =
        AvaloniaProperty.Register<FortuneWheel, double>(
            nameof(RotationAngle),
            defaultValue: 0.0);

    /// <summary>
    /// Defines the <see cref="AnimationDuration"/> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> AnimationDurationProperty =
        AvaloniaProperty.Register<FortuneWheel, TimeSpan>(
            nameof(AnimationDuration),
            defaultValue: TimeSpan.FromSeconds(3));

    /// <summary>
    /// Defines the <see cref="MinimumSpins"/> property.
    /// </summary>
    public static readonly StyledProperty<int> MinimumSpinsProperty =
        AvaloniaProperty.Register<FortuneWheel, int>(
            nameof(MinimumSpins),
            defaultValue: 3);

    /// <summary>
    /// Defines the <see cref="StyleStrategy"/> property.
    /// </summary>
    public static readonly StyledProperty<IStyleStrategy> StyleStrategyProperty =
        AvaloniaProperty.Register<FortuneWheel, IStyleStrategy>(
            nameof(StyleStrategy),
            defaultValue: new AlternatingStyleStrategy());

    /// <summary>
    /// Defines the <see cref="IsSpinning"/> property.
    /// </summary>
    public static readonly DirectProperty<FortuneWheel, bool> IsSpinningProperty =
        AvaloniaProperty.RegisterDirect<FortuneWheel, bool>(
            nameof(IsSpinning),
            o => o.IsSpinning);

    /// <summary>
    /// Defines the <see cref="ShowIndicator"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowIndicatorProperty =
        AvaloniaProperty.Register<FortuneWheel, bool>(
            nameof(ShowIndicator),
            defaultValue: true);

    /// <summary>
    /// Defines the <see cref="IndicatorPosition"/> property.
    /// </summary>
    public static readonly StyledProperty<IndicatorPosition> IndicatorPositionProperty =
        AvaloniaProperty.Register<FortuneWheel, IndicatorPosition>(
            nameof(IndicatorPosition),
            defaultValue: Controls.IndicatorPosition.Top);

    /// <summary>
    /// Defines the <see cref="IndicatorFill"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> IndicatorFillProperty =
        AvaloniaProperty.Register<FortuneWheel, IBrush?>(
            nameof(IndicatorFill),
            defaultValue: Brushes.Red);

    /// <summary>
    /// Defines the <see cref="IndicatorSize"/> property.
    /// </summary>
    public static readonly StyledProperty<double> IndicatorSizeProperty =
        AvaloniaProperty.Register<FortuneWheel, double>(
            nameof(IndicatorSize),
            defaultValue: 24.0);

    /// <summary>
    /// Defines the <see cref="CenterRadius"/> property.
    /// </summary>
    public static readonly StyledProperty<double> CenterRadiusProperty =
        AvaloniaProperty.Register<FortuneWheel, double>(
            nameof(CenterRadius),
            defaultValue: 30.0);

    /// <summary>
    /// Defines the <see cref="CenterFill"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> CenterFillProperty =
        AvaloniaProperty.Register<FortuneWheel, IBrush?>(
            nameof(CenterFill),
            defaultValue: Brushes.White);

    private const double ContentRadiusRatio = 0.65;
    private const double ImageSizeRatio = 0.25;
    private const double TextMaxWidthRatio = 0.35;

    private bool _isSpinning;

    /// <summary>
    /// Gets or sets the collection of items to display on the wheel.
    /// </summary>
    public ObservableCollection<FortuneItem> Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    /// <summary>
    /// Gets or sets the index of the currently selected item.
    /// </summary>
    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    /// <summary>
    /// Gets or sets the current rotation angle of the wheel in degrees.
    /// </summary>
    public double RotationAngle
    {
        get => GetValue(RotationAngleProperty);
        set => SetValue(RotationAngleProperty, value);
    }

    /// <summary>
    /// Gets or sets the duration of the spin animation.
    /// </summary>
    public TimeSpan AnimationDuration
    {
        get => GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum number of full rotations during a spin.
    /// </summary>
    public int MinimumSpins
    {
        get => GetValue(MinimumSpinsProperty);
        set => SetValue(MinimumSpinsProperty, value);
    }

    /// <summary>
    /// Gets or sets the strategy used to style individual items.
    /// </summary>
    public IStyleStrategy StyleStrategy
    {
        get => GetValue(StyleStrategyProperty);
        set => SetValue(StyleStrategyProperty, value);
    }

    /// <summary>
    /// Gets a value indicating whether the wheel is currently spinning.
    /// </summary>
    public bool IsSpinning
    {
        get => _isSpinning;
        private set => SetAndRaise(IsSpinningProperty, ref _isSpinning, value);
    }

    /// <summary>
    /// Gets or sets whether to show the indicator pointer.
    /// </summary>
    public bool ShowIndicator
    {
        get => GetValue(ShowIndicatorProperty);
        set => SetValue(ShowIndicatorProperty, value);
    }

    /// <summary>
    /// Gets or sets the position of the indicator.
    /// </summary>
    public IndicatorPosition IndicatorPosition
    {
        get => GetValue(IndicatorPositionProperty);
        set => SetValue(IndicatorPositionProperty, value);
    }

    /// <summary>
    /// Gets or sets the fill brush for the indicator.
    /// </summary>
    public IBrush? IndicatorFill
    {
        get => GetValue(IndicatorFillProperty);
        set => SetValue(IndicatorFillProperty, value);
    }

    /// <summary>
    /// Gets or sets the size of the indicator.
    /// </summary>
    public double IndicatorSize
    {
        get => GetValue(IndicatorSizeProperty);
        set => SetValue(IndicatorSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the radius of the center circle.
    /// </summary>
    public double CenterRadius
    {
        get => GetValue(CenterRadiusProperty);
        set => SetValue(CenterRadiusProperty, value);
    }

    /// <summary>
    /// Gets or sets the fill brush for the center circle.
    /// </summary>
    public IBrush? CenterFill
    {
        get => GetValue(CenterFillProperty);
        set => SetValue(CenterFillProperty, value);
    }

    /// <summary>
    /// Occurs when a spin animation starts.
    /// </summary>
    public event EventHandler<FortuneSelectionEventArgs>? SpinStarted;

    /// <summary>
    /// Occurs when a spin animation completes.
    /// </summary>
    public event EventHandler<FortuneSelectionEventArgs>? SpinCompleted;

    static FortuneWheel()
    {
        AffectsRender<FortuneWheel>(
            ItemsProperty,
            RotationAngleProperty,
            StyleStrategyProperty,
            ShowIndicatorProperty,
            IndicatorPositionProperty,
            IndicatorFillProperty,
            IndicatorSizeProperty,
            CenterRadiusProperty,
            CenterFillProperty);

        FocusableProperty.OverrideDefaultValue<FortuneWheel>(true);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == StyleStrategyProperty || change.Property == ItemsProperty)
        {
            InvalidateCaches();
            if (change.Property == StyleStrategyProperty)
            {
                if (change.OldValue is System.ComponentModel.INotifyPropertyChanged oldStrategy)
                {
                    oldStrategy.PropertyChanged -= OnStrategyPropertyChanged;
                }
                if (change.NewValue is System.ComponentModel.INotifyPropertyChanged newStrategy)
                {
                    newStrategy.PropertyChanged += OnStrategyPropertyChanged;
                }
            }
        }

        if (change.Property == IsSpinningProperty || change.Property == SelectedIndexProperty)
        {
            UpdateAutomationName();
        }
    }

    private void UpdateAutomationName()
    {
        var name = IsSpinning 
            ? "Spinning wheel..." 
            : $"Wheel selected {SelectedIndex + 1}. {(SelectedIndex >= 0 && SelectedIndex < Items.Count ? Items[SelectedIndex].Content : "")}";
        AutomationProperties.SetName(this, name);
    }

    private void OnStrategyPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        InvalidateVisual();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FortuneWheel"/> class.
    /// </summary>
    public FortuneWheel()
    {
        Items = new ObservableCollection<FortuneItem>();
        
        // Subscribe to default strategy changes
        if (StyleStrategy is System.ComponentModel.INotifyPropertyChanged strategy)
        {
            strategy.PropertyChanged += OnStrategyPropertyChanged;
        }

        AutomationProperties.SetLiveSetting(this, AutomationLiveSetting.Polite);
    }

    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Handled || IsSpinning)
            return;

        if (e.Key == Key.Space || e.Key == Key.Enter)
        {
            _ = SpinAsync();
            e.Handled = true;
        }
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        CancelAnimation();
        if (StyleStrategy is System.ComponentModel.INotifyPropertyChanged strategy)
        {
            strategy.PropertyChanged -= OnStrategyPropertyChanged;
        }
    }

    /// <summary>
    /// Cancels any running animation and cleans up resources.
    /// </summary>
    public void CancelAnimation()
    {
        _animationCts?.Cancel();
        _animationCts?.Dispose();
        _animationCts = null;
    }

    /// <summary>
    /// Spins the wheel to a random item.
    /// </summary>
    /// <returns>A task that completes when the spin animation finishes.</returns>
    public async Task SpinAsync()
    {
        if (Items == null || Items.Count == 0) return;

        var targetIndex = SelectRandomIndex();
        await SpinToAsync(targetIndex);
    }

    /// <summary>
    /// Spins the wheel to a specific item index.
    /// </summary>
    /// <param name="targetIndex">The index of the target item.</param>
    /// <returns>A task that completes when the spin animation finishes.</returns>
    public async Task SpinToAsync(int targetIndex)
    {
        if (Items == null || Items.Count == 0) return;
        if (targetIndex < 0 || targetIndex >= Items.Count) return;

        _animationCts?.Cancel();
        _animationCts = new CancellationTokenSource();
        var ct = _animationCts.Token;

        try
        {
            IsSpinning = true;
            SpinStarted?.Invoke(this, new FortuneSelectionEventArgs(targetIndex, Items[targetIndex]));

            var targetRotation = CalculateTargetRotation(targetIndex);
            await AnimateRotationAsync(targetRotation, ct);

            SelectedIndex = targetIndex;
            SpinCompleted?.Invoke(this, new FortuneSelectionEventArgs(targetIndex, Items[targetIndex]));
        }
        catch (OperationCanceledException)
        {
            // Animation was cancelled
        }
        finally
        {
            IsSpinning = false;
        }
    }

    private readonly List<StreamGeometry> _cachedGeometries = new();
    private readonly List<object?> _cachedContent = new(); // Stores FormattedText or IImage
    private double _lastRadius;

    private void InvalidateCaches()
    {
        _cachedGeometries.Clear();
        _cachedContent.Clear();
        _lastRadius = 0;
    }

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        if (Items == null || Items.Count == 0) return;

        var bounds = Bounds;
        var center = new Point(bounds.Width / 2, bounds.Height / 2);
        
        // Account for indicator and border stroke to prevent clipping
        var borderPadding = StyleStrategy is AlternatingStyleStrategy alt ? alt.BorderThickness / 2 :
                            StyleStrategy is GradientStyleStrategy grad ? grad.BorderThickness / 2 : 1;
        var indicatorPadding = ShowIndicator ? IndicatorSize / 2 : 0;
        var radius = Math.Min(bounds.Width, bounds.Height) / 2 - indicatorPadding - borderPadding;

        if (radius <= 0) return;

        if (Math.Abs(radius - _lastRadius) > 0.01 || _cachedGeometries.Count != Items.Count)
        {
            UpdateCaches(center, radius);
            _lastRadius = radius;
        }

        // Draw wheel slices with rotation
        var itemCount = Items.Count;
        var anglePerSlice = 360.0 / itemCount;

        using (context.PushTransform(Matrix.CreateRotation(RotationAngle * Math.PI / 180, center)))
        {
            for (int i = 0; i < itemCount; i++)
            {
                var item = Items[i];
                var style = StyleStrategy.GetStyle(i, itemCount, item.Style);
                
                var pen = style.BorderThickness > 0 && style.BorderBrush != null
                    ? new Pen(style.BorderBrush, style.BorderThickness)
                    : null;

                context.DrawGeometry(style.Background, pen, _cachedGeometries[i]);
                
                if (i < _cachedContent.Count)
                {
                    var midAngle = (i * anglePerSlice - 90) * Math.PI / 180;
                    var contentRadius = radius * ContentRadiusRatio;
                    var contentPos = new Point(
                        center.X + contentRadius * Math.Cos(midAngle),
                        center.Y + contentRadius * Math.Sin(midAngle));

                        var textAngle = midAngle;
                        var normalizedMid = (midAngle * 180 / Math.PI + 360) % 360;
                        if (normalizedMid > 90 && normalizedMid < 270)
                        {
                            textAngle += Math.PI;
                        }

                        using (context.PushTransform(Matrix.CreateRotation(textAngle, contentPos)))
                    {
                        if (_cachedContent[i] is FormattedText formattedText)
                        {
                            var textOffset = new Point(
                                contentPos.X - formattedText.Width / 2,
                                contentPos.Y - formattedText.Height / 2);

                            context.DrawText(formattedText, textOffset);
                        }
                        else if (_cachedContent[i] is IImage image)
                        {
                            var imgSize = Math.Min(radius * ImageSizeRatio, Math.Min(image.Size.Width, image.Size.Height));
                            if (imgSize <= 0) imgSize = radius * ImageSizeRatio;

                            var destRect = new Rect(
                                contentPos.X - imgSize / 2,
                                contentPos.Y - imgSize / 2,
                                imgSize,
                                imgSize);

                            context.DrawImage(image, destRect);
                        }
                    }
                }
            }

            if (CenterRadius > 0 && CenterFill != null)
            {
                context.DrawEllipse(CenterFill, null, center, CenterRadius, CenterRadius);
            }
        }

        // Draw indicator outside rotation transform
        if (ShowIndicator)
        {
            DrawIndicator(context, center, radius);
        }
    }

    private void UpdateCaches(Point center, double radius)
    {
        _cachedGeometries.Clear();
        _cachedContent.Clear();

        var itemCount = Items.Count;
        var anglePerSlice = 360.0 / itemCount;
        var typeface = new Typeface(FontFamily, FontStyle, FontWeight.Bold);

        for (int i = 0; i < itemCount; i++)
        {
            var item = Items[i];
            var style = StyleStrategy.GetStyle(i, itemCount, item.Style);
            var startAngle = i * anglePerSlice - 90 - anglePerSlice / 2;

            // Geometry Cache
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                var startRad = startAngle * Math.PI / 180;
                var endRad = (startAngle + anglePerSlice) * Math.PI / 180;

                var startPoint = new Point(
                    center.X + radius * Math.Cos(startRad),
                    center.Y + radius * Math.Sin(startRad));

                var endPoint = new Point(
                    center.X + radius * Math.Cos(endRad),
                    center.Y + radius * Math.Sin(endRad));

                ctx.BeginFigure(center, true);
                ctx.LineTo(startPoint);
                ctx.ArcTo(endPoint, new Size(radius, radius), 0, anglePerSlice > 180, SweepDirection.Clockwise);
                ctx.LineTo(center);
                ctx.EndFigure(true);
            }
            _cachedGeometries.Add(geometry);

            // Text Cache
            if (item.Content is string text)
            {
                var contentRadius = radius * ContentRadiusRatio;
                var sliceArcLength = contentRadius * anglePerSlice * Math.PI / 180;
                var maxWidth = Math.Min(sliceArcLength * 0.8, radius * TextMaxWidthRatio);
                var fontSize = Math.Max(8, Math.Min(14, maxWidth / 4));

                var displayText = text;
                var formattedText = new FormattedText(
                    displayText,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    fontSize,
                    style.Foreground ?? Brushes.White);

                while (formattedText.Width > maxWidth && displayText.Length > 1)
                {
                    displayText = displayText[..^1];
                    formattedText = new FormattedText(
                        displayText + "…",
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        typeface,
                        fontSize,
                        style.Foreground ?? Brushes.White);
                }
                _cachedContent.Add(formattedText);
            }
            else if (item.Content is IImage image)
            {
                _cachedContent.Add(image);
            }
            else
            {
                _cachedContent.Add(null);
            }
        }
    }

    private void DrawIndicator(DrawingContext context, Point center, double radius)
    {
        var indicatorSize = IndicatorSize;
        Point indicatorPos;
        var geometry = new StreamGeometry();

        using (var ctx = geometry.Open())
        {
            switch (IndicatorPosition)
            {
                case IndicatorPosition.Top:
                    indicatorPos = new Point(center.X, center.Y - radius - indicatorSize / 4);
                    ctx.BeginFigure(new Point(indicatorPos.X, indicatorPos.Y + indicatorSize), true);
                    ctx.LineTo(new Point(indicatorPos.X - indicatorSize / 2, indicatorPos.Y));
                    ctx.LineTo(new Point(indicatorPos.X + indicatorSize / 2, indicatorPos.Y));
                    ctx.EndFigure(true);
                    break;

                case IndicatorPosition.Bottom:
                    indicatorPos = new Point(center.X, center.Y + radius + indicatorSize / 4);
                    ctx.BeginFigure(new Point(indicatorPos.X, indicatorPos.Y - indicatorSize), true);
                    ctx.LineTo(new Point(indicatorPos.X - indicatorSize / 2, indicatorPos.Y));
                    ctx.LineTo(new Point(indicatorPos.X + indicatorSize / 2, indicatorPos.Y));
                    ctx.EndFigure(true);
                    break;

                case IndicatorPosition.Left:
                    indicatorPos = new Point(center.X - radius - indicatorSize / 4, center.Y);
                    ctx.BeginFigure(new Point(indicatorPos.X + indicatorSize, indicatorPos.Y), true);
                    ctx.LineTo(new Point(indicatorPos.X, indicatorPos.Y - indicatorSize / 2));
                    ctx.LineTo(new Point(indicatorPos.X, indicatorPos.Y + indicatorSize / 2));
                    ctx.EndFigure(true);
                    break;

                case IndicatorPosition.Right:
                    indicatorPos = new Point(center.X + radius + indicatorSize / 4, center.Y);
                    ctx.BeginFigure(new Point(indicatorPos.X - indicatorSize, indicatorPos.Y), true);
                    ctx.LineTo(new Point(indicatorPos.X, indicatorPos.Y - indicatorSize / 2));
                    ctx.LineTo(new Point(indicatorPos.X, indicatorPos.Y + indicatorSize / 2));
                    ctx.EndFigure(true);
                    break;
            }
        }

        context.DrawGeometry(IndicatorFill, new Pen(Brushes.White, 2), geometry);
    }

    private int SelectRandomIndex()
    {
        if (Items == null || Items.Count == 0) return 0;

        // Use weighted random selection
        var totalWeight = 0.0;
        for (int i = 0; i < Items.Count; i++)
        {
            totalWeight += Items[i].Weight;
        }

        var randomValue = _random.NextDouble() * totalWeight;
        var cumulative = 0.0;

        for (int i = 0; i < Items.Count; i++)
        {
            cumulative += Items[i].Weight;
            if (randomValue <= cumulative)
                return i;
        }

        return Items.Count - 1;
    }

    private double CalculateTargetRotation(int targetIndex)
    {
        var itemCount = Items.Count;
        var anglePerItem = 360.0 / itemCount;

        // Calculate rotation target based on IndicatorPosition
        var targetIndicatorAngle = IndicatorPosition switch
        {
            IndicatorPosition.Top => 270.0,
            IndicatorPosition.Right => 0.0,
            IndicatorPosition.Bottom => 90.0,
            IndicatorPosition.Left => 180.0,
            _ => 270.0
        };

        var startCenterAngle = targetIndex * anglePerItem - 90;

        // Current rotation normalized to [0, 360)
        var currentRotation = RotationAngle;
        var currentNormalized = currentRotation % 360;
        if (currentNormalized < 0) currentNormalized += 360;

        var targetRotationNormalized = (targetIndicatorAngle - startCenterAngle + 360) % 360;

        // Amount of rotation needed to reach targetNormalized from currentNormalized
        var deltaRotation = (targetRotationNormalized - currentNormalized + 360) % 360;

        // Add minimum spins
        var extraFullSpins = (int)Math.Max(MinimumSpins, 1) + _random.Next(0, 3);
        var totalTargetRotation = currentRotation + deltaRotation + (extraFullSpins * 360);

        return totalTargetRotation;
    }

    private async Task AnimateRotationAsync(double targetAngle, CancellationToken ct)
    {
        var startAngle = RotationAngle;
        var totalDistance = targetAngle - startAngle;
        var duration = AnimationDuration;
        var startTime = DateTime.Now;

        while (!ct.IsCancellationRequested)
        {
            var elapsed = DateTime.Now - startTime;
            if (elapsed >= duration) break;

            var progress = elapsed.TotalMilliseconds / duration.TotalMilliseconds;
            var easedProgress = EaseOutCubic(progress);

            RotationAngle = startAngle + totalDistance * easedProgress;

            await Task.Delay(16, ct);
        }

        RotationAngle = targetAngle;
        InvalidateVisual();
    }

    private static double EaseOutCubic(double t) => 1 - Math.Pow(1 - t, 3);
}
