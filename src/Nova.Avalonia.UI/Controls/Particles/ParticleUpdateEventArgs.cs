using System;
using System.Collections.ObjectModel;
using Avalonia;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Event arguments for particle updates.
/// </summary>
public class ParticleUpdateEventArgs : EventArgs
{
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
    /// Gets the total elapsed time.
    /// </summary>
    public TimeSpan Elapsed { get; private set; }

    /// <summary>
    /// Gets the elapsed time since last update.
    /// </summary>
    public double DeltaTime { get; private set; }

    /// <summary>
    /// Gets the size of the rendering canvas.
    /// </summary>
    public Size CanvasSize { get; private set; }

    internal void Update(double deltaTime, TimeSpan elapsed, Size canvasSize)
    {
        DeltaTime = deltaTime;
        Elapsed = elapsed;
        CanvasSize = canvasSize;
    }
}
