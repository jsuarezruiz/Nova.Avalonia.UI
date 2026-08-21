using System.Threading;
using System.Threading.Tasks;
using Nova.Avalonia.UI.Controls;

namespace Nova.Avalonia.UI.Tests.Controls;

internal sealed class DelayedShowcaseProgressStore : IShowcaseProgressStore
{
    private readonly TaskCompletionSource _saveStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _releaseSave = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private ShowcaseProgressState? _state;

    public bool DelayNextSave { get; set; }

    public Task SaveStarted => _saveStarted.Task;

    public void ReleaseSave() => _releaseSave.TrySetResult();

    public Task<ShowcaseProgressState?> LoadAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_state);
    }

    public async Task SaveAsync(
        string key,
        ShowcaseProgressState state,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _state = state;

        if (DelayNextSave)
        {
            DelayNextSave = false;
            _saveStarted.TrySetResult();
            await _releaseSave.Task;
        }
    }

    public Task ClearAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _state = null;
        return Task.CompletedTask;
    }
}
