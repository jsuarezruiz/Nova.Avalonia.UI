using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Nova.Avalonia.UI.Controls;
using Nova.Avalonia.UI.Gallery.ViewModels;

namespace Nova.Avalonia.UI.Gallery.Views;

public partial class ParticlesView : UserControl
{
    private readonly Random _random = new();
    private readonly GravityAffector _gravity = new() { GravityY = 200 };
    private readonly FadeAffector _fade = new() { FadeRate = 0.8 };
    private readonly RotationAffector _rotation = new() { RotationSpeed = 180 };
    private readonly WindAffector _wind = new() { WindX = 30, Turbulent = true };
    private Particles[] _particleSystems = Array.Empty<Particles>();

    // Fire colors
    private readonly Color[] _fireColors = new[]
    {
        Color.FromRgb(255, 100, 0),    // Orange
        Color.FromRgb(255, 200, 50),   // Yellow
        Color.FromRgb(255, 50, 0),     // Red
        Color.FromRgb(255, 150, 0),    // Light orange
    };

    // Confetti colors
    private readonly Color[] _confettiColors = new[]
    {
        Color.FromRgb(255, 107, 107),  // Red
        Color.FromRgb(78, 205, 196),   // Teal
        Color.FromRgb(255, 230, 109),  // Yellow
        Color.FromRgb(170, 111, 215),  // Purple
        Color.FromRgb(69, 183, 209),   // Blue
        Color.FromRgb(255, 159, 243),  // Pink
    };

    public ParticlesView()
    {
        InitializeComponent();
        DataContext = new ParticlesViewModel();

        _particleSystems = new[]
        {
            RainParticles,
            FireParticles,
            FireworksParticles,
            ConfettiParticles,
            SparkleParticles,
            SnowParticles,
            BubbleParticles,
            SmokeParticles,
            GalaxyParticles,
            PortalGlowParticles,
            PortalOrbitParticles,
            PortalSparkParticles
        };

        ParticlesScrollViewer.ScrollChanged += (_, _) => UpdateVisibleParticleSystems();
        ParticlesScrollViewer.SizeChanged += (_, _) => UpdateVisibleParticleSystems();
        foreach (var particles in _particleSystems)
        {
            particles.SizeChanged += (_, _) => UpdateVisibleParticleSystems();
        }

        AttachedToVisualTree += (_, _) => Dispatcher.UIThread.Post(UpdateVisibleParticleSystems, DispatcherPriority.Background);
        DetachedFromVisualTree += (_, _) => StopParticleTimers();
    }

    private void UpdateVisibleParticleSystems()
    {
        if (ParticlesScrollViewer.Bounds.Width <= 0 || ParticlesScrollViewer.Bounds.Height <= 0)
            return;

        var viewport = new Rect(
            0,
            -300,
            ParticlesScrollViewer.Bounds.Width,
            ParticlesScrollViewer.Bounds.Height + 600);

        foreach (var particles in _particleSystems)
        {
            if (particles.Bounds.Width <= 0 || particles.Bounds.Height <= 0)
                continue;

            var position = particles.TranslatePoint(new Point(), ParticlesScrollViewer);
            var bounds = position.HasValue
                ? new Rect(position.Value, particles.Bounds.Size)
                : default;

            particles.IsRunning = bounds.Intersects(viewport);
        }
    }

    private void StopParticleTimers()
    {
        foreach (var particles in _particleSystems)
        {
            particles.Stop();
        }
    }

