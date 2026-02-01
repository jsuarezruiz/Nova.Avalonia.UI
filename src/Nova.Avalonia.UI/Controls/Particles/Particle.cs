using Avalonia.Media;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Defines the shape of a particle.
/// </summary>
public enum ParticleShape
{
    Circle,
    Square,
    Rectangle,
    Triangle,
    Star,
    Line
}

/// <summary>
/// Represents an individual particle.
/// </summary>
public class Particle
{
    public double X { get; set; }

    public double Y { get; set; }

    public double VelocityX { get; set; }

    public double VelocityY { get; set; }

    public double Scale { get; set; } = 1.0;

    public double Rotation { get; set; }

    public double Opacity { get; set; } = 1.0;

    public Color Color { get; set; } = Colors.White;

    public int Frame { get; set; }

    public double LifeTime { get; set; }

    public object? Tag { get; set; }

    public ParticleShape Shape { get; set; } = ParticleShape.Circle;

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Updates position based on velocity.
    /// </summary>
    public void UpdatePosition(double deltaTime = 1.0)
    {
        X += VelocityX * deltaTime;
        Y += VelocityY * deltaTime;
    }

    /// <summary>
    /// Applies a force to the particle.
    /// </summary>
    public void ApplyForce(double forceX, double forceY)
    {
        VelocityX += forceX;
        VelocityY += forceY;
    }

    /// <summary>
    /// Resets the particle for reuse.
    /// </summary>
    public void Reset()
    {
        X = 0;
        Y = 0;
        VelocityX = 0;
        VelocityY = 0;
        Scale = 1.0;
        Rotation = 0;
        Opacity = 1.0;
        Color = Colors.White;
        Frame = 0;
        LifeTime = 0;
        Tag = null;
        Shape = ParticleShape.Circle;
        IsActive = true;
    }
}
