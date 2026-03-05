using System;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Nova.Avalonia.UI.Gallery.Views;

public partial class GravatarView : UserControl
{
    public GravatarView()
    {
        InitializeComponent();

        var uri = new Uri("avares://Nova.Avalonia.UI.Gallery/Assets/javier.jpeg");
        ImageGravatar.Source = new Bitmap(AssetLoader.Open(uri));
    }
}