    // ===== RAIN EFFECT =====
    private void OnRainUpdate(object? sender, ParticleUpdateEventArgs e)
    {
        var particles = sender as Particles;
        var size = e.CanvasSize;

        // Spawn new rain drops
        if (e.Items.Count < 300)
        {
            for (int i = 0; i < 3; i++)
            {
                var particle = particles!.Add();
                if (particle == null) break;

                particle.X = _random.NextDouble() * size.Width;
                particle.Y = -10;
                particle.VelocityX = _random.NextDouble() * 20 - 10;
                particle.VelocityY = 200 + _random.NextDouble() * 100;
                particle.Scale = 0.3 + _random.NextDouble() * 0.3;
                particle.Opacity = 0.5 + _random.NextDouble() * 0.5;
                particle.Color = Color.FromRgb(150, 200, 255);
                particle.Shape = ParticleShape.Line;
            }
        }

        // Update existing particles
        for (int i = e.Items.Count - 1; i >= 0; i--)
        {
            var particle = e.Items[i];

            // Remove particles that fall below screen
            if (particle.Y > size.Height + 20)
            {
                particles!.Remove(particle);
            }
        }
    }

    // ===== FIRE EFFECT =====
    private void OnFireUpdate(object? sender, ParticleUpdateEventArgs e)
    {
        var particles = sender as Particles;
        var size = e.CanvasSize;

        // Spawn fire particles from bottom center
        if (e.Items.Count < 200)
        {
            for (int i = 0; i < 2; i++)
            {
                var particle = particles!.Add();
                if (particle == null) break;

                particle.X = (_random.NextDouble() - 0.5) * 100;
                particle.Y = 0;
                particle.VelocityX = (_random.NextDouble() - 0.5) * 60;
                particle.VelocityY = -100 - _random.NextDouble() * 80;
                particle.Scale = 0.8 + _random.NextDouble() * 0.8;
                particle.Opacity = 1.0;
                particle.Color = _fireColors[_random.Next(_fireColors.Length)];
            }
        }

        // Update particles
        for (int i = e.Items.Count - 1; i >= 0; i--)
        {
            var particle = e.Items[i];

            // Apply effects
            _fade.Apply(particle, e.DeltaTime);
            particle.Scale -= 0.5 * e.DeltaTime;
            particle.VelocityX += (_random.NextDouble() - 0.5) * 20;

            // Remove faded or small particles
            if (particle.Opacity <= 0 || particle.Scale <= 0.1)
            {
                particles!.Remove(particle);
            }
        }
    }

    // ===== FIREWORKS EFFECT =====
    private void OnFireworksClick(object? sender, PointerPressedEventArgs e)
    {
        var particles = sender as Particles;
        if (particles == null) return;

        var point = e.GetPosition(particles);
        var color = Color.FromRgb(
            (byte)_random.Next(128, 256),
            (byte)_random.Next(128, 256),
            (byte)_random.Next(128, 256));

        // Create burst of particles
        for (int i = 0; i < 50; i++)
        {
            var particle = particles.Add();
            if (particle == null) break;

            var angle = _random.NextDouble() * 360;
            var speed = 100 + _random.NextDouble() * 150;
            var angleRad = angle * Math.PI / 180;

            particle.X = point.X;
            particle.Y = point.Y;
            particle.VelocityX = Math.Cos(angleRad) * speed;
            particle.VelocityY = Math.Sin(angleRad) * speed;
            particle.Scale = 0.5 + _random.NextDouble() * 0.5;
            particle.Opacity = 1.0;
            particle.Color = color;
        }
    }

    private void OnFireworksUpdate(object? sender, ParticleUpdateEventArgs e)
    {
        var particles = sender as Particles;

        for (int i = e.Items.Count - 1; i >= 0; i--)
        {
            var particle = e.Items[i];

            _gravity.Apply(particle, e.DeltaTime);
            _fade.Apply(particle, e.DeltaTime);

            if (particle.Opacity <= 0)
            {
                particles!.Remove(particle);
            }
        }
    }

    // ===== CONFETTI EFFECT =====
    private void OnConfettiBurst(object? sender, RoutedEventArgs e)
    {
        var particles = ConfettiParticles;
        var shapes = new[] { ParticleShape.Circle, ParticleShape.Square, ParticleShape.Rectangle };

        // Create burst of confetti
        for (int i = 0; i < 80; i++)
        {
            var particle = particles.Add();
            if (particle == null) break;

            particle.X = (_random.NextDouble() - 0.5) * 200;
            particle.Y = 0;
            particle.VelocityX = (_random.NextDouble() - 0.5) * 200;
            particle.VelocityY = 100 + _random.NextDouble() * 150;
            particle.Scale = 0.8 + _random.NextDouble() * 0.6;
            particle.Rotation = _random.NextDouble() * 360;
            particle.Opacity = 1.0;
            particle.Color = _confettiColors[_random.Next(_confettiColors.Length)];
            particle.Shape = shapes[_random.Next(shapes.Length)];
        }
    }

