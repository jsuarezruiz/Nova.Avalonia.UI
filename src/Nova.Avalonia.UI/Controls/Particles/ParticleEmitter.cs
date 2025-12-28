using System;
using Avalonia;
using Avalonia.Media;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Helper class for spawning particles with configurable emission parameters.
/// </summary>
public class ParticleEmitter
{
    private readonly Particles _particles;
    private readonly Random _random = new();
    private double _emissionAccumulator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParticleEmitter"/> class.
    /// </summary>
    /// <param name="particles">The particle system to emit into.</param>
    public ParticleEmitter(Particles particles)
    {
        _particles = particles ?? throw new ArgumentNullException(nameof(particles));
    }

    /// <summary>
    /// Gets or sets the emission position.
    /// </summary>
    public Point Position { get; set; }

    /// <summary>
    /// Gets or sets the number of particles to emit per second.
    /// </summary>
    public double EmissionRate { get; set; } = 10;

    /// <summary>
    /// Gets or sets the spread angle in degrees.
    /// </summary>
    public double SpreadAngle { get; set; } = 360;

    /// <summary>
    /// Gets or sets the base emission angle in degrees.
    /// </summary>
    public double BaseAngle { get; set; } = 0;

    /// <summary>
    /// Gets or sets the minimum emission speed.
    /// </summary>
    public double MinSpeed { get; set; } = 50;

    /// <summary>
    /// Gets or sets the maximum emission speed.
    /// </summary>
    public double MaxSpeed { get; set; } = 100;

    /// <summary>
    /// Gets or sets the minimum particle scale.
    /// </summary>
    public double MinScale { get; set; } = 0.5;

    /// <summary>
    /// Gets or sets the maximum particle scale.
    /// </summary>
    public double MaxScale { get; set; } = 1.5;

    /// <summary>
    /// Gets or sets the particle color.
    /// </summary>
    public Color Color { get; set; } = Colors.White;

    /// <summary>
    /// Gets or sets the minimum sprite frame index.
    /// </summary>
    public int MinFrame { get; set; } = 0;

    /// <summary>
    /// Gets or sets the maximum sprite frame index.
    /// </summary>
    public int MaxFrame { get; set; } = 0;

    /// <summary>
    /// Updates the emitter, potentially spawning new particles.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update in seconds.</param>
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
    /// Emits a single particle with the current configuration.
    /// </summary>
    /// <returns>The emitted particle, or null if the particle limit is reached.</returns>
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
    /// Emits multiple particles at once.
    /// </summary>
    /// <param name="count">Number of particles to emit.</param>
    public void Burst(int count)
    {
        for (int i = 0; i < count; i++)
        {
            EmitParticle();
        }
    }
}
