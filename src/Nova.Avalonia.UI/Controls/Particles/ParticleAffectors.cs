using System;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Base class for particle affectors that modify particle properties over time.
/// </summary>
public abstract class ParticleAffector
{
    /// <summary>
    /// Applies the affector's effect to a particle.
    /// </summary>
    /// <param name="particle">The particle to affect.</param>
    /// <param name="deltaTime">Time elapsed since last update in seconds.</param>
    public abstract void Apply(Particle particle, double deltaTime);
}

/// <summary>
/// Applies gravity force to particles.
/// </summary>
public class GravityAffector : ParticleAffector
{
    /// <summary>
    /// Gets or sets the gravity force on the X axis.
    /// </summary>
    public double GravityX { get; set; }

    /// <summary>
    /// Gets or sets the gravity force on the Y axis. Default is 100 (downward).
    /// </summary>
    public double GravityY { get; set; } = 100;

    /// <inheritdoc/>
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
    /// <summary>
    /// Gets or sets the fade rate in opacity units per second.
    /// </summary>
    public double FadeRate { get; set; } = 0.5;

    /// <inheritdoc/>
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
    /// <summary>
    /// Gets or sets the rotation speed in degrees per second.
    /// </summary>
    public double RotationSpeed { get; set; } = 90;

    /// <inheritdoc/>
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
    /// <summary>
    /// Gets or sets the scale change rate per second.
    /// </summary>
    public double ScaleRate { get; set; } = -0.5;

    /// <summary>
    /// Gets or sets the minimum scale value.
    /// </summary>
    public double MinScale { get; set; } = 0;

    /// <summary>
    /// Gets or sets the maximum scale value.
    /// </summary>
    public double MaxScale { get; set; } = 10;

    /// <inheritdoc/>
    public override void Apply(Particle particle, double deltaTime)
    {
        particle.Scale = Math.Clamp(
            particle.Scale + ScaleRate * deltaTime,
            MinScale,
            MaxScale);
    }
}

/// <summary>
/// Applies drag/friction to particles, slowing them down.
/// </summary>
public class DragAffector : ParticleAffector
{
    /// <summary>
    /// Gets or sets the drag coefficient (0-1). Higher values = more drag.
    /// </summary>
    public double Drag { get; set; } = 0.1;

    /// <inheritdoc/>
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
    /// <summary>
    /// Gets or sets the wind force on the X axis.
    /// </summary>
    public double WindX { get; set; } = 20;

    /// <summary>
    /// Gets or sets the wind force on the Y axis.
    /// </summary>
    public double WindY { get; set; }

    /// <summary>
    /// Gets or sets whether wind should oscillate (turbulence).
    /// </summary>
    public bool Turbulent { get; set; }

    /// <summary>
    /// Gets or sets the turbulence frequency.
    /// </summary>
    public double TurbulenceFrequency { get; set; } = 2;

    private double _time;

    /// <inheritdoc/>
    public override void Apply(Particle particle, double deltaTime)
    {
        _time += deltaTime;
        
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