    private void OnConfettiUpdate(object? sender, ParticleUpdateEventArgs e)
    {
        var particles = sender as Particles;
        var size = e.CanvasSize;

        for (int i = e.Items.Count - 1; i >= 0; i--)
        {
            var particle = e.Items[i];

            _gravity.Apply(particle, e.DeltaTime);
            _rotation.Apply(particle, e.DeltaTime);
            _wind.Apply(particle, e.DeltaTime);

            // Add some flutter
            particle.VelocityX += Math.Sin(particle.LifeTime * 5) * 10 * e.DeltaTime;

            // Remove particles that fall below screen
            if (particle.Y > size.Height + 20)
            {
                particles!.Remove(particle);
            }
        }
    }

    // ===== SPARKLE TRAIL (INTERACTIVE) =====
    private readonly Color[] _sparkleColors = new[]
    {
        Color.FromRgb(255, 215, 0),    // Gold
        Color.FromRgb(255, 182, 193),  // Pink
        Color.FromRgb(173, 216, 230),  // Light blue
        Color.FromRgb(255, 255, 255),  // White
        Color.FromRgb(238, 130, 238),  // Violet
    };

    private void OnSparklePointerMoved(object? sender, PointerEventArgs e)
    {
        var particles = sender as Particles;
        if (particles == null) return;

        var point = e.GetPosition(particles);

        // Spawn multiple sparkles at mouse position
        for (int i = 0; i < 3; i++)
        {
            var particle = particles.Add();
            if (particle == null) break;

            var angle = _random.NextDouble() * 360;
            var speed = 20 + _random.NextDouble() * 40;
            var angleRad = angle * Math.PI / 180;

            particle.X = point.X + (_random.NextDouble() - 0.5) * 10;
            particle.Y = point.Y + (_random.NextDouble() - 0.5) * 10;
            particle.VelocityX = Math.Cos(angleRad) * speed;
            particle.VelocityY = Math.Sin(angleRad) * speed;
            particle.Scale = 0.3 + _random.NextDouble() * 0.5;
            particle.Opacity = 1.0;
            particle.Color = _sparkleColors[_random.Next(_sparkleColors.Length)];
            particle.Shape = ParticleShape.Circle;
        }
    }

    private void OnSparkleUpdate(object? sender, ParticleUpdateEventArgs e)
    {
        var particles = sender as Particles;

        for (int i = e.Items.Count - 1; i >= 0; i--)
        {
            var particle = e.Items[i];

            // Fade and shrink
            particle.Opacity -= 2.0 * e.DeltaTime;
            particle.Scale -= 0.5 * e.DeltaTime;

            if (particle.Opacity <= 0 || particle.Scale <= 0.1)
            {
                particles!.Remove(particle);
            }
        }
    }

    // ===== SNOW EFFECT =====
    private void OnSnowUpdate(object? sender, ParticleUpdateEventArgs e)
    {
        var particles = sender as Particles;
        var size = e.CanvasSize;

        // Spawn new snowflakes
        if (e.Items.Count < 200)
        {
            for (int i = 0; i < 2; i++)
            {
                var particle = particles!.Add();
                if (particle == null) break;

                particle.X = _random.NextDouble() * size.Width;
                particle.Y = -10;
                particle.VelocityX = (_random.NextDouble() - 0.5) * 20;
                particle.VelocityY = 30 + _random.NextDouble() * 30;
                particle.Scale = 0.3 + _random.NextDouble() * 0.7;
                particle.Rotation = _random.NextDouble() * 360;
                particle.Opacity = 0.6 + _random.NextDouble() * 0.4;
                particle.Color = Color.FromRgb(100, 150, 220);
                particle.Shape = ParticleShape.Circle;
            }
        }

        // Update snowflakes with gentle sway
        for (int i = e.Items.Count - 1; i >= 0; i--)
        {
            var particle = e.Items[i];

            // Gentle horizontal sway
            particle.VelocityX = Math.Sin(particle.LifeTime * 2 + particle.X * 0.01) * 25;
            
            // Slow rotation
            particle.Rotation += 30 * e.DeltaTime;

            // Remove particles that fall below screen
            if (particle.Y > size.Height + 20)
            {
                particles!.Remove(particle);
            }
        }
    }

