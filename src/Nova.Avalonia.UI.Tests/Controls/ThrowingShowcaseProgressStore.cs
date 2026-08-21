using System;
using System.Threading;
using System.Threading.Tasks;
using Nova.Avalonia.UI.Controls;

namespace Nova.Avalonia.UI.Tests.Controls;

internal sealed class ThrowingShowcaseProgressStore : IShowcaseProgressStore
{
    private ShowcaseProgressState? _state;

    public bool ThrowOnSave { get; set; }

    public bool ThrowOnClear { get; set; }

    public Task<ShowcaseProgressState?> LoadAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_state);
    }

    public Task SaveAsync(
        string key,
        ShowcaseProgressState state,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (ThrowOnSave)
        {
            throw new InvalidOperationException("Save failed.");
        }

        _state = state;
        return Task.CompletedTask;
    }

    public Task ClearAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (ThrowOnClear)
        {
            throw new InvalidOperationException("Clear failed.");
        }

        _state = null;
        return Task.CompletedTask;
    }
}
