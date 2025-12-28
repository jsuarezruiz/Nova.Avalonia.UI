using Avalonia.Controls;
using Nova.Avalonia.UI.Gallery.ViewModels;

namespace Nova.Avalonia.UI.Gallery.Views;

public partial class PinBoxView : UserControl
{
    public PinBoxView()
    {
        InitializeComponent();
        DataContext = new PinBoxViewModel();
    }
}
