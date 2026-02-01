using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// A high-performance particle system control for creating custom particle effects.
/// </summary>
public class Particles : TemplatedControl
{
    private static readonly Dictionary<ParticleShape, StreamGeometry> ShapeGeometries = CreateShapeGeometries();
    private readonly Dictionary<Color, IBrush> _brushCache = new();
    private readonly ObservableCollection<ParticleAffector> _affectors = new();
    private readonly ObservableCollection<Particle> _items = new();
    private readonly ParticlePool _particlePool = new();
    private readonly DispatcherTimer _updateTimer;
    private readonly Stopwatch _stopwatch = new();
    private double _lastElapsedSeconds;
    private TimeSpan _totalElapsed;
    private Point _originPoint;
    private ParticleUpdateEventArgs? _sharedArgs;
    private ParticleSpriteSheet? _sharedSpriteSheet;

    /// <summary>
    /// Defines the <see cref="Items"/> property.
    /// </summary>
    public static readonly DirectProperty<Particles, ObservableCollection<Particle>> ItemsProperty =
        AvaloniaProperty.RegisterDirect<Particles, ObservableCollection<Particle>>(
            nameof(Items),
            o => o.Items);

    /// <summary>
    /// Defines the <see cref="Affectors"/> property.
    /// </summary>
    public static readonly DirectProperty<Particles, ObservableCollection<ParticleAffector>> AffectorsProperty =
        AvaloniaProperty.RegisterDirect<Particles, ObservableCollection<ParticleAffector>>(
            nameof(Affectors),
            o => o.Affectors);

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
    /// Gets the collection of affectors that modify particles over time.
    /// </summary>
    public ObservableCollection<ParticleAffector> Affectors => _affectors;

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
            _stopwatch.Start();
            _lastElapsedSeconds = _stopwatch.Elapsed.TotalSeconds;
            _updateTimer.Start();
        }
    }

    /// <summary>
    /// Stops the particle system.
    /// </summary>
    public void Stop()
    {
        _updateTimer.Stop();
        _stopwatch.Stop();
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
        if (Background != null)
        {
            context.DrawRectangle(Background, null, new Rect(Bounds.Size));
        }

        base.Render(context);

        if (_items.Count == 0) return;

        if (_sharedSpriteSheet == null)
            _sharedSpriteSheet = new ParticleSpriteSheet();

        _sharedSpriteSheet.Image = Source;
        _sharedSpriteSheet.FrameWidth = (int)FrameSize.Width;
        _sharedSpriteSheet.FrameHeight = (int)FrameSize.Height;
        _sharedSpriteSheet.Columns = FrameColumns;
 
        var viewport = new Rect(Bounds.Size);
        RenderParticles(context, _sharedSpriteSheet, viewport);
    }

    private void OnUpdateTick(object? sender, EventArgs e)
    {
        if (!IsRunning || VisualRoot == null) return;
 
        var currentElapsed = _stopwatch.Elapsed.TotalSeconds;
        var deltaTime = currentElapsed - _lastElapsedSeconds;
        
        if (deltaTime <= 0) return;
        if (deltaTime > 0.1) deltaTime = 0.1;
        
        _lastElapsedSeconds = currentElapsed;
        _totalElapsed += TimeSpan.FromSeconds(deltaTime);
 
        foreach (var affector in _affectors)
        {
            affector.Update(deltaTime);
        }
 
        // Update particles in parallel for high performance
        if (_items.Count > 100)
        {
            Parallel.For(0, _items.Count, i =>
            {
                var particle = _items[i];
                particle.LifeTime += deltaTime;
                foreach (var affector in _affectors)
                {
                    affector.Apply(particle, deltaTime);
                }
                particle.UpdatePosition(deltaTime);
            });
        }
        else
        {
            for (int i = 0; i < _items.Count; i++)
            {
                var particle = _items[i];
                particle.LifeTime += deltaTime;
                foreach (var affector in _affectors)
                {
                    affector.Apply(particle, deltaTime);
                }
                particle.UpdatePosition(deltaTime);
            }
        }

        // Cleanup inactive particles
        for (int i = _items.Count - 1; i >= 0; i--)
        {
            var particle = _items[i];
            if (particle.Opacity <= 0 || !particle.IsActive)
            {
                _particlePool.Return(particle);
                _items.RemoveAt(i);
            }
        }
 
        if (Update != null)
        {
            if (_sharedArgs == null)
            {
                _sharedArgs = new ParticleUpdateEventArgs(_items, _totalElapsed, deltaTime, Bounds.Size);
            }
            else
            {
                // Internal update of existing args to avoid allocation
                _sharedArgs.Update(deltaTime, _totalElapsed, Bounds.Size);
            }
            Update.Invoke(this, _sharedArgs);
        }
 
        InvalidateVisual();
    }

    private static Dictionary<ParticleShape, StreamGeometry> CreateShapeGeometries()
    {
        var geometries = new Dictionary<ParticleShape, StreamGeometry>();

        // Circle and Square are better drawn with DrawEllipse/DrawRectangle for performance, 
        // but for consistency we can also use geometries.
        // However, we'll keep the specialized methods for Circle/Square/Rect and use geometries for Tri/Star.

        geometries[ParticleShape.Triangle] = CreateTriangleGeometry();
        geometries[ParticleShape.Star] = CreateStarGeometry();

        // Freeze geometries for thread safety and performance
        foreach (var g in geometries.Values)
        {
            // Avalonia geometries don't have a Freeze method like WPF, 
            // but closing the StreamGeometry makes it immutable.
        }

        return geometries;
    }

    private static StreamGeometry CreateTriangleGeometry()
    {
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            const double size = 1.0;
            const double halfSize = size / 2.0;
            ctx.BeginFigure(new Point(0, -halfSize), true);
            ctx.LineTo(new Point(halfSize, halfSize));
            ctx.LineTo(new Point(-halfSize, halfSize));
            ctx.EndFigure(true);
        }
        return geometry;
    }

    private static StreamGeometry CreateStarGeometry()
    {
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            const double outerRadius = 0.5;
            const double innerRadius = 0.25;
            ctx.BeginFigure(new Point(0, -outerRadius), true);
            ctx.LineTo(new Point(innerRadius * 0.7, -innerRadius * 0.7));
            ctx.LineTo(new Point(outerRadius, 0));
            ctx.LineTo(new Point(innerRadius * 0.7, innerRadius * 0.7));
            ctx.LineTo(new Point(0, outerRadius));
            ctx.LineTo(new Point(-innerRadius * 0.7, innerRadius * 0.7));
            ctx.LineTo(new Point(-outerRadius, 0));
            ctx.LineTo(new Point(-innerRadius * 0.7, -innerRadius * 0.7));
            ctx.EndFigure(true);
        }
        return geometry;
    }

    private IBrush GetBrush(Color color)
    {
        if (!_brushCache.TryGetValue(color, out var brush))
        {
            if (_brushCache.Count > 100) _brushCache.Clear();
            brush = new SolidColorBrush(color);
            _brushCache[color] = brush;
        }
        return brush;
    }

    private void UpdateOriginPoint(Size size)
    {
        _originPoint = Origin.ToPixels(new Rect(size));
    }
 
    private void RenderParticles(DrawingContext context, ParticleSpriteSheet spriteSheet, Rect viewport)
    {
        foreach (var particle in _items)
        {
            if (particle.Opacity <= 0) continue;
 
            var renderX = _originPoint.X + particle.X;
            var renderY = _originPoint.Y + particle.Y;
            
            // Viewport clipping (rough check)
            var size = 16 * particle.Scale; // Heuristic size for clipping
            if (renderX + size < viewport.Left || renderX - size > viewport.Right ||
                renderY + size < viewport.Top || renderY - size > viewport.Bottom)
            {
                continue;
            }

            var hasOpacity = particle.Opacity < 0.999;
            var hasRotation = Math.Abs(particle.Rotation) > 0.001;

            if (spriteSheet.Image != null)
            {
                var sourceRect = spriteSheet.GetFrameRect(particle.Frame);
                var destRect = new Rect(
                    renderX - sourceRect.Width * particle.Scale / 2,
                    renderY - sourceRect.Height * particle.Scale / 2,
                    sourceRect.Width * particle.Scale,
                    sourceRect.Height * particle.Scale);

                if (hasOpacity)
                {
                    using (context.PushOpacity(particle.Opacity))
                    {
                        if (hasRotation)
                        {
                            using (context.PushTransform(Matrix.CreateRotation(particle.Rotation * Math.PI / 180, new Point(renderX, renderY))))
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
                    if (hasRotation)
                    {
                        using (context.PushTransform(Matrix.CreateRotation(particle.Rotation * Math.PI / 180, new Point(renderX, renderY))))
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
                var brush = GetBrush(particle.Color);
                var baseSize = 8 * particle.Scale;

                if (hasOpacity)
                {
                    using (context.PushOpacity(particle.Opacity))
                    {
                        if (hasRotation)
                        {
                            using (context.PushTransform(Matrix.CreateRotation(particle.Rotation * Math.PI / 180, new Point(renderX, renderY))))
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
                else
                {
                    if (hasRotation)
                    {
                        using (context.PushTransform(Matrix.CreateRotation(particle.Rotation * Math.PI / 180, new Point(renderX, renderY))))
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
            case ParticleShape.Star:
                if (ShapeGeometries.TryGetValue(shape, out var geometry))
                {
                    using (context.PushTransform(Matrix.CreateTranslation(x, y) * Matrix.CreateScale(size, size)))
                    {
                        context.DrawGeometry(brush, null, geometry);
                    }
                }
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
