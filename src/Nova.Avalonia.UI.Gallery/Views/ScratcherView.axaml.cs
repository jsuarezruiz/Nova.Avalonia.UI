using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Nova.Avalonia.UI.Controls;

namespace Nova.Avalonia.UI.Gallery.Views;

public partial class ScratcherView : UserControl
{
    public ScratcherView()
    {
        InitializeComponent();
        
        var basicScratcher = this.FindControl<Scratcher>("BasicScratcher");
        var gradientScratcher = this.FindControl<Scratcher>("GradientScratcher");
        var thresholdScratcher = this.FindControl<Scratcher>("ThresholdScratcher");
        
        if (basicScratcher != null)
        {
            basicScratcher.ProgressChanged += OnBasicProgressChanged;
            basicScratcher.ThresholdReached += OnBasicThresholdReached;
        }
        
        if (gradientScratcher != null)
        {
            gradientScratcher.ProgressChanged += OnGradientProgressChanged;
        }
        
        if (thresholdScratcher != null)
        {
            thresholdScratcher.ProgressChanged += OnThresholdProgressChanged;
            thresholdScratcher.ThresholdReached += OnThresholdReached;
        }
    }
    
    private void OnBasicProgressChanged(object? sender, ScratchProgressEventArgs e)
    {
        var progressText = this.FindControl<TextBlock>("BasicProgressText");
        if (progressText != null)
        {
            progressText.Text = $"Progress: {e.Progress:F1}%";
        }
    }
    
    private void OnBasicThresholdReached(object? sender, RoutedEventArgs e)
    {
        var progressText = this.FindControl<TextBlock>("BasicProgressText");
        if (progressText != null)
        {
            progressText.Text = "🎉 Threshold Reached! Prize Revealed!";
        }
    }
    
    private void OnGradientProgressChanged(object? sender, ScratchProgressEventArgs e)
    {
        var progressText = this.FindControl<TextBlock>("GradientProgressText");
        if (progressText != null)
        {
            progressText.Text = $"Progress: {e.Progress:F1}%";
        }
    }
    
    private void OnThresholdProgressChanged(object? sender, ScratchProgressEventArgs e)
    {
        var scratcher = sender as Scratcher;
        var statusText = this.FindControl<TextBlock>("ThresholdStatusText");
        
        // Don't overwrite the "Threshold Reached" message
        if (statusText != null && scratcher != null && !scratcher.IsThresholdReached)
        {
            statusText.Text = $"Status: Scratching... ({e.Progress:F1}% of 70% threshold)";
        }
    }
    
    private void OnThresholdReached(object? sender, RoutedEventArgs e)
    {
        var statusText = this.FindControl<TextBlock>("ThresholdStatusText");
        if (statusText != null)
        {
            statusText.Text = "Threshold Reached! Content revealed.";
        }
    }
    
    private void OnResetBasicClick(object? sender, RoutedEventArgs e)
    {
        var scratcher = this.FindControl<Scratcher>("BasicScratcher");
        scratcher?.Reset();
        
        var progressText = this.FindControl<TextBlock>("BasicProgressText");
        if (progressText != null)
        {
            progressText.Text = "Progress: 0%";
        }
    }
    
    private void OnRevealBasicClick(object? sender, RoutedEventArgs e)
    {
        var scratcher = this.FindControl<Scratcher>("BasicScratcher");
        scratcher?.Reveal();
    }
    

}
