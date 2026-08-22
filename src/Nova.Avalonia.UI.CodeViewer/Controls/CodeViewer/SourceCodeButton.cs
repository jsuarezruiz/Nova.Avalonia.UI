using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;

namespace Nova.Avalonia.UI.CodeViewer;

/// <summary>
/// Opens source documents in a compact, reusable code-viewer drawer.
/// </summary>
public class SourceCodeButton : Button
{
    /// <summary>
    /// Defines the <see cref="DocumentsSource"/> property.
    /// </summary>
    public static readonly StyledProperty<IEnumerable<SourceCodeDocument>?> DocumentsSourceProperty =
        AvaloniaProperty.Register<SourceCodeButton, IEnumerable<SourceCodeDocument>?>(nameof(DocumentsSource));

    /// <summary>
    /// Defines the <see cref="DrawerMaxWidth"/> property.
    /// </summary>
    public static readonly StyledProperty<double> DrawerMaxWidthProperty =
        AvaloniaProperty.Register<SourceCodeButton, double>(
            nameof(DrawerMaxWidth),
            720,
            validate: static value => double.IsFinite(value) && value > 0);

    private OverlayLayer? _overlayLayer;

    /// <summary>
    /// Initializes a new instance of the <see cref="SourceCodeButton"/> class.
    /// </summary>
    public SourceCodeButton()
    {
        Documents.CollectionChanged += OnDocumentsChanged;

        Viewer = new SourceCodeViewer
        {
            ItemsSource = Documents,
        };

        Drawer = new SourceCodeDrawer
        {
            Content = Viewer,
        };
        Drawer.Closed += OnDrawerClosed;
    }

    /// <summary>
    /// Gets documents declared directly in XAML or code.
    /// </summary>
    public AvaloniaList<SourceCodeDocument> Documents { get; } = [];

    /// <summary>
    /// Gets or sets a bindable source for dynamically supplied documents.
    /// </summary>
    public IEnumerable<SourceCodeDocument>? DocumentsSource
    {
        get => GetValue(DocumentsSourceProperty);
        set => SetValue(DocumentsSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum width of the window-level source drawer.
    /// </summary>
    public double DrawerMaxWidth
    {
        get => GetValue(DrawerMaxWidthProperty);
        set => SetValue(DrawerMaxWidthProperty, value);
    }

    /// <summary>
    /// Gets the viewer used by the default drawer.
    /// </summary>
    internal SourceCodeViewer Viewer { get; }

    /// <summary>
    /// Gets the drawer displayed by this button.
    /// </summary>
    internal SourceCodeDrawer Drawer { get; }

    protected override void OnClick()
    {
        base.OnClick();

        if (_overlayLayer is null)
        {
            OpenDrawer();
        }
        else
        {
            Drawer.Close();
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == DocumentsSourceProperty)
        {
            Viewer.ItemsSource = DocumentsSource ?? Documents;
        }
        else if (change.Property == DrawerMaxWidthProperty && _overlayLayer is not null)
        {
            UpdateDrawerBounds();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        RemoveDrawer(restoreFocus: false);
        base.OnDetachedFromVisualTree(e);
    }

    private void OnDocumentsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (DocumentsSource is null && Viewer.ItemsSource != Documents)
        {
            Viewer.ItemsSource = Documents;
        }

        if (DocumentsSource is null && Viewer.SelectedIndex < 0 && Documents.Count > 0)
        {
            Viewer.SelectedIndex = 0;
        }
    }

    private void OpenDrawer()
    {
        var overlayLayer = OverlayLayer.GetOverlayLayer(this);
        if (overlayLayer is null)
        {
            return;
        }

        _overlayLayer = overlayLayer;
        _overlayLayer.SizeChanged += OnOverlaySizeChanged;
        UpdateDrawerBounds();
        if (!_overlayLayer.Children.Contains(Drawer))
        {
            _overlayLayer.Children.Add(Drawer);
        }

        Drawer.Open();
    }

    private void UpdateDrawerBounds()
    {
        if (_overlayLayer is null)
        {
            return;
        }

        var availableWidth = _overlayLayer.Bounds.Width;
        Drawer.Width = availableWidth;
        Drawer.Height = _overlayLayer.Bounds.Height;
        Drawer.DrawerWidth = Math.Min(
            DrawerMaxWidth,
            availableWidth <= 640 ? availableWidth : Math.Max(420, availableWidth * 0.48));
        Canvas.SetLeft(Drawer, 0);
        Canvas.SetTop(Drawer, 0);
    }

    private void OnOverlaySizeChanged(object? sender, SizeChangedEventArgs e) => UpdateDrawerBounds();

    private void OnDrawerClosed(object? sender, EventArgs e) => RemoveDrawer(restoreFocus: true);

    private void RemoveDrawer(bool restoreFocus)
    {
        if (_overlayLayer is null)
        {
            return;
        }

        Drawer.Reset();
        _overlayLayer.SizeChanged -= OnOverlaySizeChanged;
        _overlayLayer.Children.Remove(Drawer);
        _overlayLayer = null;
        if (restoreFocus && this.IsAttachedToVisualTree())
        {
            Focus();
        }
    }
}
