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
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// A horizontal or vertical fortune bar control that can be spun to select random items.
/// Supports keyboard interaction (Space or Enter to spin) and full accessibility.
/// </summary>
public class FortuneBar : TemplatedControl
{
    private readonly Random _random = new();
    private CancellationTokenSource? _animationCts;

    /// <summary>
    /// Defines the <see cref="Items"/> property.
    /// </summary>
    public static readonly StyledProperty<ObservableCollection<FortuneItem>> ItemsProperty =
        AvaloniaProperty.Register<FortuneBar, ObservableCollection<FortuneItem>>(nameof(Items));

    /// <summary>
    /// Defines the <see cref="SelectedIndex"/> property.
    /// </summary>
    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<FortuneBar, int>(
            nameof(SelectedIndex),
            defaultValue: 0,
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Defines the <see cref="ScrollOffset"/> property.
    /// </summary>
    public static readonly StyledProperty<double> ScrollOffsetProperty =
        AvaloniaProperty.Register<FortuneBar, double>(
            nameof(ScrollOffset),
            defaultValue: 0.0);

    /// <summary>
    /// Defines the <see cref="Orientation"/> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<FortuneBar, Orientation>(
            nameof(Orientation),
            defaultValue: Orientation.Horizontal);

    /// <summary>
    /// Defines the <see cref="ItemSize"/> property.
    /// </summary>
    public static readonly StyledProperty<double> ItemSizeProperty =
        AvaloniaProperty.Register<FortuneBar, double>(
            nameof(ItemSize),
            defaultValue: 100.0);

    /// <summary>
    /// Defines the <see cref="AnimationDuration"/> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> AnimationDurationProperty =
        AvaloniaProperty.Register<FortuneBar, TimeSpan>(
            nameof(AnimationDuration),
            defaultValue: TimeSpan.FromSeconds(3));

    /// <summary>
    /// Defines the <see cref="MinimumCycles"/> property.
    /// </summary>
    public static readonly StyledProperty<int> MinimumCyclesProperty =
        AvaloniaProperty.Register<FortuneBar, int>(
            nameof(MinimumCycles),
            defaultValue: 2);

    /// <summary>
    /// Defines the <see cref="StyleStrategy"/> property.
    /// </summary>
    public static readonly StyledProperty<IStyleStrategy> StyleStrategyProperty =
        AvaloniaProperty.Register<FortuneBar, IStyleStrategy>(
            nameof(StyleStrategy),
            defaultValue: new AlternatingStyleStrategy());

    /// <summary>
    /// Defines the <see cref="IsSpinning"/> property.
    /// </summary>
    public static readonly DirectProperty<FortuneBar, bool> IsSpinningProperty =
        AvaloniaProperty.RegisterDirect<FortuneBar, bool>(
            nameof(IsSpinning),
            o => o.IsSpinning);

    /// <summary>
    /// Defines the <see cref="ShowIndicator"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowIndicatorProperty =
        AvaloniaProperty.Register<FortuneBar, bool>(
            nameof(ShowIndicator),
            defaultValue: true);

    /// <summary>
    /// Defines the <see cref="IndicatorFill"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> IndicatorFillProperty =
        AvaloniaProperty.Register<FortuneBar, IBrush?>(
            nameof(IndicatorFill),
            defaultValue: Brushes.Red);

    /// <summary>
    /// Defines the <see cref="IndicatorThickness"/> property.
    /// </summary>
    public static readonly StyledProperty<double> IndicatorThicknessProperty =
        AvaloniaProperty.Register<FortuneBar, double>(
            nameof(IndicatorThickness),
            defaultValue: 4.0);

    private const double ImageSizeRatio = 0.8;
    private const double IndicatorTriangleSize = 12.0;

    private bool _isSpinning;

    /// <summary>
    /// Gets or sets the collection of items to display on the bar.
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
    /// Gets or sets the current scroll offset.
    /// </summary>
    public double ScrollOffset
    {
        get => GetValue(ScrollOffsetProperty);
        set => SetValue(ScrollOffsetProperty, value);
    }

