using System.Reflection;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media.Imaging;
using AvaloniaAlphaFormat = Avalonia.Platform.AlphaFormat;
using AvaloniaPixelFormats = Avalonia.Platform.PixelFormats;
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
    public void TargetFrameRate_InvalidValue_UsesDefault()
    {
        var particles = new Particles();

        particles.TargetFrameRate = double.NaN;

        Assert.Equal(60.0, particles.TargetFrameRate);
    }

    [AvaloniaFact]
    public void FrameSize_ClampedToPositiveFiniteValues()
    {
        var particles = new Particles();

        particles.FrameSize = new Size(0, double.NaN);

        Assert.Equal(new Size(1, 32), particles.FrameSize);
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

    [AvaloniaFact]
    public void Update_Event_RunsBeforeBuiltInParticleAdvance()
    {
        var particles = new Particles();
        var particle = particles.Add()!;
        particle.VelocityX = 10;
        double? xDuringUpdate = null;

        particles.Update += (_, _) => xDuringUpdate = particle.X;

        Advance(particles, 0.5);

        Assert.Equal(0, xDuringUpdate);
        Assert.Equal(5, particle.X);
        Assert.Equal(0.5, particle.LifeTime);
    }

    [AvaloniaFact]
    public void Update_CanSpawnParticle_AndBuiltInAdvanceRunsOnce()
    {
        var particles = new Particles();
        Particle? spawned = null;

        particles.Update += (_, _) =>
        {
            spawned = particles.Add();
            spawned!.VelocityY = 12;
        };

        Advance(particles, 1.0);

        Assert.NotNull(spawned);
        Assert.Equal(12, spawned!.Y);
        Assert.Equal(1, spawned.LifeTime);
    }

    [AvaloniaFact]
    public void Advance_IgnoresInvalidDeltaTime()
    {
        var particles = new Particles();
        var particle = particles.Add()!;
        particle.VelocityX = 10;

        Advance(particles, double.NaN);

        Assert.Equal(0, particle.X);
        Assert.Equal(0, particle.LifeTime);
    }

    [AvaloniaFact]
    public void ControlAffectors_AreAppliedDuringBuiltInAdvance()
    {
        var particles = new Particles();
        particles.Affectors.Add(new GravityAffector { GravityX = 10, GravityY = 0 });
        var particle = particles.Add()!;

        Advance(particles, 1.0);

        Assert.Equal(10, particle.VelocityX);
        Assert.Equal(10, particle.X);
    }

    [AvaloniaFact]
    public void ParticleSpriteSheet_GetFrameRect_ClampsNegativeFrame()
    {
        var spriteSheet = new ParticleSpriteSheet
        {
            FrameWidth = 32,
            FrameHeight = 16,
            Columns = 4
        };

        var frame = spriteSheet.GetFrameRect(-2);

        Assert.Equal(new Rect(0, 0, 32, 16), frame);
    }

    [AvaloniaFact]
    public void ParticleSpriteSheet_GetFrameRect_ReturnsEmpty_WhenFrameSizeInvalid()
    {
        var spriteSheet = new ParticleSpriteSheet
        {
            FrameWidth = 0,
            FrameHeight = 16
        };

        Assert.Equal(default, spriteSheet.GetFrameRect(0));
    }

    [AvaloniaFact]
    public void ParticleSpriteSheet_GetFrameRect_UsesImageColumnLimit()
    {
        using var image = new WriteableBitmap(
            new PixelSize(64, 64),
            new Vector(96, 96),
            AvaloniaPixelFormats.Rgba8888,
            AvaloniaAlphaFormat.Premul);
        var spriteSheet = new ParticleSpriteSheet
        {
            Image = image,
            FrameWidth = 32,
            FrameHeight = 32,
            Columns = 4
        };

        Assert.Equal(4, spriteSheet.FrameCount);
        Assert.Equal(new Rect(0, 32, 32, 32), spriteSheet.GetFrameRect(2));
    }

    [AvaloniaFact]
    public void ParticleSpriteSheet_GetFrameRect_ReturnsEmpty_WhenImageHasNoFullFrame()
    {
        using var image = new WriteableBitmap(
            new PixelSize(16, 16),
            new Vector(96, 96),
            AvaloniaPixelFormats.Rgba8888,
            AvaloniaAlphaFormat.Premul);
        var spriteSheet = new ParticleSpriteSheet
        {
            Image = image,
            FrameWidth = 32,
            FrameHeight = 32,
            Columns = 1
        };

        Assert.Equal(0, spriteSheet.FrameCount);
        Assert.Equal(default, spriteSheet.GetFrameRect(0));
    }

    [AvaloniaFact]
    public void ParticleEmitter_Update_IgnoresInvalidDeltaTimeAndEmissionRate()
    {
        var particles = new Particles();
        var emitter = new ParticleEmitter(particles);

        emitter.Update(double.NaN);
        emitter.EmissionRate = double.PositiveInfinity;
        emitter.Update(1);

        Assert.Empty(particles.Items);
    }

    [AvaloniaFact]
    public void ParticleEmitter_Update_DiscardsBacklogWhileParticlesControlIsFull()
    {
        var particles = new Particles { MaxItems = 3 };
        var emitter = new ParticleEmitter(particles) { EmissionRate = 10 };

        emitter.Burst(3);
        emitter.Update(10);
        particles.Clear();
        emitter.Update(0.01);

        Assert.Empty(particles.Items);
    }

    [AvaloniaFact]
    public void ParticleEmitter_Update_StopsWhenParticlesControlIsFull()
    {
        var particles = new Particles { MaxItems = 1 };
        var emitter = new ParticleEmitter(particles) { EmissionRate = 1000 };

        emitter.Update(1);

        Assert.Single(particles.Items);
    }

    [AvaloniaFact]
    public void ParticleEmitter_Burst_StopsWhenParticlesControlIsFull()
    {
        var particles = new Particles { MaxItems = 1 };
        var emitter = new ParticleEmitter(particles);

        emitter.Burst(100);

        Assert.Single(particles.Items);
    }

    private static void Advance(Particles particles, double deltaTime)
    {
        var advance = typeof(Particles).GetMethod("Advance", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(advance);
        advance.Invoke(particles, new object[] { deltaTime });
    }
}
