using Avalonia;
using Avalonia.Interactivity;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Event arguments for scratch progress changes.
/// </summary>
public class ScratchProgressEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Initializes a new instance of <see cref="ScratchProgressEventArgs"/>.
    /// </summary>
    public ScratchProgressEventArgs(RoutedEvent routedEvent, double progress, double previousProgress)
        : base(routedEvent)
    {
        Progress = progress;
        PreviousProgress = previousProgress;
    }

    /// <summary>
    /// Gets the current scratch progress (0-100).
    /// </summary>
    public double Progress { get; }

    /// <summary>
    /// Gets the previous progress value.
    /// </summary>
    public double PreviousProgress { get; }
}

/// <summary>
/// Event arguments for scratch events (start, update, end).
/// </summary>
public class ScratchEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Initializes a new instance of <see cref="ScratchEventArgs"/>.
    /// </summary>
    public ScratchEventArgs(RoutedEvent routedEvent, Point position, double brushSize)
        : base(routedEvent)
    {
        Position = position;
        BrushSize = brushSize;
    }

    /// <summary>
    /// Gets the position in control coordinates.
    /// </summary>
    public Point Position { get; }

    /// <summary>
    /// Gets the current brush size.
    /// </summary>
    public double BrushSize { get; }
}