    // ===== BUBBLES EFFECT =====
    private void OnBubblesUpdate(object? sender, ParticleUpdateEventArgs e)
    {
        var particles = sender as Particles;
        var size = e.CanvasSize;

        // Spawn new bubbles from bottom
        if (e.Items.Count < 100)
        {
            var particle = particles!.Add();
            if (particle != null)
            {
                particle.X = _random.NextDouble() * size.Width;
                particle.Y = size.Height + 10;
                particle.VelocityX = (_random.NextDouble() - 0.5) * 20;
                particle.VelocityY = -40 - _random.NextDouble() * 30;
                particle.Scale = 0.4 + _random.NextDouble() * 0.8;
                particle.Opacity = 0.4 + _random.NextDouble() * 0.4;
                particle.Color = Color.FromArgb(180, 150, 220, 255);
                particle.Shape = ParticleShape.Circle;
            }
        }

        // Update bubbles with wobble
        for (int i = e.Items.Count - 1; i >= 0; i--)
        {
            var particle = e.Items[i];

            // Wobble side to side
            particle.VelocityX = Math.Sin(particle.LifeTime * 3 + particle.X * 0.05) * 30;
            
            // Slight scale pulsing
            particle.Scale += Math.Sin(particle.LifeTime * 5) * 0.01;

            // Remove bubbles that rise above screen
            if (particle.Y < -20)
            {
                particles!.Remove(particle);
            }
        }
    }

    // ===== SMOKE EFFECT =====
    private void OnSmokeUpdate(object? sender, ParticleUpdateEventArgs e)
    {
        var particles = sender as Particles;

        // Spawn smoke from bottom center
        if (e.Items.Count < 150)
        {
            var particle = particles!.Add();
            if (particle != null)
            {
                particle.X = (_random.NextDouble() - 0.5) * 60;
                particle.Y = 0;
                particle.VelocityX = (_random.NextDouble() - 0.5) * 30;
                particle.VelocityY = -50 - _random.NextDouble() * 40;
                particle.Scale = 0.5 + _random.NextDouble() * 0.5;
                particle.Opacity = 0.6 + _random.NextDouble() * 0.3;
                var gray = (byte)(80 + _random.Next(60));
                particle.Color = Color.FromRgb(gray, gray, gray);
                particle.Shape = ParticleShape.Circle;
            }
        }

        // Update smoke - spread, rise, and fade
        for (int i = e.Items.Count - 1; i >= 0; i--)
        {
            var particle = e.Items[i];

            // Spread outward as it rises
            particle.VelocityX += (_random.NextDouble() - 0.5) * 10;
            
            // Slow down rise
            particle.VelocityY *= 0.99;
            
            // Grow and fade
            particle.Scale += 0.3 * e.DeltaTime;
            particle.Opacity -= 0.4 * e.DeltaTime;

            if (particle.Opacity <= 0)
            {
                particles!.Remove(particle);
            }
        }
    }

    // ===== GALAXY EFFECT =====
    private readonly Color[] _galaxyColors = new[]
    {
        Color.FromRgb(255, 255, 255),  // White
        Color.FromRgb(200, 200, 255),  // Light blue
        Color.FromRgb(255, 200, 200),  // Light pink
        Color.FromRgb(255, 255, 200),  // Light yellow
    };

