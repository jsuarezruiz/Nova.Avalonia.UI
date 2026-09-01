using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Nova.Avalonia.UI.Gallery.ViewModels;
using Nova.Avalonia.UI.Gallery.Views;

namespace Nova.Avalonia.UI.Gallery;

/// <summary>
/// Given a view model, returns the corresponding view if possible.
/// </summary>
public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        return param switch
        {
            null => null,
            ArcPanelViewModel => new ArcPanelView(),
            AutoLayoutViewModel => new AutoLayoutView(),
            AvatarViewModel => new AvatarView(),
            BadgeViewModel => new BadgeView(),
            BarcodeGeneratorViewModel => new BarcodeGeneratorView(),
            BubblePanelViewModel => new BubblePanelView(),
            CircularPanelViewModel => new CircularPanelView(),
            CodeViewerViewModel => new CodeViewerView(),
            CompareSliderViewModel => new CompareSliderView(),
            FortuneViewModel => new FortuneView(),
            GravatarViewModel => new GravatarView(),
            HexPanelViewModel => new HexPanelView(),
            HomeViewModel => new HomeView(),
            LoopPanelViewModel => new LoopPanelView(),
            NavigationMenuViewModel => new NavigationMenuView(),
            OrbitPanelViewModel => new OrbitPanelView(),
            OverlapPanelViewModel => new OverlapPanelView(),
            PinBoxViewModel => new PinBoxView(),
            RadialPanelViewModel => new RadialPanelView(),
            RatingControlViewModel => new RatingControlView(),
            ResponsivePanelViewModel => new ResponsivePanelView(),
            ScratcherViewModel => new ScratcherView(),
            SegmentedSliderViewModel => new SegmentedSliderView(),
            ShieldViewModel => new ShieldView(),
            ShimmerViewModel => new ShimmerView(),
            StaggeredPanelViewModel => new StaggeredPanelView(),
            TimelinePanelViewModel => new TimelinePanelView(),
            VariableSizeWrapPanelViewModel => new VariableSizeWrapPanelView(),
            VirtualizedStaggeredPanelViewModel => new VirtualizedStaggeredPanelView(),
            VirtualizedVariableSizeWrapPanelViewModel => new VirtualizedVariableSizeWrapPanelView(),
            WatermarkViewModel => new WatermarkView(),
            _ => new TextBlock { Text = $"Not Found: {param.GetType().FullName}" }
        };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
