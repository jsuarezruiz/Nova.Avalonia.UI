using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// A high-performance particle system control for creating custom particle effects.
/// </summary>
public class Particles : TemplatedControl
{
    private readonly ObservableCollection<Particle> _items = new();
    private readonly ParticlePool _particlePool = new();
    private readonly DispatcherTimer _updateTimer;
    private DateTime _lastUpdate;
    private TimeSpan _totalElapsed;
    private Point _originPoint;

    /// <summary>
    /// Defines the <see cref="Items"/> property.
    /// </summary>
    public static readonly DirectProperty<Particles, ObservableCollection<Particle>> ItemsProperty =
        AvaloniaProperty.RegisterDirect<Particles, ObservableCollection<Particle>>(
            nameof(Items),
            o => o.Items);

    /// <summary>
    /// Defines the <see cref="Source"/> property.
    /// </summary>
    public static readonly StyledProperty<IImage?> SourceProperty =
        AvaloniaProperty.Register<Particles, IImage?>(nameof(Source));

    /// <summary>
    /// Defines the <see cref="FrameSize"/> property.
    /// </summary>
    public static readonly StyledProperty<Size> FrameSizeProperty =
        AvaloniaProperty.Register<Particles, Size>(
            nameof(FrameSize),
            defaultValue: new Size(32, 32));

    /// <summary>
    /// Defines the <see cref="FrameColumns"/> property.
    /// </summary>
    public static readonly StyledProperty<int> FrameColumnsProperty =
        AvaloniaProperty.Register<Particles, int>(
            nameof(FrameColumns),
            defaultValue: 1,
            coerce: (_, value) => Math.Max(1, value));

    /// <summary>
    /// Defines the <see cref="Origin"/> property.
    /// </summary>
    public static readonly StyledProperty<RelativePoint> OriginProperty =
        AvaloniaProperty.Register<Particles, RelativePoint>(
            nameof(Origin),
            defaultValue: RelativePoint.TopLeft);

    /// <summary>
    /// Defines the <see cref="IsRunning"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsRunningProperty =
        AvaloniaProperty.Register<Particles, bool>(
            nameof(IsRunning),
            defaultValue: true);

    /// <summary>
    /// Defines the <see cref="TargetFrameRate"/> property.
    /// </summary>
    public static readonly StyledProperty<double> TargetFrameRateProperty =
        AvaloniaProperty.Register<Particles, double>(
            nameof(TargetFrameRate),
            defaultValue: 60.0,
            coerce: (_, value) => Math.Clamp(value, 1, 240));

    /// <summary>
    /// Defines the <see cref="MaxItems"/> property.
    /// </summary>
    public static readonly StyledProperty<int> MaxItemsProperty =
        AvaloniaProperty.Register<Particles, int>(
            nameof(MaxItems),
            defaultValue: 5000,
            coerce: (_, value) => Math.Max(1, value));

    static Particles()
    {
        AffectsRender<Particles>(SourceProperty);
        AffectsMeasure<Particles>(OriginProperty);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Particles"/> class.
    /// </summary>
    public Particles()
    {
        _updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(1000.0 / 60.0)
        };
        _updateTimer.Tick += OnUpdateTick;
        _lastUpdate = DateTime.Now;

        AttachedToVisualTree += (s, e) =>
        {
            if (IsRunning)
                Start();
        };

        DetachedFromVisualTree += (s, e) => Stop();
    }

    /// <summary>
    /// Occurs when particles need to be updated.
    /// </summary>
    public event EventHandler<ParticleUpdateEventArgs>? Update;

    /// <summary>
    /// Gets the collection of particles.
    /// </summary>
    public ObservableCollection<Particle> Items => _items;

