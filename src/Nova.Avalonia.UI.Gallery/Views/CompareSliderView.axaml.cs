using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;

namespace Nova.Avalonia.UI.Gallery.Views;

public partial class CompareSliderView : UserControl
{
    public CompareSliderView()
    {
        InitializeComponent();
    }

    private async void OnAnimateToStartClick(object? sender, RoutedEventArgs e)
    {
        await InteractiveSlider.AnimateTo(0.0, TimeSpan.FromMilliseconds(500));
    }

    private async void OnAnimateToEndClick(object? sender, RoutedEventArgs e)
    {
        await InteractiveSlider.AnimateTo(1.0, TimeSpan.FromMilliseconds(500));
    }

    private void OnResetClick(object? sender, RoutedEventArgs e)
    {
        InteractiveSlider.Reset(animate: true);
    }
}
