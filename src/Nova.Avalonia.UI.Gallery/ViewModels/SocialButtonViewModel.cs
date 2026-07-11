using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nova.Avalonia.UI.Gallery.ViewModels;

public sealed partial class SocialButtonViewModel : PageViewModel
{
    [ObservableProperty]
    private string _lastSignIn = "No provider selected yet.";

    public SocialButtonViewModel() : base("SocialButton")
    {
    }

    [RelayCommand]
    private void SignIn(string provider)
    {
        LastSignIn = $"Signed in with {provider}.";
    }
}