    /// <summary>
    /// Gets or sets the orientation of the bar.
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Gets or sets the size of each item.
    /// </summary>
    public double ItemSize
    {
        get => GetValue(ItemSizeProperty);
        set => SetValue(ItemSizeProperty, value);
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
    /// Gets or sets the minimum number of full cycles during a spin.
    /// </summary>
    public int MinimumCycles
    {
        get => GetValue(MinimumCyclesProperty);
        set => SetValue(MinimumCyclesProperty, value);
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
    /// Gets a value indicating whether the bar is currently spinning.
    /// </summary>
    public bool IsSpinning
    {
        get => _isSpinning;
        private set => SetAndRaise(IsSpinningProperty, ref _isSpinning, value);
    }

    /// <summary>
    /// Gets or sets whether to show the center indicator.
    /// </summary>
    public bool ShowIndicator
    {
        get => GetValue(ShowIndicatorProperty);
        set => SetValue(ShowIndicatorProperty, value);
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
    /// Gets or sets the thickness of the indicator line.
    /// </summary>
    public double IndicatorThickness
    {
        get => GetValue(IndicatorThicknessProperty);
        set => SetValue(IndicatorThicknessProperty, value);
    }

    /// <summary>
    /// Occurs when a spin animation starts.
    /// </summary>
    public event EventHandler<FortuneSelectionEventArgs>? SpinStarted;

    /// <summary>
    /// Occurs when a spin animation completes.
    /// </summary>
    public event EventHandler<FortuneSelectionEventArgs>? SpinCompleted;

    static FortuneBar()
    {
        AffectsRender<FortuneBar>(
            ItemsProperty,
            ScrollOffsetProperty,
            OrientationProperty,
            ItemSizeProperty,
            StyleStrategyProperty,
            ShowIndicatorProperty,
            IndicatorFillProperty,
            IndicatorThicknessProperty);

        ClipToBoundsProperty.OverrideDefaultValue<FortuneBar>(true);
        FocusableProperty.OverrideDefaultValue<FortuneBar>(true);
    }

    private readonly List<object?> _cachedContent = new();

    private void InvalidateCaches()
    {
        _cachedContent.Clear();
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
            ? "Spinning bar..." 
            : $"Bar selected {SelectedIndex + 1}. {(SelectedIndex >= 0 && SelectedIndex < Items.Count ? Items[SelectedIndex].Content : "")}";
        AutomationProperties.SetName(this, name);
    }

    private void OnStrategyPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        InvalidateCaches();
        InvalidateVisual();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FortuneBar"/> class.
    /// </summary>
    public FortuneBar()
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
    /// Spins the bar to a random item.
    /// </summary>
    /// <returns>A task that completes when the spin animation finishes.</returns>
    public async Task SpinAsync()
    {
        if (Items == null || Items.Count == 0) return;

        var targetIndex = SelectRandomIndex();
        await SpinToAsync(targetIndex);
    }

    /// <summary>
    /// Spins the bar to a specific item index.
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

            var targetScroll = CalculateTargetScroll(targetIndex);
            await AnimateScrollAsync(targetScroll, ct);

            var actualIndex = GetCenteredItemIndex();
            SelectedIndex = actualIndex;
            SpinCompleted?.Invoke(this, new FortuneSelectionEventArgs(actualIndex, Items[actualIndex]));
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

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        if (Items == null || Items.Count == 0) return;

        var bounds = Bounds;
        var isHorizontal = Orientation == Orientation.Horizontal;
        var itemSize = ItemSize;
        var totalSize = Items.Count * itemSize;

        if (_cachedContent.Count != Items.Count)
        {
            UpdateCaches();
        }

        // Calculate visible range
        var viewSize = isHorizontal ? bounds.Width : bounds.Height;
        var offset = ScrollOffset % totalSize;
        if (offset < 0) offset += totalSize;

        var visibleStart = (int)Math.Floor(offset / itemSize);
        var visibleCount = (int)Math.Ceiling(viewSize / itemSize) + 2;

        // Draw items (with wrapping for infinite scroll effect)
        for (int v = -1; v <= visibleCount; v++)
        {
            var wrappedIndex = ((visibleStart + v) % Items.Count + Items.Count) % Items.Count;
            var item = Items[wrappedIndex];
            var style = StyleStrategy.GetStyle(wrappedIndex, Items.Count, item.Style);

            var position = (visibleStart + v) * itemSize - offset;
            var itemBounds = isHorizontal
                ? new Rect(position, 0, itemSize, bounds.Height)
                : new Rect(0, position, bounds.Width, itemSize);

            // Only draw if visible
            if ((isHorizontal && itemBounds.Right > 0 && itemBounds.Left < bounds.Width) ||
                (!isHorizontal && itemBounds.Bottom > 0 && itemBounds.Top < bounds.Height))
            {
                DrawItem(context, itemBounds, item, style, wrappedIndex);
            }
        }

        // Draw center indicator
        if (ShowIndicator)
        {
            DrawIndicator(context, bounds, isHorizontal);
        }
    }

    private void UpdateCaches()
    {
        _cachedContent.Clear();
        var typeface = new Typeface(FontFamily, FontStyle, FontWeight);

        for (int i = 0; i < Items.Count; i++)
        {
            var item = Items[i];
            var style = StyleStrategy.GetStyle(i, Items.Count, item.Style);
            
            if (item.Content is string text)
            {
                var formattedText = new FormattedText(
                    text,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    16,
                    style.Foreground ?? Brushes.White);
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

    /// <inheritdoc/>
    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new FortuneBarAutomationPeer(this);
    }

    private void DrawItem(DrawingContext context, Rect bounds, FortuneItem item, FortuneItemStyle style, int index)
    {
        // Draw background
        if (style.Background != null)
        {
            context.FillRectangle(style.Background, bounds);
        }

        // Draw border
        if (style.BorderThickness > 0 && style.BorderBrush != null)
        {
            context.DrawRectangle(new Pen(style.BorderBrush, style.BorderThickness), bounds);
        }

        // Draw content
        if (index < _cachedContent.Count)
        {
            var content = _cachedContent[index];
            if (content is FormattedText formattedText)
            {
                var textPos = new Point(
                    bounds.Center.X - formattedText.Width / 2,
                    bounds.Center.Y - formattedText.Height / 2);

                context.DrawText(formattedText, textPos);
            }
            else if (content is IImage image)
            {
                var imgSize = Math.Min(Math.Min(bounds.Width, bounds.Height) * ImageSizeRatio, 
                                       Math.Min(image.Size.Width, image.Size.Height));
                if (imgSize <= 0) imgSize = Math.Min(bounds.Width, bounds.Height) * ImageSizeRatio;

                var destRect = new Rect(
                    bounds.Center.X - imgSize / 2,
                    bounds.Center.Y - imgSize / 2,
                    imgSize,
                    imgSize);

                context.DrawImage(image, destRect);
            }
        }
    }

    private void DrawIndicator(DrawingContext context, Rect bounds, bool isHorizontal)
    {
        var thickness = IndicatorThickness;
        var pen = new Pen(IndicatorFill, thickness);

        if (isHorizontal)
        {
            var centerX = bounds.Width / 2;
            context.DrawLine(pen, new Point(centerX, 0), new Point(centerX, bounds.Height));

            // Draw triangles at top and bottom
            DrawIndicatorTriangle(context, new Point(centerX, 0), IndicatorTriangleSize, true);
            DrawIndicatorTriangle(context, new Point(centerX, bounds.Height), IndicatorTriangleSize, false);
        }
        else
        {
            var centerY = bounds.Height / 2;
            context.DrawLine(pen, new Point(0, centerY), new Point(bounds.Width, centerY));

            // Draw triangles at left and right
            DrawIndicatorTriangle(context, new Point(0, centerY), IndicatorTriangleSize, true, false);
            DrawIndicatorTriangle(context, new Point(bounds.Width, centerY), IndicatorTriangleSize, false, false);
        }
    }

    private void DrawIndicatorTriangle(DrawingContext context, Point tip, double size, bool pointingIn, bool vertical = true)
    {
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            if (vertical)
            {
                var y = pointingIn ? tip.Y + size : tip.Y - size;
                ctx.BeginFigure(tip, true);
                ctx.LineTo(new Point(tip.X - size / 2, y));
                ctx.LineTo(new Point(tip.X + size / 2, y));
                ctx.EndFigure(true);
            }
            else
            {
                var x = pointingIn ? tip.X + size : tip.X - size;
                ctx.BeginFigure(tip, true);
                ctx.LineTo(new Point(x, tip.Y - size / 2));
                ctx.LineTo(new Point(x, tip.Y + size / 2));
                ctx.EndFigure(true);
            }
        }

        context.DrawGeometry(IndicatorFill, null, geometry);
    }

    private int SelectRandomIndex()
    {
        if (Items == null || Items.Count == 0) return 0;

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

    /// <summary>
    /// Gets the index of the item currently under the center indicator.
    /// </summary>
    private int GetCenteredItemIndex()
    {
        if (Items == null || Items.Count == 0) return 0;

        var itemSize = ItemSize;
        var totalSize = Items.Count * itemSize;
        var viewSize = Orientation == Orientation.Horizontal ? Bounds.Width : Bounds.Height;

        // The center of the view
        var centerPosition = viewSize / 2;

        // Normalize scroll offset
        var offset = ScrollOffset % totalSize;
        if (offset < 0) offset += totalSize;

        var virtualIndex = (int)Math.Floor((centerPosition + offset) / itemSize);

        // Wrap to actual item index
        return ((virtualIndex % Items.Count) + Items.Count) % Items.Count;
    }

    private double CalculateTargetScroll(int targetIndex)
    {
        var itemSize = ItemSize;
        var totalItems = Items.Count;
        var totalSize = totalItems * itemSize;

        // Calculate scroll offset to center the target item
        var viewSize = Orientation == Orientation.Horizontal ? Bounds.Width : Bounds.Height;
        var centerPosition = viewSize / 2;
        var targetScrollBase = targetIndex * itemSize - centerPosition + itemSize / 2;

        // Normalize to positive value
        while (targetScrollBase < 0)
        {
            targetScrollBase += totalSize;
        }

        // Calculate current offset relative to totalSize
        var currentOffset = ScrollOffset % totalSize;
        if (currentOffset < 0) currentOffset += totalSize;

        // Delta to target
        var deltaOffset = targetScrollBase - currentOffset;

        var extraSpins = Math.Max(MinimumCycles, 1) + _random.Next(0, 3);
        var totalTargetOffset = ScrollOffset + deltaOffset + (extraSpins * totalSize);

        // Ensure we're always scrolling forward
        while (totalTargetOffset <= ScrollOffset)
        {
            totalTargetOffset += totalSize;
        }

        return totalTargetOffset;
    }

    private async Task AnimateScrollAsync(double targetScroll, CancellationToken ct)
    {
        var startScroll = ScrollOffset;
        var totalDistance = targetScroll - startScroll;
        var duration = AnimationDuration;
        var startTime = DateTime.Now;

        while (!ct.IsCancellationRequested)
        {
            var elapsed = DateTime.Now - startTime;
            if (elapsed >= duration) break;

            var progress = elapsed.TotalMilliseconds / duration.TotalMilliseconds;
            var easedProgress = EaseOutQuart(progress);

            ScrollOffset = startScroll + totalDistance * easedProgress;

            await Task.Delay(16, ct);
        }

        ScrollOffset = targetScroll;
        InvalidateVisual();
    }

    private static double EaseOutQuart(double t) => 1 - Math.Pow(1 - t, 4);
}
