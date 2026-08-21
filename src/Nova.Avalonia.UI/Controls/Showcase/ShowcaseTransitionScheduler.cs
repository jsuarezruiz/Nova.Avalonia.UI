using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nova.Avalonia.UI.Controls;

internal sealed class ShowcaseTransitionScheduler
{
    private readonly SemaphoreSlim _transitionLock = new(1, 1);
    private readonly object _transitionSync = new();
    private CancellationTokenSource? _transitionCts;

    public async Task RunAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken)
    {
        await RunAsync(
            async token =>
            {
                await operation(token);
                return true;
            },
            cancellationToken);
    }

    public async Task<T> RunAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken)
    {
        CancellationTokenSource transitionCts;
        CancellationTokenSource? previousTransition;

        lock (_transitionSync)
        {
            previousTransition = _transitionCts;
            transitionCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _transitionCts = transitionCts;
        }

        // Cancellation callbacks run synchronously, so never invoke them while
        // holding the transition state lock.
        try
        {
            previousTransition?.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // The previous transition completed between the swap and cancellation.
        }

        try
        {
            await _transitionLock.WaitAsync(transitionCts.Token);
            try
            {
                if (!ReferenceEquals(_transitionCts, transitionCts))
                {
                    return default!;
                }

                return await operation(transitionCts.Token);
            }
            finally
            {
                _transitionLock.Release();
            }
        }
        finally
        {
            lock (_transitionSync)
            {
                if (ReferenceEquals(_transitionCts, transitionCts))
                {
                    _transitionCts = null;
                }
            }

            transitionCts.Dispose();
        }
    }

    public void Cancel()
    {
        CancellationTokenSource? transition;
        lock (_transitionSync)
        {
            transition = _transitionCts;
        }

        try
        {
            transition?.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // The transition completed between capture and cancellation.
        }
    }
}
