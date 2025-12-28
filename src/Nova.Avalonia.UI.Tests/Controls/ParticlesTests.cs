using Avalonia;
using Avalonia.Headless.XUnit;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class ParticlesTests
{
    [AvaloniaFact]
    public void DefaultPropertyValues()
    {
        var particles = new Particles();

        Assert.Empty(particles.Items);
        Assert.Null(particles.Source);
        Assert.Equal(new Size(32, 32), particles.FrameSize);
        Assert.Equal(1, particles.FrameColumns);
        Assert.True(particles.IsRunning);
        Assert.Equal(60.0, particles.TargetFrameRate);
        Assert.Equal(5000, particles.MaxItems);
    }

    [AvaloniaFact]
    public void Add_ReturnsParticle_WhenBelowMaxItems()
    {
        var particles = new Particles { MaxItems = 10 };

        var particle = particles.Add();

        Assert.NotNull(particle);
        Assert.Single(particles.Items);
    }

    [AvaloniaFact]
    public void Add_ReturnsNull_WhenAtMaxItems()
    {
        var particles = new Particles { MaxItems = 2 };

        particles.Add();
        particles.Add();
        var third = particles.Add();

        Assert.Null(third);
        Assert.Equal(2, particles.Items.Count);
    }

    [AvaloniaFact]
    public void Remove_ReturnsTrue_WhenParticleExists()
    {
        var particles = new Particles();
        var particle = particles.Add()!;

        var result = particles.Remove(particle);

        Assert.True(result);
        Assert.Empty(particles.Items);
    }

    [AvaloniaFact]
    public void Remove_ReturnsFalse_WhenParticleDoesNotExist()
    {
        var particles = new Particles();
        var particle = new Particle();

        var result = particles.Remove(particle);

        Assert.False(result);
    }

    [AvaloniaFact]
    public void Clear_RemovesAllParticles()
    {
        var particles = new Particles();
        particles.Add();
        particles.Add();
        particles.Add();

        particles.Clear();

        Assert.Empty(particles.Items);
    }

    [AvaloniaFact]
    public void FrameColumns_ClampedToMinimum1()
    {
        var particles = new Particles();

        particles.FrameColumns = 0;

        Assert.Equal(1, particles.FrameColumns);
    }

    [AvaloniaFact]
    public void FrameColumns_ClampedToMinimum1_NegativeValue()
    {
        var particles = new Particles();

        particles.FrameColumns = -5;

        Assert.Equal(1, particles.FrameColumns);
    }

    [AvaloniaFact]
    public void TargetFrameRate_ClampedToRange()
    {
        var particles = new Particles();

        particles.TargetFrameRate = 0;
        Assert.Equal(1, particles.TargetFrameRate);

        particles.TargetFrameRate = 300;
        Assert.Equal(240, particles.TargetFrameRate);
    }

    [AvaloniaFact]
    public void MaxItems_ClampedToMinimum1()
    {
        var particles = new Particles();

        particles.MaxItems = 0;

        Assert.Equal(1, particles.MaxItems);
    }

    [AvaloniaFact]
    public void Particle_UpdatePosition_AppliesVelocity()
    {
        var particle = new Particle
        {
            X = 10,
            Y = 20,
            VelocityX = 5,
            VelocityY = 10
        };

        particle.UpdatePosition(1.0);

        Assert.Equal(15, particle.X);
        Assert.Equal(30, particle.Y);
    }

    [AvaloniaFact]
    public void Particle_UpdatePosition_UsesDeltaTime()
    {
        var particle = new Particle
        {
            X = 0,
            Y = 0,
            VelocityX = 100,
            VelocityY = 100
        };

        particle.UpdatePosition(0.5);

        Assert.Equal(50, particle.X);
        Assert.Equal(50, particle.Y);
    }

    [AvaloniaFact]
    public void Particle_ApplyForce_ModifiesVelocity()
    {
        var particle = new Particle
        {
            VelocityX = 10,
            VelocityY = 20
        };

        particle.ApplyForce(5, -10);

        Assert.Equal(15, particle.VelocityX);
        Assert.Equal(10, particle.VelocityY);
    }

    [AvaloniaFact]
    public void Particle_Reset_ClearsAllProperties()
    {
        var particle = new Particle
        {
            X = 100,
            Y = 200,
            VelocityX = 50,
            VelocityY = 60,
            Scale = 2.0,
            Rotation = 45,
            Opacity = 0.5,
            Frame = 5,
            LifeTime = 10,
            Tag = "test"
        };

        particle.Reset();

        Assert.Equal(0, particle.X);
        Assert.Equal(0, particle.Y);
        Assert.Equal(0, particle.VelocityX);
        Assert.Equal(0, particle.VelocityY);
        Assert.Equal(1.0, particle.Scale);
        Assert.Equal(0, particle.Rotation);
        Assert.Equal(1.0, particle.Opacity);
        Assert.Equal(0, particle.Frame);
        Assert.Equal(0, particle.LifeTime);
        Assert.Null(particle.Tag);
    }

    [AvaloniaFact]
    public void ParticlePool_Rent_ReturnsNewParticle_WhenPoolEmpty()
    {
        var pool = new ParticlePool();

        var particle = pool.Rent();

        Assert.NotNull(particle);
    }

    [AvaloniaFact]
    public void ParticlePool_Return_ReusesParticle()
    {
        var pool = new ParticlePool();
        var particle = pool.Rent();
        particle.X = 100;
        pool.Return(particle);

        var rented = pool.Rent();

        Assert.Same(particle, rented);
        Assert.Equal(0, rented.X); // Should be reset
    }

    [AvaloniaFact]
    public void GravityAffector_Apply_ModifiesVelocity()
    {
        var affector = new GravityAffector { GravityY = 100 };
        var particle = new Particle();

        affector.Apply(particle, 1.0);

        Assert.Equal(100, particle.VelocityY);
    }

    [AvaloniaFact]
    public void FadeAffector_Apply_ReducesOpacity()
    {
        var affector = new FadeAffector { FadeRate = 0.5 };
        var particle = new Particle { Opacity = 1.0 };

        affector.Apply(particle, 1.0);

        Assert.Equal(0.5, particle.Opacity);
    }

    [AvaloniaFact]
    public void FadeAffector_Apply_ClampsToZero()
    {
        var affector = new FadeAffector { FadeRate = 2.0 };
        var particle = new Particle { Opacity = 1.0 };

        affector.Apply(particle, 1.0);

        Assert.Equal(0, particle.Opacity);
    }

    [AvaloniaFact]
    public void RotationAffector_Apply_RotatesParticle()
    {
        var affector = new RotationAffector { RotationSpeed = 90 };
        var particle = new Particle();

        affector.Apply(particle, 1.0);

        Assert.Equal(90, particle.Rotation);
    }
}
