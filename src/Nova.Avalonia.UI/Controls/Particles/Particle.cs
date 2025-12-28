using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Defines the shape of a particle when rendered without a sprite.
/// </summary>
public enum ParticleShape
{
    /// <summary>Circle/ellipse shape.</summary>
    Circle,
    /// <summary>Square shape.</summary>
    Square,
    /// <summary>Rectangle shape (wider than tall).</summary>
    Rectangle,
    /// <summary>Triangle shape pointing up.</summary>
    Triangle,
    /// <summary>Four-pointed star shape.</summary>
    Star,
    /// <summary>Vertical line/streak shape.</summary>
    Line
}

/// <summary>
/// Represents an individual particle with position, velocity, and visual properties.
/// </summary>
public class Particle : INotifyPropertyChanged
{
    private double _x;
    private double _y;
    private double _velocityX;
    private double _velocityY;
    private double _scale = 1.0;
    private double _rotation;
    private double _opacity = 1.0;
    private Color _color = Colors.White;
    private int _frame;
    private double _lifeTime;
    private object? _tag;
    private ParticleShape _shape = ParticleShape.Circle;

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the X position of the particle.
    /// </summary>
    public double X
    {
        get => _x;
        set => SetField(ref _x, value);
    }

    /// <summary>
    /// Gets or sets the Y position of the particle.
    /// </summary>
    public double Y
    {
        get => _y;
        set => SetField(ref _y, value);
    }

    /// <summary>
    /// Gets or sets the X velocity of the particle.
    /// </summary>
    public double VelocityX
    {
        get => _velocityX;
        set => _velocityX = value;
    }

    /// <summary>
    /// Gets or sets the Y velocity of the particle.
    /// </summary>
    public double VelocityY
    {
        get => _velocityY;
        set => _velocityY = value;
    }

    /// <summary>
    /// Gets or sets the scale of the particle. Default is 1.0.
    /// </summary>
    public double Scale
    {
        get => _scale;
        set => _scale = value;
    }

    /// <summary>
    /// Gets or sets the rotation of the particle in degrees.
    /// </summary>
    public double Rotation
    {
        get => _rotation;
        set => _rotation = value;
    }

    /// <summary>
    /// Gets or sets the opacity of the particle. Range: 0.0 to 1.0.
    /// </summary>
    public double Opacity
    {
        get => _opacity;
        set => _opacity = value;
    }

    /// <summary>
    /// Gets or sets the tint color of the particle.
    /// </summary>
    public Color Color
    {
        get => _color;
        set => _color = value;
    }

    /// <summary>
    /// Gets or sets the sprite frame index for animated particles.
    /// </summary>
    public int Frame
    {
        get => _frame;
        set => _frame = value;
    }

    /// <summary>
    /// Gets or sets the lifetime of the particle in seconds.
    /// </summary>
    public double LifeTime
    {
        get => _lifeTime;
        set => _lifeTime = value;
    }

    /// <summary>
    /// Gets or sets custom user data associated with the particle.
    /// </summary>
    public object? Tag
    {
        get => _tag;
        set => _tag = value;
    }

    /// <summary>
    /// Gets or sets the shape of the particle. Default is Circle.
    /// </summary>
    public ParticleShape Shape
    {
        get => _shape;
        set => _shape = value;
    }

    /// <summary>
    /// Updates the particle position based on velocity and delta time.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update in seconds.</param>
    public void UpdatePosition(double deltaTime = 1.0)
    {
        X += VelocityX * deltaTime;
        Y += VelocityY * deltaTime;
    }

    /// <summary>
    /// Applies a force to the particle, modifying its velocity.
    /// </summary>
    /// <param name="forceX">Force to apply on X axis.</param>
    /// <param name="forceY">Force to apply on Y axis.</param>
    public void ApplyForce(double forceX, double forceY)
    {
        VelocityX += forceX;
        VelocityY += forceY;
    }

    /// <summary>
    /// Resets the particle to its default state for reuse.
    /// </summary>
    public void Reset()
    {
        _x = 0;
        _y = 0;
        _velocityX = 0;
        _velocityY = 0;
        _scale = 1.0;
        _rotation = 0;
        _opacity = 1.0;
        _color = Colors.White;
        _frame = 0;
        _lifeTime = 0;
        _tag = null;
        _shape = ParticleShape.Circle;
    }

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
