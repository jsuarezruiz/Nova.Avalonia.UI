using System;
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
using AvaloniaPixelFormats = global::Avalonia.Platform.PixelFormats;
using AvaloniaAlphaFormat = global::Avalonia.Platform.AlphaFormat;

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
    private Image? _overlayImage;
    private int _totalPixels;
    private int _scratchedPixels;
    private readonly DispatcherTimer _progressTimer;
    private bool _progressDirty;

    static Scratcher()
    {
        AffectsRender<Scratcher>(
            OverlayBrushProperty,
            BrushSizeProperty);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Scratcher"/>.
    /// </summary>
    public Scratcher()
    {
        _progressTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _progressTimer.Tick += OnProgressTimerTick;
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
    public bool IsScratching => _isScratching;

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
    public async void Reset(TimeSpan? duration = null)
    {
        _isThresholdReached = false;
        _scratchProgress = 0;
        _scratchedPixels = 0;
        
        if (duration.HasValue && duration.Value > TimeSpan.Zero && _overlayImage != null)
        {
            // Animate opacity from 0 to 1
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
    }

    /// <summary>
    /// Reveals all content by removing the entire overlay.
    /// </summary>
    /// <param name="duration">Animation duration for reveal. If null, reveals instantly.</param>
    public async void Reveal(TimeSpan? duration = null)
    {
        if (_scratchBuffer == null) return;

        _scratchProgress = 100;
        _isThresholdReached = true;
        
        if (duration.HasValue && duration.Value > TimeSpan.Zero && _overlayImage != null)
        {
            // Animate opacity from current to 0
            var animation = new Animation
            {
                Duration = duration.Value,
                Children = { new KeyFrame { Cue = new Cue(0), Setters = { new Setter(OpacityProperty, _overlayImage.Opacity) } },
                             new KeyFrame { Cue = new Cue(1), Setters = { new Setter(OpacityProperty, 0.0) } } }
            };
            await animation.RunAsync(_overlayImage);
            ClearBuffer();
            _overlayImage.Opacity = 1; // Reset opacity for next use
        }
        else
        {
            ClearBuffer();
        }
        
        _overlayImage?.InvalidateVisual();
    }

    /// <summary>
    /// Returns a copy of the current scratch mask as a <see cref="WriteableBitmap"/>.
    /// </summary>
    public WriteableBitmap? GetScratchMask()
    {
        if (_scratchBuffer == null) return null;

        var copy = new WriteableBitmap(
            _scratchBuffer.PixelSize,
            _scratchBuffer.Dpi,
            _scratchBuffer.Format,
            _scratchBuffer.AlphaFormat);

        using var srcLock = _scratchBuffer.Lock();
        using var dstLock = copy.Lock();

        unsafe
        {
            Buffer.MemoryCopy(
                srcLock.Address.ToPointer(),
                dstLock.Address.ToPointer(),
                dstLock.RowBytes * dstLock.Size.Height,
                srcLock.RowBytes * srcLock.Size.Height);
        }

        return copy;
    }

    /// <summary>
    /// Applies a pre-defined scratch mask to the control.
    /// </summary>
    /// <param name="mask">Bitmap to use as scratch mask.</param>
    public void SetScratchMask(WriteableBitmap mask)
    {
        if (_scratchBuffer == null || mask.PixelSize != _scratchBuffer.PixelSize) return;

        using var srcLock = mask.Lock();
        using var dstLock = _scratchBuffer.Lock();

        unsafe
        {
            Buffer.MemoryCopy(
                srcLock.Address.ToPointer(),
                dstLock.Address.ToPointer(),
                dstLock.RowBytes * dstLock.Size.Height,
                srcLock.RowBytes * srcLock.Size.Height);
        }

        _progressDirty = true;
        InvalidateVisual();
    }

    /// <inheritdoc/>
    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new ScratcherAutomationPeer(this);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _overlayImage = e.NameScope.Find<Image>("PART_OverlayImage");
        // Buffer will be built in OnSizeChanged when we have actual dimensions
    }

    /// <inheritdoc/>
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        // Always rebuild when size changes and we have valid dimensions
        if (e.NewSize.Width > 0 && e.NewSize.Height > 0)
        {
            RebuildScratchBuffer();
        }
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!IsEnabled) return;

        _isScratching = true;
        _lastPoint = e.GetPosition(this);
        _progressTimer.Start();

        RaiseEvent(new ScratchEventArgs(ScratchStartedEvent, _lastPoint, BrushSize));
        e.Pointer.Capture(this);
    }

    /// <inheritdoc/>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (!_isScratching) return;

        var currentPoint = e.GetPosition(this);
        ScratchLine(_lastPoint, currentPoint);
        _lastPoint = currentPoint;

        RaiseEvent(new ScratchEventArgs(ScratchUpdatedEvent, currentPoint, BrushSize));
    }

    /// <inheritdoc/>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (!_isScratching) return;

        _isScratching = false;
        var finalPoint = e.GetPosition(this);
        _progressTimer.Stop();
        UpdateProgress();

        RaiseEvent(new ScratchEventArgs(ScratchEndedEvent, finalPoint, BrushSize));
        e.Pointer.Capture(null);
    }

    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Space || e.Key == Key.Enter)
        {
            Reveal();
            e.Handled = true;
        }
    }

    private void UpdateOverlayImage()
    {
        if (_overlayImage != null && _scratchBuffer != null)
        {
            _overlayImage.Source = _scratchBuffer;
        }
    }

    private void RebuildScratchBuffer()
    {
        var width = (int)Math.Max(1, Bounds.Width);
        var height = (int)Math.Max(1, Bounds.Height);

        if (width <= 0 || height <= 0) return;

        _scratchBuffer = new WriteableBitmap(
            new PixelSize(width, height),
            new Vector(96, 96),
            AvaloniaPixelFormats.Bgra8888,
            AvaloniaAlphaFormat.Premul);

        _totalPixels = width * height;
        _scratchedPixels = 0;
        _scratchProgress = 0;
        _isThresholdReached = false;

        FillBuffer();
        UpdateOverlayImage();
    }

    private void FillBuffer()
    {
        if (_scratchBuffer == null) return;

        using var fb = _scratchBuffer.Lock();
        var width = fb.Size.Width;
        var height = fb.Size.Height;

        unsafe
        {
            var ptr = (uint*)fb.Address;

            if (OverlayBrush is SolidColorBrush scb)
            {
                var color = scb.Color;
                uint pixel = (uint)((color.A << 24) | (color.R << 16) | (color.G << 8) | color.B);

                for (int i = 0; i < width * height; i++)
                {
                    ptr[i] = pixel;
                }
            }
            else
            {
                uint grayPixel = 0xFF808080;
                for (int i = 0; i < width * height; i++)
                {
                    ptr[i] = grayPixel;
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
            var ptr = (uint*)fb.Address;
            for (int i = 0; i < width * height; i++)
            {
                ptr[i] = 0;
            }
        }

        _scratchedPixels = _totalPixels;
    }

    private void ScratchLine(Point from, Point to)
    {
        if (_scratchBuffer == null) return;

        var radius = BrushSize / 2;
        var distance = Math.Sqrt(Math.Pow(to.X - from.X, 2) + Math.Pow(to.Y - from.Y, 2));
        var steps = Math.Max(1, (int)(distance / (radius / 2)));

        for (int i = 0; i <= steps; i++)
        {
            var t = steps > 0 ? (double)i / steps : 0;
            var x = from.X + (to.X - from.X) * t;
            var y = from.Y + (to.Y - from.Y) * t;
            ScratchCircle((int)x, (int)y, (int)radius);
        }

        _overlayImage?.InvalidateVisual();
        
        // Update progress immediately to ensure threshold detection
        UpdateProgress();
    }

    private void ScratchCircle(int cx, int cy, int radius)
    {
        if (_scratchBuffer == null) return;

        using var fb = _scratchBuffer.Lock();
        var width = fb.Size.Width;
        var height = fb.Size.Height;
        var radiusSq = radius * radius;

        unsafe
        {
            var ptr = (uint*)fb.Address;

            var minX = Math.Max(0, cx - radius);
            var maxX = Math.Min(width - 1, cx + radius);
            var minY = Math.Max(0, cy - radius);
            var maxY = Math.Min(height - 1, cy + radius);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    var dx = x - cx;
                    var dy = y - cy;
                    if (dx * dx + dy * dy <= radiusSq)
                    {
                        var idx = y * width + x;
                        if (ptr[idx] != 0)
                        {
                            ptr[idx] = 0;
                            _scratchedPixels++;
                        }
                    }
                }
            }
        }
    }

    private void OnProgressTimerTick(object? sender, EventArgs e)
    {
        if (_progressDirty)
        {
            UpdateProgress();
        }
    }

    private void UpdateProgress()
    {
        if (_totalPixels <= 0) return;

        var previousProgress = _scratchProgress;
        _scratchProgress = (_scratchedPixels * 100.0) / _totalPixels;
        _progressDirty = false;

        if (Math.Abs(_scratchProgress - previousProgress) >= 0.1)
        {
            RaiseEvent(new ScratchProgressEventArgs(ProgressChangedEvent, _scratchProgress, previousProgress));
        }

        if (!_isThresholdReached && _scratchProgress >= Threshold)
        {
            _isThresholdReached = true;
            RaiseEvent(new RoutedEventArgs(ThresholdReachedEvent));
        }
    }
}
