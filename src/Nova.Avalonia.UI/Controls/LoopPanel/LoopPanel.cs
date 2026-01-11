using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// An infinite/looping scrolling panel that wraps its children seamlessly.
/// </summary>
public class LoopPanel : Panel
{
    static LoopPanel()
    {
        AffectsArrange<LoopPanel>(OrientationProperty, OffsetProperty, AnchorPositionProperty, SpacingProperty);
    }
    /// <summary>
    /// Defines the <see cref="Orientation"/> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<LoopPanel, Orientation>(nameof(Orientation), Orientation.Horizontal);

    /// <summary>
    /// Gets or sets the layout direction.
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="Offset"/> property.
    /// </summary>
    public static readonly StyledProperty<double> OffsetProperty =
        AvaloniaProperty.Register<LoopPanel, double>(nameof(Offset), 0.0);

    /// <summary>
    /// Gets or sets the fractional scroll position. 1.0 = one full item.
    /// </summary>
    public double Offset
    {
        get => GetValue(OffsetProperty);
        set => SetValue(OffsetProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="AnchorPosition"/> property.
    /// </summary>
    public static readonly StyledProperty<double> AnchorPositionProperty =
        AvaloniaProperty.Register<LoopPanel, double>(nameof(AnchorPosition), 0.5, coerce: CoerceAnchorPosition);

    /// <summary>
    /// Gets or sets the viewport anchor (0.0=start, 0.5=center, 1.0=end).
    /// </summary>
    public double AnchorPosition
    {
        get => GetValue(AnchorPositionProperty);
        set => SetValue(AnchorPositionProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="Spacing"/> property.
    /// </summary>
    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<LoopPanel, double>(nameof(Spacing), 0.0);

    /// <summary>
    /// Gets or sets the gap between items.
    /// </summary>
    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="IsInertiaEnabled"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsInertiaEnabledProperty =
        AvaloniaProperty.Register<LoopPanel, bool>(nameof(IsInertiaEnabled), true);

    /// <summary>
    /// Gets or sets whether momentum scrolling is enabled.
    /// </summary>
    public bool IsInertiaEnabled
    {
        get => GetValue(IsInertiaEnabledProperty);
        set => SetValue(IsInertiaEnabledProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="SnapToItems"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> SnapToItemsProperty =
        AvaloniaProperty.Register<LoopPanel, bool>(nameof(SnapToItems), true);

    /// <summary>
    /// Gets or sets whether to snap to the nearest whole item when scrolling ends.
    /// </summary>
    public bool SnapToItems
    {
        get => GetValue(SnapToItemsProperty);
        set => SetValue(SnapToItemsProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="ScrollFactor"/> property.
    /// </summary>
    public static readonly StyledProperty<double> ScrollFactorProperty =
        AvaloniaProperty.Register<LoopPanel, double>(nameof(ScrollFactor), 1.0);

    /// <summary>
    /// Gets or sets the multiplier for drag/wheel sensitivity.
    /// </summary>
    public double ScrollFactor
    {
        get => GetValue(ScrollFactorProperty);
        set => SetValue(ScrollFactorProperty, value);
    }

    /// <summary>
    /// Raised when the pivotal (center) item changes.
    /// </summary>
    public event EventHandler<int>? CurrentIndexChanged;

    private int _pivotalChildIndex = -1;
    private double _inertiaVelocity;
    private double _dragVelocity;
    private bool _isInertiaActive;
    private bool _isDragging;
    private Point _lastDragPosition;
    private DateTime _lastDragTime;
    private const double Deceleration = 0.95;
    private const double MinInertiaThreshold = 1.0;

    public LoopPanel()
    {
        ClipToBounds = true;
        
        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;
        PointerCaptureLost += OnPointerCaptureLost;
        PointerWheelChanged += OnPointerWheelChanged;
    }

    /// <summary>
    /// Scroll by specified viewport units.
    /// </summary>
    public void ScrollBy(double units)
    {
        if (Children.Count == 0) return;

        bool isHorizontal = Orientation == Orientation.Horizontal;
        int childIndex = _pivotalChildIndex >= 0 ? _pivotalChildIndex : 0;
        var child = Children[childIndex];
        double childExtent = isHorizontal ? child.DesiredSize.Width : child.DesiredSize.Height;

        if (childExtent > 0)
        {
            Offset += units / childExtent;
            InvalidateArrange();
        }
    }

    /// <summary>
    /// Scroll to bring specific child to anchor.
    /// </summary>
    public void ScrollToIndex(int index, bool animate = false)
    {
        if (Children.Count == 0) return;
        int targetIndex = ((index % Children.Count) + Children.Count) % Children.Count;

        if (animate && IsInertiaEnabled)
        {
            double delta = targetIndex - (Offset % Children.Count);
            if (delta > Children.Count / 2.0) delta -= Children.Count;
            if (delta < -Children.Count / 2.0) delta += Children.Count;
            _inertiaVelocity = delta * 5;
            StartInertia();
        }
        else
        {
            Offset = targetIndex;
            InvalidateArrange();
        }
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _isDragging = true;
        _lastDragPosition = e.GetPosition(this);
        _lastDragTime = DateTime.Now;
        _dragVelocity = 0;
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDragging) return;

        var current = e.GetPosition(this);
        double delta = Orientation == Orientation.Horizontal
            ? _lastDragPosition.X - current.X
            : _lastDragPosition.Y - current.Y;

        ScrollBy(delta * ScrollFactor);

        var now = DateTime.Now;
        double ms = (now - _lastDragTime).TotalMilliseconds;
        if (ms > 0)
        {
            double v = delta / ms * 16.667;
            _dragVelocity = _dragVelocity * 0.35 + v * 0.65;
        }

        _lastDragPosition = current;
        _lastDragTime = now;
        e.Handled = true;
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isDragging) return;

        _isDragging = false;
        e.Pointer.Capture(null);
        e.Handled = true;

        if (IsInertiaEnabled && Math.Abs(_dragVelocity) > MinInertiaThreshold)
        {
            _inertiaVelocity = _dragVelocity;
            StartInertia();
        }
        else if (SnapToItems)
        {
            SnapToNearestChild();
        }
    }

    private void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        _isDragging = false;
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        double delta = Orientation == Orientation.Horizontal ? -e.Delta.X : -e.Delta.Y;
        if (delta == 0) delta = -e.Delta.Y;
        double scrollAmount = delta * 50 * ScrollFactor;

        if (IsInertiaEnabled)
        {
            _inertiaVelocity = scrollAmount;
            StartInertia();
        }
        else
        {
            ScrollBy(scrollAmount);
            if (SnapToItems) SnapToNearestChild();
        }

        e.Handled = true;
    }

    private DispatcherTimer? _inertiaTimer;

    private void StartInertia()
    {
        if (_isInertiaActive) return;
        _isInertiaActive = true;
        
        _inertiaTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _inertiaTimer.Tick += OnInertiaTick;
        _inertiaTimer.Start();
    }

    private void StopInertia()
    {
        if (!_isInertiaActive) return;
        _isInertiaActive = false;
        
        _inertiaTimer?.Stop();
        _inertiaTimer = null;
    }

    private void OnInertiaTick(object? sender, EventArgs e)
    {
        if (Math.Abs(_inertiaVelocity) < MinInertiaThreshold)
        {
            _inertiaVelocity = 0;
            if (SnapToItems) SnapToNearestChild();
            StopInertia();
            return;
        }

        ScrollBy(_inertiaVelocity);
        _inertiaVelocity *= Deceleration;
    }

    private void SnapToNearestChild()
    {
        if (Children.Count == 0) return;
        Offset = Math.Round(Offset);
        InvalidateArrange();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var desiredSize = new Size();
        bool isHorizontal = Orientation == Orientation.Horizontal;
        var childSize = isHorizontal
            ? new Size(double.PositiveInfinity, availableSize.Height)
            : new Size(availableSize.Width, double.PositiveInfinity);

        int visibleCount = 0;
        for (int i = 0; i < Children.Count; i++)
        {
            var child = Children[i];
            if (!child.IsVisible) continue;
            
            child.Measure(childSize);

            if (isHorizontal)
            {
                desiredSize = new Size(
                    desiredSize.Width + child.DesiredSize.Width + (visibleCount > 0 ? Spacing : 0),
                    Math.Max(desiredSize.Height, child.DesiredSize.Height));
            }
            else
            {
                desiredSize = new Size(
                    Math.Max(desiredSize.Width, child.DesiredSize.Width),
                    desiredSize.Height + child.DesiredSize.Height + (visibleCount > 0 ? Spacing : 0));
            }
            visibleCount++;
        }

        return isHorizontal
            ? new Size(Math.Min(availableSize.Width, desiredSize.Width), desiredSize.Height)
            : new Size(desiredSize.Width, Math.Min(availableSize.Height, desiredSize.Height));
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        // Loop to find visible children for layout
        var visibleChildren = new List<Control>(Children.Count);
        foreach (var child in Children)
        {
            if (child.IsVisible) visibleChildren.Add(child);
        }

        int visibleCount = visibleChildren.Count;
        if (visibleCount == 0) return finalSize;

        bool isHorizontal = Orientation == Orientation.Horizontal;
        double adjustedOffset = ((Offset % visibleCount) + visibleCount) % visibleCount;
        int pivotalIndex = (int)adjustedOffset;
        double fractionalOffset = adjustedOffset - pivotalIndex;

        if (pivotalIndex != _pivotalChildIndex)
        {
            _pivotalChildIndex = pivotalIndex;
            CurrentIndexChanged?.Invoke(this, pivotalIndex);
        }

        var pivotalChild = visibleChildren[pivotalIndex];
        double pivotalExtent = isHorizontal ? pivotalChild.DesiredSize.Width : pivotalChild.DesiredSize.Height;
        double viewportExtent = isHorizontal ? finalSize.Width : finalSize.Height;
        double anchorPoint = viewportExtent * AnchorPosition;
        double pivotalStart = anchorPoint - pivotalExtent * fractionalOffset;

        // Reset all children to zero bounds initially or handles collapses

        ArrangeChild(pivotalChild, pivotalStart, finalSize, isHorizontal);

        double nextEdge = pivotalStart + pivotalExtent + Spacing;
        double priorEdge = pivotalStart - Spacing;
        int nextIndex = (pivotalIndex + 1) % visibleCount;
        int priorIndex = pivotalIndex == 0 ? visibleCount - 1 : pivotalIndex - 1;

        int nextPlaced = 0;
        int priorPlaced = 0;
        int maxPerSide = (visibleCount - 1 + 1) / 2;

        for (int i = 1; i < visibleCount; i++)
        {
            bool placeNext = (i % 2 == 1);
            if (placeNext && nextPlaced >= maxPerSide && priorPlaced < visibleCount - 1 - maxPerSide) 
                placeNext = false;
            else if (!placeNext && priorPlaced >= maxPerSide && nextPlaced < visibleCount - 1 - maxPerSide) 
                placeNext = true;

            if (placeNext)
            {
                var child = visibleChildren[nextIndex];
                double childExtent = isHorizontal ? child.DesiredSize.Width : child.DesiredSize.Height;
                ArrangeChild(child, nextEdge, finalSize, isHorizontal);
                nextEdge += childExtent + Spacing;
                nextIndex = (nextIndex + 1) % visibleCount;
                nextPlaced++;
            }
            else
            {
                var child = visibleChildren[priorIndex];
                double childExtent = isHorizontal ? child.DesiredSize.Width : child.DesiredSize.Height;
                double itemStart = priorEdge - childExtent;
                ArrangeChild(child, itemStart, finalSize, isHorizontal);
                priorEdge = itemStart - Spacing;
                priorIndex = priorIndex == 0 ? visibleCount - 1 : priorIndex - 1;
                priorPlaced++;
            }
        }

        return finalSize;
    }

    private void ArrangeChild(Control child, double position, Size finalSize, bool isHorizontal, bool collapsed = false)
    {
        if (collapsed)
        {
            child.Arrange(new Rect(0, 0, 0, 0));
            return;
        }

        var rect = isHorizontal
            ? new Rect(position, 0, child.DesiredSize.Width, finalSize.Height)
            : new Rect(0, position, finalSize.Width, child.DesiredSize.Height);
        child.Arrange(rect);
    }

    private static double CoerceAnchorPosition(AvaloniaObject sender, double value)
    {
        return Math.Clamp(value, 0.0, 1.0);
    }
}