    private bool _galaxyInitialized;
    private bool _portalGlowInitialized;
    private bool _portalOrbitInitialized;

    private void OnGalaxyUpdate(object? sender, ParticleUpdateEventArgs e)
    {
        var particles = sender as Particles;

        // Initialize galaxy stars once
        if (!_galaxyInitialized && e.Items.Count < 300)
        {
            for (int i = 0; i < 300; i++)
            {
                var particle = particles!.Add();
                if (particle == null) break;

                // Distribute in spiral pattern
                var arm = _random.Next(3); // 3 spiral arms
                var distance = 20 + _random.NextDouble() * 120;
                var baseAngle = arm * 120 + distance * 0.5;
                var spread = (_random.NextDouble() - 0.5) * 30;
                var angle = (baseAngle + spread) * Math.PI / 180;

                particle.X = Math.Cos(angle) * distance;
                particle.Y = Math.Sin(angle) * distance;
                particle.Scale = 0.2 + _random.NextDouble() * 0.4;
                particle.Opacity = 0.5 + _random.NextDouble() * 0.5;
                particle.Color = _galaxyColors[_random.Next(_galaxyColors.Length)];
                particle.Shape = ParticleShape.Circle;
                particle.Tag = distance; // Store distance for orbit calculation
            }
            _galaxyInitialized = true;
        }

        // Orbit all particles around center
        foreach (var particle in e.Items)
        {
            var distance = particle.Tag as double? ?? 50;
            var currentAngle = Math.Atan2(particle.Y, particle.X);
            
            // Inner particles orbit faster
            var orbitSpeed = 0.5 / (distance * 0.02 + 1);
            var newAngle = currentAngle + orbitSpeed * e.DeltaTime;

            particle.X = Math.Cos(newAngle) * distance;
            particle.Y = Math.Sin(newAngle) * distance;

            // Twinkle effect
            particle.Opacity = 0.5 + Math.Sin(e.Elapsed.TotalSeconds * 3 + distance) * 0.3;
        }
    }

    // ===== ENERGY PORTAL (LAYERED) =====
    private sealed class PortalParticleState
    {
        public double Radius { get; init; }

        public double Angle { get; set; }

        public double AngularSpeed { get; init; }

        public double Phase { get; init; }

        public double PulseSpeed { get; init; }
    }

    private readonly Color[] _portalColors = new[]
    {
        Color.FromArgb(210, 57, 214, 255),
        Color.FromArgb(190, 121, 92, 255),
        Color.FromArgb(230, 255, 255, 255),
        Color.FromArgb(180, 66, 255, 191),
    };

    private void OnPortalGlowUpdate(object? sender, ParticleUpdateEventArgs e)
    {
        var particles = sender as Particles;

        if (!_portalGlowInitialized)
        {
            for (int i = 0; i < 24; i++)
            {
                var particle = particles!.Add();
                if (particle == null) break;

                particle.Shape = ParticleShape.Circle;
                particle.Color = i % 3 == 0
                    ? Color.FromArgb(70, 255, 255, 255)
                    : Color.FromArgb(90, 57, 214, 255);
                particle.Tag = new PortalParticleState
                {
                    Radius = 28 + _random.NextDouble() * 92,
                    Angle = _random.NextDouble() * Math.PI * 2,
                    AngularSpeed = (_random.NextDouble() - 0.5) * 0.35,
                    Phase = _random.NextDouble() * Math.PI * 2,
                    PulseSpeed = 0.7 + _random.NextDouble() * 1.2
                };
            }

            _portalGlowInitialized = true;
        }

        foreach (var particle in e.Items)
        {
            if (particle.Tag is not PortalParticleState state)
                continue;

            state.Angle += state.AngularSpeed * e.DeltaTime;
            var pulse = 0.5 + Math.Sin(e.Elapsed.TotalSeconds * state.PulseSpeed + state.Phase) * 0.5;
            var radius = state.Radius + pulse * 12;

            particle.X = Math.Cos(state.Angle) * radius;
            particle.Y = Math.Sin(state.Angle) * radius * 0.72;
            particle.Scale = 6.5 + pulse * 4.0;
            particle.Opacity = 0.08 + pulse * 0.18;
        }
    }

