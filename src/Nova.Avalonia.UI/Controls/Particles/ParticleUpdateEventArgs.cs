using System;
using System.Collections.ObjectModel;
using Avalonia;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Event arguments for the particle update callback.
/// </summary>
public class ParticleUpdateEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ParticleUpdateEventArgs"/> class.
    /// </summary>
    /// <param name="items">The collection of particles to update.</param>
    /// <param name="elapsed">Total elapsed time since the particle system started.</param>
    /// <param name="deltaTime">Time elapsed since the last update in seconds.</param>
    /// <param name="canvasSize">The size of the rendering canvas.</param>
    public ParticleUpdateEventArgs(
        ObservableCollection<Particle> items,
        TimeSpan elapsed,
        double deltaTime,
        Size canvasSize)
    {
        Items = items;
        Elapsed = elapsed;
        DeltaTime = deltaTime;
        CanvasSize = canvasSize;
    }

    /// <summary>
    /// Gets the collection of particles.
    /// </summary>
    public ObservableCollection<Particle> Items { get; }

    /// <summary>
    /// Gets the total elapsed time since the particle system started.
    /// </summary>
    public TimeSpan Elapsed { get; }

    /// <summary>
    /// Gets the time elapsed since the last update in seconds.
    /// </summary>
    public double DeltaTime { get; }

    /// <summary>
    /// Gets the size of the rendering canvas.
    /// </summary>
    public Size CanvasSize { get; }
}
