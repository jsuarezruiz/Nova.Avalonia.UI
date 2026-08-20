using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nova.Avalonia.UI.Gallery.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] private PageViewModel _currentPage = new HomeViewModel();
    [ObservableProperty] private bool _isHome = true;
    [ObservableProperty] private NavigationSample? _selectedSample;

    public MainViewModel()
    {
        Categories = new ObservableCollection<SampleCategory>
        {
            new("Controls", new ObservableCollection<NavigationSample>
            {
                new("Avatar", new AvatarViewModel(), "Profile avatar control and styling"),
                new("Badge", new BadgeViewModel(), "Notification badge control"),
                new("BarcodeGenerator", new BarcodeGeneratorViewModel(), "QR codes, barcodes, and 2D symbologies"),
                new("CompareSlider", new CompareSliderViewModel(), "Side-by-side content comparison with slider"),
                new("Fortune", new FortuneViewModel(), "Spin-to-win wheel and bar controls"),
                new("Gravatar", new GravatarViewModel(), "Identicon avatars from emails/IDs"),
                new("RatingControl", new RatingControlViewModel(), "Five-star rating control"),
                new("Scratcher", new ScratcherViewModel(), "Interactive scratch card overlay"),
                new("SegmentedSlider", new SegmentedSliderViewModel(), "Segmented range slider with labels"),
                new("Shield", new ShieldViewModel(), "Status and subject indicator"),
                new("Shimmer", new ShimmerViewModel(), "Loading placeholders with animated shimmer"),
                new("Watermark", new WatermarkViewModel(), "Tiled text or image overlay watermarks"),
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
    }

    public ObservableCollection<SampleCategory> Categories { get; }

    public int SampleCount => Categories.Sum(category => category.Samples.Count);

    partial void OnSelectedSampleChanged(NavigationSample? value)
    {
        if (value?.Page is null)
        {
            return;
        }

        CurrentPage = value.Page;
        IsHome = false;
    }

    [RelayCommand]
    private void NavigateTo(NavigationSample? sample)
    {
        SelectedSample = sample;
    }

    [RelayCommand]
    private void GoHome()
    {
        SelectedSample = null;
        CurrentPage = new HomeViewModel();
        IsHome = true;
    }
}
