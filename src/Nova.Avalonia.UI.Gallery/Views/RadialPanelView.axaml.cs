using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Nova.Avalonia.UI.Gallery.Views;

public partial class RadialPanelView : UserControl
{
    public RadialPanelView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
