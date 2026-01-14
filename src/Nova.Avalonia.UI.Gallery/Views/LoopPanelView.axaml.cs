using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nova.Avalonia.UI.Controls;

namespace Nova.Avalonia.UI.Gallery.Views;

public partial class LoopPanelView : UserControl
{
    private int _dynamicItemCounter = 3;
    private readonly string[] _colors = { "#3366FF", "#FF6633", "#33CC66", "#CC33FF", "#FFCC33", "#33CCFF", "#FF3399", "#99FF33" };

    public LoopPanelView()
    {
        InitializeComponent();
    }

    private void OnScrollLeftClick(object? sender, RoutedEventArgs e)
    {
        ProgrammaticLoopPanel.ScrollBy(-50);
    }

    private void OnScrollRightClick(object? sender, RoutedEventArgs e)
    {
        ProgrammaticLoopPanel.ScrollBy(50);
    }

    private void OnGoToIndex0Click(object? sender, RoutedEventArgs e)
    {
        ProgrammaticLoopPanel.ScrollToIndex(0, animate: true);
    }

    private void OnGoToIndex3Click(object? sender, RoutedEventArgs e)
    {
        ProgrammaticLoopPanel.ScrollToIndex(5, animate: true);
    }

    private void OnCurrentIndexChanged(object? sender, int index)
    {
        CurrentIndexDisplay.Text = $"Current Index: {index}";
    }

    private void OnAddItemClick(object? sender, RoutedEventArgs e)
    {
        _dynamicItemCounter++;
        var color = _colors[(_dynamicItemCounter - 1) % _colors.Length];
        
        var border = new Border
        {
            Width = 100,
            Height = 80,
            Background = Brush.Parse(color),
            CornerRadius = new CornerRadius(8),
            Child = new TextBlock
            {
                Text = _dynamicItemCounter.ToString(),
                FontSize = 24,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            }
        };
        
        DynamicLoopPanel.Children.Add(border);
        DynamicItemCount.Text = $"Items: {DynamicLoopPanel.Children.Count}";
    }

    private void OnRemoveItemClick(object? sender, RoutedEventArgs e)
    {
        if (DynamicLoopPanel.Children.Count > 1)
        {
            DynamicLoopPanel.Children.RemoveAt(DynamicLoopPanel.Children.Count - 1);
            _dynamicItemCounter = DynamicLoopPanel.Children.Count;
            DynamicItemCount.Text = $"Items: {DynamicLoopPanel.Children.Count}";
        }
    }
}
