using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Identifies a showcase navigation operation.
/// </summary>
public enum ShowcaseNavigationAction
{
    /// <summary>
    /// Start the showcase from the beginning.
    /// </summary>
    Start,

    /// <summary>
    /// Resume the showcase from persisted progress.
    /// </summary>
    Resume,

    /// <summary>
    /// Advance to the next step.
    /// </summary>
    Next,

    /// <summary>
    /// Go back to the previous step.
    /// </summary>
    Previous,

    /// <summary>
    /// Skip the showcase.
    /// </summary>
    Skip,

    /// <summary>
    /// Reset the showcase to its initial state.
    /// </summary>
    Reset
}

/// <summary>
/// Identifies why a showcase step transition occurred.
/// </summary>
public enum ShowcaseStepTransitionReason
{
    /// <summary>
    /// The showcase started from the beginning.
    /// </summary>
    Start,

    /// <summary>
    /// The showcase resumed from persisted progress.
    /// </summary>
    Resume,

    /// <summary>
    /// The showcase advanced to the next step.
    /// </summary>
    Next,

    /// <summary>
    /// The showcase moved back to the previous step.
    /// </summary>
    Previous
}

/// <summary>
/// Provides context for async showcase step hooks.
/// </summary>
public sealed class ShowcaseStepTransitionContext
{
    /// <summary>
    /// Creates a new transition context.
    /// </summary>
    public ShowcaseStepTransitionContext(
        ShowcaseController controller,
        ShowcaseStep? previousStep,
        ShowcaseStep nextStep,
        int? previousIndex,
        int nextIndex,
        ShowcaseStepTransitionReason reason)
    {
        Controller = controller;
        PreviousStep = previousStep;
        NextStep = nextStep;
        PreviousIndex = previousIndex;
        NextIndex = nextIndex;
        Reason = reason;
    }

    /// <summary>
    /// Gets the controller performing the transition.
    /// </summary>
    public ShowcaseController Controller { get; }

    /// <summary>
    /// Gets the previous step, if any.
    /// </summary>
    public ShowcaseStep? PreviousStep { get; }

    /// <summary>
    /// Gets the next step that will become active.
    /// </summary>
    public ShowcaseStep NextStep { get; }

    /// <summary>
    /// Gets the previous step index, if any.
    /// </summary>
    public int? PreviousIndex { get; }

    /// <summary>
    /// Gets the index of the next active step.
    /// </summary>
    public int NextIndex { get; }

    /// <summary>
    /// Gets the reason for the transition.
    /// </summary>
    public ShowcaseStepTransitionReason Reason { get; }
}

/// <summary>
/// Represents persisted showcase progress.
/// </summary>
public sealed class ShowcaseProgressState
{
    /// <summary>
    /// Creates a new progress snapshot.
    /// </summary>
    public ShowcaseProgressState(int currentIndex, bool isActive)
    {
        CurrentIndex = currentIndex;
        IsActive = isActive;
    }

    /// <summary>
    /// Gets the active step index.
    /// </summary>
    public int CurrentIndex { get; }

    /// <summary>
    /// Gets whether the showcase was active when persisted.
    /// </summary>
    public bool IsActive { get; }
}

/// <summary>
/// Stores and restores showcase progress.
/// </summary>
public interface IShowcaseProgressStore
{
    /// <summary>
    /// Loads persisted progress for a showcase key.
    /// </summary>
    Task<ShowcaseProgressState?> LoadAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves persisted progress for a showcase key.
    /// </summary>
    Task SaveAsync(string key, ShowcaseProgressState state, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears persisted progress for a showcase key.
    /// </summary>
    Task ClearAsync(string key, CancellationToken cancellationToken = default);
}

/// <summary>
/// In-memory showcase progress store for simple persistence and tests.
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

/// <summary>
/// Event args for asynchronous showcase transition failures.
/// </summary>
public sealed class ShowcaseTransitionFailedEventArgs : EventArgs
{
    /// <summary>
    /// Creates new transition failure args.
    /// </summary>
    public ShowcaseTransitionFailedEventArgs(ShowcaseNavigationAction action, Exception exception)
    {
        Action = action;
        Exception = exception;
    }

    /// <summary>
    /// Gets the navigation action that failed.
    /// </summary>
    public ShowcaseNavigationAction Action { get; }

    /// <summary>
    /// Gets the underlying exception.
    /// </summary>
    public Exception Exception { get; }
}
