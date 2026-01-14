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

        Categories = new ObservableCollection<SampleCategory>
        {
            new("Controls", new ObservableCollection<NavigationSample>
            {
                new("Avatar", new AvatarViewModel(), "Profile avatar control and styling"),
                new("Badge", new BadgeViewModel(), "Notification badge control"),
                new("BarcodeGenerator", new BarcodeGeneratorViewModel(), "QR codes, barcodes, and 2D symbologies"),
                new("RatingControl", new RatingControlViewModel(), "Five-star rating control"),
                new("Shimmer", new ShimmerViewModel(), "Loading placeholders with animated shimmer"),
            }),
            new("Panels", new ObservableCollection<NavigationSample>
            {
                new("ArcPanel", new ArcPanelViewModel(), "Arc (partial circle) layout panel"),
                new("AutoLayout", new AutoLayoutViewModel(), "Figma-like auto layout panel"),
                new("BubblePanel", new BubblePanelViewModel(), "Circle packing layout panel"),
                new("CircularPanel", new CircularPanelViewModel(), "Circular layout panel"),
                new("HexPanel", new HexPanelViewModel(), "Honeycomb hexagonal grid layout"),
                new("LoopPanel", new LoopPanelViewModel(), "Infinite looping scroll panel"),
                new("OrbitPanel", new OrbitPanelViewModel(), "Concentric orbit rings layout"),
                new("OverlapPanel", new OverlapPanelViewModel(), "Stacked cards with offset"),
                new("RadialPanel", new RadialPanelViewModel(), "Radial fan layout"),
                new("ResponsivePanel", new ResponsivePanelViewModel(), "Adaptive layout switching"),
                new("StaggeredPanel", new StaggeredPanelViewModel(), "Masonry-like staggered grid layout"),
                new("TimelinePanel", new TimelinePanelViewModel(), "Timeline/step process layout"),
                new("VariableSizeWrapPanel", new VariableSizeWrapPanelViewModel(), "Windows Metro-style tile layout"),
                new("VirtualizedStaggeredPanel", new VirtualizedStaggeredPanelViewModel(), "Virtualized masonry layout"),
                new("VirtualizedVariableSizeWrapPanel", new VirtualizedVariableSizeWrapPanelViewModel(), "Virtualized tile grid layout"),
            }),
        };

        // Start on menu hub; initial navigation clears stack once.
        _initialNavigated = true;
        _ = NavigationRouter.NavigateToAsync(new NavigationMenuViewModel(), NavigationMode.Clear);
    }

    public ObservableCollection<SampleCategory> Categories { get; }

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
