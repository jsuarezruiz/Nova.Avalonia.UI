using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;

namespace Nova.Avalonia.UI.CodeViewer;

/// <summary>
/// Hosts source documents in a window-level drawer with light-dismiss support.
/// </summary>
public class SourceCodeDrawer : ContentControl
{
    /// <summary>
    /// Defines the <see cref="DrawerWidth"/> property.
    /// </summary>
    public static readonly StyledProperty<double> DrawerWidthProperty =
        AvaloniaProperty.Register<SourceCodeDrawer, double>(nameof(DrawerWidth), 640);

    private static readonly TimeSpan TransitionDuration = TimeSpan.FromMilliseconds(180);

    private Border? _backdrop;
    private Border? _drawerPanel;
    private Button? _closeButton;
    private TranslateTransform? _translation;
    private bool _isClosing;

    /// <summary>
    /// Occurs after the drawer exit transition completes.
    /// </summary>
    public event EventHandler? Closed;

    /// <summary>
    /// Gets or sets the width of the drawer panel.
    /// </summary>
    public double DrawerWidth
    {
        get => GetValue(DrawerWidthProperty);
        set => SetValue(DrawerWidthProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        DetachTemplateHandlers();
        base.OnApplyTemplate(e);

        _backdrop = e.NameScope.Find<Border>("PART_Backdrop");
        _drawerPanel = e.NameScope.Find<Border>("PART_DrawerPanel");
        _closeButton = e.NameScope.Find<Button>("PART_CloseButton");

        if (_backdrop is not null)
        {
            _backdrop.PointerPressed += OnBackdropPressed;
            _backdrop.Opacity = 0;
            _backdrop.Transitions =
            [
                new DoubleTransition
                {
                    Property = OpacityProperty,
                    Duration = TransitionDuration,
                    Easing = new CubicEaseOut(),
                },
            ];
        }

        if (_drawerPanel is not null)
        {
            _translation = new TranslateTransform(DrawerWidth, 0)
            {
                Transitions =
                [
                    new DoubleTransition
                    {
                        Property = TranslateTransform.XProperty,
                        Duration = TransitionDuration,
                        Easing = new CubicEaseOut(),
                    },
                ],
            };
            _drawerPanel.RenderTransform = _translation;
        }

        if (_closeButton is not null)
        {
            _closeButton.Click += OnCloseClicked;
        }

        Open();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
            return;
        }

        base.OnKeyDown(e);
    }

    /// <summary>
    /// Opens the drawer and restores pointer and keyboard interaction.
    /// </summary>
    public void Open()
    {
        _isClosing = false;
        IsHitTestVisible = true;

        if (_backdrop is not null)
        {
            _backdrop.Opacity = 0;
        }

        if (_translation is not null)
        {
            _translation.X = DrawerWidth;
        }

        Dispatcher.UIThread.Post(BeginOpen, DispatcherPriority.Render);
    }

    /// <summary>
    /// Closes the drawer after its exit transition completes.
    /// </summary>
    public void Close()
    {
        if (_isClosing)
        {
            return;
        }

        _isClosing = true;
        IsHitTestVisible = false;

        if (_backdrop is not null)
        {
            _backdrop.Opacity = 0;
        }

        if (_translation is not null)
        {
            _translation.X = DrawerWidth;
        }

        DispatcherTimer.RunOnce(
            () => Closed?.Invoke(this, EventArgs.Empty),
            TransitionDuration);
    }

    private void BeginOpen()
    {
        if (_isClosing)
        {
            return;
        }

        if (_backdrop is not null)
        {
            _backdrop.Opacity = 1;
        }

        if (_translation is not null)
        {
            _translation.X = 0;
        }

        Focus();
    }

    private void OnBackdropPressed(object? sender, PointerPressedEventArgs e)
    {
        e.Handled = true;
        Close();
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e) => Close();

    private void DetachTemplateHandlers()
    {
        if (_backdrop is not null)
        {
            _backdrop.PointerPressed -= OnBackdropPressed;
        }

        if (_closeButton is not null)
        {
            _closeButton.Click -= OnCloseClicked;
        }
    }
}
