using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Stores showcase progress in memory for simple scenarios and tests.
/// </summary>
public sealed class InMemoryShowcaseProgressStore : IShowcaseProgressStore
{
    private readonly ConcurrentDictionary<string, ShowcaseProgressState> _states = new();

    /// <inheritdoc />
    public Task<ShowcaseProgressState?> LoadAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _states.TryGetValue(key, out var state);
        return Task.FromResult(state);
    }

    /// <inheritdoc />
    public Task SaveAsync(string key, ShowcaseProgressState state, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _states[key] = state;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task ClearAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _states.TryRemove(key, out _);
        return Task.CompletedTask;
    }
}
