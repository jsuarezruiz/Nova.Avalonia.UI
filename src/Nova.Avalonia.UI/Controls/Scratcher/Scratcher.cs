using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaPixelFormats = Avalonia.Platform.PixelFormats;
using AvaloniaAlphaFormat = Avalonia.Platform.AlphaFormat;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// An interactive control that temporarily hides content beneath a scratchable overlay.
/// Users can reveal the hidden content by "scratching" the overlay with pointer input.
/// </summary>
public class Scratcher : ContentControl
{
    private WriteableBitmap? _scratchBuffer;
    private bool _isScratching;
    private Point _lastPoint;
    private double _scratchProgress;
    private bool _isThresholdReached;
    private bool _isParentHandlerActive;
    private Image? _overlayImage;
    private int _totalPixels;
    private int _scratchedPixels;
    private readonly DispatcherTimer _scrollLockTimer;
    private IPointer? _activePointer;
    private readonly System.Collections.Generic.List<ScrollViewer> _activeParentScrollViewers = new();
    private readonly System.Collections.Generic.Dictionary<ScrollViewer, Vector> _lockedScrollOffsets = new();

    static Scratcher()
    {
        AffectsRender<Scratcher>(
            OverlayBrushProperty,
            BrushSizeProperty);

        OverlayBrushProperty.Changed.AddClassHandler<Scratcher>((x, e) => x.OnOverlayBrushChanged(e));
    }

