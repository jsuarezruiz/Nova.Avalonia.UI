using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nova.Avalonia.UI.CodeViewer;

namespace Nova.Avalonia.UI.Gallery.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] private PageViewModel _currentPage = new HomeViewModel();
    [ObservableProperty] private bool _isHome = true;
    [ObservableProperty] private NavigationSample? _selectedSample;
    [ObservableProperty] private IReadOnlyList<SourceCodeDocument> _currentSourceDocuments = [];

    public MainViewModel()
    {
        Categories = new ObservableCollection<SampleCategory>
        {
            new("Controls", new ObservableCollection<NavigationSample>
            {
                new("Avatar", new AvatarViewModel(), "Represent a user with an image, initials, icon, or custom content."),
                new("Badge", new BadgeViewModel(), "Display notification counts and status indicators."),
                new("Barcode Generator", new BarcodeGeneratorViewModel(), "Generate QR codes, barcodes, and other 2D symbologies."),
                new("Compare Slider", new CompareSliderViewModel(), "Compare two pieces of content with an interactive slider."),
                new("Code Viewer", new CodeViewerViewModel(), "Display read-only source code inline or in a reusable drawer."),
                new("Fortune", new FortuneViewModel(), "Build spin-to-win wheels and scrolling selectors for games."),
                new("Gravatar", new GravatarViewModel(), "Generate consistent identicon avatars from email addresses or IDs."),
                new("Pin Box", new PinBoxViewModel(), "Collect PIN codes, one-time passwords, and other security codes."),
                new("Rating Control", new RatingControlViewModel(), "Collect or display ratings with customizable shapes."),
                new("Scratcher", new ScratcherViewModel(), "Reveal content through an interactive scratch-card overlay."),
                new("Segmented Slider", new SegmentedSliderViewModel(), "Present stepped, weighted, or labeled ranges."),
                new("Shield", new ShieldViewModel(), "Combine a subject and status in a compact indicator."),
                new("Shimmer", new ShimmerViewModel(), "Create animated loading placeholders from existing content."),
                new("Watermark", new WatermarkViewModel(), "Overlay tiled text or images on protected content."),
            }),
            new("Panels", new ObservableCollection<NavigationSample>
            {
                new("Arc Panel", new ArcPanelViewModel(), "Arrange items along a configurable partial circle."),
                new("Auto Layout", new AutoLayoutViewModel(), "Arrange and distribute content with a Figma-inspired layout."),
                new("Bubble Panel", new BubblePanelViewModel(), "Pack circular items together without overlap."),
                new("Circular Panel", new CircularPanelViewModel(), "Arrange items around a complete circle."),
                new("Hex Panel", new HexPanelViewModel(), "Arrange items in a honeycomb grid."),
                new("Loop Panel", new LoopPanelViewModel(), "Scroll through items in a continuous loop."),
                new("Orbit Panel", new OrbitPanelViewModel(), "Arrange items across concentric orbit rings."),
                new("Overlap Panel", new OverlapPanelViewModel(), "Stack items with configurable horizontal and vertical offsets."),
                new("Radial Panel", new RadialPanelViewModel(), "Arrange items in a radial fan around a center point."),
                new("Responsive Panel", new ResponsivePanelViewModel(), "Switch child layouts at configurable breakpoints."),
                new("Staggered Panel", new StaggeredPanelViewModel(), "Arrange items in a masonry-style staggered grid."),
                new("Timeline Panel", new TimelinePanelViewModel(), "Arrange steps and events along a timeline."),
                new("Variable Size Wrap Panel", new VariableSizeWrapPanelViewModel(), "Arrange variable-size tiles in a wrapping grid."),
                new("Virtualized Staggered Panel", new VirtualizedStaggeredPanelViewModel(), "Virtualize large masonry-style data sets."),
                new("Virtualized Variable Size Wrap Panel", new VirtualizedVariableSizeWrapPanelViewModel(), "Virtualize large grids of variable-size tiles."),
            }),
        };
    }

    public ObservableCollection<SampleCategory> Categories { get; }

    public int SampleCount => Categories.Sum(category => category.Samples.Count);

    public bool IsSamplePage => !IsHome;

    partial void OnIsHomeChanged(bool value)
    {
        OnPropertyChanged(nameof(IsSamplePage));
    }

    partial void OnSelectedSampleChanged(NavigationSample? value)
    {
        if (value?.Page is null)
        {
            return;
        }

        CurrentPage = value.Page;
        CurrentSourceDocuments = CreateSourceDocuments(value.Page.GetType());
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
        CurrentSourceDocuments = [];
        IsHome = true;
    }

    private static IReadOnlyList<SourceCodeDocument> CreateSourceDocuments(Type? viewModelType)
    {
        if (viewModelType is null)
        {
            return [];
        }

        var pageName = viewModelType.Name.EndsWith("ViewModel", StringComparison.Ordinal)
            ? viewModelType.Name[..^"ViewModel".Length] + "View"
            : viewModelType.Name + "View";
        var resourceRoot = $"resm:Nova.Avalonia.UI.Gallery.Views.{pageName}";
        const string assembly = "?assembly=Nova.Avalonia.UI.Gallery";

        return
        [
            new SourceCodeDocument
            {
                Title = "XAML",
                Language = "XAML",
                Source = new Uri(resourceRoot + ".axaml" + assembly),
            },
            new SourceCodeDocument
            {
                Title = "C#",
                Language = "C#",
                Source = new Uri(resourceRoot + ".axaml.cs" + assembly),
            },
        ];
    }
}
