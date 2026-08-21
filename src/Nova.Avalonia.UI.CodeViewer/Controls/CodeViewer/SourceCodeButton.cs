using System;
using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Nova.Avalonia.UI.CodeViewer;

/// <summary>
/// Opens source documents in a compact, reusable code-viewer drawer.
/// </summary>
public class SourceCodeButton : Button
{
    /// <summary>
    /// Defines the <see cref="DocumentsSource"/> property.
    /// </summary>
    public static readonly StyledProperty<IEnumerable?> DocumentsSourceProperty =
        AvaloniaProperty.Register<SourceCodeButton, IEnumerable?>(nameof(DocumentsSource));

    /// <summary>
    /// Defines the <see cref="DrawerMaxWidth"/> property.
    /// </summary>
    public static readonly StyledProperty<double> DrawerMaxWidthProperty =
        AvaloniaProperty.Register<SourceCodeButton, double>(nameof(DrawerMaxWidth), 720);

    private OverlayLayer? _overlayLayer;

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

        AutomationProperties.SetName(this, "View source");
        ToolTip.SetTip(this, "View source");
    }

    /// <summary>
    /// Gets documents declared directly in XAML or code.
    /// </summary>
    public AvaloniaList<SourceCodeDocument> Documents { get; } = [];

    /// <summary>
    /// Gets or sets a bindable source for dynamically supplied documents.
    /// </summary>
    public IEnumerable? DocumentsSource
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
    public SourceCodeViewer Viewer { get; }

    /// <summary>
    /// Gets the drawer displayed by this button.
    /// </summary>
    public SourceCodeDrawer Drawer { get; }

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
        _overlayLayer.Children.Add(Drawer);
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

    private void OnDrawerClosed(object? sender, EventArgs e)
    {
        if (_overlayLayer is null)
        {
            return;
        }

        _overlayLayer.SizeChanged -= OnOverlaySizeChanged;
        _overlayLayer.Children.Remove(Drawer);
        _overlayLayer = null;
        Focus();
    }
}
