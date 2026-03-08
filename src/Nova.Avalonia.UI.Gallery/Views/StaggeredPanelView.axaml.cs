using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Nova.Avalonia.UI.Gallery.Views;

public partial class StaggeredPanelView : UserControl
{
    public StaggeredPanelView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
