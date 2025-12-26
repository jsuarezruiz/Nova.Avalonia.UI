using System;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Provides data for fortune selection events.
/// </summary>
public sealed class FortuneSelectionEventArgs : EventArgs
{
    /// <summary>
    /// Gets the index of the selected item.
    /// </summary>
    public int SelectedIndex { get; }

    /// <summary>
    /// Gets the selected item, or null if no item is selected.
    /// </summary>
    public FortuneItem? SelectedItem { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FortuneSelectionEventArgs"/> class.
    /// </summary>
    /// <param name="selectedIndex">The index of the selected item.</param>
    /// <param name="selectedItem">The selected item.</param>
    public FortuneSelectionEventArgs(int selectedIndex, FortuneItem? selectedItem)
    {
        SelectedIndex = selectedIndex;
        SelectedItem = selectedItem;
    }
}
