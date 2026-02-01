using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class ScratcherTests
{
    [AvaloniaFact]
    public void BrushSize_DefaultValue_Is30()
    {
        var scratcher = new Scratcher();
        Assert.Equal(30.0, scratcher.BrushSize);
    }

    [AvaloniaFact]
    public void Threshold_DefaultValue_Is50()
    {
        var scratcher = new Scratcher();
        Assert.Equal(50.0, scratcher.Threshold);
    }

    [AvaloniaFact]
    public void Threshold_ClampedBetween0And100()
    {
        var scratcher = new Scratcher();
        
        scratcher.Threshold = 150;
        Assert.Equal(100.0, scratcher.Threshold);
        
        scratcher.Threshold = -10;
        Assert.Equal(0.0, scratcher.Threshold);
    }

    [AvaloniaFact]
    public void OverlayBrush_DefaultValue_IsGray()
    {
        var scratcher = new Scratcher();
        Assert.NotNull(scratcher.OverlayBrush);
    }

    [AvaloniaFact]
    public void CornerRadius_DefaultValue_IsZero()
    {
        var scratcher = new Scratcher();
        Assert.Equal(new CornerRadius(0), scratcher.CornerRadius);
    }

    [AvaloniaFact]
    public void RebuildOnResize_DefaultValue_IsTrue()
    {
        var scratcher = new Scratcher();
        Assert.True(scratcher.RebuildOnResize);
    }

    [AvaloniaFact]
    public void ScratchProgress_InitialValue_IsZero()
    {
        var scratcher = new Scratcher();
        Assert.Equal(0.0, scratcher.ScratchProgress);
    }

    [AvaloniaFact]
    public void IsThresholdReached_InitialValue_IsFalse()
    {
        var scratcher = new Scratcher();
        Assert.False(scratcher.IsThresholdReached);
    }

    [AvaloniaFact]
    public void IsScratching_InitialValue_IsFalse()
    {
        var scratcher = new Scratcher();
        Assert.False(scratcher.IsScratching);
    }

    [AvaloniaFact]
    public void OverlayBrush_CanBeSet()
    {
        var scratcher = new Scratcher { OverlayBrush = Brushes.Red };
        Assert.Equal(Brushes.Red, scratcher.OverlayBrush);
    }

    [AvaloniaFact]
    public void BrushSize_CanBeSet()
    {
        var scratcher = new Scratcher { BrushSize = 50.0 };
        Assert.Equal(50.0, scratcher.BrushSize);
    }

    [AvaloniaFact]
    public void OverlayBrush_AcceptsGradient()
    {
        var gradient = new LinearGradientBrush();
        var scratcher = new Scratcher { OverlayBrush = gradient };
        Assert.Equal(gradient, scratcher.OverlayBrush);
    }

    [AvaloniaFact]
    public void Reset_ResetsProgress()
    {
        var scratcher = new Scratcher { Width = 100, Height = 100 };
        var window = new global::Avalonia.Controls.Window { Content = scratcher };
        window.Show();
        
        // Ensure layout passes and buffer creation
        scratcher.Measure(new Size(100, 100));
        scratcher.Arrange(new Rect(0, 0, 100, 100));
        
        scratcher.Reset();
        
        Assert.Equal(0.0, scratcher.ScratchProgress);
        Assert.False(scratcher.IsThresholdReached);
    }

    [AvaloniaFact]
    public void Reveal_SetsProgressTo100()
    {
        var scratcher = new Scratcher { Width = 100, Height = 100 };
        var window = new global::Avalonia.Controls.Window { Content = scratcher };
        window.Show();
        
        scratcher.Measure(new Size(100, 100));
        scratcher.Arrange(new Rect(0, 0, 100, 100));
        
        scratcher.Reveal();
        
        Assert.Equal(100.0, scratcher.ScratchProgress);
        Assert.True(scratcher.IsThresholdReached);
    }
    
    [AvaloniaFact]
    public void CornerRadius_CanBeSet()
    {
        var scratcher = new Scratcher { CornerRadius = new CornerRadius(5) };
        Assert.Equal(new CornerRadius(5), scratcher.CornerRadius);
    }

    [AvaloniaFact]
    public void RebuildOnResize_CanBeToggled()
    {
        var scratcher = new Scratcher { RebuildOnResize = false };
        Assert.False(scratcher.RebuildOnResize);
        
        scratcher.RebuildOnResize = true;
        Assert.True(scratcher.RebuildOnResize);
    }

    [AvaloniaFact]
    public void IsEnabled_DefaultValue_IsTrue()
    {
        var scratcher = new Scratcher();
        Assert.True(scratcher.IsEnabled);
    }

    [AvaloniaFact]
    public void ScratchProgress_IsReadOnly()
    {
        var property = typeof(Scratcher).GetProperty(nameof(Scratcher.ScratchProgress));
        Assert.NotNull(property);
        Assert.Null(property.GetSetMethod());
    }

    [AvaloniaFact]
    public void IsThresholdReached_IsReadOnly()
    {
        var property = typeof(Scratcher).GetProperty(nameof(Scratcher.IsThresholdReached));
        Assert.NotNull(property);
        Assert.Null(property.GetSetMethod());
    }

    [AvaloniaFact]
    public void Scratcher_IsFocusableByDefault()
    {
        var scratcher = new Scratcher();
        Assert.True(scratcher.Focusable);
    }

    [AvaloniaFact]
    public void ScratcherAutomationPeer_Returns_Correct_ClassName()
    {
        var scratcher = new Scratcher();
        var peer = new ScratcherAutomationPeer(scratcher);
        Assert.Equal("Scratcher", peer.GetClassName());
    }

    [AvaloniaFact]
    public void ScratcherAutomationPeer_Returns_Correct_Name()
    {
        var scratcher = new Scratcher { Width = 100, Height = 100 };
        var window = new global::Avalonia.Controls.Window { Content = scratcher };
        window.Show();
        
        var peer = new ScratcherAutomationPeer(scratcher);
        Assert.Equal("Scratcher", peer.GetName());
        
        scratcher.Reveal();
        Assert.Equal("Scratcher: Content revealed", peer.GetName());
    }

    [AvaloniaFact]
    public void ScratchPreservation_OnBrushChange()
    {
        var scratcher = new Scratcher
        {
            Width = 100,
            Height = 100,
            OverlayBrush = Brushes.Gray
        };

        // Initialize
        scratcher.ApplyTemplate();
        scratcher.Measure(new Size(100, 100));
        scratcher.Arrange(new Rect(0, 0, 100, 100));

        // Scratch some area (approx 100%)
        scratcher.Reveal(); 
        Assert.Equal(100, scratcher.ScratchProgress);

        // Change brush - this should NOT reset progress to 0
        scratcher.OverlayBrush = Brushes.Silver;
        Assert.Equal(100, scratcher.ScratchProgress);
    }
}
