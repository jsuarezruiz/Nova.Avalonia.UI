using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Labs.Controls;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nova.Avalonia.UI.Gallery.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] private bool? _showNavBar = true;
    [ObservableProperty] private bool? _showBackButton = false;
    [ObservableProperty] private NavigationSample? _selectedSample;
    private bool _initialNavigated;
    private bool _isDark;

    public MainViewModel()
    {
        NavigationRouter = new StackNavigationRouter();
        NavigationRouter.Navigated += OnNavigated;

        Samples = new ObservableCollection<NavigationSample>
        {
            new("Avatar", new AvatarViewModel(), "Profile avatar control and styling"),
            new("RatingControl", new RatingControlViewModel(), "Five-star rating control"),
            new("Shimmer", new ShimmerViewModel(), "Loading placeholders with animated shimmer")
        };

        // Start on menu hub; initial navigation clears stack once.
        _initialNavigated = true;
        _ = NavigationRouter.NavigateToAsync(new NavigationMenuViewModel(), NavigationMode.Clear);
    }

    public ObservableCollection<NavigationSample> Samples { get; }

    public INavigationRouter NavigationRouter { get; }

    partial void OnSelectedSampleChanged(NavigationSample? value)
    {
        _ = NavigateToAsync(value);
    }

    [RelayCommand]
    private Task NavigateTo(NavigationSample? sample) => NavigateToAsync(sample);

    [RelayCommand]
    private void SwapTheme()
    {
        _isDark = !_isDark;
        Application.Current!.RequestedThemeVariant = _isDark ? ThemeVariant.Dark : ThemeVariant.Light;
    }

    private async Task NavigateToAsync(NavigationSample? sample)
    {
        if (sample?.Page is null)
        {
            return;
        }

        var mode = _initialNavigated ? NavigationMode.Normal : NavigationMode.Clear;
        _initialNavigated = true;

        await NavigationRouter.NavigateToAsync(sample.Page, mode);
    }

    private void OnNavigated(object? sender, NavigatedEventArgs e)
    {
        ShowBackButton = NavigationRouter.CanGoBack;
    }
}
