using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Event args for step change events.
/// </summary>
public class ShowcaseStepChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the previous step, or null if starting.
    /// </summary>
    public ShowcaseStep? PreviousStep { get; }

    /// <summary>
    /// Gets the current step.
    /// </summary>
    public ShowcaseStep CurrentStep { get; }

    /// <summary>
    /// Gets the index of the current step.
    /// </summary>
    public int CurrentIndex { get; }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public ShowcaseStepChangedEventArgs(ShowcaseStep? previous, ShowcaseStep current, int index)
    {
        PreviousStep = previous;
        CurrentStep = current;
        CurrentIndex = index;
    }
}

/// <summary>
/// Controls the flow of a showcase tutorial sequence.
/// </summary>
public class ShowcaseController : INotifyPropertyChanged, IDisposable
{
    private readonly SemaphoreSlim _transitionLock = new(1, 1);
    private readonly object _transitionSync = new();
    private CancellationTokenSource? _transitionCts;
    private bool _disposed;
    private int _currentIndex = -1;
    private bool _isActive;
    private string _nextButtonText = "Next";
    private string _finishButtonText = "Finish";
    private string _previousButtonText = "Previous";
    private string _skipButtonText = "Skip";
    private IShowcaseProgressStore? _progressStore;
    private string? _persistenceKey;

    /// <summary>
    /// Gets the collection of showcase steps.
    /// </summary>
    public ObservableCollection<ShowcaseStep> Steps { get; } = new();

    /// <summary>
    /// Gets the current step being displayed, or null if not active.
    /// </summary>
    public ShowcaseStep? CurrentStep => _currentIndex >= 0 && _currentIndex < Steps.Count
        ? Steps[_currentIndex]
        : null;

    /// <summary>
    /// Gets the index of the current step.
    /// </summary>
    public int CurrentIndex
    {
        get => _currentIndex;
        private set
        {
            if (_currentIndex != value)
            {
                var previous = CurrentStep;
                _currentIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentStep));
                OnPropertyChanged(nameof(CanGoPrevious));
                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(CurrentButtonText));

                _nextCommand.RaiseCanExecuteChanged();
                _previousCommand.RaiseCanExecuteChanged();
                _skipCommand.RaiseCanExecuteChanged();

