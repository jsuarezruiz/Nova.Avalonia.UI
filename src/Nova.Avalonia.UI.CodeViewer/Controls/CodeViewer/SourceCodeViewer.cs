using System;
using System.Collections;
using Avalonia;
using Avalonia.Controls;

namespace Nova.Avalonia.UI.CodeViewer;

/// <summary>
/// Displays a collection of <see cref="SourceCodeDocument"/> instances as tabs.
/// </summary>
public class SourceCodeViewer : TabControl
{
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ItemsSourceProperty)
        {
            SelectFirstDocument();
        }
    }

    private void SelectFirstDocument()
    {
        if (SelectedIndex >= 0 || ItemsSource is not IEnumerable items)
        {
            return;
        }

        var enumerator = items.GetEnumerator();
        try
        {
            if (enumerator.MoveNext())
            {
                SelectedIndex = 0;
            }
        }
        finally
        {
            (enumerator as IDisposable)?.Dispose();
        }
    }
}
