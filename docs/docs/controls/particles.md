---
title: Particles
description: High-performance particle system control for creating visual effects like rain, fire, snow, and more.
ms.date: 2025-12-28
---

# Particles

The `Particles` control is a high-performance particle system for creating visual effects. It features object pooling, GPU-accelerated rendering, and a flexible event-driven update model.

## Basic usage

Add a `Particles` control and handle the `Update` event to define particle behavior.

```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:nova="clr-namespace:Nova.Avalonia.UI.Controls;assembly=Nova.Avalonia.UI">

    <nova:Particles x:Name="MyParticles"
                    Height="300"
                    Background="#1a1a2e"
                    MaxItems="500"
                    TargetFrameRate="60"
                    Update="OnParticleUpdate" />
</UserControl>
```

```csharp
private void OnParticleUpdate(object? sender, ParticleUpdateEventArgs e)
{
    var particles = sender as Particles;
    
    // Spawn new particles
    if (e.Items.Count < 100)
    {
        var p = particles!.Add();
        if (p != null)
        {
            p.X = random.NextDouble() * e.CanvasSize.Width;
            p.Y = 0;
            p.VelocityY = 100;
            p.Color = Colors.White;
        }
    }
    
    // Update existing particles
    foreach (var p in e.Items)
    {
        p.UpdatePosition(e.DeltaTime);
    }
}
```

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Items` | `ObservableCollection<Particle>` | Empty | Read-only collection of active particles |
| `Source` | `IImage?` | `null` | Sprite sheet image for particles |
| `FrameSize` | `Size` | `32×32` | Size of each frame in the sprite sheet |
| `FrameColumns` | `int` | `1` | Number of columns in the sprite sheet |
| `Origin` | `RelativePoint` | `TopLeft` | Origin point for particle positioning |
| `IsRunning` | `bool` | `true` | Whether the particle system is running |
| `TargetFrameRate` | `double` | `60` | Target update rate (1-240 fps) |
| `MaxItems` | `int` | `5000` | Maximum number of particles allowed |

## Particle properties

Each `Particle` has the following properties:

| Property | Description |
|----------|-------------|
| `X`, `Y` | Position relative to Origin |
| `VelocityX`, `VelocityY` | Movement speed |
| `Scale` | Size multiplier (default: 1.0) |
| `Rotation` | Rotation in degrees |
| `Opacity` | Transparency (0.0 to 1.0) |
| `Color` | Tint color |
| `Shape` | `Circle`, `Square`, `Rectangle`, `Triangle`, `Star`, or `Line` |
| `LifeTime` | Time since creation (seconds) |
| `Tag` | Custom user data |

## Built-in affectors

Affectors modify particle properties over time. Apply them in your update handler:

```csharp
var gravity = new GravityAffector { GravityY = 100 };
var fade = new FadeAffector { FadeRate = 0.5 };
var rotation = new RotationAffector { RotationSpeed = 90 };

foreach (var particle in e.Items)
{
    gravity.Apply(particle, e.DeltaTime);
    fade.Apply(particle, e.DeltaTime);
    rotation.Apply(particle, e.DeltaTime);
    particle.UpdatePosition(e.DeltaTime);
}
```

Available affectors:
- **GravityAffector**: Applies gravity force
- **FadeAffector**: Reduces opacity over time
- **RotationAffector**: Rotates particles
- **ScaleAffector**: Changes size over time
- **DragAffector**: Applies friction
- **WindAffector**: Applies wind force with optional turbulence

## Using ParticleEmitter

The `ParticleEmitter` helper simplifies particle spawning:

```csharp
var emitter = new ParticleEmitter(particles)
{
    Position = new Point(100, 100),
    EmissionRate = 20,
    SpreadAngle = 45,
    MinSpeed = 50,
    MaxSpeed = 100,
    MinScale = 0.5,
    MaxScale = 1.5,
    Color = Colors.Orange
};

// In update handler
emitter.Update(e.DeltaTime);
```

## Interactive example

React to mouse/touch events:

```csharp
private void OnPointerMoved(object? sender, PointerEventArgs e)
{
    var particles = sender as Particles;
    var point = e.GetPosition(particles);
    
    var p = particles!.Add();
    if (p != null)
    {
        p.X = point.X;
        p.Y = point.Y;
        p.Scale = 0.5;
        p.Color = Colors.Gold;
    }
}
```

## Effect examples

The gallery includes 9 sample effects:

| Effect | Description |
|--------|-------------|
| Rain | Falling particles with streaks |
| Fire | Rising flames with color variation |
| Fireworks | Click-triggered bursts |
| Confetti | Mixed shapes celebration |
| Sparkle Trail | Mouse-following sparkles |
| Snow | Gentle snowfall with sway |
| Bubbles | Rising bubbles with wobble |
| Smoke | Spreading, fading clouds |
| Galaxy | Orbiting spiral stars |
