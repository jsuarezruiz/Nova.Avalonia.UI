using System;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Base class for particle affectors.
/// </summary>
public abstract class ParticleAffector
{
    /// <summary>
    /// Updates the affector's internal state.
    /// </summary>
    public virtual void Update(double deltaTime) { }

    /// <summary>
    /// Applies the effect to a particle.
    /// </summary>
    public abstract void Apply(Particle particle, double deltaTime);
}

/// <summary>
/// Applies gravity to particles.
/// </summary>
public class GravityAffector : ParticleAffector
{
    public double GravityX { get; set; }

    public double GravityY { get; set; } = 100;

    public override void Apply(Particle particle, double deltaTime)
    {
        particle.VelocityX += GravityX * deltaTime;
        particle.VelocityY += GravityY * deltaTime;
    }
}

/// <summary>
/// Fades particle opacity over time.
/// </summary>
public class FadeAffector : ParticleAffector
{
    public double FadeRate { get; set; } = 0.5;

    public override void Apply(Particle particle, double deltaTime)
    {
        particle.Opacity = Math.Max(0, particle.Opacity - FadeRate * deltaTime);
    }
}

/// <summary>
/// Rotates particles over time.
/// </summary>
public class RotationAffector : ParticleAffector
{
    public double RotationSpeed { get; set; } = 90;

    public override void Apply(Particle particle, double deltaTime)
    {
        particle.Rotation += RotationSpeed * deltaTime;
    }
}

/// <summary>
/// Scales particles over time.
/// </summary>
public class ScaleAffector : ParticleAffector
{
    public double ScaleRate { get; set; } = -0.5;

    public double MinScale { get; set; } = 0;

    public double MaxScale { get; set; } = 10;

    public override void Apply(Particle particle, double deltaTime)
    {
        particle.Scale = Math.Clamp(
            particle.Scale + ScaleRate * deltaTime,
            MinScale,
            MaxScale);
    }
}

/// <summary>
/// Applies drag to particles.
/// </summary>
public class DragAffector : ParticleAffector
{
    public double Drag { get; set; } = 0.1;

    public override void Apply(Particle particle, double deltaTime)
    {
        var factor = Math.Max(0, 1 - Drag * deltaTime);
        particle.VelocityX *= factor;
        particle.VelocityY *= factor;
    }
}

/// <summary>
/// Applies wind force to particles.
/// </summary>
public class WindAffector : ParticleAffector
{
    public double WindX { get; set; } = 20;

    public double WindY { get; set; }

    public bool Turbulent { get; set; }

    public double TurbulenceFrequency { get; set; } = 2;

    private double _time;

    public override void Update(double deltaTime)
    {
        _time += deltaTime;
    }

    public override void Apply(Particle particle, double deltaTime)
    {
        var windX = WindX;
        var windY = WindY;

        if (Turbulent)
        {
            var turbulence = Math.Sin(_time * TurbulenceFrequency * Math.PI * 2);
            windX *= (1 + turbulence * 0.5);
        }

        particle.VelocityX += windX * deltaTime;
        particle.VelocityY += windY * deltaTime;
    }
}

/// <summary>
/// Kills particles after a set amount of time.
/// </summary>
public class LifetimeAffector : ParticleAffector
{
    public double MaxLifeTime { get; set; } = 5.0;

    public override void Apply(Particle particle, double deltaTime)
    {
        if (particle.LifeTime >= MaxLifeTime)
        {
            particle.Opacity = 0;
            particle.IsActive = false;
        }
    }
}
