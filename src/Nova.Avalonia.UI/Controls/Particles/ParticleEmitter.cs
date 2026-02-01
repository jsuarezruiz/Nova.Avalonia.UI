using System;
using Avalonia;
using Avalonia.Media;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Helper class for spawning particles.
/// </summary>
public class ParticleEmitter
{
    private readonly Particles _particles;
    private readonly Random _random = new();
    private double _emissionAccumulator;

    public ParticleEmitter(Particles particles)
    {
        _particles = particles ?? throw new ArgumentNullException(nameof(particles));
    }

    public Point Position { get; set; }

    public double EmissionRate { get; set; } = 10;

    public double SpreadAngle { get; set; } = 360;

    public double BaseAngle { get; set; } = 0;

    public double MinSpeed { get; set; } = 50;

    public double MaxSpeed { get; set; } = 100;

    public double MinScale { get; set; } = 0.5;

    public double MaxScale { get; set; } = 1.5;

    public Color Color { get; set; } = Colors.White;

    public int MinFrame { get; set; } = 0;

    public int MaxFrame { get; set; } = 0;

    /// <summary>
    /// Updates the emitter and spawns particles based on EmissionRate.
    /// </summary>
    public void Update(double deltaTime)
    {
        _emissionAccumulator += EmissionRate * deltaTime;

        while (_emissionAccumulator >= 1.0)
        {
            EmitParticle();
            _emissionAccumulator -= 1.0;
        }
    }

    /// <summary>
    /// Emits a single particle.
    /// </summary>
    public Particle? EmitParticle()
    {
        var particle = _particles.Add();
        if (particle == null) return null;

        particle.X = Position.X;
        particle.Y = Position.Y;

        var angle = BaseAngle + _random.NextDouble() * SpreadAngle - SpreadAngle / 2;
        var speed = MinSpeed + _random.NextDouble() * (MaxSpeed - MinSpeed);
        var angleRad = angle * Math.PI / 180;

        particle.VelocityX = Math.Cos(angleRad) * speed;
        particle.VelocityY = Math.Sin(angleRad) * speed;
        particle.Scale = MinScale + _random.NextDouble() * (MaxScale - MinScale);
        particle.Color = Color;

        if (MaxFrame > MinFrame)
        {
            particle.Frame = _random.Next(MinFrame, MaxFrame + 1);
        }
        else
        {
            particle.Frame = MinFrame;
        }

        return particle;
    }

    /// <summary>
    /// Emits a burst of particles.
    /// </summary>
    public void Burst(int count)
    {
        for (int i = 0; i < count; i++)
        {
            EmitParticle();
        }
    }
}