    /// <summary>
    /// Gets or sets the sprite sheet image source for particles.
    /// </summary>
    public IImage? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the size of each frame in the sprite sheet.
    /// </summary>
    public Size FrameSize
    {
        get => GetValue(FrameSizeProperty);
        set => SetValue(FrameSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the number of columns in the sprite sheet.
    /// </summary>
    public int FrameColumns
    {
        get => GetValue(FrameColumnsProperty);
        set => SetValue(FrameColumnsProperty, value);
    }

    /// <summary>
    /// Gets or sets the origin point for particle positioning.
    /// </summary>
    public RelativePoint Origin
    {
        get => GetValue(OriginProperty);
        set => SetValue(OriginProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the particle system is running.
    /// </summary>
    public bool IsRunning
    {
        get => GetValue(IsRunningProperty);
        set => SetValue(IsRunningProperty, value);
    }

    /// <summary>
    /// Gets or sets the target frame rate for updates.
    /// </summary>
    public double TargetFrameRate
    {
        get => GetValue(TargetFrameRateProperty);
        set => SetValue(TargetFrameRateProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum number of particles allowed.
    /// </summary>
    public int MaxItems
    {
        get => GetValue(MaxItemsProperty);
        set => SetValue(MaxItemsProperty, value);
    }

    /// <summary>
    /// Starts the particle system.
    /// </summary>
    public void Start()
    {
        if (!_updateTimer.IsEnabled)
        {
            _lastUpdate = DateTime.Now;
            _updateTimer.Start();
        }
    }

    /// <summary>
    /// Stops the particle system.
    /// </summary>
    public void Stop()
    {
        _updateTimer.Stop();
    }

    /// <summary>
    /// Clears all particles and returns them to the pool.
    /// </summary>
    public void Clear()
    {
        foreach (var particle in _items)
        {
            _particlePool.Return(particle);
        }
        _items.Clear();
        InvalidateVisual();
    }

    /// <summary>
    /// Adds a new particle to the system.
    /// </summary>
    /// <returns>The new particle, or null if the maximum is reached.</returns>
    public Particle? Add()
    {
        if (_items.Count >= MaxItems)
            return null;

        var particle = _particlePool.Rent();
        _items.Add(particle);
        return particle;
    }

    /// <summary>
    /// Removes a particle from the system and returns it to the pool.
    /// </summary>
    /// <param name="particle">The particle to remove.</param>
    /// <returns>True if the particle was removed, false otherwise.</returns>
    public bool Remove(Particle particle)
    {
        if (_items.Remove(particle))
        {
            _particlePool.Return(particle);
            return true;
        }
        return false;
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsRunningProperty)
        {
            if (IsRunning && VisualRoot != null)
                Start();
            else
                Stop();
        }
        else if (change.Property == TargetFrameRateProperty)
        {
            _updateTimer.Interval = TimeSpan.FromMilliseconds(1000.0 / TargetFrameRate);
        }
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        UpdateOriginPoint(availableSize);
        return base.MeasureOverride(availableSize);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        UpdateOriginPoint(finalSize);
        return base.ArrangeOverride(finalSize);
    }

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        // Draw background manually since we don't use a template
        if (Background != null)
        {
            context.DrawRectangle(Background, null, new Rect(Bounds.Size));
        }

        base.Render(context);

        if (_items.Count == 0) return;

        var spriteSheet = new ParticleSpriteSheet
        {
            Image = Source,
            FrameWidth = (int)FrameSize.Width,
            FrameHeight = (int)FrameSize.Height,
            Columns = FrameColumns
        };

        RenderParticles(context, spriteSheet);
    }

    private void OnUpdateTick(object? sender, EventArgs e)
    {
        if (!IsRunning) return;

        var now = DateTime.Now;
        var deltaTime = (now - _lastUpdate).TotalSeconds;
        _lastUpdate = now;
        _totalElapsed += TimeSpan.FromSeconds(deltaTime);

        // Update timer interval if frame rate changed
        var targetInterval = 1000.0 / TargetFrameRate;
        if (Math.Abs(_updateTimer.Interval.TotalMilliseconds - targetInterval) > 1)
        {
            _updateTimer.Interval = TimeSpan.FromMilliseconds(targetInterval);
        }

        // Update particle lifetimes
        for (int i = _items.Count - 1; i >= 0; i--)
        {
            _items[i].LifeTime += deltaTime;
        }

        // Raise update event for user-defined logic
        var args = new ParticleUpdateEventArgs(
            _items,
            _totalElapsed,
            deltaTime,
            Bounds.Size);

        Update?.Invoke(this, args);

        InvalidateVisual();
    }

    private void UpdateOriginPoint(Size size)
    {
        _originPoint = Origin.ToPixels(new Rect(size));
    }

    private void RenderParticles(DrawingContext context, ParticleSpriteSheet spriteSheet)
    {
        foreach (var particle in _items)
        {
            if (particle.Opacity <= 0) continue;

            var renderX = _originPoint.X + particle.X;
            var renderY = _originPoint.Y + particle.Y;

            if (spriteSheet.Image != null)
            {
                var sourceRect = spriteSheet.GetFrameRect(particle.Frame);

                var destRect = new Rect(
                    renderX - sourceRect.Width * particle.Scale / 2,
                    renderY - sourceRect.Height * particle.Scale / 2,
                    sourceRect.Width * particle.Scale,
                    sourceRect.Height * particle.Scale);

                using (context.PushOpacity(particle.Opacity))
                {
                    if (Math.Abs(particle.Rotation) > 0.001)
                    {
                        using (context.PushTransform(Matrix.CreateRotation(
                            particle.Rotation * Math.PI / 180,
                            new Point(renderX, renderY))))
                        {
                            context.DrawImage(spriteSheet.Image, sourceRect, destRect);
                        }
                    }
                    else
                    {
                        context.DrawImage(spriteSheet.Image, sourceRect, destRect);
                    }
                }
            }
            else
            {
                // Fallback: render shapes based on particle.Shape
                var brush = new SolidColorBrush(particle.Color);
                var baseSize = 8 * particle.Scale;

                using (context.PushOpacity(particle.Opacity))
                {
                    if (Math.Abs(particle.Rotation) > 0.001)
                    {
                        using (context.PushTransform(Matrix.CreateRotation(
                            particle.Rotation * Math.PI / 180,
                            new Point(renderX, renderY))))
                        {
                            RenderShape(context, brush, renderX, renderY, baseSize, particle.Shape);
                        }
                    }
                    else
                    {
                        RenderShape(context, brush, renderX, renderY, baseSize, particle.Shape);
                    }
                }
            }
        }
    }

    private static void RenderShape(DrawingContext context, IBrush brush, double x, double y, double size, ParticleShape shape)
    {
        switch (shape)
        {
            case ParticleShape.Circle:
                context.DrawEllipse(brush, null, new Point(x, y), size / 2, size / 2);
                break;
            case ParticleShape.Square:
                var squareRect = new Rect(x - size / 2, y - size / 2, size, size);
                context.DrawRectangle(brush, null, squareRect);
                break;
            case ParticleShape.Rectangle:
                var rectWidth = size * 1.5;
                var rectHeight = size * 0.6;
                var rectRect = new Rect(x - rectWidth / 2, y - rectHeight / 2, rectWidth, rectHeight);
                context.DrawRectangle(brush, null, rectRect);
                break;
            case ParticleShape.Triangle:
                var triangleGeometry = new StreamGeometry();
                using (var ctx = triangleGeometry.Open())
                {
                    var halfSize = size / 2;
                    ctx.BeginFigure(new Point(x, y - halfSize), true);
                    ctx.LineTo(new Point(x + halfSize, y + halfSize));
                    ctx.LineTo(new Point(x - halfSize, y + halfSize));
                    ctx.EndFigure(true);
                }
                context.DrawGeometry(brush, null, triangleGeometry);
                break;
            case ParticleShape.Star:
                var starGeometry = new StreamGeometry();
                using (var ctx = starGeometry.Open())
                {
                    var outerRadius = size / 2;
                    var innerRadius = size / 4;
                    ctx.BeginFigure(new Point(x, y - outerRadius), true);
                    ctx.LineTo(new Point(x + innerRadius * 0.7, y - innerRadius * 0.7));
                    ctx.LineTo(new Point(x + outerRadius, y));
                    ctx.LineTo(new Point(x + innerRadius * 0.7, y + innerRadius * 0.7));
                    ctx.LineTo(new Point(x, y + outerRadius));
                    ctx.LineTo(new Point(x - innerRadius * 0.7, y + innerRadius * 0.7));
                    ctx.LineTo(new Point(x - outerRadius, y));
                    ctx.LineTo(new Point(x - innerRadius * 0.7, y - innerRadius * 0.7));
                    ctx.EndFigure(true);
                }
                context.DrawGeometry(brush, null, starGeometry);
                break;
            case ParticleShape.Line:
                var lineHeight = size * 2;
                var lineWidth = size * 0.3;
                var lineRect = new Rect(x - lineWidth / 2, y - lineHeight / 2, lineWidth, lineHeight);
                context.DrawRectangle(brush, null, lineRect);
                break;
        }
    }
}
