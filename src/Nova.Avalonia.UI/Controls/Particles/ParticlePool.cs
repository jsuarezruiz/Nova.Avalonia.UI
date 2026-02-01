using System.Collections.Generic;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Object pool for efficient particle reuse.
/// </summary>
public class ParticlePool
{
    private readonly Stack<Particle> _pool = new();
    private readonly int _maxPoolSize;

    public ParticlePool(int maxPoolSize = 10000)
    {
        _maxPoolSize = maxPoolSize;
    }

    /// <summary>
    /// Gets the current number of particles in the pool.
    /// </summary>
    public int Count => _pool.Count;

    /// <summary>
    /// Rents a particle from the pool or creates a new one.
    /// </summary>
    public Particle Rent()
    {
        if (_pool.Count > 0)
        {
            return _pool.Pop();
        }

        return new Particle();
    }

    /// <summary>
    /// Returns a particle to the pool for reuse.
    /// </summary>
    public void Return(Particle particle)
    {
        if (_pool.Count < _maxPoolSize)
        {
            particle.Reset();
            _pool.Push(particle);
        }
    }

    /// <summary>
    /// Clears all particles from the pool.
    /// </summary>
    public void Clear() => _pool.Clear();

    /// <summary>
    /// Pre-warms the pool with the specified number of particles.
    /// </summary>
    public void PreWarm(int count)
    {
        for (int i = 0; i < count && _pool.Count < _maxPoolSize; i++)
        {
            _pool.Push(new Particle());
        }
    }
}
