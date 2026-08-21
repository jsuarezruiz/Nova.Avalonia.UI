using System.Text.Json.Serialization;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Represents persisted showcase progress.
/// </summary>
public sealed class ShowcaseProgressState
{
    /// <summary>
    /// Creates a new progress snapshot.
    /// </summary>
    public ShowcaseProgressState(int currentIndex, bool isActive)
        : this(currentIndex, isActive, null)
    {
    }

    /// <summary>
    /// Creates a new progress snapshot with a stable step identity.
    /// </summary>
    [JsonConstructor]
    public ShowcaseProgressState(int currentIndex, bool isActive, string? stepKey)
    {
        CurrentIndex = currentIndex;
        IsActive = isActive;
        StepKey = stepKey;
    }

    /// <summary>
    /// Gets the active step index.
    /// </summary>
    public int CurrentIndex { get; }

    /// <summary>
    /// Gets whether the showcase was active when persisted.
    /// </summary>
    public bool IsActive { get; }

    /// <summary>
    /// Gets the stable identity of the active step when available.
    /// </summary>
    public string? StepKey { get; }
}