                if (CurrentStep != null)
                {
                    StepChanged?.Invoke(this, new ShowcaseStepChangedEventArgs(previous, CurrentStep, value));
                }
            }
        }
    }

    /// <summary>
    /// Gets whether the previous step is available.
    /// </summary>
    public bool CanGoPrevious => _currentIndex > 0;

    /// <summary>
    /// Gets whether the next step is available.
    /// </summary>
    public bool CanGoNext => _currentIndex < Steps.Count - 1;

    /// <summary>
    /// Gets the text for the next/finish button based on step position.
    /// </summary>
    public string CurrentButtonText => _currentIndex >= Steps.Count - 1 ? _finishButtonText : _nextButtonText;

    /// <summary>
    /// Gets or sets the text for the Next button.
    /// </summary>
    public string NextButtonText
    {
        get => _nextButtonText;
        set
        {
            if (_nextButtonText != value)
            {
                _nextButtonText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentButtonText));
            }
        }
    }

    /// <summary>
    /// Gets or sets the text for the Finish button (shown on the last step).
    /// </summary>
    public string FinishButtonText
    {
        get => _finishButtonText;
        set
        {
            if (_finishButtonText != value)
            {
                _finishButtonText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentButtonText));
            }
        }
    }

    /// <summary>
    /// Gets or sets the text for the Previous button.
    /// </summary>
    public string PreviousButtonText
    {
        get => _previousButtonText;
        set
        {
            if (_previousButtonText != value)
            {
                _previousButtonText = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the text for the Skip button.
    /// </summary>
    public string SkipButtonText
    {
        get => _skipButtonText;
        set
        {
            if (_skipButtonText != value)
            {
                _skipButtonText = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets whether the showcase is currently active.
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        private set
        {
            if (_isActive != value)
            {
                _isActive = value;
                OnPropertyChanged();
                _nextCommand.RaiseCanExecuteChanged();
                _previousCommand.RaiseCanExecuteChanged();
                _skipCommand.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets an async hook that runs before a new step becomes active.
    /// </summary>
    public Func<ShowcaseStepTransitionContext, CancellationToken, Task>? BeforeStepAsync { get; set; }

    /// <summary>
    /// Gets or sets an async hook that runs after a new step becomes active.
    /// </summary>
    public Func<ShowcaseStepTransitionContext, CancellationToken, Task>? AfterStepAsync { get; set; }

    /// <summary>
    /// Gets or sets the store used to persist showcase progress.
    /// </summary>
    public IShowcaseProgressStore? ProgressStore
    {
        get => _progressStore;
        set
        {
            if (!ReferenceEquals(_progressStore, value))
            {
                _progressStore = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the persistence key used with <see cref="ProgressStore"/>.
    /// </summary>
    public string? PersistenceKey
    {
        get => _persistenceKey;
        set
        {
            if (_persistenceKey != value)
            {
                _persistenceKey = value;
                OnPropertyChanged();
            }
        }
    }

    private readonly RelayCommand _nextCommand;
    private readonly RelayCommand _previousCommand;
    private readonly RelayCommand _skipCommand;

    /// <summary>
    /// Command to advance to the next step.
    /// </summary>
    public ICommand NextCommand => _nextCommand;

    /// <summary>
    /// Command to go back to the previous step.
    /// </summary>
    public ICommand PreviousCommand => _previousCommand;

    /// <summary>
    /// Command to skip/cancel the showcase.
    /// </summary>
    public ICommand SkipCommand => _skipCommand;

    /// <summary>
    /// Raised when the showcase starts.
    /// </summary>
    public event EventHandler? Started;

    /// <summary>
    /// Raised when the showcase resumes from persisted progress.
    /// </summary>
    public event EventHandler? Resumed;

    /// <summary>
    /// Raised when the showcase completes (all steps finished).
    /// </summary>
    public event EventHandler? Completed;

    /// <summary>
    /// Raised when the showcase is skipped/cancelled.
    /// </summary>
    public event EventHandler? Skipped;

    /// <summary>
    /// Raised when the current step changes.
    /// </summary>
    public event EventHandler<ShowcaseStepChangedEventArgs>? StepChanged;

    /// <summary>
    /// Raised when an asynchronous transition fails via a fire-and-forget sync wrapper.
    /// </summary>
    public event EventHandler<ShowcaseTransitionFailedEventArgs>? TransitionFailed;

    /// <summary>
    /// Creates a new ShowcaseController.
    /// </summary>
    public ShowcaseController()
    {
        _nextCommand = new RelayCommand(Next, () => IsActive);
        _previousCommand = new RelayCommand(Previous, () => CanGoPrevious && IsActive);
        _skipCommand = new RelayCommand(Skip, () => IsActive);
    }

    /// <summary>
    /// Creates a new ShowcaseController with the given steps.
    /// </summary>
    public ShowcaseController(IEnumerable<ShowcaseStep> steps) : this()
    {
        foreach (var step in steps)
        {
            Steps.Add(step);
        }
    }

    /// <summary>
    /// Starts the showcase from the first step.
    /// If already active, resets and restarts from the beginning.
    /// </summary>
    public void Start() => FireAndForget(StartAsync(), ShowcaseNavigationAction.Start);

    /// <summary>
    /// Starts the showcase from the first step.
    /// If already active, resets and restarts from the beginning.
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken = default) =>
        RunTransitionAsync(StartCoreAsync, cancellationToken);

    /// <summary>
    /// Resumes the showcase from persisted progress when available.
    /// </summary>
    public void Resume() => FireAndForget(ResumeAsync(), ShowcaseNavigationAction.Resume);

    /// <summary>
    /// Resumes the showcase from persisted progress when available.
    /// </summary>
    public Task<bool> ResumeAsync(CancellationToken cancellationToken = default) =>
        RunTransitionAsync(ResumeCoreAsync, cancellationToken);

    /// <summary>
    /// Advances to the next step, or completes if on the last step.
    /// </summary>
    public void Next() => FireAndForget(NextAsync(), ShowcaseNavigationAction.Next);

    /// <summary>
    /// Advances to the next step, or completes if on the last step.
    /// </summary>
    public Task NextAsync(CancellationToken cancellationToken = default) =>
        RunTransitionAsync(NextCoreAsync, cancellationToken);

    /// <summary>
    /// Goes back to the previous step.
    /// </summary>
    public void Previous() => FireAndForget(PreviousAsync(), ShowcaseNavigationAction.Previous);

    /// <summary>
    /// Goes back to the previous step.
    /// </summary>
    public Task PreviousAsync(CancellationToken cancellationToken = default) =>
        RunTransitionAsync(PreviousCoreAsync, cancellationToken);

    /// <summary>
    /// Skips/cancels the showcase.
    /// </summary>
    public void Skip() => FireAndForget(SkipAsync(), ShowcaseNavigationAction.Skip);

    /// <summary>
    /// Skips/cancels the showcase.
    /// </summary>
    public Task SkipAsync(CancellationToken cancellationToken = default) =>
        RunTransitionAsync(SkipCoreAsync, cancellationToken);

    /// <summary>
    /// Resets the controller to its initial state.
    /// </summary>
    public void Reset() => FireAndForget(ResetAsync(), ShowcaseNavigationAction.Reset);

    /// <summary>
    /// Resets the controller to its initial state.
    /// </summary>
    public Task ResetAsync(CancellationToken cancellationToken = default) =>
        RunTransitionAsync(ResetCoreAsync, cancellationToken);

    private async Task StartCoreAsync(CancellationToken cancellationToken)
    {
        if (Steps.Count == 0)
        {
            return;
        }

        if (IsActive)
        {
            _currentIndex = -1;
        }

        await ActivateStepAsync(0, ShowcaseStepTransitionReason.Start, cancellationToken);
        Started?.Invoke(this, EventArgs.Empty);
    }

    private async Task<bool> ResumeCoreAsync(CancellationToken cancellationToken)
    {
        if (Steps.Count == 0)
        {
            return false;
        }

        var state = await LoadProgressAsync(cancellationToken);
        if (state == null || !state.IsActive)
        {
            return false;
        }

        if (state.CurrentIndex < 0 || state.CurrentIndex >= Steps.Count)
        {
            await ClearProgressAsync(cancellationToken);
            return false;
        }

        if (IsActive)
        {
            _currentIndex = -1;
        }

        await ActivateStepAsync(state.CurrentIndex, ShowcaseStepTransitionReason.Resume, cancellationToken);
        Resumed?.Invoke(this, EventArgs.Empty);
        return true;
    }

    private async Task NextCoreAsync(CancellationToken cancellationToken)
    {
        if (!IsActive)
        {
            return;
        }

        if (_currentIndex < Steps.Count - 1)
        {
            await ActivateStepAsync(_currentIndex + 1, ShowcaseStepTransitionReason.Next, cancellationToken);
            return;
        }

        await CompleteAsync(cancellationToken);
    }

    private Task PreviousCoreAsync(CancellationToken cancellationToken)
    {
        if (!IsActive || _currentIndex <= 0)
        {
            return Task.CompletedTask;
        }

        return ActivateStepAsync(_currentIndex - 1, ShowcaseStepTransitionReason.Previous, cancellationToken);
    }

    private async Task SkipCoreAsync(CancellationToken cancellationToken)
    {
        if (!IsActive)
        {
            return;
        }

        Deactivate();
        await ClearProgressAsync(cancellationToken);
        Skipped?.Invoke(this, EventArgs.Empty);
    }

    private async Task ResetCoreAsync(CancellationToken cancellationToken)
    {
        Deactivate();
        await ClearProgressAsync(cancellationToken);
    }

    private async Task CompleteAsync(CancellationToken cancellationToken)
    {
        Deactivate();
        await ClearProgressAsync(cancellationToken);
        Completed?.Invoke(this, EventArgs.Empty);
    }

    // Resets index before deactivating so that listeners see consistent state:
    // CurrentStep is null by the time IsActive becomes false.
    // We set the field directly first, then fire IsActive, then manually raise
    // the notifications that the CurrentIndex setter would have raised.
    private void Deactivate()
    {
        var hadActiveStep = _currentIndex >= 0;
        _currentIndex = -1;
        IsActive = false;

        if (hadActiveStep)
        {
            OnPropertyChanged(nameof(CurrentIndex));
            OnPropertyChanged(nameof(CurrentStep));
            OnPropertyChanged(nameof(CanGoPrevious));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(CurrentButtonText));
        }
    }

    private async Task ActivateStepAsync(
        int targetIndex,
        ShowcaseStepTransitionReason reason,
        CancellationToken cancellationToken)
    {
        if (targetIndex < 0 || targetIndex >= Steps.Count)
        {
            return;
        }

        var context = new ShowcaseStepTransitionContext(
            this,
            CurrentStep,
            Steps[targetIndex],
            _currentIndex >= 0 ? _currentIndex : null,
            targetIndex,
            reason);

        if (BeforeStepAsync != null)
        {
            await BeforeStepAsync(context, cancellationToken);
        }

        cancellationToken.ThrowIfCancellationRequested();

        IsActive = true;
        CurrentIndex = targetIndex;

        await SaveProgressAsync(cancellationToken);

        if (AfterStepAsync != null)
        {
            await AfterStepAsync(context, cancellationToken);
        }

        cancellationToken.ThrowIfCancellationRequested();
    }

    private bool CanPersistProgress() =>
        ProgressStore != null && !string.IsNullOrWhiteSpace(PersistenceKey);

    private Task<ShowcaseProgressState?> LoadProgressAsync(CancellationToken cancellationToken)
    {
        if (!CanPersistProgress())
        {
            return Task.FromResult<ShowcaseProgressState?>(null);
        }

        return ProgressStore!.LoadAsync(PersistenceKey!, cancellationToken);
    }

    private Task SaveProgressAsync(CancellationToken cancellationToken)
    {
        if (!CanPersistProgress())
        {
            return Task.CompletedTask;
        }

        return ProgressStore!.SaveAsync(
            PersistenceKey!,
            new ShowcaseProgressState(CurrentIndex, IsActive),
            cancellationToken);
    }

    private Task ClearProgressAsync(CancellationToken cancellationToken)
    {
        if (!CanPersistProgress())
        {
            return Task.CompletedTask;
        }

        return ProgressStore!.ClearAsync(PersistenceKey!, cancellationToken);
    }

    private async Task RunTransitionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken)
    {
        await RunTransitionAsync(
            async token =>
            {
                await operation(token);
                return true;
            },
            cancellationToken);
    }

    private async Task<T> RunTransitionAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        CancellationTokenSource transitionCts;

        lock (_transitionSync)
        {
            _transitionCts?.Cancel();
            _transitionCts?.Dispose();
            transitionCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _transitionCts = transitionCts;
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
                    _transitionCts = null;
            }

            transitionCts.Dispose();
        }
    }

    private void FireAndForget(Task task, ShowcaseNavigationAction action)
    {
        _ = ObserveTransitionAsync(task, action);
    }

    private async Task ObserveTransitionAsync(Task task, ShowcaseNavigationAction action)
    {
        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
            // Expected when a newer navigation operation supersedes the current one.
        }
        catch (Exception ex)
        {
            TransitionFailed?.Invoke(this, new ShowcaseTransitionFailedEventArgs(action, ex));
        }
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Deactivate();

        lock (_transitionSync)
        {
            _transitionCts?.Cancel();
            _transitionCts?.Dispose();
            _transitionCts = null;
        }

        _transitionLock.Dispose();
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ShowcaseController));
        }
    }

    private class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute();

        public void Execute(object? parameter) => _execute();

        public event EventHandler? CanExecuteChanged;

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