    private void OnPortalOrbitUpdate(object? sender, ParticleUpdateEventArgs e)
    {
        var particles = sender as Particles;

        if (!_portalOrbitInitialized)
        {
            for (int i = 0; i < 140; i++)
            {
                var particle = particles!.Add();
                if (particle == null) break;

                var radiusBand = i % 3;
                var radius = 58 + radiusBand * 22 + _random.NextDouble() * 16;
                particle.Shape = radiusBand == 0 ? ParticleShape.Star : ParticleShape.Circle;
                particle.Color = _portalColors[_random.Next(_portalColors.Length)];
                particle.Tag = new PortalParticleState
                {
                    Radius = radius,
                    Angle = _random.NextDouble() * Math.PI * 2,
                    AngularSpeed = (radiusBand % 2 == 0 ? 1 : -1) * (0.7 + _random.NextDouble() * 1.8),
                    Phase = _random.NextDouble() * Math.PI * 2,
                    PulseSpeed = 1.5 + _random.NextDouble() * 2.5
                };
            }

            _portalOrbitInitialized = true;
        }

        foreach (var particle in e.Items)
        {
            if (particle.Tag is not PortalParticleState state)
                continue;

            state.Angle += state.AngularSpeed * e.DeltaTime;
            var pulse = 0.5 + Math.Sin(e.Elapsed.TotalSeconds * state.PulseSpeed + state.Phase) * 0.5;
            var wobble = Math.Sin(e.Elapsed.TotalSeconds * 1.7 + state.Phase) * 7;
            var radius = state.Radius + wobble;

            particle.X = Math.Cos(state.Angle) * radius;
            particle.Y = Math.Sin(state.Angle) * radius * 0.58;
            particle.Scale = 0.35 + pulse * 0.85;
            particle.Rotation += 160 * e.DeltaTime;
            particle.Opacity = 0.28 + pulse * 0.72;
        }
    }

    private void OnPortalSparkUpdate(object? sender, ParticleUpdateEventArgs e)
    {
        var particles = sender as Particles;

        if (e.Items.Count < 220)
        {
            for (int i = 0; i < 4; i++)
            {
                var particle = particles!.Add();
                if (particle == null) break;

                var angle = _random.NextDouble() * Math.PI * 2;
                var radius = 32 + _random.NextDouble() * 62;
                var speed = 45 + _random.NextDouble() * 95;
                var tangent = angle + Math.PI / 2 + (_random.NextDouble() - 0.5) * 0.9;

                particle.X = Math.Cos(angle) * radius;
                particle.Y = Math.Sin(angle) * radius * 0.58;
                particle.VelocityX = Math.Cos(tangent) * speed;
                particle.VelocityY = Math.Sin(tangent) * speed * 0.58;
                particle.Scale = 0.45 + _random.NextDouble() * 0.55;
                particle.Opacity = 0.75 + _random.NextDouble() * 0.25;
                particle.Rotation = _random.NextDouble() * 360;
                particle.Shape = _random.Next(3) == 0 ? ParticleShape.Line : ParticleShape.Circle;
                particle.Color = _portalColors[_random.Next(_portalColors.Length)];
            }
        }

        for (int i = e.Items.Count - 1; i >= 0; i--)
        {
            var particle = e.Items[i];
            var distance = Math.Sqrt(particle.X * particle.X + particle.Y * particle.Y);

            particle.Opacity -= 1.8 * e.DeltaTime;
            particle.Scale -= 0.35 * e.DeltaTime;
            particle.VelocityX += -particle.X * 0.9 * e.DeltaTime;
            particle.VelocityY += -particle.Y * 0.9 * e.DeltaTime;
            particle.Rotation += 220 * e.DeltaTime;

            if (particle.Opacity <= 0 || particle.Scale <= 0.1 || distance > 190)
            {
                particles!.Remove(particle);
            }
        }
    }
}
