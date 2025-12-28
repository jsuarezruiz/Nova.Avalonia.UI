using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
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
            particle.UpdatePosition(e.DeltaTime);

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

            particle.UpdatePosition(e.DeltaTime);

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

            particle.UpdatePosition(e.DeltaTime);

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

            particle.UpdatePosition(e.DeltaTime);

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

            particle.UpdatePosition(e.DeltaTime);

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

            particle.UpdatePosition(e.DeltaTime);

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

            particle.UpdatePosition(e.DeltaTime);

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

            particle.UpdatePosition(e.DeltaTime);

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
}

