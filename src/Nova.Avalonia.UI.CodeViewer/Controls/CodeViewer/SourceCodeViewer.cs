using System;
using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.VisualTree;

namespace Nova.Avalonia.UI.CodeViewer;

/// <summary>
/// Displays a collection of <see cref="SourceCodeDocument"/> instances as tabs.
/// </summary>
public class SourceCodeViewer : TabControl
{
    private INotifyCollectionChanged? _observableItems;
    private SourceCodeDocument? _resolvingDocument;

    /// <summary>
    /// Initializes a new instance of the <see cref="SourceCodeViewer"/> class.
    /// </summary>
    public SourceCodeViewer()
    {
        SelectionChanged += OnSelectionChanged;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ItemsSourceProperty)
        {
            UnsubscribeFromItems();
            _observableItems = ItemsSource as INotifyCollectionChanged;
            if (this.IsAttachedToVisualTree())
            {
                SubscribeToItems();
            }

            SelectFirstDocument();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SubscribeToItems();
        SelectFirstDocument();
        ResolveSelectedDocument();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        UnsubscribeFromItems();
        _resolvingDocument?.ReleaseCodeResolution();
        _resolvingDocument = null;
        base.OnDetachedFromVisualTree(e);
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

    private void SubscribeToItems()
    {
        if (_observableItems is not null)
        {
            _observableItems.CollectionChanged -= OnItemsCollectionChanged;
            _observableItems.CollectionChanged += OnItemsCollectionChanged;
        }
    }

    private void UnsubscribeFromItems()
    {
        if (_observableItems is not null)
        {
            _observableItems.CollectionChanged -= OnItemsCollectionChanged;
        }
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SelectFirstDocument();
        ResolveSelectedDocument();
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e) =>
        ResolveSelectedDocument();

    private void ResolveSelectedDocument()
    {
        var document = this.IsAttachedToVisualTree()
            ? SelectedItem as SourceCodeDocument
            : null;
        if (!ReferenceEquals(document, _resolvingDocument))
        {
            _resolvingDocument?.ReleaseCodeResolution();
            _resolvingDocument = document;
            _resolvingDocument?.RequestCodeResolution();
        }
    }
}
