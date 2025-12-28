using System;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Represents the state of a PinBox item.
/// </summary>
public enum PinBoxItemState
{
    /// <summary>
    /// Default state when the box is empty and not focused.
    /// </summary>
    Default,

    /// <summary>
    /// The box is currently focused and ready for input.
    /// </summary>
    Focused,

    /// <summary>
    /// The box contains a character.
    /// </summary>
    Filled,

    /// <summary>
    /// The box is in an error state due to validation failure.
    /// </summary>
    Error,

    /// <summary>
    /// The box is disabled and cannot receive input.
    /// </summary>
    Disabled
}

/// <summary>
/// Event arguments for when a PinBox entry is completed.
/// </summary>
public class PinBoxCompletedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the completed PIN value.
    /// </summary>
    public string Pin { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="PinBoxCompletedEventArgs"/>.
    /// </summary>
    /// <param name="pin">The completed PIN value.</param>
    public PinBoxCompletedEventArgs(string pin)
    {
        Pin = pin;
    }
}

/// <summary>
/// Event arguments for when the PinBox text changes.
/// </summary>
public class PinBoxTextChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the previous text value.
    /// </summary>
    public string OldText { get; }

    /// <summary>
    /// Gets the new text value.
    /// </summary>
    public string NewText { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="PinBoxTextChangedEventArgs"/>.
    /// </summary>
    /// <param name="oldText">The previous text value.</param>
    /// <param name="newText">The new text value.</param>
    public PinBoxTextChangedEventArgs(string oldText, string newText)
    {
        OldText = oldText;
        NewText = newText;
    }
}
