using Avalonia.Controls;
using Avalonia.Interactivity;
using Nova.Avalonia.UI.Controls;

namespace Nova.Avalonia.UI.Gallery.Views;

public partial class ShowcaseView : UserControl
{
    private readonly ShowcaseController _controller;
    
    public ShowcaseView()
    {
        InitializeComponent();
        
        // Create controller and add steps
        _controller = new ShowcaseController();
        _controller.Steps.Add(new ShowcaseStep
        {
            Key = "WelcomeTitle",
            Title = "Welcome!",
            Description = "This is the main heading of the application. Let's take a quick tour of the features.",
            TooltipPosition = ShowcaseTooltipPosition.Bottom
        });
        _controller.Steps.Add(new ShowcaseStep
        {
            Key = "StartButton",
            Title = "Start Tutorial",
            Description = "Click this button anytime to restart the interactive tutorial.",
            TooltipPosition = ShowcaseTooltipPosition.Bottom
        });
        _controller.Steps.Add(new ShowcaseStep
        {
            Key = "SettingsButton",
            Title = "Settings",
            Description = "Access application settings and preferences here.",
            TooltipPosition = ShowcaseTooltipPosition.Bottom
        });
        _controller.Steps.Add(new ShowcaseStep
        {
            Key = "InfoCard",
            Title = "Information Cards",
            Description = "Important information is displayed in these styled cards.",
            TooltipPosition = ShowcaseTooltipPosition.Bottom
        });
        _controller.Steps.Add(new ShowcaseStep
        {
            Key = "SearchBox",
            Title = "Quick Search",
            Description = "Use this search box to quickly find what you're looking for.",
            TooltipPosition = ShowcaseTooltipPosition.Top
        });
        
        // Bind controller to showcase
        ShowcaseControl.Controller = _controller;
        
        // Start button triggers the showcase
        StartButton.Click += OnStartButtonClick;
    }
    
    private void OnStartButtonClick(object? sender, RoutedEventArgs e)
    {
        _controller.Start();
        ShowcaseControl.IsActive = true;
    }
}