    private void OnOverlayBrushChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_scratchBuffer != null)
        {
            FillBuffer(true);
            _overlayImage?.InvalidateVisual();
        }
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Scratcher"/>.
    /// </summary>
    public Scratcher()
    {
        _scrollLockTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(8) };
        _scrollLockTimer.Tick += OnScrollLockTimerTick;

        Focusable = true;

        // Tunnel phase ensures we see events before ancestor ScrollViewers do.
        AddHandler(PointerPressedEvent, OnPointerPressedTunnel, RoutingStrategies.Tunnel, true);
        AddHandler(PointerMovedEvent, OnPointerMovedTunnel, RoutingStrategies.Tunnel, true);
        AddHandler(PointerReleasedEvent, OnPointerReleasedTunnel, RoutingStrategies.Tunnel, true);

        AddHandler(Gestures.ScrollGestureEvent, OnScratcherScrollGesture, RoutingStrategies.Bubble, true);
        AddHandler(PointerWheelChangedEvent, OnScratcherPointerWheel, RoutingStrategies.Bubble, true);
    }

    private void OnScratcherPointerWheel(object? sender, PointerWheelEventArgs e)
    {
        if (_isScratching)
            e.Handled = true;
    }

    private void OnScratcherScrollGesture(object? sender, ScrollGestureEventArgs e)
    {
        if (_isScratching)
            e.Handled = true;
    }

    /// <summary>
    /// Defines the <see cref="OverlayBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> OverlayBrushProperty =
        AvaloniaProperty.Register<Scratcher, IBrush?>(nameof(OverlayBrush), Brushes.Gray);

    /// <summary>
    /// Defines the <see cref="BrushSize"/> property.
    /// </summary>
    public static readonly StyledProperty<double> BrushSizeProperty =
        AvaloniaProperty.Register<Scratcher, double>(nameof(BrushSize), 30.0);

    /// <summary>
    /// Defines the <see cref="Threshold"/> property.
    /// </summary>
    public static readonly StyledProperty<double> ThresholdProperty =
        AvaloniaProperty.Register<Scratcher, double>(nameof(Threshold), 50.0,
            coerce: (_, v) => Math.Clamp(v, 0, 100));

    /// <summary>
    /// Defines the <see cref="RebuildOnResize"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> RebuildOnResizeProperty =
        AvaloniaProperty.Register<Scratcher, bool>(nameof(RebuildOnResize), true);

    /// <summary>
    /// Defines the <see cref="ScratchProgress"/> property.
    /// </summary>
    public static readonly DirectProperty<Scratcher, double> ScratchProgressProperty =
        AvaloniaProperty.RegisterDirect<Scratcher, double>(
            nameof(ScratchProgress),
            o => o.ScratchProgress);

    /// <summary>
    /// Defines the <see cref="IsThresholdReached"/> property.
    /// </summary>
    public static readonly DirectProperty<Scratcher, bool> IsThresholdReachedProperty =
        AvaloniaProperty.RegisterDirect<Scratcher, bool>(
            nameof(IsThresholdReached),
            o => o.IsThresholdReached);

    public static readonly DirectProperty<Scratcher, bool> IsScratchingProperty =
        AvaloniaProperty.RegisterDirect<Scratcher, bool>(
            nameof(IsScratching),
            o => o.IsScratching);

    /// <summary>
    /// Defines the <see cref="ProgressChanged"/> routed event.
    /// </summary>
    public static readonly RoutedEvent<ScratchProgressEventArgs> ProgressChangedEvent =
        RoutedEvent.Register<Scratcher, ScratchProgressEventArgs>(nameof(ProgressChanged), RoutingStrategies.Bubble);

    /// <summary>
    /// Defines the <see cref="ThresholdReached"/> routed event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> ThresholdReachedEvent =
        RoutedEvent.Register<Scratcher, RoutedEventArgs>(nameof(ThresholdReached), RoutingStrategies.Bubble);

    /// <summary>
    /// Defines the <see cref="ScratchStarted"/> routed event.
    /// </summary>
    public static readonly RoutedEvent<ScratchEventArgs> ScratchStartedEvent =
        RoutedEvent.Register<Scratcher, ScratchEventArgs>(nameof(ScratchStarted), RoutingStrategies.Bubble);

    /// <summary>
    /// Defines the <see cref="ScratchUpdated"/> routed event.
    /// </summary>
    public static readonly RoutedEvent<ScratchEventArgs> ScratchUpdatedEvent =
        RoutedEvent.Register<Scratcher, ScratchEventArgs>(nameof(ScratchUpdated), RoutingStrategies.Bubble);

    /// <summary>
    /// Defines the <see cref="ScratchEnded"/> routed event.
    /// </summary>
    public static readonly RoutedEvent<ScratchEventArgs> ScratchEndedEvent =
        RoutedEvent.Register<Scratcher, ScratchEventArgs>(nameof(ScratchEnded), RoutingStrategies.Bubble);

    /// <summary>
    /// Gets or sets the solid color brush used to cover the content.
    /// </summary>
    public IBrush? OverlayBrush
    {
        get => GetValue(OverlayBrushProperty);
        set => SetValue(OverlayBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the diameter of the scratch brush in pixels.
    /// </summary>
    public double BrushSize
    {
        get => GetValue(BrushSizeProperty);
        set => SetValue(BrushSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the percentage (0-100) of scratched area required to trigger <see cref="ThresholdReached"/>.
    /// </summary>
    public double Threshold
    {
        get => GetValue(ThresholdProperty);
        set => SetValue(ThresholdProperty, value);
    }

    /// <summary>
    /// Gets or sets whether to rebuild scratch surface when control resizes.
    /// </summary>
    public bool RebuildOnResize
    {
        get => GetValue(RebuildOnResizeProperty);
        set => SetValue(RebuildOnResizeProperty, value);
    }

    /// <summary>
    /// Gets the current scratch progress (0-100).
    /// </summary>
    public double ScratchProgress => _scratchProgress;

    /// <summary>
    /// Gets whether the scratch threshold has been reached.
    /// </summary>
    public bool IsThresholdReached => _isThresholdReached;

    /// <summary>
    /// Gets whether the user is currently scratching.
    /// </summary>
    public bool IsScratching
    {
        get => _isScratching;
        private set => SetAndRaise(IsScratchingProperty, ref _isScratching, value);
    }

    /// <summary>
    /// Occurs when scratch progress changes (minimum 0.1% difference).
    /// </summary>
    public event EventHandler<ScratchProgressEventArgs>? ProgressChanged
    {
        add => AddHandler(ProgressChangedEvent, value);
        remove => RemoveHandler(ProgressChangedEvent, value);
    }

    /// <summary>
    /// Occurs when scratch threshold is reached (only once per scratch session).
    /// </summary>
    public event EventHandler<RoutedEventArgs>? ThresholdReached
    {
        add => AddHandler(ThresholdReachedEvent, value);
        remove => RemoveHandler(ThresholdReachedEvent, value);
    }

    /// <summary>
    /// Occurs when scratching begins.
    /// </summary>
    public event EventHandler<ScratchEventArgs>? ScratchStarted
    {
        add => AddHandler(ScratchStartedEvent, value);
        remove => RemoveHandler(ScratchStartedEvent, value);
    }

    /// <summary>
    /// Occurs continuously during scratching.
    /// </summary>
    public event EventHandler<ScratchEventArgs>? ScratchUpdated
    {
        add => AddHandler(ScratchUpdatedEvent, value);
        remove => RemoveHandler(ScratchUpdatedEvent, value);
    }

    /// <summary>
    /// Occurs when scratching ends.
    /// </summary>
    public event EventHandler<ScratchEventArgs>? ScratchEnded
    {
        add => AddHandler(ScratchEndedEvent, value);
        remove => RemoveHandler(ScratchEndedEvent, value);
    }

    /// <summary>
    /// Resets the scratcher to its initial state, covering all content again.
    /// </summary>
    /// <param name="duration">Animation duration for reset. If null, resets instantly.</param>
    public async Task Reset(TimeSpan? duration = null)
    {
        var previousProgress = _scratchProgress;
        _scratchProgress = 0;
        _scratchedPixels = 0;
        _isThresholdReached = false;

        RaisePropertyChanged(ScratchProgressProperty, previousProgress, 0.0);
        RaisePropertyChanged(IsThresholdReachedProperty, true, false);

        if (duration.HasValue && duration.Value > TimeSpan.Zero && _overlayImage != null)
        {
            _overlayImage.Opacity = 0;
            RebuildScratchBuffer();

            var animation = new Animation
            {
                Duration = duration.Value,
                Children = { new KeyFrame { Cue = new Cue(0), Setters = { new Setter(OpacityProperty, 0.0) } },
                             new KeyFrame { Cue = new Cue(1), Setters = { new Setter(OpacityProperty, 1.0) } } }
            };
            await animation.RunAsync(_overlayImage);
        }
        else
        {
            RebuildScratchBuffer();
        }

        if (_overlayImage != null)
            _overlayImage.Opacity = 1;
    }

    /// <summary>
    /// Reveals all content by removing the entire overlay.
    /// </summary>
    /// <param name="duration">Animation duration for reveal. If null, reveals instantly.</param>
    public async Task Reveal(TimeSpan? duration = null)
    {
        if (_scratchBuffer == null) return;

        var previousProgress = _scratchProgress;
        _scratchProgress = 100;
        _isThresholdReached = true;

        RaisePropertyChanged(ScratchProgressProperty, previousProgress, 100.0);
        RaisePropertyChanged(IsThresholdReachedProperty, false, true);

        if (duration.HasValue && duration.Value > TimeSpan.Zero && _overlayImage != null)
        {
            var animation = new Animation
            {
                Duration = duration.Value,
                Children = { new KeyFrame { Cue = new Cue(0), Setters = { new Setter(OpacityProperty, _overlayImage.Opacity) } },
                             new KeyFrame { Cue = new Cue(1), Setters = { new Setter(OpacityProperty, 0.0) } } }
            };
            await animation.RunAsync(_overlayImage);
            ClearBuffer();
            _overlayImage.Opacity = 1;
        }
        else
        {
            ClearBuffer();
        }

        _overlayImage?.InvalidateVisual();
    }

    /// <summary>
    /// Returns a copy of the current scratch mask as a byte array.
    /// Can be stored and later passed to <see cref="SetScratchMask"/> to restore the scratch state.
    /// </summary>
    public byte[]? GetScratchMask()
    {
        if (_scratchBuffer == null) return null;

        using var srcLock = _scratchBuffer.Lock();
        int size = srcLock.RowBytes * srcLock.Size.Height;
        var copy = new byte[size];
        
        System.Runtime.InteropServices.Marshal.Copy(srcLock.Address, copy, 0, size);
        return copy;
    }

    /// <summary>
    /// Applies a pre-defined scratch mask byte array to the control.
    /// </summary>
    /// <param name="mask">Byte array representing the scratch mask.</param>
    public void SetScratchMask(byte[] mask)
    {
        if (_scratchBuffer == null) return;

        using var dstLock = _scratchBuffer.Lock();
        int expectedSize = dstLock.RowBytes * dstLock.Size.Height;
        if (mask.Length != expectedSize) return;

        System.Runtime.InteropServices.Marshal.Copy(mask, 0, dstLock.Address, expectedSize);

        int count = 0;
        unsafe
        {
            fixed (byte* pMask = mask)
            {
                var width = _scratchBuffer.PixelSize.Width;
                var height = _scratchBuffer.PixelSize.Height;
                var stride = dstLock.RowBytes;

                for (int y = 0; y < height; y++)
                {
                    uint* rowPtr = (uint*)(pMask + y * stride);
                    for (int x = 0; x < width; x++)
                    {
                        if (rowPtr[x] == 0)
                            count++;
                    }
                }
            }
        }
        _scratchedPixels = count;
        
        UpdateProgress();
        _overlayImage?.InvalidateVisual();
    }

    /// <inheritdoc/>
    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new ScratcherAutomationPeer(this);
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AttachInterceptors();
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        DetachInterceptors();
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _overlayImage = e.NameScope.Find<Image>("PART_OverlayImage");
        UpdateOverlayImage();
    }

    /// <inheritdoc/>
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        var newWidth = (int)Math.Max(1, e.NewSize.Width);
        var newHeight = (int)Math.Max(1, e.NewSize.Height);

        // Guard against sub-pixel jitter triggering unnecessary resets.
        var currentWidth = _scratchBuffer?.PixelSize.Width ?? 0;
        var currentHeight = _scratchBuffer?.PixelSize.Height ?? 0;

        if (RebuildOnResize && newWidth > 0 && newHeight > 0 && (newWidth != currentWidth || newHeight != currentHeight))
            RebuildScratchBuffer();
    }

    /// <inheritdoc/>
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);

        if (_isScratching && _activePointer != null)
        {
            // ScrollGestureRecognizer steals capture on scroll detection. Re-capturing here
            // keeps the scratch session alive until the pointer is actually released.
            _activePointer.Capture(this);
        }
    }

    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Space || e.Key == Key.Enter)
        {
            _ = Reveal();
            e.Handled = true;
        }
    }

    private void OnPointerPressedTunnel(object? sender, PointerPressedEventArgs e)
    {
        if (!IsEnabled) return;

        _activePointer = e.Pointer;
        IsScratching = true;
        _lastPoint = e.GetPosition(this);
        _scrollLockTimer.Start();

        LockParentScroll();
        RaiseEvent(new ScratchEventArgs(ScratchStartedEvent, _lastPoint, BrushSize));

        e.Pointer.Capture(this);
        e.Handled = true;
    }

    private void OnPointerMovedTunnel(object? sender, PointerEventArgs e)
    {
        if (!_isScratching) return;

        var currentPoint = e.GetPosition(this);
        ScratchLine(_lastPoint, currentPoint);
        _lastPoint = currentPoint;

        RaiseEvent(new ScratchEventArgs(ScratchUpdatedEvent, currentPoint, BrushSize));
        e.Handled = true;
    }

    private void OnPointerReleasedTunnel(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isScratching) return;

        EndScratchSession(e.GetPosition(this));
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    private void EndScratchSession(Point finalPoint)
    {
        if (!_isScratching) return;

        IsScratching = false;
        _activePointer = null;
        _scrollLockTimer.Stop();
        UpdateProgress();
        RestoreParentScroll();
        RaiseEvent(new ScratchEventArgs(ScratchEndedEvent, finalPoint, BrushSize));
    }

    private void UpdateOverlayImage()
    {
        if (_overlayImage != null && _scratchBuffer != null)
        {
            _overlayImage.Source = _scratchBuffer;
            _overlayImage.Opacity = 1.0;
            _overlayImage.IsVisible = true;
            _overlayImage.InvalidateVisual();
        }
    }

    private void RebuildScratchBuffer()
    {
        var width = (int)Math.Max(1, Bounds.Width);
        var height = (int)Math.Max(1, Bounds.Height);

        if (width <= 0 || height <= 0) return;

        _scratchBuffer?.Dispose();
        _scratchBuffer = new WriteableBitmap(
            new PixelSize(width, height),
            new Vector(96, 96),
            AvaloniaPixelFormats.Rgba8888,
            AvaloniaAlphaFormat.Premul);

        _totalPixels = width * height;
        _scratchedPixels = 0;
        _scratchProgress = 0;
        _isThresholdReached = false;

        FillBuffer(false);
        UpdateOverlayImage();
    }

    private void FillBuffer(bool preserveScratches)
    {
        if (_scratchBuffer == null || _scratchBuffer.PixelSize.Width <= 0 || _scratchBuffer.PixelSize.Height <= 0) return;

        var width = _scratchBuffer.PixelSize.Width;
        var height = _scratchBuffer.PixelSize.Height;

        using var fb = _scratchBuffer.Lock();
        unsafe
        {
            var ptr = (byte*)fb.Address;
            var stride = fb.RowBytes;
            var brush = OverlayBrush ?? Brushes.Gray;

            if (brush is ISolidColorBrush scb)
            {
                var color = scb.Color;
                // Rgba8888 (LE): byte 0=R, 1=G, 2=B, 3=A → uint 0xAABBGGRR
                uint pixel = (uint)((0xFFU << 24) | ((uint)color.B << 16) | ((uint)color.G << 8) | (uint)color.R);

                for (int y = 0; y < height; y++)
                {
                    uint* rowPtr = (uint*)(ptr + y * stride);
                    for (int x = 0; x < width; x++)
                    {
                        if (!preserveScratches || rowPtr[x] != 0)
                            rowPtr[x] = pixel;
                    }
                }
            }
            else
            {
                try
                {
                    using var rtb = new RenderTargetBitmap(new PixelSize(width, height), new Vector(96, 96));
                    using (var context = rtb.CreateDrawingContext())
                        context.FillRectangle(brush, new Rect(0, 0, width, height));

                    using var temp = new WriteableBitmap(new PixelSize(width, height), new Vector(96, 96), AvaloniaPixelFormats.Rgba8888, AvaloniaAlphaFormat.Premul);
                    using var tempLock = temp.Lock();
                    rtb.CopyPixels(new PixelRect(rtb.PixelSize), tempLock.Address, tempLock.RowBytes * tempLock.Size.Height, tempLock.RowBytes);

                    var tempPtr = (byte*)tempLock.Address;
                    var tempStride = tempLock.RowBytes;

                    for (int y = 0; y < height; y++)
                    {
                        uint* rowPtr = (uint*)(ptr + y * stride);
                        uint* tempRowPtr = (uint*)(tempPtr + y * tempStride);
                        for (int x = 0; x < width; x++)
                        {
                            if (!preserveScratches || rowPtr[x] != 0)
                                rowPtr[x] = tempRowPtr[x] | 0xFF000000;
                        }
                    }
                }
                catch (Exception)
                {
                    // Fallback for headless environments.
                    const uint fallbackPixel = 0xFF808080;
                    for (int y = 0; y < height; y++)
                    {
                        uint* rowPtr = (uint*)(ptr + y * stride);
                        for (int x = 0; x < width; x++)
                        {
                            if (!preserveScratches || rowPtr[x] != 0)
                                rowPtr[x] = fallbackPixel;
                        }
                    }
                }
            }
        }
    }

    private void ClearBuffer()
    {
        if (_scratchBuffer == null) return;

        using var fb = _scratchBuffer.Lock();
        var width = fb.Size.Width;
        var height = fb.Size.Height;

        unsafe
        {
            var ptr = (byte*)fb.Address;
            var stride = fb.RowBytes;

            for (int y = 0; y < height; y++)
            {
                uint* rowPtr = (uint*)(ptr + y * stride);
                for (int x = 0; x < width; x++)
                    rowPtr[x] = 0;
            }
        }

        _scratchedPixels = _totalPixels;
    }

    private static int CountZeroPixels(WriteableBitmap bitmap)
    {
        int count = 0;
        using var fb = bitmap.Lock();
        var width = fb.Size.Width;
        var height = fb.Size.Height;

        unsafe
        {
            var ptr = (byte*)fb.Address;
            var stride = fb.RowBytes;
            for (int y = 0; y < height; y++)
            {
                uint* rowPtr = (uint*)(ptr + y * stride);
                for (int x = 0; x < width; x++)
                {
                    if (rowPtr[x] == 0)
                        count++;
                }
            }
        }

        return count;
    }

    private void ScratchLine(Point from, Point to)
    {
        if (_scratchBuffer == null) return;

        var radius = BrushSize / 2;
        var distance = Math.Sqrt(Math.Pow(to.X - from.X, 2) + Math.Pow(to.Y - from.Y, 2));
        var steps = Math.Max(1, (int)(distance / (radius / 2)));

        using (var fb = _scratchBuffer.Lock())
        {
            for (int i = 0; i <= steps; i++)
            {
                var t = steps > 0 ? (double)i / steps : 0;
                var x = from.X + (to.X - from.X) * t;
                var y = from.Y + (to.Y - from.Y) * t;
                ScratchCircleInternal(fb, (int)x, (int)y, (int)radius);
            }
        }

        _overlayImage?.InvalidateVisual();
        UpdateProgress();
    }

    private unsafe void ScratchCircleInternal(global::Avalonia.Platform.ILockedFramebuffer fb, int cx, int cy, int radius)
    {
        var width = fb.Size.Width;
        var height = fb.Size.Height;
        var radiusSq = radius * radius;
        var ptr = (byte*)fb.Address;
        var stride = fb.RowBytes;

        var minX = Math.Max(0, cx - radius);
        var maxX = Math.Min(width - 1, cx + radius);
        var minY = Math.Max(0, cy - radius);
        var maxY = Math.Min(height - 1, cy + radius);

        for (int y = minY; y <= maxY; y++)
        {
            uint* rowPtr = (uint*)(ptr + y * stride);
            for (int x = minX; x <= maxX; x++)
            {
                var dx = x - cx;
                var dy = y - cy;
                if (dx * dx + dy * dy <= radiusSq && rowPtr[x] != 0)
                {
                    rowPtr[x] = 0;
                    _scratchedPixels++;
                }
            }
        }
    }

    private void OnScrollLockTimerTick(object? sender, EventArgs e)
    {
        if (!_isScratching || !_isParentHandlerActive) return;

        foreach (var kvp in _lockedScrollOffsets)
        {
            if (kvp.Key.Offset != kvp.Value)
                kvp.Key.Offset = kvp.Value;
        }
    }

    private void UpdateProgress()
    {
        if (_totalPixels <= 0) return;

        var previousProgress = _scratchProgress;
        _scratchProgress = (_scratchedPixels * 100.0) / _totalPixels;

        if (Math.Abs(_scratchProgress - previousProgress) >= 0.1)
            RaiseEvent(new ScratchProgressEventArgs(ProgressChangedEvent, _scratchProgress, previousProgress));

        if (!_isThresholdReached && _scratchProgress >= Threshold)
        {
            _isThresholdReached = true;
            RaiseEvent(new RoutedEventArgs(ThresholdReachedEvent));
            RaisePropertyChanged(IsThresholdReachedProperty, false, true);
        }

        if (previousProgress != _scratchProgress)
            RaisePropertyChanged(ScratchProgressProperty, previousProgress, _scratchProgress);
    }

    private void AttachInterceptors()
    {
        DetachInterceptors();

        foreach (var ancestor in this.GetVisualAncestors())
        {
            if (ancestor is not ScrollViewer sv) continue;

            sv.AddHandler(PointerPressedEvent, OnAncestorPointerPressed, RoutingStrategies.Tunnel, true);
            sv.AddHandler(PointerMovedEvent, OnAncestorPointerMoved, RoutingStrategies.Tunnel, true);
            sv.AddHandler(PointerReleasedEvent, OnAncestorPointerReleased, RoutingStrategies.Tunnel, true);
            sv.AddHandler(PointerWheelChangedEvent, OnAncestorPointerWheel, RoutingStrategies.Tunnel, true);
            sv.AddHandler(Gestures.ScrollGestureEvent, OnAncestorScrollGesture, RoutingStrategies.Tunnel, true);
            sv.AddHandler(PointerPressedEvent, OnAncestorPointerPressedBubble, RoutingStrategies.Bubble, true);
            sv.AddHandler(Gestures.ScrollGestureEvent, OnAncestorScrollGestureBubble, RoutingStrategies.Bubble, true);
            _activeParentScrollViewers.Add(sv);
        }
    }

    private void DetachInterceptors()
    {
        foreach (var sv in _activeParentScrollViewers)
        {
            sv.RemoveHandler(PointerPressedEvent, OnAncestorPointerPressed);
            sv.RemoveHandler(PointerMovedEvent, OnAncestorPointerMoved);
            sv.RemoveHandler(PointerReleasedEvent, OnAncestorPointerReleased);
            sv.RemoveHandler(PointerWheelChangedEvent, OnAncestorPointerWheel);
            sv.RemoveHandler(Gestures.ScrollGestureEvent, OnAncestorScrollGesture);
            sv.RemoveHandler(PointerPressedEvent, OnAncestorPointerPressedBubble);
            sv.RemoveHandler(Gestures.ScrollGestureEvent, OnAncestorScrollGestureBubble);
        }
        _activeParentScrollViewers.Clear();
        _lockedScrollOffsets.Clear();
    }

    private void OnAncestorPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (new Rect(Bounds.Size).Contains(e.GetPosition(this)))
            e.Handled = true;
    }

    private void OnAncestorPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isScratching && _isParentHandlerActive)
        {
            foreach (var kvp in _lockedScrollOffsets)
            {
                if (kvp.Key.Offset != kvp.Value)
                    kvp.Key.Offset = kvp.Value;
            }
        }

        var pos = e.GetPosition(this);
        if (new Rect(Bounds.Size).Contains(pos))
        {
            e.Handled = true;
            if (e.Pointer.Captured != this)
                e.Pointer.Capture(this);
        }
    }

    private void OnAncestorPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        // If the gesture recognizer held capture when the pointer lifted,
        // OnPointerReleasedTunnel won't reach us, end the session here instead.
        if (_isScratching)
        {
            EndScratchSession(e.GetPosition(this));
            e.Pointer.Capture(null);
            e.Handled = true;
        }
    }

    private void OnAncestorScrollGesture(object? sender, ScrollGestureEventArgs e)
    {
        if (!_isScratching) return;

        foreach (var kvp in _lockedScrollOffsets)
        {
            if (kvp.Key.Offset != kvp.Value)
                kvp.Key.Offset = kvp.Value;
        }

        e.Handled = true;
    }

    private void OnAncestorPointerWheel(object? sender, PointerWheelEventArgs e)
    {
        if (_isScratching && new Rect(Bounds.Size).Contains(e.GetPosition(this)))
            e.Handled = true;
    }

    private void OnAncestorPointerPressedBubble(object? sender, PointerPressedEventArgs e)
    {
        if (!e.Handled && new Rect(Bounds.Size).Contains(e.GetPosition(this)))
            e.Handled = true;
    }

    private void OnAncestorScrollGestureBubble(object? sender, ScrollGestureEventArgs e)
    {
        if (!_isScratching) return;

        foreach (var kvp in _lockedScrollOffsets)
            kvp.Key.Offset = kvp.Value;

        e.Handled = true;
    }

    private void LockParentScroll()
    {
        if (_isParentHandlerActive) return;

        // Visual tree may have changed since OnAttachedToVisualTree; re-scan if needed.
        if (_activeParentScrollViewers.Count == 0)
            AttachInterceptors();

        _lockedScrollOffsets.Clear();
        foreach (var sv in _activeParentScrollViewers)
            _lockedScrollOffsets[sv] = sv.Offset;

        _isParentHandlerActive = true;
    }

    private void RestoreParentScroll()
    {
        if (!_isParentHandlerActive) return;

        foreach (var kvp in _lockedScrollOffsets)
            kvp.Key.Offset = kvp.Value;

        _lockedScrollOffsets.Clear();
        _isParentHandlerActive = false;
    }
}
