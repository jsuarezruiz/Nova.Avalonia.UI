using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Nova.Avalonia.UI.Controls;

namespace Nova.Avalonia.UI.Gallery.Views;

public partial class WatermarkView : UserControl
{
    public WatermarkView()
    {
        InitializeComponent();
        
        Loaded += OnLoaded;
    }
    
    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (this.FindControl<Watermark>("ImageWatermark") is { } imageWatermark)
        {
            var uri = new Uri("avares://Nova.Avalonia.UI.Gallery/Assets/avalonia-logo.ico");
            imageWatermark.Source = new Bitmap(AssetLoader.Open(uri));
        }
    }
}
