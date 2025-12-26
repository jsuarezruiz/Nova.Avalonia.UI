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
                OnPropertyChanged(nameof(NextButtonText));
                
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
    /// Gets the text for the next button (changes to "Finish" on last step).
    /// </summary>
    public string NextButtonText => _currentIndex >= Steps.Count - 1 ? "Finish" : "Next";
    
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
            }
        }
    }
    
    /// <summary>
    /// Command to advance to the next step.
    /// </summary>
    public ICommand NextCommand { get; }
    
    /// <summary>
    /// Command to go back to the previous step.
    /// </summary>
    public ICommand PreviousCommand { get; }
    
    /// <summary>
    /// Command to skip/cancel the showcase.
    /// </summary>
    public ICommand SkipCommand { get; }
    
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
        NextCommand = new RelayCommand(Next, () => IsActive);
        PreviousCommand = new RelayCommand(Previous, () => CanGoPrevious && IsActive);
        SkipCommand = new RelayCommand(Skip, () => IsActive);
    }
    
    /// <summary>
    /// Starts the showcase from the first step.
    /// </summary>
    public void Start()
    {
        if (Steps.Count == 0) return;
        
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
