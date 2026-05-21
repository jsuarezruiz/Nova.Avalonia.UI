using CommunityToolkit.Mvvm.ComponentModel;

namespace Nova.Avalonia.UI.Gallery.ViewModels;

public partial class ShieldViewModel : PageViewModel
{
    [ObservableProperty]
    private string _description = "A control that displays a status and a subject with a distinct background color for each part.";

    public ShieldViewModel() : base("Shield")
    {
    }
}
