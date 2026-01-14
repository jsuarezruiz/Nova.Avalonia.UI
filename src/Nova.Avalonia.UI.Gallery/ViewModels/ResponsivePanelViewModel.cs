using Avalonia.Animation;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;

namespace Nova.Avalonia.UI.Gallery.ViewModels;

public partial class ResponsivePanelViewModel : PageViewModel
{
    [ObservableProperty] private double _currentWidth;
    [ObservableProperty] private string _activeBreakpoint = "Unknown";

    public record NamedTransition(string Name, IPageTransition? Transition)
    {
        public override string ToString() => Name;
    }

    public ResponsivePanelViewModel() : base("ResponsivePanel")
    {
        Transitions = new List<NamedTransition>
        {
            new("Cross Fade", new CrossFade(TimeSpan.FromSeconds(0.5))),
            new("Slide Horizontal", new PageSlide(TimeSpan.FromSeconds(0.5), PageSlide.SlideAxis.Horizontal)),
            new("Slide Vertical", new PageSlide(TimeSpan.FromSeconds(0.5), PageSlide.SlideAxis.Vertical)),
            new("Slide + Fade", new CompositePageTransition
            {
                PageTransitions = new List<IPageTransition>
                {
                    new PageSlide(TimeSpan.FromSeconds(0.5), PageSlide.SlideAxis.Horizontal),
                    new CrossFade(TimeSpan.FromSeconds(0.5))
                }
            }),
            new("No Animation", null)
        };
        SelectedTransition = Transitions[0];
    }

    public List<NamedTransition> Transitions { get; }

    [ObservableProperty]
    private NamedTransition _selectedTransition;
}
