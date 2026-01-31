using Avalonia.Layout;
using CommunityToolkit.Mvvm.ComponentModel;
using Nova.Avalonia.UI.Controls;

namespace Nova.Avalonia.UI.Gallery.ViewModels;

public partial class AutoLayoutViewModel : PageViewModel
{
    [ObservableProperty] private Orientation _orientation = Orientation.Vertical;
    [ObservableProperty] private double _spacing = 10;
    [ObservableProperty] private AutoLayoutJustify _justification = AutoLayoutJustify.Packed;
    [ObservableProperty] private HorizontalAlignment _horizontalContent = HorizontalAlignment.Left;
    [ObservableProperty] private VerticalAlignment _verticalContent = VerticalAlignment.Stretch;
    [ObservableProperty] private bool _isReverseZIndex = false;

    public AutoLayoutViewModel() : base("AutoLayout")
    {
    }
}
