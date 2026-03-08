using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
public class ShowcaseController : INotifyPropertyChanged
{
    private int _currentIndex = -1;
    private bool _isActive;
    private string _nextButtonText = "Next";
    private string _finishButtonText = "Finish";
    private string _previousButtonText = "Previous";
    private string _skipButtonText = "Skip";

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
    /// Creates a new ShowcaseController.
    /// </summary>
    public ShowcaseController()
    {
        _nextCommand = new RelayCommand(Next, () => IsActive);
        _previousCommand = new RelayCommand(Previous, () => CanGoPrevious && IsActive);
        _skipCommand = new RelayCommand(Skip, () => IsActive);
    }

    /// <summary>
    /// Starts the showcase from the first step.
    /// If already active, resets and restarts from the beginning.
    /// </summary>
    public void Start()
    {
        if (Steps.Count == 0) return;

        if (IsActive)
        {
            _currentIndex = -1;
        }

        CurrentIndex = 0;
        IsActive = true;
        Started?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Advances to the next step, or completes if on the last step.
    /// </summary>
    public void Next()
    {
        if (!IsActive) return;

        if (_currentIndex < Steps.Count - 1)
        {
            CurrentIndex++;
        }
        else
        {
            Complete();
        }
    }

    /// <summary>
    /// Goes back to the previous step.
    /// </summary>
    public void Previous()
    {
        if (!IsActive || _currentIndex <= 0) return;
        CurrentIndex--;
    }

    /// <summary>
    /// Skips/cancels the showcase.
    /// </summary>
    public void Skip()
    {
        if (!IsActive) return;

        IsActive = false;
        CurrentIndex = -1;
        Skipped?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Resets the controller to its initial state.
    /// </summary>
    public void Reset()
    {
        IsActive = false;
        CurrentIndex = -1;
    }

    private void Complete()
    {
        IsActive = false;
        CurrentIndex = -1;
        Completed?.Invoke(this, EventArgs.Empty);
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
